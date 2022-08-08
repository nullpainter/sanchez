using System.Text.RegularExpressions;

namespace Sanchez.Processing.Services.Filesystem.Parsers;

/// <summary>
///     Parses filenames of satellite imagery received from GOES-R satellites and processed by <c>goesproc</c>.
/// </summary>
public class GoesFilenameParser : AbstractFilenameParser
{
    protected override Regex Regex { get; }

    protected override string TimestampFormat => "yyyyMMddTHHmmssZ";

    public GoesFilenameParser(string? prefix, string? suffix)
    {
        Regex = new Regex(prefix + "([0-9]{8}T[0-9]{6}Z)" + suffix + "\\.[^ ]*", RegexOptions.Compiled);
    }
}