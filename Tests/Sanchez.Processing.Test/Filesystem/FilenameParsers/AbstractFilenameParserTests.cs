using Sanchez.Processing.Models.Angles;
using Sanchez.Processing.Models.Configuration;
using Sanchez.Processing.Services.Filesystem;
using Sanchez.Test.Common;

namespace Sanchez.Processing.Test.Filesystem.FilenameParsers
{
    public abstract class AbstractFilenameParserTests : AbstractTests
    {
        protected static SatelliteDefinition NewDefinition(FilenameParserType filenameParserType, string? prefix = null, string? suffix = null) 
            => new("", prefix, suffix, filenameParserType, false, 0, new Range(), new Range());
    }
}