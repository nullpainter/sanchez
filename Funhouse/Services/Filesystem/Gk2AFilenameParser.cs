using System;

namespace Funhouse.Services.Filesystem
{
    /// <summary>
    ///     Parses filenames of satellite imagery received from GK-2A satellite and processed by <c>xrit-rc</c>.
    /// </summary>
    public class Gk2AFilenameParser : IFilenameParser
    {
        public DateTime? GetTimestamp(string filename)
        {
            throw new NotImplementedException();
        }
    }
}