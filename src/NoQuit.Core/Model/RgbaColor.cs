namespace NoQuit.Core.Model;

public readonly record struct RgbaColor(byte R, byte G, byte B, byte A = 255)
{
    public uint Argb => ((uint)A << 24) | ((uint)R << 16) | ((uint)G << 8) | B;
}
