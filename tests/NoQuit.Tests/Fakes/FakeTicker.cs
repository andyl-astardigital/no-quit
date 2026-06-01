using NoQuit.Core.Abstractions;

namespace NoQuit.Tests.Fakes;

public sealed class FakeTicker : ITicker
{
    public event EventHandler? Tick;
    public TimeSpan Interval { get; set; } = TimeSpan.FromSeconds(1);
    public bool IsRunning { get; private set; }
    public bool IsDisposed { get; private set; }
    public void Start() => IsRunning = true;
    public void Stop()  => IsRunning = false;
    public void RaiseTick() => Tick?.Invoke(this, EventArgs.Empty);
    public void Dispose()
    {
        IsRunning = false;
        IsDisposed = true;
    }
}
