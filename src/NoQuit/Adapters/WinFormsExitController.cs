using NoQuit.Core.Abstractions;

namespace NoQuit.Adapters;

public sealed class WinFormsExitController : IExitController
{
    public void RequestExit() => Application.Exit();
}
