using System.Collections.Generic;
using System.IO;
using System.Linq;
using DotNet.Globbing;
using Funhouse.Helpers;
using Funhouse.Models;

namespace Funhouse.Services
{
    public interface IFileService
    {
        /// <summary>
        ///     Creates the target directory if required.
        /// </summary>
        void PrepareOutput();

        /// <summary>
        ///     Returns the output filename, based on whether we are processing a single or multiple source files. For multiple source
        ///     files, the output filename is the source filename with a <see cref="FileService.BatchFileSuffix"/> suffix.
        /// </summary>
        string GetOutputFilename(string sourceFile);

        /// <summary>
        ///     Returns a list of files to process, based on <see cref="RenderOptions.SourcePath"/>. This property
        ///     can be a single file, a directory or a glob and wildcard pattern (such as <c>source/**/*IR.jpg</c>)
        /// </summary>
        List<string> GetSourceFiles();
    }

    internal class FileService : IFileService
    {
        private readonly RenderOptions _options;

        public FileService(RenderOptions options) => _options = options;

        /// <summary>
        ///     Suffix applied to filenames when converting files in bulk.
        /// </summary>
        private const string BatchFileSuffix = "-FC";

        /// <summary>
        ///     Creates the target directory if required.
        /// </summary>
        public void PrepareOutput()
        {
            if (!_options.MultipleTargets || Directory.Exists(_options.OutputPath!)) return;
            
            try
            {
                Directory.CreateDirectory(_options.OutputPath!);
            }
            catch (IOException)
            {
                ConsoleLog.Error( "Unable to create target directory");
                throw;
            }
        }

        /// <summary>
        ///     Returns the output filename, based on whether we are processing a batch. For batches, the output
        ///     filename is the source filename with a <see cref="BatchFileSuffix"/> suffix.
        /// </summary>
        public string GetOutputFilename(string sourceFile)
        {
            return _options.MultipleTargets
                ? Path.Combine(_options.OutputPath!, $"{Path.GetFileNameWithoutExtension(sourceFile)}{BatchFileSuffix}{Path.GetExtension(sourceFile)}"!)
                : _options.OutputPath!;
        }

        /// <summary>
        ///     Returns a list of files to process, based on <see cref="RenderOptions.SourcePath"/>. This property
        ///     can be a single file, a directory or a glob and wildcard pattern (such as <c>source/**/*IR.jpg</c>)
        /// </summary>
        public List<string> GetSourceFiles()
        {
            var absolutePath = Path.GetFullPath(_options.SourcePath!);

            // Source is a single file
            if (!_options.MultipleSources) return new List<string> { absolutePath };

            // If the source is a directory, enumerate all files
            if (Directory.Exists(absolutePath))
            {
                return Directory
                    .GetFiles(absolutePath, "*.*", SearchOption.AllDirectories)
                    .OrderBy(file => file)
                    .ToList();
            }

            // Source is a glob, so enumerate all files in its base directory directory and return
            // glob matches
            var sourceGlob = Glob.Parse(absolutePath);

            return Directory
                .GetFiles(GetGlobBase(absolutePath), "*.*", SearchOption.AllDirectories)
                .Where(file => sourceGlob.IsMatch(file))
                .OrderBy(file => file)
                .ToList();
        }

        private static string GetGlobBase(string path)
        {
            // Normalise separators
            path = path.Replace('\\', '/');

            // Extract all directories in the path prior to the glob pattern. Note that the glob library
            // also supports [a-z] style ranges, however we don't.
            var directorySegments = path
                .Split('/')
                .TakeWhile(segment => !segment.Contains('?') && !segment.Contains('*'))
                .ToList();

            // Recombine path
            return string.Join(Path.DirectorySeparatorChar, directorySegments);
        }
    }
}