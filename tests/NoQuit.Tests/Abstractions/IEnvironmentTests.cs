using NoQuit.Core.Abstractions;
using NoQuit.Tests.Fakes;

namespace NoQuit.Tests.Abstractions;

public class IEnvironmentTests
{
    [Fact]
    public void FullUserId_joins_domain_and_user_with_backslash()
    {
        IEnvironment env = new FakeEnvironment { UserDomainName = "OFFICE", UserName = "bob" };
        env.FullUserId.Should().Be(@"OFFICE\bob");
    }

    [Fact]
    public void FullUserId_uses_default_implementation_when_not_overridden()
    {
        IEnvironment env = new FakeEnvironment();
        env.FullUserId.Should().Be(@"ACME\alice");
    }
}
