using System.Text;
using NoQuit.Core.Abstractions;

namespace NoQuit.Adapters;

public sealed class RealFileSystem : IFileSystem
{
    public string TempPath => Path.GetTempPath();

    public void WriteAllText(string path, string content)
    {
        // Task Scheduler XML must be UTF-16; the XML declaration says so.
        File.WriteAllText(path, content, Encoding.Unicode);
    }

    public void Delete(string path)
    {
        if (File.Exists(path)) File.Delete(path);
    }

    public string CombinePath(string left, string right) => Path.Combine(left, right);

    public string NewGuidToken() => Guid.NewGuid().ToString("N");
}
