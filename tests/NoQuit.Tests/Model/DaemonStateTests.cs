using NoQuit.Core.Model;

namespace NoQuit.Tests.Model;

public class DaemonStateTests
{
    private static readonly DateTime T0 = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Initial_starts_active_with_zero_nudges_at_supplied_time()
    {
        var s = DaemonState.Initial(T0);
        s.Status.Should().Be(Status.Active);
        s.NudgeCount.Should().Be(0);
        s.StartedAt.Should().Be(T0);
    }

    [Fact]
    public void Uptime_is_difference_when_now_is_after_start()
    {
        var s = new DaemonState(Status.Active, 0, T0);
        s.Uptime(T0.AddMinutes(7)).Should().Be(TimeSpan.FromMinutes(7));
    }

    [Fact]
    public void Uptime_clamps_to_zero_when_now_is_before_start()
    {
        var s = new DaemonState(Status.Active, 0, T0);
        s.Uptime(T0.AddSeconds(-1)).Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public void Records_are_value_equal()
    {
        var a = new DaemonState(Status.Active, 5, T0);
        var b = new DaemonState(Status.Active, 5, T0);
        a.Should().Be(b);
    }

    [Fact]
    public void With_expressions_produce_new_instances()
    {
        var a = DaemonState.Initial(T0);
        var b = a with { NudgeCount = 1 };
        a.NudgeCount.Should().Be(0);
        b.NudgeCount.Should().Be(1);
    }
}
