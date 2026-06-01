using NoQuit.Core.Abstractions;

namespace NoQuit.Tests.Fakes;

public sealed class FakeConsoleHost : IConsoleHost
{
    public event EventHandler? ToggleHotkeyPressed;
    public event EventHandler? KillHotkeyPressed;

    public bool IsOpen { get; private set; }
    public int Redraws { get; private set; }
    public int Opens { get; private set; }

    public void Open()  { Opens++; IsOpen = true; }
    public void Redraw() => Redraws++;

    public void RaiseToggle() => ToggleHotkeyPressed?.Invoke(this, EventArgs.Empty);
    public void RaiseKill()   => KillHotkeyPressed?.Invoke(this, EventArgs.Empty);
}
