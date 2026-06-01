namespace NoQuit.Core.Events;

public abstract record DaemonEvent;

public sealed record AppStarted           : DaemonEvent;
public sealed record AppStopping          : DaemonEvent;
public sealed record ToggleRequested      : DaemonEvent;
public sealed record SetActiveRequested(bool Active) : DaemonEvent;
public sealed record NudgeTimerTicked     : DaemonEvent;
public sealed record SystemResumed        : DaemonEvent;
public sealed record SessionUnlocked      : DaemonEvent;
public sealed record ConsoleOpenRequested : DaemonEvent;
public sealed record ExitRequested        : DaemonEvent;
