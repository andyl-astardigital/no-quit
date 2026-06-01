using System.Runtime.InteropServices;

namespace NoQuit.Adapters;

internal static partial class NativeMethods
{
    // --- power -------------------------------------------------------------
    [LibraryImport("kernel32.dll", SetLastError = true)]
    public static partial uint SetThreadExecutionState(uint esFlags);

    public const uint ES_CONTINUOUS       = 0x80000000;
    public const uint ES_SYSTEM_REQUIRED  = 0x00000001;
    public const uint ES_DISPLAY_REQUIRED = 0x00000002;

    // --- input -------------------------------------------------------------
    [LibraryImport("user32.dll")]
    public static partial void mouse_event(uint dwFlags, int dx, int dy, uint dwData, UIntPtr dwExtraInfo);
    public const uint MOUSEEVENTF_MOVE = 0x0001;

    [LibraryImport("user32.dll")]
    public static partial void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);
    public const byte VK_F15          = 0x7E;
    public const uint KEYEVENTF_KEYUP = 0x0002;

    // --- icon --------------------------------------------------------------
    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool DestroyIcon(IntPtr hIcon);

    // --- borderless window drag --------------------------------------------
    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool ReleaseCapture();

    [LibraryImport("user32.dll")]
    public static partial IntPtr SendMessageW(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

    public const uint WM_NCLBUTTONDOWN = 0x00A1;
    public const int  HT_CAPTION       = 0x2;
}
