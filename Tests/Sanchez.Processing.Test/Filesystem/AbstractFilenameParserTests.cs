using Sanchez.Processing.Models.Angles;
using Sanchez.Processing.Models.Configuration;
using Sanchez.Processing.Services.Filesystem;
using Sanchez.Test.Common;

namespace Sanchez.Processing.Test.Filesystem
{
    public abstract class AbstractFilenameParserTests : AbstractTests
    {
        protected static SatelliteDefinition NewDefinition(string? prefix = null, string? suffix = null) => new SatelliteDefinition("", prefix, suffix, FilenameParserType.Goesproc, false, 0, new Range(), new Range());
        
    }
}