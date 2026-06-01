using NoQuit.Core.Abstractions;
using NoQuit.Core.Model;

namespace NoQuit.Tests.Fakes;

public sealed class FakeTrayShell : ITrayShell
{
    public event EventHandler? LeftClicked;
    public event EventHandler? LeftDoubleClicked;
    public event EventHandler<TrayMenuAction>? MenuActionInvoked;

    public bool IsShown { get; private set; }
    public bool IsDisposed { get; private set; }
    public (Status status, string tooltip)? LastPresentation { get; private set; }

    public void Show() => IsShown = true;
    public void Hide() => IsShown = false;

    public void UpdatePresentation(Status status, string tooltip) =>
        LastPresentation = (status, tooltip);

    public void RaiseLeft()        => LeftClicked?.Invoke(this, EventArgs.Empty);
    public void RaiseLeftDouble()  => LeftDoubleClicked?.Invoke(this, EventArgs.Empty);
    public void RaiseMenu(TrayMenuAction action) => MenuActionInvoked?.Invoke(this, action);

    public void Dispose() => IsDisposed = true;
}
