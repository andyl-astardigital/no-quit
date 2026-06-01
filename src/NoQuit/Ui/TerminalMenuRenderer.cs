using System.Drawing;
using NoQuit.Core.Model;

namespace NoQuit.Ui;

internal sealed class TerminalMenuRenderer : ToolStripProfessionalRenderer
{
    public TerminalMenuRenderer() : base(new TerminalColors()) { }

    protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
    {
        e.TextColor = e.Item.Selected
            ? Palette.ToDrawing(ThemePalette.Bg)
            : Palette.ToDrawing(ThemePalette.Green);
        base.OnRenderItemText(e);
    }

    protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
    {
        var rect = new Rectangle(Point.Empty, e.Item.Size);
        using var bg = new SolidBrush(e.Item.Selected
            ? Palette.ToDrawing(ThemePalette.Green)
            : Palette.ToDrawing(ThemePalette.Bg));
        e.Graphics.FillRectangle(bg, rect);
    }

    protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
    {
        int y = e.Item.Height / 2;
        using var pen = new Pen(Palette.ToDrawing(ThemePalette.GreenFade));
        e.Graphics.DrawLine(pen, 6, y, e.Item.Width - 6, y);
    }

    private sealed class TerminalColors : ProfessionalColorTable
    {
        public override Color MenuItemSelected            => Palette.ToDrawing(ThemePalette.Green);
        public override Color MenuItemBorder              => Palette.ToDrawing(ThemePalette.GreenDim);
        public override Color MenuBorder                  => Palette.ToDrawing(ThemePalette.GreenDim);
        public override Color ToolStripDropDownBackground => Palette.ToDrawing(ThemePalette.Bg);
        public override Color ImageMarginGradientBegin    => Palette.ToDrawing(ThemePalette.Bg);
        public override Color ImageMarginGradientMiddle   => Palette.ToDrawing(ThemePalette.Bg);
        public override Color ImageMarginGradientEnd      => Palette.ToDrawing(ThemePalette.Bg);
    }
}
