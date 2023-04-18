using System.Diagnostics;
using System.Reflection;
using Extend;

namespace Sanchez.Processing.Helpers;

public static class PathHelper
{
    /// <summary>
    ///     Returns the path to where the cached underlay image and database is stored.
    /// </summary>
    public static string CachePath() => Path.Combine(GetApplicationPath(), "cache");

    /// <summary>
    ///     Returns the path to where the log files are written.
    /// </summary>
    public static string LogPath() => Path.Combine(GetApplicationPath(), "logs");
        
    /// <summary>
    ///     Returns the path to resources.
    /// </summary>
    private static string ResourcePath() => Path.Combine(GetApplicationPath(), "Resources");

    public static string ResourcePath(string resource) => Path.Combine(ResourcePath(), resource);

    /// <summary>
    ///     Returns the absolute path to the running application.
    /// </summary>
    private static string GetApplicationPath()
    {
        var processFilename = Environment.ProcessPath;

        // Determine correct location of directories relative to application, depending on whether we are running
        // from a published executable or via dotnet.
        var applicationPath = Path.GetFileNameWithoutExtension(processFilename).CompareOrdinalIgnoreCase("sanchez")
            ? Path.GetDirectoryName(processFilename)
            : Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        return applicationPath!;
    }
}