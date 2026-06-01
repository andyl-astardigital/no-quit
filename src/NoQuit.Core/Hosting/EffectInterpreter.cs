using NoQuit.Core.Abstractions;
using NoQuit.Core.Effects;

namespace NoQuit.Core.Hosting;

public sealed class EffectInterpreter : IEffectInterpreter
{
    private readonly IPowerApi _power;
    private readonly IInputSynthesizer _input;
    private readonly ITrayShell _tray;
    private readonly IConsoleHost _console;
    private readonly IExitController _exit;

    public EffectInterpreter(
        IPowerApi power,
        IInputSynthesizer input,
        ITrayShell tray,
        IConsoleHost console,
        IExitController exit)
    {
        ArgumentNullException.ThrowIfNull(power);
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(tray);
        ArgumentNullException.ThrowIfNull(console);
        ArgumentNullException.ThrowIfNull(exit);

        _power = power;
        _input = input;
        _tray = tray;
        _console = console;
        _exit = exit;
    }

    public void Apply(DaemonEffect effect)
    {
        ArgumentNullException.ThrowIfNull(effect);

        switch (effect)
        {
            case AssertAwake a:
                _power.AssertAwake(a.On);
                break;
            case EmitNudge:
                _input.NudgeMouse();
                _input.PressUnusedKey();
                break;
            case UpdateTrayPresentation u:
                _tray.UpdatePresentation(u.Status, u.Tooltip);
                break;
            case OpenConsole:
                _console.Open();
                break;
            case InvalidateConsole:
                _console.Redraw();
                break;
            case ExitApp:
                _exit.RequestExit();
                break;
            default:
                throw new InvalidOperationException($"Unhandled effect: {effect.GetType().Name}");
        }
    }
}
