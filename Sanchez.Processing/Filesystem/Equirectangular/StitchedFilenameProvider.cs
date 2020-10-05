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

            Guard.Against.Null(timestamp, nameof(timestamp));
            return Path.HasExtension(outputPath) ? outputPath : Path.Combine(outputPath, $"stitched-{timestamp:yyyyMMddTHHmmssZ}.jpg");
        }
    }
}