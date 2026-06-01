using NoQuit.Core.Abstractions;

namespace NoQuit.Tests.Fakes;

public sealed class FakeEnvironment : IEnvironment
{
    public string UserName       { get; set; } = "alice";
    public string UserDomainName { get; set; } = "ACME";
    public string MachineName    { get; set; } = "WORKSTATION";
}
