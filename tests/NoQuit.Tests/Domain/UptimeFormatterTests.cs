using NoQuit.Core.Domain;

namespace NoQuit.Tests.Domain;

public class UptimeFormatterTests
{
    [Theory]
    [InlineData(0, 0, 0,    "00:00:00")]
    [InlineData(0, 0, 5,    "00:00:05")]
    [InlineData(0, 1, 30,   "00:01:30")]
    [InlineData(1, 23, 45,  "01:23:45")]
    [InlineData(48, 0, 0,   "48:00:00")]
    [InlineData(123, 59, 59,"123:59:59")]
    public void Formats_HH_MM_SS_with_unbounded_hours(int h, int m, int s, string expected)
    {
        UptimeFormatter.Format(new TimeSpan(h, m, s)).Should().Be(expected);
    }

    [Fact]
    public void Negative_inputs_clamp_to_zero()
    {
        UptimeFormatter.Format(TimeSpan.FromSeconds(-99)).Should().Be("00:00:00");
    }

    [Fact]
    public void Zero_returns_double_zero_columns()
    {
        UptimeFormatter.Format(TimeSpan.Zero).Should().Be("00:00:00");
    }
}
