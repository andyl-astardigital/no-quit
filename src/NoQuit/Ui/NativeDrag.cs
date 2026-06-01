using NoQuit.Adapters;

namespace NoQuit.Ui;

internal static class NativeDrag
{
    public static void Drag(IntPtr hWnd)
    {
        NativeMethods.ReleaseCapture();
        NativeMethods.SendMessageW(hWnd, NativeMethods.WM_NCLBUTTONDOWN, NativeMethods.HT_CAPTION, IntPtr.Zero);
    }
}
