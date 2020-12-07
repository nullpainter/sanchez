using System.IO;
using Sanchez.Processing.Models;
using Sanchez.Processing.Models.Projections;

namespace Sanchez.Processing.Filesystem
{
    public class SingleFilenameProvider : IFilenameProvider
    {
        private readonly RenderOptions _options;

        public SingleFilenameProvider(RenderOptions options) => _options = options;

        public string GetOutputFilename(Activity activity, Registration registration)
        {
            var outputPath = _options.OutputPath;
            if (!_options.MultipleTargets) return outputPath;

            var prefix = GetPrefix(activity, registration);
            
            var outputFilename = $"{prefix}{Path.GetFileNameWithoutExtension(registration.Path)}{Constants.OutputFileSuffix}.jpg";
            return Path.Combine(outputPath, outputFilename);
        }

        /// <summary>
        ///     Returns a numeric prefix to apply the output filename, if <see cref="RenderOptions.AddSequenceNumber"/> is
        ///     set. 
        /// </summary>
        private string GetPrefix(Activity activity, Registration registration)
        {
            if (!_options.AddSequenceNumber) return "";

            var index = activity.Registrations.IndexOf(registration);
            
            // Pad the sequence number with zeros so column-based sorting can be applied.
            var length = $"{activity.Registrations.Count-1}".Length;
            return $"{index.ToString(new string('0', length))}_";
        }
    }
}