using NoQuit.Core.Abstractions;

namespace NoQuit.Tests.Fakes;

public sealed class FakeMutexFactory : IMutexFactory
{
    public bool CanAcquire { get; set; } = true;
    public List<string> RequestedNames { get; } = new();
    public bool LastHandleDisposed { get; private set; }

    public bool TryAcquire(string name, out IDisposable handle)
    {
        RequestedNames.Add(name);
        if (CanAcquire)
        {
            handle = new TrackingDisposable(this);
            return true;
        }
        handle = new TrackingDisposable(this);
        return false;
    }

    private sealed class TrackingDisposable : IDisposable
    {
        private readonly FakeMutexFactory _owner;
        public TrackingDisposable(FakeMutexFactory owner) => _owner = owner;
        public void Dispose() => _owner.LastHandleDisposed = true;
    }
}
