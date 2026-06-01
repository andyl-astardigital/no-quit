namespace NoQuit.Core.Abstractions;

public interface ISystemEvents
{
    event EventHandler? Resumed;
    event EventHandler? SessionUnlocked;
    void Start();
    void Stop();
}
