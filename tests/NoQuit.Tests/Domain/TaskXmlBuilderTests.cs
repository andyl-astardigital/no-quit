using System.Xml.Linq;
using NoQuit.Core.Domain;

namespace NoQuit.Tests.Domain;

public class TaskXmlBuilderTests
{
    private const string ExePath = @"C:\Tools\NoQuit\NoQuit.exe";
    private const string User    = @"ACME\alice";

    [Fact]
    public void Build_produces_well_formed_xml()
    {
        string xml = TaskXmlBuilder.Build(ExePath, User);
        var act = () => XDocument.Parse(xml);
        act.Should().NotThrow();
    }

    [Fact]
    public void Build_uses_task_scheduler_namespace()
    {
        var doc = XDocument.Parse(TaskXmlBuilder.Build(ExePath, User));
        doc.Root!.Name.NamespaceName.Should().Be("http://schemas.microsoft.com/windows/2004/02/mit/task");
    }

    [Fact]
    public void Build_emits_three_trigger_kinds()
    {
        var doc = XDocument.Parse(TaskXmlBuilder.Build(ExePath, User));
        XNamespace ns = "http://schemas.microsoft.com/windows/2004/02/mit/task";
        var triggers = doc.Descendants(ns + "Triggers").Single();

        triggers.Elements(ns + "LogonTrigger").Should().HaveCount(1);
        triggers.Elements(ns + "SessionStateChangeTrigger").Should().HaveCount(1);
        triggers.Elements(ns + "EventTrigger").Should().HaveCount(1);
    }

    [Fact]
    public void Build_session_unlock_state_is_correct()
    {
        var doc = XDocument.Parse(TaskXmlBuilder.Build(ExePath, User));
        XNamespace ns = "http://schemas.microsoft.com/windows/2004/02/mit/task";
        var state = doc.Descendants(ns + "SessionStateChangeTrigger").Single()
                       .Element(ns + "StateChange")!.Value;
        state.Should().Be("SessionUnlock");
    }

    [Fact]
    public void Build_event_subscription_targets_power_troubleshooter_event_1()
    {
        var doc = XDocument.Parse(TaskXmlBuilder.Build(ExePath, User));
        XNamespace ns = "http://schemas.microsoft.com/windows/2004/02/mit/task";
        var subscription = doc.Descendants(ns + "Subscription").Single().Value;
        subscription.Should().Contain("Microsoft-Windows-Power-Troubleshooter");
        subscription.Should().Contain("EventID=1");
    }

    [Fact]
    public void Build_uses_provided_exe_path()
    {
        var doc = XDocument.Parse(TaskXmlBuilder.Build(ExePath, User));
        XNamespace ns = "http://schemas.microsoft.com/windows/2004/02/mit/task";
        doc.Descendants(ns + "Command").Single().Value.Should().Be(ExePath);
    }

    [Fact]
    public void Build_uses_provided_user_id_in_all_user_slots()
    {
        var doc = XDocument.Parse(TaskXmlBuilder.Build(ExePath, User));
        XNamespace ns = "http://schemas.microsoft.com/windows/2004/02/mit/task";
        doc.Descendants(ns + "UserId").Select(e => e.Value).Should().AllBe(User);
    }

    [Fact]
    public void Build_escapes_special_xml_chars_in_paths()
    {
        const string nasty = @"C:\a&b<c>'""\NoQuit.exe";
        string xml = TaskXmlBuilder.Build(nasty, User);

        var act = () => XDocument.Parse(xml);
        act.Should().NotThrow();

        var doc = XDocument.Parse(xml);
        XNamespace ns = "http://schemas.microsoft.com/windows/2004/02/mit/task";
        doc.Descendants(ns + "Command").Single().Value.Should().Be(nasty);
    }

    [Fact]
    public void Build_throws_on_null_arguments()
    {
        FluentActions.Invoking(() => TaskXmlBuilder.Build(null!, User)).Should().Throw<ArgumentNullException>();
        FluentActions.Invoking(() => TaskXmlBuilder.Build(ExePath, null!)).Should().Throw<ArgumentNullException>();
    }
}
