using NoQuit.Core.Abstractions;

namespace NoQuit.Adapters;

public sealed class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
