namespace NoQuit.Core.Abstractions;

public interface IEnvironment
{
    string UserName { get; }
    string UserDomainName { get; }
    string MachineName { get; }
    string FullUserId => $@"{UserDomainName}\{UserName}";
}
