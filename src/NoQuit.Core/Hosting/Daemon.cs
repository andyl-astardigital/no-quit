using NoQuit.Core.Abstractions;
using NoQuit.Core.Domain;
using NoQuit.Core.Events;
using NoQuit.Core.Model;

namespace NoQuit.Core.Hosting;

public sealed class Daemon
{
    private readonly IClock _clock;
    private readonly IEffectInterpreter _interpreter;
    private readonly object _gate = new();
    private DaemonState _state;

    public Daemon(IClock clock, IEffectInterpreter interpreter)
    {
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(interpreter);
        _clock = clock;
        _interpreter = interpreter;
        _state = DaemonState.Initial(_clock.UtcNow);
    }

    public DaemonState CurrentState
    {
        get { lock (_gate) return _state; }
    }

    public event EventHandler<DaemonState>? StateChanged;

    public void Dispatch(DaemonEvent ev)
    {
        ArgumentNullException.ThrowIfNull(ev);

        Transition transition;
        lock (_gate)
        {
            transition = DaemonReducer.Reduce(_state, ev, _clock.UtcNow);
            _state = transition.State;
        }

        foreach (var effect in transition.Effects)
        {
            _interpreter.Apply(effect);
        }

        StateChanged?.Invoke(this, transition.State);
    }
}
