using NoQuit.Adapters;
using NoQuit.Core.Abstractions;
using NoQuit.Core.Events;
using NoQuit.Core.Hosting;
using NoQuit.Core.Model;
using NoQuit.Ui;

namespace NoQuit;

internal static class Program
{
    private const string SingleInstanceLock = @"Global\NoQuit.SingleInstance.v2";

    [STAThread]
    private static int Main(string[] args)
    {
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
        ApplicationConfiguration.Initialize();

        if (args.Length > 0)
        {
            return args[0].ToLowerInvariant() switch
            {
                "--install"   => RunInstaller(install: true),
                "--uninstall" => RunInstaller(install: false),
                "--help" or "-h" or "/?" => ShowHelp(),
                _ => RunTrayApp(),
            };
        }

        return RunTrayApp();
    }

    private static int RunTrayApp()
    {
        var mutexFactory = new NamedMutexFactory();
        if (!mutexFactory.TryAcquire(SingleInstanceLock, out var lockHandle))
            return 0;
        using var _ = lockHandle;

        // Silently (re-)register the scheduled task at the current exe path on every launch.
        // Idempotent — overwrites any prior registration. Best-effort: never blocks startup.
        SilentlyRegisterAutoStart();

        // --- adapters --------------------------------------------------------
        IClock clock         = new SystemClock();
        IPowerApi power      = new Win32PowerApi();
        IInputSynthesizer in_ = new Win32InputSynthesizer();
        ISystemEvents sysEv  = new WinFormsSystemEvents();
        using ITicker ticker = new WinFormsTicker { Interval = TimeSpan.FromSeconds(50) };
        IEnvironment env     = new RealEnvironment();
        IProcessApi proc     = new RealProcessApi();
        IExitController exit = new WinFormsExitController();
        IDialogHost dialog   = new TerminalDialog();

        // --- forward reference for daemon ------------------------------------
        Daemon? daemonRef = null;
        DaemonState GetState() => daemonRef?.CurrentState ?? DaemonState.Initial(clock.UtcNow);

        // --- UI shells -------------------------------------------------------
        using var tray    = new TrayShell();
        using var console = new ConsoleWindow(clock, GetState, env, proc);

        // --- daemon ----------------------------------------------------------
        var interpreter = new EffectInterpreter(power, in_, tray, console, exit);
        var daemon = new Daemon(clock, interpreter);
        daemonRef = daemon;

        // --- wire events into daemon ----------------------------------------
        tray.LeftClicked        += (_, _) => daemon.Dispatch(new ToggleRequested());
        tray.LeftDoubleClicked  += (_, _) => daemon.Dispatch(new ConsoleOpenRequested());
        tray.MenuActionInvoked  += (_, a) => daemon.Dispatch(TrayActionToEvent(a));
        sysEv.Resumed           += (_, _) => daemon.Dispatch(new SystemResumed());
        sysEv.SessionUnlocked   += (_, _) => daemon.Dispatch(new SessionUnlocked());
        ticker.Tick             += (_, _) => daemon.Dispatch(new NudgeTimerTicked());
        console.ToggleHotkeyPressed += (_, _) => daemon.Dispatch(new ToggleRequested());
        console.KillHotkeyPressed   += (_, _) => daemon.Dispatch(new ExitRequested());

        daemon.StateChanged += (_, _) => console.Redraw();

        // --- crash → terminal dialog -----------------------------------------
        Application.ThreadException += (_, e) => ShowCrash(dialog, e.Exception);
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            if (e.ExceptionObject is Exception ex) ShowCrash(dialog, ex);
        };

        // --- boot ------------------------------------------------------------
        tray.Show();
        sysEv.Start();
        ticker.Start();
        daemon.Dispatch(new AppStarted());

        try { Application.Run(); }
        finally
        {
            daemon.Dispatch(new AppStopping());
            ticker.Stop();
            sysEv.Stop();
            console.ForceClose();
        }
        return 0;
    }

    private static DaemonEvent TrayActionToEvent(TrayMenuAction action) => action switch
    {
        TrayMenuAction.Activate    => new SetActiveRequested(true),
        TrayMenuAction.Pause       => new SetActiveRequested(false),
        TrayMenuAction.OpenConsole => new ConsoleOpenRequested(),
        TrayMenuAction.Exit        => new ExitRequested(),
        _ => throw new InvalidOperationException($"Unhandled tray action: {action}"),
    };

    private static void ShowCrash(IDialogHost dialog, Exception ex)
    {
        dialog.Show(
            "[ FAULT :: segfault ]",
            $"{ex.GetType().Name}: {ex.Message}",
            ex.StackTrace?.Split('\n').FirstOrDefault()?.Trim(),
            DialogTone.Error);
    }

    private static void SilentlyRegisterAutoStart()
    {
        try
        {
            var installer = new Installer(
                new RealProcessApi(),
                new RealEnvironment(),
                new RealFileSystem(),
                new SchtasksScheduler());
            installer.Install(runImmediately: false);
        }
        catch { /* best effort */ }
    }

    private static int RunInstaller(bool install)
    {
        var installer = new Installer(
            new RealProcessApi(),
            new RealEnvironment(),
            new RealFileSystem(),
            new SchtasksScheduler());

        var result = install ? installer.Install() : installer.Uninstall();
        var dialog = new TerminalDialog();
        dialog.Show(result.Header, result.Message, result.Detail,
            result.Ok ? DialogTone.Info : DialogTone.Error);
        return result.Ok ? 0 : 1;
    }

    private static int ShowHelp()
    {
        var dialog = new TerminalDialog();
        dialog.Show(
            "[ no_quit :: usage ]",
            "tray-resident daemon that prevents Windows from sleeping.",
            "NoQuit.exe              run + auto-register (logon | unlock | resume)\n" +
            "NoQuit.exe --install    re-register and launch via scheduler\n" +
            "NoQuit.exe --uninstall  remove auto-start, kill running instance\n\n" +
            "hotkeys: [SPACE] toggle  [ESC] close  [CTRL+Q] kill",
            DialogTone.Info);
        return 0;
    }
}
