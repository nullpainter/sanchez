using Funhouse.Models.Angles;
using Funhouse.Models.Configuration;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Funhouse.Models.Projections
{
    public class ProjectionActivity
    {
        public ProjectionActivity(string path)
        {
            Path = path;
        }

        public SatelliteDefinition? Definition { get; set; }
                public Image<Rgba32>? Source { get; set; }
        public Image<Rgba32>? Output { get; set; }

        public string Path { get; }
        public PointF Offset { get; set; }
        public Range LongitudeRange { get; set; }
    }
}