using NoQuit.Core.Abstractions;
using NoQuit.Core.Domain;

namespace NoQuit.Core.Hosting;

public sealed class Installer
{
    public const string DefaultTaskName = "NoQuit";

    private readonly IProcessApi _process;
    private readonly IEnvironment _env;
    private readonly IFileSystem _fs;
    private readonly ITaskScheduler _scheduler;
    private readonly string _taskName;

    public Installer(
        IProcessApi process,
        IEnvironment env,
        IFileSystem fs,
        ITaskScheduler scheduler,
        string taskName = DefaultTaskName)
    {
        ArgumentNullException.ThrowIfNull(process);
        ArgumentNullException.ThrowIfNull(env);
        ArgumentNullException.ThrowIfNull(fs);
        ArgumentNullException.ThrowIfNull(scheduler);
        ArgumentException.ThrowIfNullOrEmpty(taskName);

        _process = process;
        _env = env;
        _fs = fs;
        _scheduler = scheduler;
        _taskName = taskName;
    }

    public InstallResult Install()
    {
        string exePath = _process.CurrentExecutablePath;
        string user    = _env.FullUserId;
        string xmlPath = _fs.CombinePath(_fs.TempPath, $"NoQuit-{_fs.NewGuidToken()}.xml");

        try
        {
            _fs.WriteAllText(xmlPath, TaskXmlBuilder.Build(exePath, user));

            // Best-effort cleanup of any prior registration. Ignore success/failure.
            _scheduler.Delete(_taskName);

            if (!_scheduler.CreateFromXml(_taskName, xmlPath))
            {
                return new InstallResult(
                    Ok:      false,
                    Header:  "[ INSTALL :: FAILED ]",
                    Message: "task registration rejected by scheduler",
                    Detail:  $"manual fallback:\nschtasks /Create /TN {_taskName} /XML \"{xmlPath}\" /F");
            }

            _scheduler.Run(_taskName);

            return new InstallResult(
                Ok:      true,
                Header:  "[ INSTALL :: OK ]",
                Message: "no_quit daemon registered.",
                Detail:  "triggers : logon | unlock | resume\nstatus   : daemon launched\navatar   : look for the cup in your tray");
        }
        finally
        {
            try { _fs.Delete(xmlPath); } catch { /* best effort */ }
        }
    }

    public InstallResult Uninstall()
    {
        int self = _process.CurrentProcessId;
        foreach (var pid in _process.FindProcessIds("NoQuit"))
        {
            if (pid == self) continue;
            try { _process.KillProcess(pid); } catch { /* best effort */ }
        }

        bool deleted = _scheduler.Delete(_taskName);
        return new InstallResult(
            Ok:      deleted,
            Header:  deleted ? "[ UNINSTALL :: OK ]" : "[ UNINSTALL :: WARN ]",
            Message: deleted ? "no_quit daemon removed." : "task removal returned non-zero -- may not have existed.",
            Detail:  null);
    }
}
