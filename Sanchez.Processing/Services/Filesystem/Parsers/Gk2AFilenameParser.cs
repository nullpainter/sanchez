using System.Text.RegularExpressions;

namespace Sanchez.Processing.Services.Filesystem.Parsers
{
    /// <summary>
    ///     Parses filenames of satellite imagery received from GK-2A and processed by <c>xrit-rc</c>.
    /// </summary>
    public class Gk2AFilenameParser : AbstractFilenameParser
    {
        protected override Regex Regex => new("(.*)_([0-9]{8}_[0-9]{6})(.*)\\..*", RegexOptions.Compiled);
        protected override string TimestampFormat => "yyyyMMdd_HHmmss";
    }
}