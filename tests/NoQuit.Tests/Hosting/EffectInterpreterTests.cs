using NoQuit.Core.Effects;
using NoQuit.Core.Hosting;
using NoQuit.Core.Model;
using NoQuit.Tests.Fakes;

namespace NoQuit.Tests.Hosting;

public class EffectInterpreterTests
{
    private sealed record Rig(
        FakePowerApi Power,
        FakeInputSynthesizer Input,
        FakeTrayShell Tray,
        FakeConsoleHost Console,
        FakeExitController Exit,
        EffectInterpreter Interp);

    private static Rig MakeRig()
    {
        var power = new FakePowerApi();
        var input = new FakeInputSynthesizer();
        var tray = new FakeTrayShell();
        var console = new FakeConsoleHost();
        var exit = new FakeExitController();
        var interp = new EffectInterpreter(power, input, tray, console, exit);
        return new Rig(power, input, tray, console, exit, interp);
    }

    [Fact]
    public void AssertAwake_true_calls_power_api_with_true()
    {
        var rig = MakeRig();
        rig.Interp.Apply(new AssertAwake(On: true));
        rig.Power.Calls.Should().ContainSingle().Which.Should().BeTrue();
    }

    [Fact]
    public void AssertAwake_false_calls_power_api_with_false()
    {
        var rig = MakeRig();
        rig.Interp.Apply(new AssertAwake(On: false));
        rig.Power.Calls.Should().ContainSingle().Which.Should().BeFalse();
    }

    [Fact]
    public void EmitNudge_triggers_mouse_and_keystroke()
    {
        var rig = MakeRig();
        rig.Interp.Apply(new EmitNudge());
        rig.Input.MouseNudges.Should().Be(1);
        rig.Input.Keystrokes.Should().Be(1);
    }

    [Fact]
    public void UpdateTrayPresentation_forwards_status_and_tooltip()
    {
        var rig = MakeRig();
        rig.Interp.Apply(new UpdateTrayPresentation(Status.Paused, "hello"));
        rig.Tray.LastPresentation.Should().Be((Status.Paused, "hello"));
    }

    [Fact]
    public void OpenConsole_opens_the_console_host()
    {
        var rig = MakeRig();
        rig.Interp.Apply(new OpenConsole());
        rig.Console.Opens.Should().Be(1);
    }

    [Fact]
    public void InvalidateConsole_calls_redraw()
    {
        var rig = MakeRig();
        rig.Interp.Apply(new InvalidateConsole());
        rig.Console.Redraws.Should().Be(1);
    }

    [Fact]
    public void ExitApp_requests_exit()
    {
        var rig = MakeRig();
        rig.Interp.Apply(new ExitApp());
        rig.Exit.ExitRequests.Should().Be(1);
    }

    [Fact]
    public void Apply_null_throws()
    {
        var rig = MakeRig();
        FluentActions.Invoking(() => rig.Interp.Apply(null!))
            .Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Ctor_rejects_null_dependencies()
    {
        var p = new FakePowerApi();
        var i = new FakeInputSynthesizer();
        var t = new FakeTrayShell();
        var c = new FakeConsoleHost();
        var x = new FakeExitController();

        FluentActions.Invoking(() => new EffectInterpreter(null!, i, t, c, x)).Should().Throw<ArgumentNullException>();
        FluentActions.Invoking(() => new EffectInterpreter(p, null!, t, c, x)).Should().Throw<ArgumentNullException>();
        FluentActions.Invoking(() => new EffectInterpreter(p, i, null!, c, x)).Should().Throw<ArgumentNullException>();
        FluentActions.Invoking(() => new EffectInterpreter(p, i, t, null!, x)).Should().Throw<ArgumentNullException>();
        FluentActions.Invoking(() => new EffectInterpreter(p, i, t, c, null!)).Should().Throw<ArgumentNullException>();
    }
}
