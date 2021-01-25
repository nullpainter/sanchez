using System.Text.RegularExpressions;

namespace Sanchez.Processing.Services.Filesystem.Parsers
{
    /// <summary>
    ///     Parses filenames of satellite imagery received from Electro-L no. 2.
    /// </summary>
    public class ElectroFilenameParser : AbstractFilenameParser
    {
        protected override Regex Regex { get; }
        protected override string TimestampFormat => "yyMMdd_HHmm";

        public ElectroFilenameParser(string? prefix, string? suffix) 
        {
            Regex = new Regex(prefix + "([0-9]{6}_[0-9]{4})_" + suffix + "\\.[^ ]*", RegexOptions.Compiled);
        }
    }
}