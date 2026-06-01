using NoQuit.Core.Domain;
using NoQuit.Core.Effects;
using NoQuit.Core.Model;

namespace NoQuit.Tests.Domain;

public class TransitionTests
{
    [Fact]
    public void NoOp_preserves_state_and_emits_no_effects()
    {
        var state = new DaemonState(Status.Active, 9, new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        var t = Transition.NoOp(state);

        t.State.Should().Be(state);
        t.Effects.Should().BeEmpty();
    }

    [Fact]
    public void Records_carry_state_and_effect_list_by_value()
    {
        var state = DaemonState.Initial(DateTime.UnixEpoch);
        var effects = new DaemonEffect[] { new AssertAwake(On: true) };
        var t = new Transition(state, effects);

        t.State.Should().Be(state);
        t.Effects.Should().BeSameAs(effects);
    }
}
