using NoQuit.Core.Abstractions;

namespace NoQuit.Tests.Fakes;

public sealed class FakeSystemEvents : ISystemEvents
{
    public event EventHandler? Resumed;
    public event EventHandler? SessionUnlocked;
    public bool IsStarted { get; private set; }
    public void Start() => IsStarted = true;
    public void Stop()  => IsStarted = false;
    public void RaiseResume() => Resumed?.Invoke(this, EventArgs.Empty);
    public void RaiseUnlock() => SessionUnlocked?.Invoke(this, EventArgs.Empty);
}
