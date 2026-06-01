using NoQuit.Core.Abstractions;

namespace NoQuit.Tests.Fakes;

public sealed class FakeProcessApi : IProcessApi
{
    public string CurrentExecutablePath { get; set; } = @"C:\fake\NoQuit.exe";
    public int    CurrentProcessId      { get; set; } = 1234;
    public Dictionary<string, int[]> ProcessesByName { get; } = new();
    public List<int> Killed { get; } = new();

    public IReadOnlyList<int> FindProcessIds(string processName) =>
        ProcessesByName.TryGetValue(processName, out var ids) ? ids : Array.Empty<int>();

    public void KillProcess(int processId) => Killed.Add(processId);
}
