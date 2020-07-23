using System.Collections.Generic;
using System.IO;
using Sanchez.Models;

namespace Sanchez.Services
{
    internal static class CommandLineValidator
    {
        /// <summary>
        ///     Verifies that all paths provided by the user for input files correspond to files
        ///     which exist on disk.
        /// </summary>
        /// <param name="options">populated command-line options</param>
        /// <param name="invalidPaths">populated with invalid paths</param>
        /// <returns>whether all paths are valid</returns>
        internal static bool VerifyPaths(CommandLineOptions options, out List<string> invalidPaths)
        {
            invalidPaths = new List<string>();
            
            var valid = true;
            var paths = new[] { options.MaskPath, options.UnderlayPath, options.SourceImagePath };

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