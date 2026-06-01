using NoQuit.Core.Abstractions;

namespace NoQuit.Tests.Fakes;

public sealed class FakeExitController : IExitController
{
    public int ExitRequests { get; private set; }
    public void RequestExit() => ExitRequests++;
}
