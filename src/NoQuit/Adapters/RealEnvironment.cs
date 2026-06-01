using NoQuit.Core.Abstractions;

namespace NoQuit.Adapters;

public sealed class RealEnvironment : IEnvironment
{
    public string UserName       => Environment.UserName;
    public string UserDomainName => Environment.UserDomainName;
    public string MachineName    => Environment.MachineName;
}
