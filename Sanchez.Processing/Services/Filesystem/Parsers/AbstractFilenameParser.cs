using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Sanchez.Processing.Services.Filesystem.Parsers
{
    public abstract class AbstractFilenameParser : IFilenameParser
    {
        protected abstract Regex Regex { get; }
        protected abstract string TimestampFormat { get; }

        public DateTime? GetTimestamp(string filename)
        {
            var matches = Regex.Matches(filename);

            if (!matches.Any()) return null;
            var match = matches[0];
            if (match.Groups.Count != 2) throw new InvalidOperationException($"Invalid regular expression for {GetType().Name}; expected one component.");

            var filenameTimestamp = match.Groups[1].Value;

            // parse timestamp
            return DateTime.TryParseExact(filenameTimestamp, TimestampFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var timestamp)
                ? (DateTime?) timestamp.ToUniversalTime()
                : null;
        }
    }
}