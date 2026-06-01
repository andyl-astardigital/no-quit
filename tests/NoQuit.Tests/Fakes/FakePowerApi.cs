using NoQuit.Core.Abstractions;

namespace NoQuit.Tests.Fakes;

public sealed class FakePowerApi : IPowerApi
{
    public List<bool> Calls { get; } = new();
    public bool? Last => Calls.Count == 0 ? null : Calls[^1];
    public void AssertAwake(bool stayAwake) => Calls.Add(stayAwake);
}
