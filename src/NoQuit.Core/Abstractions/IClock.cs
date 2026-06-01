namespace NoQuit.Core.Abstractions;

public interface IClock
{
    DateTime UtcNow { get; }
}
