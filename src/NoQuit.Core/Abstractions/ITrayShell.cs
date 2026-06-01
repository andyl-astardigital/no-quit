using NoQuit.Core.Model;

namespace NoQuit.Core.Abstractions;

public enum TrayMenuAction
{
    Activate,
    Pause,
    OpenConsole,
    Exit,
}

public interface ITrayShell : IDisposable
{
    event EventHandler? LeftClicked;
    event EventHandler? LeftDoubleClicked;
    event EventHandler<TrayMenuAction>? MenuActionInvoked;

    void Show();
    void Hide();
    void UpdatePresentation(Status status, string tooltip);
}
