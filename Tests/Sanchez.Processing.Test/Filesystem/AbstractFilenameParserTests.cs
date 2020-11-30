using Sanchez.Processing.Models.Angles;
using Sanchez.Processing.Models.Configuration;
using Sanchez.Test.Common;

namespace Sanchez.Processing.Test.Filesystem
{
    public abstract class AbstractFilenameParserTests : AbstractTests
    {
        protected static SatelliteDefinition NewDefinition(string? prefix = null, string? suffix = null) => new SatelliteDefinition("", prefix, suffix, false, 0, new Range(), new Range());
        
    }
}