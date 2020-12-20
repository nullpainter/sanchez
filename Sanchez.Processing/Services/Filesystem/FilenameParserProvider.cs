using System.Collections.Generic;
using Sanchez.Processing.Services.Filesystem.Parsers;

namespace Sanchez.Processing.Services.Filesystem
{
    public class FilenameParserProvider
    {
        private readonly Dictionary<FilenameParserType, IFilenameParser> _parsers = new();
        
        public FilenameParserProvider()
        {
            _parsers.Add(FilenameParserType.Goesproc, new GoesFilenameParser());
            _parsers.Add(FilenameParserType.Xrit, new Gk2AFilenameParser());
            _parsers.Add(FilenameParserType.Electro, new ElectroFilenameParser());
        }

        public IFilenameParser? GetParser(FilenameParserType type) => _parsers.TryGetValue(type, out var parser) ? parser : null;
    }
}