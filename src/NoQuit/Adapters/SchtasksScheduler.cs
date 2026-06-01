using System.Diagnostics;
using NoQuit.Core.Abstractions;

namespace NoQuit.Adapters;

public sealed class SchtasksScheduler : ITaskScheduler
{
    public bool Delete(string taskName) =>
        Exec($"/Delete /TN \"{taskName}\" /F") == 0;

    public bool CreateFromXml(string taskName, string xmlPath) =>
        Exec($"/Create /TN \"{taskName}\" /XML \"{xmlPath}\" /F") == 0;

    public bool Run(string taskName) =>
        Exec($"/Run /TN \"{taskName}\"") == 0;

    private static int Exec(string args)
    {
        var psi = new ProcessStartInfo("schtasks.exe", args)
        {
            CreateNoWindow         = true,
            UseShellExecute        = false,
            RedirectStandardOutput = true,
            RedirectStandardError  = true,
        };
        using var p = Process.Start(psi)
            ?? throw new InvalidOperationException("Failed to launch schtasks.exe");
        p.WaitForExit();
        return p.ExitCode;
    }
}
