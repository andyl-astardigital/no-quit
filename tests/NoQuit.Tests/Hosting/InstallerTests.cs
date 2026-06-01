using NoQuit.Core.Hosting;
using NoQuit.Tests.Fakes;

namespace NoQuit.Tests.Hosting;

public class InstallerTests
{
    private sealed record Rig(
        FakeProcessApi Proc,
        FakeEnvironment Env,
        FakeFileSystem Fs,
        FakeTaskScheduler Sched,
        Installer Installer);

    private static Rig MakeRig()
    {
        var proc = new FakeProcessApi();
        var env = new FakeEnvironment();
        var fs = new FakeFileSystem();
        var sched = new FakeTaskScheduler();
        var inst = new Installer(proc, env, fs, sched, taskName: "NoQuit");
        return new Rig(proc, env, fs, sched, inst);
    }

    // ---- Install happy path -------------------------------------------------

    [Fact]
    public void Install_writes_xml_and_creates_and_runs_task()
    {
        var r = MakeRig();
        var result = r.Installer.Install();

        result.Ok.Should().BeTrue();
        r.Sched.Calls.Should().ContainInOrder("delete:NoQuit", "create:NoQuit:" + r.Fs.CombinePath(r.Fs.TempPath, "NoQuit-deadbeef.xml"), "run:NoQuit");
    }

    [Fact]
    public void Install_writes_xml_into_temp_with_guid_token()
    {
        var r = MakeRig();
        r.Installer.Install();

        // After install the file is deleted, but FakeFileSystem records that path was written and then deleted.
        r.Fs.Deleted.Should().ContainSingle().Which.Should().Contain("deadbeef");
    }

    [Fact]
    public void Install_xml_contains_user_id_and_exe_path()
    {
        var r = MakeRig();
        r.Env.UserName = "bob"; r.Env.UserDomainName = "OFFICE";
        r.Proc.CurrentExecutablePath = @"D:\apps\NoQuit.exe";

        // Capture: Write happens before Delete, but FakeFileSystem stores Files[path]=content,
        // then Delete removes it. Snapshot the dictionary before install completes.
        // Trick: replace OnCreate so it stops the pipeline before the deletion in `finally`.
        string capturedContent = "";
        r.Sched.OnCreate = (_, p) =>
        {
            capturedContent = r.Fs.Files[p];
            return true;
        };

        r.Installer.Install();

        capturedContent.Should().Contain(@"OFFICE\bob");
        capturedContent.Should().Contain(@"D:\apps\NoQuit.exe");
    }

    // ---- Install failure ----------------------------------------------------

    [Fact]
    public void Install_returns_not_ok_when_scheduler_create_fails()
    {
        var r = MakeRig();
        r.Sched.OnCreate = (_, _) => false;

        var result = r.Installer.Install();

        result.Ok.Should().BeFalse();
        result.Header.Should().Contain("FAILED");
    }

    [Fact]
    public void Install_does_not_run_task_when_create_failed()
    {
        var r = MakeRig();
        r.Sched.OnCreate = (_, _) => false;

        r.Installer.Install();

        r.Sched.Calls.Should().NotContain(c => c.StartsWith("run:"));
    }

    [Fact]
    public void Install_deletes_xml_file_even_on_failure()
    {
        var r = MakeRig();
        r.Sched.OnCreate = (_, _) => false;
        r.Installer.Install();

        r.Fs.Deleted.Should().NotBeEmpty();
    }

    // ---- Uninstall ----------------------------------------------------------

    [Fact]
    public void Uninstall_kills_other_NoQuit_processes_excluding_self()
    {
        var r = MakeRig();
        r.Proc.CurrentProcessId = 9999;
        r.Proc.ProcessesByName["NoQuit"] = new[] { 1, 2, 9999, 3 };

        r.Installer.Uninstall();

        r.Proc.Killed.Should().BeEquivalentTo(new[] { 1, 2, 3 });
    }

    [Fact]
    public void Uninstall_returns_ok_when_delete_succeeds()
    {
        var r = MakeRig();
        r.Sched.OnDelete = _ => true;
        r.Installer.Uninstall().Ok.Should().BeTrue();
    }

    [Fact]
    public void Uninstall_returns_warn_when_delete_fails()
    {
        var r = MakeRig();
        r.Sched.OnDelete = _ => false;
        var res = r.Installer.Uninstall();
        res.Ok.Should().BeFalse();
        res.Header.Should().Contain("WARN");
    }

    // ---- ctor validation ----------------------------------------------------

    [Fact]
    public void Ctor_rejects_null_dependencies()
    {
        var p = new FakeProcessApi();
        var e = new FakeEnvironment();
        var f = new FakeFileSystem();
        var s = new FakeTaskScheduler();

        FluentActions.Invoking(() => new Installer(null!, e, f, s)).Should().Throw<ArgumentNullException>();
        FluentActions.Invoking(() => new Installer(p, null!, f, s)).Should().Throw<ArgumentNullException>();
        FluentActions.Invoking(() => new Installer(p, e, null!, s)).Should().Throw<ArgumentNullException>();
        FluentActions.Invoking(() => new Installer(p, e, f, null!)).Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Ctor_rejects_empty_task_name()
    {
        var p = new FakeProcessApi();
        var e = new FakeEnvironment();
        var f = new FakeFileSystem();
        var s = new FakeTaskScheduler();
        FluentActions.Invoking(() => new Installer(p, e, f, s, taskName: ""))
            .Should().Throw<ArgumentException>();
    }
}
