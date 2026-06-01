using NoQuit.Core.Abstractions;

namespace NoQuit.Tests.Fakes;

public sealed class FakeDialogHost : IDialogHost
{
    public sealed record Shown(string Header, string Line1, string? Body, DialogTone Tone);

    public List<Shown> Calls { get; } = new();
    public Shown? Last => Calls.Count == 0 ? null : Calls[^1];

    public void Show(string header, string line1, string? body, DialogTone tone) =>
        Calls.Add(new Shown(header, line1, body, tone));
}
