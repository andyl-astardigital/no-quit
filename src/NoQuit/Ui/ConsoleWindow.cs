using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using NoQuit.Core.Abstractions;
using NoQuit.Core.Domain;
using NoQuit.Core.Model;

namespace NoQuit.Ui;

public sealed class ConsoleWindow : Form, IConsoleHost
{
    private readonly IClock _clock;
    private readonly Func<DaemonState> _stateProvider;
    private readonly IEnvironment _env;
    private readonly IProcessApi _process;
    private readonly System.Windows.Forms.Timer _refreshTimer;
    private readonly List<string> _bootLog;
    private readonly Sprite _sprite = SpriteAtlas.CoffeeCup16;
    private Font? _smallFont;
    private bool _caretOn = true;
    private bool _allowClose;

    public event EventHandler? ToggleHotkeyPressed;
    public event EventHandler? KillHotkeyPressed;

    public ConsoleWindow(IClock clock, Func<DaemonState> stateProvider, IEnvironment env, IProcessApi process)
    {
        _clock = clock;
        _stateProvider = stateProvider;
        _env = env;
        _process = process;

        Text = "no_quit :: console";
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(720, 520);
        MinimumSize = new Size(640, 440);
        BackColor = Palette.ToDrawing(ThemePalette.Bg);
        ForeColor = Palette.ToDrawing(ThemePalette.Green);
        Font = MonoFontProvider.Get(10f);
        DoubleBuffered = true;
        FormBorderStyle = FormBorderStyle.None;
        KeyPreview = true;
        ShowInTaskbar = false;

        _refreshTimer = new System.Windows.Forms.Timer { Interval = 500 };
        _refreshTimer.Tick += (_, _) => { _caretOn = !_caretOn; Invalidate(); };

        _bootLog = new List<string>
        {
            "> boot     :: loading no_quit.sys",
            "> probe    :: SetThreadExecutionState ... ok",
            "> probe    :: input synthesis (mouse+f15) ... ok",
            "> probe    :: power event subscription ... ok",
            "> daemon   :: ONLINE",
        };

        KeyDown += OnKeyDown;
        MouseDown += OnDragOrCloseDown;
        Paint += OnPaintFrame;
        FormClosing += OnClosing;
    }

    public bool IsOpen => Visible;

    public void Open()
    {
        if (IsDisposed) return;
        if (Visible) { Activate(); return; }
        _refreshTimer.Start();
        Show();
        Activate();
    }

    public void Redraw()
    {
        if (IsDisposed || !IsHandleCreated) return;
        if (InvokeRequired) BeginInvoke(new Action(Invalidate));
        else Invalidate();
    }

    public void ForceClose()
    {
        _allowClose = true;
        Close();
    }

    // --- input ---------------------------------------------------------------

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        switch (e.KeyCode)
        {
            case Keys.Escape:
                e.Handled = true;
                Hide();
                _refreshTimer.Stop();
                break;
            case Keys.Space:
                e.Handled = true;
                ToggleHotkeyPressed?.Invoke(this, EventArgs.Empty);
                break;
            case Keys.Q when e.Control:
                e.Handled = true;
                KillHotkeyPressed?.Invoke(this, EventArgs.Empty);
                break;
        }
    }

    private void OnDragOrCloseDown(object? sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left) return;

        // Close glyph
        var closeRect = new Rectangle(ClientSize.Width - 32, 4, 24, 20);
        if (closeRect.Contains(e.Location))
        {
            Hide();
            _refreshTimer.Stop();
            return;
        }

        // Drag from title strip
        if (e.Y < 28) NativeDrag.Drag(Handle);
    }

    private void OnClosing(object? sender, FormClosingEventArgs e)
    {
        if (_allowClose) return;
        if (e.CloseReason is CloseReason.UserClosing or CloseReason.None)
        {
            e.Cancel = true;
            Hide();
            _refreshTimer.Stop();
        }
    }

    // --- paint ---------------------------------------------------------------

    private void OnPaintFrame(object? sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.None;
        g.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;
        g.Clear(Palette.ToDrawing(ThemePalette.Bg));

        DrawFrame(g);
        DrawTitleBar(g);
        DrawSprite(g);
        DrawStatusPanel(g);
        DrawLogPanel(g);
        DrawCaret(g);
        DrawScanlines(g);
    }

    private void DrawFrame(Graphics g)
    {
        using var pen = new Pen(Palette.ToDrawing(ThemePalette.GreenDim));
        g.DrawRectangle(pen, 0, 0, ClientSize.Width - 1, ClientSize.Height - 1);
    }

    private void DrawTitleBar(Graphics g)
    {
        var rect = new Rectangle(0, 0, ClientSize.Width, 28);
        using (var bg = new SolidBrush(Palette.ToDrawing(ThemePalette.BgLight)))
            g.FillRectangle(bg, rect);
        using (var pen = new Pen(Palette.ToDrawing(ThemePalette.GreenDim)))
            g.DrawLine(pen, 0, 28, ClientSize.Width, 28);

        DrawString(g, "[ no_quit.sys ]  // tty-1", 12, 7, ThemePalette.Green);
        DrawString(g, " X ",                       ClientSize.Width - 28, 7, ThemePalette.Red);
    }

    private void DrawSprite(Graphics g)
    {
        const int pixelSize = 14;
        const int x = 28;
        const int y = 60;
        var status = _stateProvider().Status;
        IconFactory.Paint(g, _sprite, x, y, pixelSize, status);
        DrawSmall(g, "// avatar.bin", x, y + _sprite.Height * pixelSize + 12, ThemePalette.GreenDim);
    }

    private void DrawStatusPanel(Graphics g)
    {
        var state = _stateProvider();
        int x = 28 + _sprite.Width * 14 + 40;
        int y = 60;

        DrawString(g, "// status", x, y, ThemePalette.GreenDim);

        bool active = state.Status == Status.Active;
        DrawString(g, "  state  : ", x, y + 28, ThemePalette.Green);
        DrawString(g,
            active ? "ACTIVE   <<< running" : "PAUSED   <<< idle",
            x + 100, y + 28,
            active ? ThemePalette.Green : ThemePalette.Amber);

        DrawString(g, $"  uptime : {UptimeFormatter.Format(state.Uptime(_clock.UtcNow))}", x, y + 50,  ThemePalette.Green);
        DrawString(g, $"  nudges : {state.NudgeCount}",                                     x, y + 70,  ThemePalette.Green);
        DrawString(g, $"  pid    : {_process.CurrentProcessId}",                            x, y + 90,  ThemePalette.Green);
        DrawString(g, $"  host   : {_env.MachineName.ToLowerInvariant()}",                  x, y + 110, ThemePalette.Green);
        DrawString(g, $"  user   : {_env.UserName.ToLowerInvariant()}",                     x, y + 130, ThemePalette.Green);

        DrawString(g, "// keymap", x, y + 170, ThemePalette.GreenDim);
        DrawString(g, "  [SPACE] toggle    [ESC] close    [CTRL+Q] kill", x, y + 190, ThemePalette.Green);
    }

    private void DrawLogPanel(Graphics g)
    {
        const int top = 308;
        using (var pen = new Pen(Palette.ToDrawing(ThemePalette.GreenDim)))
            g.DrawLine(pen, 12, top - 8, ClientSize.Width - 12, top - 8);

        DrawString(g, "// log", 12, top - 24, ThemePalette.GreenDim);

        int lineY = top;
        foreach (var line in _bootLog)
        {
            DrawString(g, line, 18, lineY, ThemePalette.GreenDim);
            lineY += 16;
        }
    }

    private void DrawCaret(Graphics g)
    {
        int y = ClientSize.Height - 28;
        DrawString(g, _caretOn ? "no_quit$ _" : "no_quit$  ", 18, y, ThemePalette.Green);
    }

    private void DrawScanlines(Graphics g)
    {
        using var pen = new Pen(Color.FromArgb(18, 0, 0, 0));
        for (int y = 0; y < ClientSize.Height; y += 3)
            g.DrawLine(pen, 0, y, ClientSize.Width, y);
    }

    private void DrawString(Graphics g, string text, int x, int y, RgbaColor color)
    {
        using var brush = new SolidBrush(Palette.ToDrawing(color));
        g.DrawString(text, Font, brush, x, y);
    }

    private void DrawSmall(Graphics g, string text, int x, int y, RgbaColor color)
    {
        _smallFont ??= MonoFontProvider.Get(8f);
        using var brush = new SolidBrush(Palette.ToDrawing(color));
        g.DrawString(text, _smallFont, brush, x, y);
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

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _refreshTimer.Stop();
            _refreshTimer.Dispose();
            _smallFont?.Dispose();
        }
        base.Dispose(disposing);
    }
}
