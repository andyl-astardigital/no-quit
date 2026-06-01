namespace NoQuit.Core.Hosting;

public sealed record InstallResult(bool Ok, string Header, string Message, string? Detail);
