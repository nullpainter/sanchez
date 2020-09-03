using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Funhouse.Helpers
{
    public static class PathHelper
    {
        /// <summary>
        ///     Returns the path to where the cached underlay image and database is stored.
        /// </summary>
        /// <returns></returns>
        public static string CachePath() => Path.Combine(GetApplicationPath(), "cache");

        /// <summary>
        ///     Returns the path to the where the log files are written.
        /// </summary>
        /// <returns></returns>
        public static string LogPath() => Path.Combine(GetApplicationPath(), "logs");


        /// <summary>
        ///     Returns the absolute path to the running application.
        /// </summary>
        private static string GetApplicationPath()
        {
            var processFilename = Process.GetCurrentProcess().MainModule.FileName;

            // Determine correct location of directories relative to application, depending on whether we are running
            // from a published executable or via dotnet.
            var applicationPath = (Path.GetFileNameWithoutExtension(processFilename) == "dotnet"
                ? Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
                : Path.GetDirectoryName(processFilename))!;

            return applicationPath;
        }
    }
}