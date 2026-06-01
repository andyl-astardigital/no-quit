using NoQuit.Core.Effects;

namespace NoQuit.Core.Abstractions;

public interface IEffectInterpreter
{
    void Apply(DaemonEffect effect);
}
