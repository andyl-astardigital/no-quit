using System.Diagnostics;
using NoQuit.Core.Abstractions;

namespace NoQuit.Adapters;

public sealed class RealProcessApi : IProcessApi
{
    public string CurrentExecutablePath =>
        Environment.ProcessPath
        ?? Process.GetCurrentProcess().MainModule?.FileName
        ?? throw new InvalidOperationException("Cannot resolve current executable path.");

    public int CurrentProcessId => Environment.ProcessId;

    public IReadOnlyList<int> FindProcessIds(string processName)
    {
        ArgumentException.ThrowIfNullOrEmpty(processName);
        var procs = Process.GetProcessesByName(processName);
        try
        {
            return procs.Select(p => p.Id).ToList();
        }
        finally
        {
            foreach (var p in procs) p.Dispose();
        }
    }

    public void KillProcess(int processId)
    {
        try
        {
            using var p = Process.GetProcessById(processId);
            p.Kill(entireProcessTree: true);
        }
        catch (ArgumentException) { /* already gone */ }
        catch (InvalidOperationException) { /* already exited */ }
    }
}
