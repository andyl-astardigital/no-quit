using NoQuit.Core.Abstractions;
using NoQuit.Core.Effects;

namespace NoQuit.Tests.Fakes;

/// <summary>
/// Records every effect applied to it without executing any side effect.
/// </summary>
public sealed class RecordingInterpreter : IEffectInterpreter
{
    public List<DaemonEffect> Effects { get; } = new();
    public void Apply(DaemonEffect effect) => Effects.Add(effect);
    public void Clear() => Effects.Clear();
}
