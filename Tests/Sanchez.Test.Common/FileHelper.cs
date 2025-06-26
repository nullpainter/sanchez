namespace Sanchez.Test.Common;

/// <summary>
///     Helper methods for file manipulation and cleanup in unit tests
/// </summary>
public class FileState : IDisposable
{
    private readonly List<string> _tempDirectories = [];

    public string CreateFile(string directory, string filename)
    {
        var filePath = Path.Combine(directory, filename);

        using var stream = File.Create(filePath);
        return stream.Name;
    }
        
    public string CreateFile(string filename)
    {
        var tempDirectory = CreateTempDirectory();
        var filePath = Path.Combine(tempDirectory, filename);

        using var stream = File.Create(filePath);
        return stream.Name;
    }

    public string CreateTempDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(path);

        _tempDirectories.Add(path);

        return path;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposing) return;

        foreach (var path in _tempDirectories.Where(Directory.Exists))
        {
            Directory.Delete(path, true);
        }
    }
}