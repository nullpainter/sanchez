using System.IO;
using Sanchez.Processing.Models;

namespace Sanchez.Processing.Filesystem.Equirectangular
{
    public class StitchedBatchFilenameProvider : IFilenameProvider
    {
        private readonly RenderOptions _options;
        public StitchedBatchFilenameProvider(RenderOptions options) => _options = options;

        public string GetOutputFilename(string? input = null)
        {
            var outputPath = _options.OutputPath;
            var timestamp = _options.Timestamp;

            return Path.Combine(outputPath, timestamp == null ? "stitched.jpg" : $"stitched-{timestamp:yyyyMMddTHHmmssZ}.jpg");
        }
    }
}