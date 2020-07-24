using System;
using System.Collections.Generic;
using System.IO;
using Sanchez.Models;

namespace Sanchez.Services
{
    internal static class CommandLineValidator
    {
        /// <summary>
        ///     Performs validation on parsed command line and render options.
        /// </summary>
        /// <returns>if validation succeeded</returns>
        internal static bool Validate(CommandLineOptions options, RenderOptions renderOptions)
        {
            var valid = true;

            // Verify arguments
            if (!ValidatePaths(options, out var invalidPaths))
            {
                invalidPaths.ForEach(path => Console.Error.WriteLine($"Path {path} isn't valid."));
                valid = false;
            }

            // Verify output format
            if (renderOptions.OutputFormat == null)
            {
                var outputExtension = Path.GetExtension(options.OutputFile);
                Console.Error.WriteLine($"Invalid file extension {outputExtension} - only .jpg, .jpeg and .png are supported");
                valid = false;
            }

            return valid;
        }
        /// <summary>
        ///     Verifies that all paths provided by the user for input files correspond to files
        ///     which exist on disk.
        /// </summary>
        /// <param name="options">populated command-line options</param>
        /// <param name="invalidPaths">populated with invalid paths</param>
        /// <returns>if all paths are valid</returns>
        internal static bool ValidatePaths(CommandLineOptions options, out List<string> invalidPaths)
        {
            invalidPaths = new List<string>();

            var valid = true;
            var paths = new[] { options.MaskPath, options.UnderlayPath, options.SourceImagePath, options.OverlayPath };

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