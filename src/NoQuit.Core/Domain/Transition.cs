using NoQuit.Core.Effects;
using NoQuit.Core.Model;

namespace NoQuit.Core.Domain;

public sealed record Transition(DaemonState State, IReadOnlyList<DaemonEffect> Effects)
{
    public static Transition NoOp(DaemonState state) => new(state, Array.Empty<DaemonEffect>());
}
