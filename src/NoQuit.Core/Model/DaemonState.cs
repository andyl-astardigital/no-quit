namespace NoQuit.Core.Model;

public sealed record DaemonState(Status Status, long NudgeCount, DateTime StartedAt)
{
    public static DaemonState Initial(DateTime now) => new(Status.Active, NudgeCount: 0, StartedAt: now);

    public TimeSpan Uptime(DateTime now) => now > StartedAt ? now - StartedAt : TimeSpan.Zero;
}
