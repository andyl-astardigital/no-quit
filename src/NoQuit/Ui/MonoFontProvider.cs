using System.Drawing;
using System.Drawing.Text;

namespace NoQuit.Ui;

internal static class MonoFontProvider
{
    private static readonly string[] Candidates =
    {
        "Cascadia Mono",
        "Cascadia Code",
        "Consolas",
        "Courier New",
    };

    private static readonly Lazy<HashSet<string>> Installed = new(() =>
    {
        using var fonts = new InstalledFontCollection();
        return new HashSet<string>(fonts.Families.Select(f => f.Name), StringComparer.OrdinalIgnoreCase);
    });

    public static Font Get(float size)
    {
        foreach (var name in Candidates)
        {
            if (Installed.Value.Contains(name))
                return new Font(name, size, FontStyle.Regular);
        }
        return new Font(FontFamily.GenericMonospace, size);
    }
}
