using NoQuit.Core.Abstractions;

namespace NoQuit.Adapters;

public sealed class WinFormsTicker : ITicker
{
    private readonly System.Windows.Forms.Timer _timer = new();
    private TimeSpan _interval = TimeSpan.FromSeconds(50);
    private bool _disposed;

    public WinFormsTicker()
    {
        _timer.Interval = (int)_interval.TotalMilliseconds;
        _timer.Tick += (_, _) => Tick?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler? Tick;

    public TimeSpan Interval
    {
        get => _interval;
        set
        {
            if (value <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(value));
            _interval = value;
            _timer.Interval = (int)value.TotalMilliseconds;
        }
    }

    public void Start() => _timer.Start();
    public void Stop()  => _timer.Stop();

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _timer.Stop();
        _timer.Dispose();
    }
}
