namespace NoQuit.Core.Abstractions;

public interface ITaskScheduler
{
    bool Delete(string taskName);
    bool CreateFromXml(string taskName, string xmlPath);
    bool Run(string taskName);
}
