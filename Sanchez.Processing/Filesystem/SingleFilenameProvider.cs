using System.IO;
using Ardalis.GuardClauses;
using Sanchez.Processing.Models;

namespace Sanchez.Processing.Filesystem
{
    public class SingleFilenameProvider : IFilenameProvider
    {
        private readonly RenderOptions _options;

        public SingleFilenameProvider(RenderOptions options) => _options = options;

        public string GetOutputFilename(string? input = null)
        {
            Guard.Against.Null(input, nameof(input));
            
            var outputPath = _options.OutputPath;
            if (!_options.MultipleTargets) return outputPath;
            
            var outputFilename = $"{Path.GetFileNameWithoutExtension(input)}{Constants.OutputFileSuffix}.jpg";
            return Path.Combine(outputPath, outputFilename);
        }
    }
}