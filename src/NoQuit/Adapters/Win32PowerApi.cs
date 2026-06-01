using NoQuit.Core.Abstractions;

namespace NoQuit.Adapters;

public sealed class Win32PowerApi : IPowerApi
{
    public void AssertAwake(bool stayAwake)
    {
        uint flags = stayAwake
            ? NativeMethods.ES_CONTINUOUS | NativeMethods.ES_SYSTEM_REQUIRED | NativeMethods.ES_DISPLAY_REQUIRED
            : NativeMethods.ES_CONTINUOUS;
        NativeMethods.SetThreadExecutionState(flags);
    }
}
