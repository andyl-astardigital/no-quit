namespace NoQuit.Core.Abstractions;

public enum DialogTone
{
    Info,
    Warning,
    Error,
}

public interface IDialogHost
{
    void Show(string header, string line1, string? body, DialogTone tone);
}
