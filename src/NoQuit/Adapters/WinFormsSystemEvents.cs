using Microsoft.Win32;
using NoQuit.Core.Abstractions;

namespace NoQuit.Adapters;

public sealed class WinFormsSystemEvents : ISystemEvents
{
    private bool _started;

    public event EventHandler? Resumed;
    public event EventHandler? SessionUnlocked;

    public void Start()
    {
        if (_started) return;
        SystemEvents.PowerModeChanged += OnPowerModeChanged;
        SystemEvents.SessionSwitch    += OnSessionSwitch;
        _started = true;
    }

    public void Stop()
    {
        if (!_started) return;
        SystemEvents.PowerModeChanged -= OnPowerModeChanged;
        SystemEvents.SessionSwitch    -= OnSessionSwitch;
        _started = false;
    }

    private void OnPowerModeChanged(object sender, PowerModeChangedEventArgs e)
    {
        if (e.Mode == PowerModes.Resume) Resumed?.Invoke(this, EventArgs.Empty);
    }

    private void OnSessionSwitch(object sender, SessionSwitchEventArgs e)
    {
        if (e.Reason == SessionSwitchReason.SessionUnlock) SessionUnlocked?.Invoke(this, EventArgs.Empty);
    }
}
