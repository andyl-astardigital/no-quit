using System.Drawing;
using System.Drawing.Drawing2D;
using NoQuit.Adapters;
using NoQuit.Core.Domain;
using NoQuit.Core.Model;

namespace NoQuit.Ui;

internal static class IconFactory
{
    public static Icon Build(Sprite sprite, Status status, int outputSize)
    {
        int pixelSize = Math.Max(1, outputSize / sprite.Width);
        using var bmp = new Bitmap(outputSize, outputSize);
        using (var g = Graphics.FromImage(bmp))
        {
            g.SmoothingMode = SmoothingMode.None;
            g.PixelOffsetMode = PixelOffsetMode.Half;
            g.Clear(Color.Transparent);
            Paint(g, sprite, 0, 0, pixelSize, status);
        }

        IntPtr hIcon = bmp.GetHicon();
        try
        {
            return (Icon)Icon.FromHandle(hIcon).Clone();
        }
        finally
        {
            NativeMethods.DestroyIcon(hIcon);
        }
    }

    public static void Paint(Graphics g, Sprite sprite, int originX, int originY, int pixelSize, Status status)
    {
        Color bright = Palette.ToDrawing(ThemePalette.BrightFor(status));
        Color dim    = Palette.ToDrawing(ThemePalette.DimFor(status));
        using var brightBrush = new SolidBrush(bright);
        using var dimBrush    = new SolidBrush(dim);

        foreach (var px in SpriteRenderer.Render(sprite, originX, originY, pixelSize))
        {
            var brush = px.Cell == SpriteCell.Bright ? brightBrush : dimBrush;
            g.FillRectangle(brush, px.X, px.Y, px.Size, px.Size);
        }
    }
}
