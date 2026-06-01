using NoQuit.Core.Abstractions;

namespace NoQuit.Tests.Fakes;

public sealed class FakeFileSystem : IFileSystem
{
    public string TempPath { get; set; } = @"C:\fake\temp";
    public string GuidToken { get; set; } = "deadbeef";
    public Dictionary<string, string> Files { get; } = new();
    public List<string> Deleted { get; } = new();

    public void WriteAllText(string path, string content) => Files[path] = content;

    public void Delete(string path)
    {
        Deleted.Add(path);
        Files.Remove(path);
    }

    public string CombinePath(string left, string right) =>
        (left.EndsWith('\\') || left.EndsWith('/')) ? left + right : left + @"\" + right;

    public string NewGuidToken() => GuidToken;
}
