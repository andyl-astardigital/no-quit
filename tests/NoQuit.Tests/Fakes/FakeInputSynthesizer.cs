using NoQuit.Core.Abstractions;

namespace NoQuit.Tests.Fakes;

public sealed class FakeInputSynthesizer : IInputSynthesizer
{
    public int MouseNudges { get; private set; }
    public int Keystrokes { get; private set; }
    public void NudgeMouse()      => MouseNudges++;
    public void PressUnusedKey()  => Keystrokes++;
}
