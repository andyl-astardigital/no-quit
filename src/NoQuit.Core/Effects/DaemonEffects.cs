using NoQuit.Core.Model;

namespace NoQuit.Core.Effects;

public abstract record DaemonEffect;

public sealed record AssertAwake(bool On) : DaemonEffect;
public sealed record EmitNudge : DaemonEffect;
public sealed record UpdateTrayPresentation(Status Status, string Tooltip) : DaemonEffect;
public sealed record OpenConsole : DaemonEffect;
public sealed record InvalidateConsole : DaemonEffect;
public sealed record ExitApp : DaemonEffect;
