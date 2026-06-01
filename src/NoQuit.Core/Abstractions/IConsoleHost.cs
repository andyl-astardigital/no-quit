namespace NoQuit.Core.Abstractions;

public interface IConsoleHost
{
    event EventHandler? ToggleHotkeyPressed;
    event EventHandler? KillHotkeyPressed;

    void Open();
    void Redraw();
    bool IsOpen { get; }
}
