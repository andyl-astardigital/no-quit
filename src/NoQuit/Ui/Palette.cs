using System.Drawing;
using NoQuit.Core.Model;

namespace NoQuit.Ui;

internal static class Palette
{
    public static Color ToDrawing(RgbaColor c) => Color.FromArgb(c.A, c.R, c.G, c.B);
}
