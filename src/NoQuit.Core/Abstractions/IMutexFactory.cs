namespace NoQuit.Core.Abstractions;

public interface IMutexFactory
{
    // Returns true if this is the only holder; false if another process holds the lock.
    bool TryAcquire(string name, out IDisposable handle);
}
