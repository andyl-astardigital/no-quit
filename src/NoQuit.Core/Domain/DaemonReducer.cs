using NoQuit.Core.Effects;
using NoQuit.Core.Events;
using NoQuit.Core.Model;

namespace NoQuit.Core.Domain;

public static class DaemonReducer
{
    public static Transition Reduce(DaemonState state, DaemonEvent ev, DateTime now) => ev switch
    {
        AppStarted             => Start(state, now),
        AppStopping            => Stop(state),
        ToggleRequested        => SetStatus(state, state.Status == Status.Active ? Status.Paused : Status.Active),
        SetActiveRequested req => SetStatus(state, req.Active ? Status.Active : Status.Paused),
        NudgeTimerTicked       => Tick(state),
        SystemResumed          => Reassert(state),
        SessionUnlocked        => Reassert(state),
        ConsoleOpenRequested   => new Transition(state, new DaemonEffect[] { new OpenConsole() }),
        ExitRequested          => Exit(state),
        _                      => Transition.NoOp(state),
    };

    private static Transition Start(DaemonState s, DateTime now)
    {
        var next = s with { Status = Status.Active, StartedAt = now, NudgeCount = 0 };
        return new Transition(next, new DaemonEffect[]
        {
            new AssertAwake(On: true),
            new UpdateTrayPresentation(Status.Active, Tooltip(Status.Active)),
        });
    }

    private static Transition Stop(DaemonState s) =>
        new(s, new DaemonEffect[] { new AssertAwake(On: false) });

    private static Transition SetStatus(DaemonState s, Status next)
    {
        if (s.Status == next) return Transition.NoOp(s);

        return new Transition(s with { Status = next }, new DaemonEffect[]
        {
            new AssertAwake(On: next == Status.Active),
            new UpdateTrayPresentation(next, Tooltip(next)),
            new InvalidateConsole(),
        });
    }

    private static Transition Tick(DaemonState s)
    {
        if (s.Status != Status.Active) return Transition.NoOp(s);

        return new Transition(s with { NudgeCount = s.NudgeCount + 1 }, new DaemonEffect[]
        {
            new EmitNudge(),
            new AssertAwake(On: true),
            new InvalidateConsole(),
        });
    }

    private static Transition Reassert(DaemonState s) =>
        s.Status == Status.Active
            ? new Transition(s, new DaemonEffect[] { new AssertAwake(On: true) })
            : Transition.NoOp(s);

    private static Transition Exit(DaemonState s) =>
        new(s, new DaemonEffect[] { new AssertAwake(On: false), new ExitApp() });

    private static string Tooltip(Status s) =>
        s == Status.Active ? "NoQuit :: ACTIVE" : "NoQuit :: PAUSED";
}
