using System.Text.RegularExpressions;
using TimeZoneConverter;

namespace Sanchez.Processing.Services.Filesystem.Parsers;

/// <summary>
///     Parses filenames of satellite imagery received from Electro-L no. 2 and Electro-L no. 3. 
/// </summary>
/// <remarks>
///     Electro timestamps are in Russian Standard Time, not UTC.
/// </remarks>
public class ElectroFilenameParser(string? prefix, string? suffix) : AbstractFilenameParser
{
    protected override Regex Regex { get; } = new(prefix + "([0-9]{6}_[0-9]{4})_" + suffix + "\\.[^ ]*", RegexOptions.Compiled);
    protected override string TimestampFormat => "yyMMdd_HHmm";
    protected override TimeZoneInfo TimeZone => TZConvert.GetTimeZoneInfo("Russian Standard Time");
}