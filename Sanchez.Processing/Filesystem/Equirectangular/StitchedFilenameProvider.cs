using System;
using System.IO;
using Ardalis.GuardClauses;
using Sanchez.Processing.Models;

namespace Sanchez.Processing.Filesystem.Equirectangular
{
    public class StitchedFilenameProvider : IFilenameProvider
    {
        private readonly RenderOptions _options;
        public StitchedFilenameProvider(RenderOptions options) => _options = options;

        public string GetOutputFilename(DateTime timestamp)
        {
            var outputPath = _options.OutputPath;
            var targetExtension = _options.GetTargetExtension();

            Guard.Against.Null(timestamp, nameof(timestamp));
            
            return Path.HasExtension(outputPath) 
                ? Path.ChangeExtension(outputPath, targetExtension) 
                : Path.Combine(outputPath, $"stitched-{timestamp:yyyyMMddTHHmmssZ}.{targetExtension}");
        }
    }
}