using NoQuit.Core.Domain;
using NoQuit.Core.Effects;
using NoQuit.Core.Events;
using NoQuit.Core.Model;

namespace NoQuit.Tests.Domain;

public class DaemonReducerTests
{
    private static readonly DateTime T0 = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime T1 = T0.AddMinutes(5);

    // ---- AppStarted ---------------------------------------------------------

    [Fact]
    public void AppStarted_sets_state_to_active_resets_nudge_count_and_records_now()
    {
        var initial = new DaemonState(Status.Paused, NudgeCount: 42, StartedAt: T0);
        var t = DaemonReducer.Reduce(initial, new AppStarted(), T1);

        t.State.Status.Should().Be(Status.Active);
        t.State.NudgeCount.Should().Be(0);
        t.State.StartedAt.Should().Be(T1);
    }

    [Fact]
    public void AppStarted_emits_assert_awake_true_then_tray_update()
    {
        var t = DaemonReducer.Reduce(DaemonState.Initial(T0), new AppStarted(), T1);

        t.Effects.Should().HaveCount(2);
        t.Effects[0].Should().BeOfType<AssertAwake>().Which.On.Should().BeTrue();
        t.Effects[1].Should().BeOfType<UpdateTrayPresentation>().Which.Should().BeEquivalentTo(
            new UpdateTrayPresentation(Status.Active, "NoQuit :: ACTIVE"));
    }

    // ---- AppStopping --------------------------------------------------------

    [Fact]
    public void AppStopping_emits_assert_awake_false_only()
    {
        var s = new DaemonState(Status.Active, 5, T0);
        var t = DaemonReducer.Reduce(s, new AppStopping(), T1);

        t.State.Should().Be(s);
        t.Effects.Should().ContainSingle().Which.Should().Be(new AssertAwake(On: false));
    }

    // ---- ToggleRequested ----------------------------------------------------

    [Fact]
    public void ToggleRequested_from_active_yields_paused_and_asserts_awake_off()
    {
        var s = new DaemonState(Status.Active, 7, T0);
        var t = DaemonReducer.Reduce(s, new ToggleRequested(), T1);

        t.State.Status.Should().Be(Status.Paused);
        t.State.NudgeCount.Should().Be(7); // preserved
        t.Effects.Should().Contain(new AssertAwake(On: false));
        t.Effects.Should().Contain(new UpdateTrayPresentation(Status.Paused, "NoQuit :: PAUSED"));
        t.Effects.Should().Contain(new InvalidateConsole());
    }

    [Fact]
    public void ToggleRequested_from_paused_yields_active_and_asserts_awake_on()
    {
        var s = new DaemonState(Status.Paused, 7, T0);
        var t = DaemonReducer.Reduce(s, new ToggleRequested(), T1);

        t.State.Status.Should().Be(Status.Active);
        t.Effects.Should().Contain(new AssertAwake(On: true));
        t.Effects.Should().Contain(new UpdateTrayPresentation(Status.Active, "NoQuit :: ACTIVE"));
    }

    // ---- SetActiveRequested -------------------------------------------------

    [Theory]
    [InlineData(Status.Active, true,  Status.Active,  false)]
    [InlineData(Status.Active, false, Status.Paused,  true)]
    [InlineData(Status.Paused, true,  Status.Active,  true)]
    [InlineData(Status.Paused, false, Status.Paused,  false)]
    public void SetActiveRequested_drives_state_to_requested_value_and_only_emits_when_changed(
        Status from, bool active, Status expected, bool shouldEmit)
    {
        var s = new DaemonState(from, 0, T0);
        var t = DaemonReducer.Reduce(s, new SetActiveRequested(active), T1);

        t.State.Status.Should().Be(expected);
        if (shouldEmit)
            t.Effects.Should().NotBeEmpty();
        else
            t.Effects.Should().BeEmpty();
    }

    // ---- NudgeTimerTicked ---------------------------------------------------

    [Fact]
    public void NudgeTimerTicked_when_active_increments_count_and_emits_nudge_plus_assert()
    {
        var s = new DaemonState(Status.Active, 4, T0);
        var t = DaemonReducer.Reduce(s, new NudgeTimerTicked(), T1);

        t.State.NudgeCount.Should().Be(5);
        t.Effects.Should().Contain(new EmitNudge());
        t.Effects.Should().Contain(new AssertAwake(On: true));
        t.Effects.Should().Contain(new InvalidateConsole());
    }

    [Fact]
    public void NudgeTimerTicked_when_paused_is_a_noop()
    {
        var s = new DaemonState(Status.Paused, 4, T0);
        var t = DaemonReducer.Reduce(s, new NudgeTimerTicked(), T1);

        t.State.Should().Be(s);
        t.Effects.Should().BeEmpty();
    }

    // ---- SystemResumed / SessionUnlocked ------------------------------------

    [Fact]
    public void SystemResumed_when_active_reasserts_awake()
    {
        var s = new DaemonState(Status.Active, 0, T0);
        var t = DaemonReducer.Reduce(s, new SystemResumed(), T1);
        t.Effects.Should().ContainSingle().Which.Should().Be(new AssertAwake(On: true));
        t.State.Should().Be(s);
    }

    [Fact]
    public void SystemResumed_when_paused_is_a_noop()
    {
        var s = new DaemonState(Status.Paused, 0, T0);
        var t = DaemonReducer.Reduce(s, new SystemResumed(), T1);
        t.Effects.Should().BeEmpty();
        t.State.Should().Be(s);
    }

    [Fact]
    public void SessionUnlocked_behaves_like_SystemResumed()
    {
        var s = new DaemonState(Status.Active, 0, T0);
        var t = DaemonReducer.Reduce(s, new SessionUnlocked(), T1);
        t.Effects.Should().ContainSingle().Which.Should().Be(new AssertAwake(On: true));
    }

    // ---- ConsoleOpenRequested -----------------------------------------------

    [Fact]
    public void ConsoleOpenRequested_emits_OpenConsole_only_and_preserves_state()
    {
        var s = new DaemonState(Status.Paused, 99, T0);
        var t = DaemonReducer.Reduce(s, new ConsoleOpenRequested(), T1);

        t.State.Should().Be(s);
        t.Effects.Should().ContainSingle().Which.Should().BeOfType<OpenConsole>();
    }

    // ---- ExitRequested ------------------------------------------------------

    [Fact]
    public void ExitRequested_emits_assert_awake_off_then_exit()
    {
        var s = new DaemonState(Status.Active, 11, T0);
        var t = DaemonReducer.Reduce(s, new ExitRequested(), T1);

        t.Effects.Should().HaveCount(2);
        t.Effects[0].Should().Be(new AssertAwake(On: false));
        t.Effects[1].Should().BeOfType<ExitApp>();
    }

    // ---- determinism -------------------------------------------------------

    [Fact]
    public void Reduce_is_deterministic_for_same_inputs()
    {
        var s = new DaemonState(Status.Active, 3, T0);
        var t1 = DaemonReducer.Reduce(s, new NudgeTimerTicked(), T1);
        var t2 = DaemonReducer.Reduce(s, new NudgeTimerTicked(), T1);

        t1.Should().BeEquivalentTo(t2);
    }
}
