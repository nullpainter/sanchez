using System.Collections.Generic;
using System.IO;
using Sanchez.Models;

namespace Sanchez.Services
{
    internal static class CommandLineValidator
    {
        internal static bool VerifyPaths(CommandLineOptions options, out List<string> invalidPaths)
        {
            invalidPaths = new List<string>();
            
            var valid = true;
            var paths = new[] { options.MaskPath, options.UnderlayPath, options.SourceImagePath };

            foreach (var path in paths)
            {
                if (path == null || File.Exists(path)) continue;
                
                invalidPaths.Add(path);
                valid = false;
            }

            return valid;
        }
    }
}