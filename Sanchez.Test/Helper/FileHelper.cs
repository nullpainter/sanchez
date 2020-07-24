using System;
using System.Collections.Generic;
using System.IO;

namespace Sanchez.Test.Helper
{
    /// <summary>
    ///     Helper methods for file manipulation and cleanup in unit tests
    /// </summary>
    public static class FileHelper
    {
        public static FileState NewState()
        {
            return new FileState();
        }
    }

    public class FileState : IDisposable
    {
        private readonly List<string> _tempDirectories = new List<string>();

        internal FileState()
        {
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
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

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;

            foreach (var path in _tempDirectories)
                if (Directory.Exists(path))
                    Directory.Delete(path, true);
        }
    }
}