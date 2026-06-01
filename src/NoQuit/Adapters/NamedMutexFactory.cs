using NoQuit.Core.Abstractions;

namespace NoQuit.Adapters;

public sealed class NamedMutexFactory : IMutexFactory
{
    public bool TryAcquire(string name, out IDisposable handle)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        var mutex = new Mutex(initiallyOwned: true, name: name, out bool createdNew);
        if (createdNew)
        {
            handle = new MutexHolder(mutex);
            return true;
        }

        mutex.Dispose();
        handle = NullDisposable.Instance;
        return false;
    }

    private sealed class MutexHolder : IDisposable
    {
        private readonly Mutex _mutex;
        private bool _disposed;

        public MutexHolder(Mutex mutex) => _mutex = mutex;

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            try { _mutex.ReleaseMutex(); } catch (ApplicationException) { /* not owned */ }
            _mutex.Dispose();
        }
    }

    private sealed class NullDisposable : IDisposable
    {
        public static readonly NullDisposable Instance = new();
        public void Dispose() { }
    }
}
