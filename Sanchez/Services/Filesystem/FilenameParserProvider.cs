using System.Collections.Generic;
using Sanchez.Services.Filesystem.Parsers;

namespace Sanchez.Services.Filesystem
{
    public class FilenameParserProvider
    {
        private readonly Dictionary<FilenameParserType, IFilenameParser> _parsers = new Dictionary<FilenameParserType, IFilenameParser>();
        
        public FilenameParserProvider()
        {
            _parsers.Add(FilenameParserType.Goesproc, new GoesFilenameParser());
            _parsers.Add(FilenameParserType.Xrit, new Gk2AFilenameParser());
        }

        public IFilenameParser? GetParser(FilenameParserType type) => _parsers.TryGetValue(type, out var parser) ? parser : null;
    }
}