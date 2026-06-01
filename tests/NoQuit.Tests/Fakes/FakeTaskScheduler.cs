using NoQuit.Core.Abstractions;

namespace NoQuit.Tests.Fakes;

public sealed class FakeTaskScheduler : ITaskScheduler
{
    public Func<string, bool>          OnDelete = _ => true;
    public Func<string, string, bool>  OnCreate = (_, _) => true;
    public Func<string, bool>          OnRun    = _ => true;

    public List<string> Calls { get; } = new();

    public bool Delete(string taskName)
    {
        Calls.Add($"delete:{taskName}");
        return OnDelete(taskName);
    }

    public bool CreateFromXml(string taskName, string xmlPath)
    {
        Calls.Add($"create:{taskName}:{xmlPath}");
        return OnCreate(taskName, xmlPath);
    }

    public bool Run(string taskName)
    {
        Calls.Add($"run:{taskName}");
        return OnRun(taskName);
    }
}
