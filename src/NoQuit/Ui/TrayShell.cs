using System.Drawing;
using NoQuit.Core.Abstractions;
using NoQuit.Core.Domain;
using NoQuit.Core.Model;

namespace NoQuit.Ui;

public sealed class TrayShell : ITrayShell
{
    private readonly NotifyIcon _notify;
    private readonly ContextMenuStrip _menu;
    private readonly ToolStripMenuItem _activeItem;
    private readonly ToolStripMenuItem _pausedItem;
    private readonly Icon _activeIcon;
    private readonly Icon _pausedIcon;
    private bool _disposed;

    public event EventHandler? LeftClicked;
    public event EventHandler? LeftDoubleClicked;
    public event EventHandler<TrayMenuAction>? MenuActionInvoked;

    public TrayShell()
    {
        _activeIcon = IconFactory.Build(SpriteAtlas.CoffeeCup16, Status.Active, outputSize: 32);
        _pausedIcon = IconFactory.Build(SpriteAtlas.CoffeeCup16, Status.Paused, outputSize: 32);

        var monoBold    = new Font(MonoFontProvider.Get(9f), FontStyle.Bold);
        var monoRegular = MonoFontProvider.Get(9f);

        _activeItem = Item("[*] ACTIVE", () => MenuActionInvoked?.Invoke(this, TrayMenuAction.Activate));
        _pausedItem = Item("[ ] PAUSED", () => MenuActionInvoked?.Invoke(this, TrayMenuAction.Pause));
        _activeItem.Font = monoBold;
        _pausedItem.Font = monoRegular;

        _menu = new ContextMenuStrip
        {
            BackColor = Palette.ToDrawing(ThemePalette.Bg),
            ForeColor = Palette.ToDrawing(ThemePalette.Green),
            Renderer  = new TerminalMenuRenderer(),
            Font      = monoRegular,
        };
        _menu.Items.Add(_activeItem);
        _menu.Items.Add(_pausedItem);
        _menu.Items.Add(new ToolStripSeparator());
        _menu.Items.Add(Item("> CONSOLE", () => MenuActionInvoked?.Invoke(this, TrayMenuAction.OpenConsole)));
        _menu.Items.Add(new ToolStripSeparator());
        _menu.Items.Add(Item("EXIT", () => MenuActionInvoked?.Invoke(this, TrayMenuAction.Exit)));

        _notify = new NotifyIcon
        {
            Icon = _activeIcon,
            Text = "NoQuit :: ACTIVE",
            Visible = false,
            ContextMenuStrip = _menu,
        };

        _notify.MouseClick += (_, e) =>
        {
            if (e.Button == MouseButtons.Left) LeftClicked?.Invoke(this, EventArgs.Empty);
        };
        _notify.MouseDoubleClick += (_, e) =>
        {
            if (e.Button == MouseButtons.Left) LeftDoubleClicked?.Invoke(this, EventArgs.Empty);
        };
    }

    public void Show() => _notify.Visible = true;
    public void Hide() => _notify.Visible = false;

    public void UpdatePresentation(Status status, string tooltip)
    {
        _notify.Icon = status == Status.Active ? _activeIcon : _pausedIcon;
        _notify.Text = tooltip;

        var monoBold    = new Font(MonoFontProvider.Get(9f), FontStyle.Bold);
        var monoRegular = MonoFontProvider.Get(9f);
        bool active = status == Status.Active;

        _activeItem.Text = active ? "[*] ACTIVE" : "[ ] ACTIVE";
        _pausedItem.Text = active ? "[ ] PAUSED" : "[*] PAUSED";
        _activeItem.Font = active ? monoBold    : monoRegular;
        _pausedItem.Font = active ? monoRegular : monoBold;
    }

    private static ToolStripMenuItem Item(string text, Action onClick)
    {
        var item = new ToolStripMenuItem(text)
        {
            BackColor = Palette.ToDrawing(ThemePalette.Bg),
            ForeColor = Palette.ToDrawing(ThemePalette.Green),
            Font      = MonoFontProvider.Get(9f),
        };
        item.Click += (_, _) => onClick();
        return item;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _notify.Visible = false;
        _notify.Dispose();
        _menu.Dispose();
        _activeIcon.Dispose();
        _pausedIcon.Dispose();
    }
}
