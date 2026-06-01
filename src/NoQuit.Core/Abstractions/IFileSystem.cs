namespace NoQuit.Core.Abstractions;

public interface IFileSystem
{
    string TempPath { get; }
    void WriteAllText(string path, string content);
    void Delete(string path);
    string CombinePath(string left, string right);
    string NewGuidToken();
}
