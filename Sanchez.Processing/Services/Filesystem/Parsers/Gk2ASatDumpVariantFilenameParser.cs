using System.Text.RegularExpressions;

namespace Sanchez.Processing.Services.Filesystem.Parsers;

/// <summary>
///     Parses filenames of satellite imagery received from GK-2A and processed by <c>xrit-rc</c>.
/// </summary>
public class Gk2ASatDumpVariantFilenameParser : AbstractFilenameParser
{
    protected override Regex Regex { get; }

    protected override string TimestampFormat => "yyyyMMddTHHmmssZ";

    public Gk2ASatDumpVariantFilenameParser(string? prefix, string? suffix)
    {
        Regex = new Regex(prefix + "([0-9]{8}T[0-9]{6}Z)" + suffix + "\\.[^ ]*", RegexOptions.Compiled);
    }
}