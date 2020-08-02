using System;
using System.Collections.Generic;
using System.IO;
using Ardalis.GuardClauses;
using Sanchez.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;

namespace Sanchez.Services
{
    internal interface IOptionValidator
    {
        /// <summary>
        ///     Performs validation on parsed command line and render options.
        /// </summary>
        /// <returns>if validation succeeded</returns>
        bool Validate(CommandLineOptions options);

        /// <summary>
        ///     Verifies that paths provided by the user for single input files correspond
        ///     to files which exist on disk.
        /// </summary>
        /// <param name="options">populated command-line options</param>
        /// <param name="invalidPaths">populated with invalid paths</param>
        /// <returns>if all paths are valid</returns>
        bool ValidatePaths(CommandLineOptions options, out List<string> invalidPaths);
    }

    internal class OptionValidator : IOptionValidator
    {
        /// <summary>
        ///     Performs validation on parsed command line and render options.
        /// </summary>
        /// <returns>if validation succeeded</returns>
        public bool Validate(CommandLineOptions options)
        {
            var valid = true;

            // Verify arguments
            if (!ValidatePaths(options, out var invalidPaths))
            {
                invalidPaths.ForEach(path => Console.Error.WriteLine($"Path {path} isn't valid"));
                valid = false;
            }

            // Verify output format
            if (!ValidateOutputFormat(options))
            {
                Console.Error.WriteLine("Unsupported output file format");
                valid = false;
            }

            valid &= ValidateSource(options);
            valid &= ValidateThreads(options);

            return valid;
        }

        private bool ValidateThreads(CommandLineOptions options)
        {
            if (options.Threads <= 0)
            {
                Console.Error.WriteLine("Thread count needs to be greater than zero");
                return false;
            }

            return true;
        }

        private static bool ValidateOutputFormat(CommandLineOptions options)
        {
            if (options.IsBatch) return true;
            
            var outputExtension = Path.GetExtension(options.OutputPath);
            var imageFormatManager = Configuration.Default.ImageFormatsManager;
            
            if (string.IsNullOrWhiteSpace(outputExtension)) return false;
            var outputFormat = imageFormatManager.FindFormatByFileExtension(outputExtension);
            return outputFormat != null;
        }

        private static bool ValidateSource(CommandLineOptions options)
        {
            Guard.Against.Null(options.SourcePath, nameof(options.SourcePath));
            var sourcePath = Path.GetFullPath(options.SourcePath);

            // Don't validate directories as we may be dealing with a glob
            if (options.IsBatch) return true;

            if (File.Exists(sourcePath)) return true;

            Console.Error.WriteLine($"Path {sourcePath} isn't valid");
            return false;
        }

        /// <summary>
        ///     Verifies that all paths provided by the user for input files correspond to files
        ///     which exist on disk. Note that the source path is separately validated in
        ///     <see cref="ValidateSource"/> due to the need to deal with glob patterns.
        /// </summary>
        /// <param name="options">populated command-line options</param>
        /// <param name="invalidPaths">populated with invalid paths</param>
        /// <returns>if all paths are valid</returns>
        public bool ValidatePaths(CommandLineOptions options, out List<string> invalidPaths)
        {
            invalidPaths = new List<string>();

            var valid = true;
            var paths = new[] { options.MaskPath, options.UnderlayPath, options.OverlayPath };

            foreach (var path in paths)
            {
                // Null path isn't a failure as this corresponds to an optional argument. Mandatory arguments
                // have already been enforced by the command-line parser library.
                if (path == null || File.Exists(path)) continue;

                invalidPaths.Add(path);
                valid = false;
            }

            return valid;
        }
    }
}