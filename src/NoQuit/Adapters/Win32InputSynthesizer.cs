using NoQuit.Core.Abstractions;

namespace NoQuit.Adapters;

public sealed class Win32InputSynthesizer : IInputSynthesizer
{
    public void NudgeMouse()
    {
        // Net zero movement: 1px right then 1px back. Invisible, but registers as input.
        NativeMethods.mouse_event(NativeMethods.MOUSEEVENTF_MOVE,  1, 0, 0, UIntPtr.Zero);
        Thread.Sleep(40);
        NativeMethods.mouse_event(NativeMethods.MOUSEEVENTF_MOVE, -1, 0, 0, UIntPtr.Zero);
    }

    public void PressUnusedKey()
    {
        // F15 isn't present on modern keyboards, so no app reacts — but Windows counts it as input.
        NativeMethods.keybd_event(NativeMethods.VK_F15, 0, 0, UIntPtr.Zero);
        Thread.Sleep(40);
        NativeMethods.keybd_event(NativeMethods.VK_F15, 0, NativeMethods.KEYEVENTF_KEYUP, UIntPtr.Zero);
    }
}
