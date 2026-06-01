namespace NoQuit.Core.Abstractions;

public interface IProcessApi
{
    string CurrentExecutablePath { get; }
    int CurrentProcessId { get; }
    IReadOnlyList<int> FindProcessIds(string processName);
    void KillProcess(int processId);
}
