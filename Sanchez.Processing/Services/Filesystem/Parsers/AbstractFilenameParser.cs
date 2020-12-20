using System;
using System.Globalization;
using System.Linq;
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
            var matches = Regex.Matches(filename);

            if (!matches.Any() || filename.Contains("enhanced", StringComparison.CurrentCultureIgnoreCase)) return null;
            var match = matches[0];

            if (match.Groups.Count != 4) throw new InvalidOperationException($"Invalid regular expression for {GetType().Name}; expected three components.");

            var prefix = match.Groups[1].Value;
            var filenameTimestamp = match.Groups[2].Value;
            var suffix = match.Groups[3].Value;

            if (definition.PrefixRegex != null && !definition.PrefixRegex.IsMatch(prefix)) return null;
            if (definition.SuffixRegex != null && !definition.SuffixRegex.IsMatch(suffix)) return null;
            
            if (suffix == Constants.OutputFileSuffix) return null;

            // parse timestamp
            return DateTime.TryParseExact(filenameTimestamp, TimestampFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var timestamp)
                ? (DateTime?) timestamp.ToUniversalTime()
                : null;
        }
    }
}