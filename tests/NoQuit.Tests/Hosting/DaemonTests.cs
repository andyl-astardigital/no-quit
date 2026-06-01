using NoQuit.Core.Effects;
using NoQuit.Core.Events;
using NoQuit.Core.Hosting;
using NoQuit.Core.Model;
using NoQuit.Tests.Fakes;

namespace NoQuit.Tests.Hosting;

public class DaemonTests
{
    private static readonly DateTime T0 = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Ctor_initializes_state_via_clock()
    {
        var clock = new FakeClock(T0);
        var d = new Daemon(clock, new RecordingInterpreter());

        d.CurrentState.Status.Should().Be(Status.Active);
        d.CurrentState.StartedAt.Should().Be(T0);
        d.CurrentState.NudgeCount.Should().Be(0);
    }

    [Fact]
    public void Dispatch_runs_reducer_and_advances_state()
    {
        var clock = new FakeClock(T0);
        var interp = new RecordingInterpreter();
        var d = new Daemon(clock, interp);

        d.Dispatch(new NudgeTimerTicked());

        d.CurrentState.NudgeCount.Should().Be(1);
        interp.Effects.Should().Contain(new EmitNudge());
        interp.Effects.Should().Contain(new AssertAwake(On: true));
    }

    [Fact]
    public void Dispatch_emits_state_changed_event()
    {
        var d = new Daemon(new FakeClock(T0), new RecordingInterpreter());
        DaemonState? observed = null;
        d.StateChanged += (_, s) => observed = s;

        d.Dispatch(new ToggleRequested());

        observed.Should().NotBeNull();
        observed!.Status.Should().Be(Status.Paused);
    }

    [Fact]
    public void Dispatch_null_throws()
    {
        var d = new Daemon(new FakeClock(T0), new RecordingInterpreter());
        FluentActions.Invoking(() => d.Dispatch(null!))
            .Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Ctor_null_args_throw()
    {
        FluentActions.Invoking(() => new Daemon(null!, new RecordingInterpreter()))
            .Should().Throw<ArgumentNullException>();
        FluentActions.Invoking(() => new Daemon(new FakeClock(T0), null!))
            .Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Toggle_twice_returns_to_active_with_same_nudge_count_preserved()
    {
        var d = new Daemon(new FakeClock(T0), new RecordingInterpreter());
        d.Dispatch(new NudgeTimerTicked()); // count = 1
        d.Dispatch(new ToggleRequested());  // paused
        d.Dispatch(new NudgeTimerTicked()); // no-op while paused
        d.Dispatch(new ToggleRequested());  // active again

        d.CurrentState.Status.Should().Be(Status.Active);
        d.CurrentState.NudgeCount.Should().Be(1);
    }

    [Fact]
    public void Many_dispatches_in_sequence_are_recorded_in_order()
    {
        var interp = new RecordingInterpreter();
        var d = new Daemon(new FakeClock(T0), interp);

        d.Dispatch(new AppStarted());
        d.Dispatch(new ToggleRequested());
        d.Dispatch(new ToggleRequested());
        d.Dispatch(new ExitRequested());

        interp.Effects.OfType<ExitApp>().Should().HaveCount(1);
        interp.Effects.OfType<AssertAwake>().Last().On.Should().BeFalse();
    }
}
