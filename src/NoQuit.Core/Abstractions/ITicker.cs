namespace NoQuit.Core.Abstractions;

public interface ITicker : IDisposable
{
    event EventHandler? Tick;
    TimeSpan Interval { get; set; }
    void Start();
    void Stop();
}
