using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Sanchez.Processing.Models;
using Sanchez.Processing.Models.Configuration;

namespace Sanchez.Processing.Services.Filesystem.Parsers
{
    public abstract class AbstractFilenameParser : IFilenameParser
    {
        protected abstract Regex Regex { get; }
        protected abstract string TimestampFormat { get; }
        
        public DateTime? GetTimestamp(string filename, SatelliteDefinition definition)
        {
            if (!Regex.IsMatch(filename) || filename.Contains("enhanced", StringComparison.CurrentCultureIgnoreCase)) return null;
            var match = Regex.Match(filename);

            if (match.Groups.Count != 4) throw new InvalidOperationException($"Invalid regular expression for {GetType().Name}; expected three components.");

            var prefix = match.Groups[1].Value;
            var filenameTimestamp = match.Groups[2].Value;
            var suffix = match.Groups[3].Value;

            // Match filename prefix, if provided
            if (definition.FilenamePrefix != null)
            {
                var prefixRegex = new Regex(definition.FilenamePrefix);
                if (!prefixRegex.IsMatch(prefix)) return null;
            }

            // Match filename suffix, if provided
            if (definition.FilenameSuffix != null)
            {
                var suffixRegex = new Regex(definition.FilenameSuffix);
                if (!suffixRegex.IsMatch(suffix)) return null;
            }
            
            // Ignore files created by Sanchez
            else if (suffix == Constants.OutputFileSuffix) return null;

            // parse timestamp
            return DateTime.TryParseExact(filenameTimestamp, TimestampFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var timestamp)
                ? (DateTime?) timestamp.ToUniversalTime()
                : null;
        }
    }
}