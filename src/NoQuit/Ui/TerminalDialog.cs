using System.Drawing;
using NoQuit.Core.Abstractions;
using NoQuit.Core.Model;

namespace NoQuit.Ui;

public sealed class TerminalDialog : IDialogHost
{
    public void Show(string header, string line1, string? body, DialogTone tone)
    {
        using var form = new DialogForm(header, line1, body, tone);
        form.ShowDialog();
    }

    private sealed class DialogForm : Form
    {
        private readonly string _header;
        private readonly string _line1;
        private readonly string? _body;
        private readonly DialogTone _tone;

        public DialogForm(string header, string line1, string? body, DialogTone tone)
        {
            _header = header;
            _line1 = line1;
            _body = body;
            _tone = tone;

            Text = header;
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(560, 260);
            BackColor = Palette.ToDrawing(ThemePalette.Bg);
            ForeColor = Palette.ToDrawing(ThemePalette.Green);
            Font = MonoFontProvider.Get(10f);
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            DoubleBuffered = true;
            KeyPreview = true;

            var ok = new Button
            {
                Text = "[ OK ]",
                FlatStyle = FlatStyle.Flat,
                BackColor = Palette.ToDrawing(ThemePalette.Bg),
                ForeColor = Palette.ToDrawing(AccentColor()),
                Font = MonoFontProvider.Get(10f),
                Width = 120,
                Height = 32,
                Left = ClientSize.Width / 2 - 60,
                Top  = ClientSize.Height - 50,
            };
            ok.FlatAppearance.BorderColor = Palette.ToDrawing(AccentColor());
            ok.FlatAppearance.MouseOverBackColor = Palette.ToDrawing(AccentColor());
            ok.Click += (_, _) => Close();
            AcceptButton = ok;
            CancelButton = ok;
            Controls.Add(ok);

            KeyDown += (_, e) =>
            {
                if (e.KeyCode is Keys.Escape or Keys.Enter) Close();
            };
            MouseDown += (_, e) =>
            {
                if (e.Button == MouseButtons.Left && e.Y < 28) NativeDrag.Drag(Handle);
            };
            Paint += OnPaint;
        }

        private RgbaColor AccentColor() => _tone switch
        {
            DialogTone.Error   => ThemePalette.Red,
            DialogTone.Warning => ThemePalette.Amber,
            _                  => ThemePalette.Green,
        };

        private void OnPaint(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Palette.ToDrawing(ThemePalette.Bg));

            using (var pen = new Pen(Palette.ToDrawing(AccentColor())))
                g.DrawRectangle(pen, 0, 0, ClientSize.Width - 1, ClientSize.Height - 1);

            using (var titleBg = new SolidBrush(Palette.ToDrawing(ThemePalette.BgLight)))
                g.FillRectangle(titleBg, 0, 0, ClientSize.Width, 28);
            using (var pen = new Pen(Palette.ToDrawing(AccentColor())))
                g.DrawLine(pen, 0, 28, ClientSize.Width, 28);

            using (var brush = new SolidBrush(Palette.ToDrawing(AccentColor())))
                g.DrawString(_header, Font, brush, 12, 7);

            using (var body = new SolidBrush(Palette.ToDrawing(ThemePalette.Green)))
            {
                g.DrawString("> " + _line1, Font, body, 20, 56);
                if (_body is null) return;
                int y = 84;
                foreach (var line in _body.Split('\n'))
                {
                    g.DrawString("  " + line, Font, body, 20, y);
                    y += 20;
                }
            }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ClassStyle |= 0x00020000; // CS_DROPSHADOW
                return cp;
            }
        }
    }
}
