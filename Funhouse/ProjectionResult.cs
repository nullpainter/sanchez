using Funhouse.Models.Configuration;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Funhouse
{
    // TODO use me please
    public class ProjectionResult
    {
        public SatelliteDefinition Definition { get; }
        public Image<Rgba32> Image { get; }

        public ProjectionResult(SatelliteDefinition definition, Image<Rgba32> image)
        {
            Definition = definition;
            Image = image;
        }
    }
}