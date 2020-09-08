using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Funhouse.Services.Filesystem
{
    /// <summary>
    ///     Parses filenames of satellite imagery received from GOES-R satellites and processed by <c>goesproc</c>.
    /// </summary>
    public class GoesFilenameParser : IFilenameParser
    {
        private static Regex _regex = new Regex(".*_([0-9]{8}T[0-9]{6}Z).jpg", RegexOptions.Compiled);
        private const string TimestampFormat = "yyyyMMddTHHmmssZ";

        public DateTime? GetTimestamp(string filename)
        {
            if (!_regex.IsMatch(filename)) return null;

            // Extract timestamp from filename
            var match = _regex.Match(filename);
            var filenameTimestamp = match.Groups[1].Value;

            // parse timestamp
            return DateTime.TryParseExact(filenameTimestamp, TimestampFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var timestamp)
                ? (DateTime?) timestamp.ToUniversalTime()
                : null;
        }
    }
}