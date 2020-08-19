using Funhouse.Extensions;
using Funhouse.Models.Angles;
using MathNet.Spatial.Units;

namespace Funhouse.Models.Configuration
{
    public class SatelliteDefinition
    {
        public string FilePrefix { get; }
        public string DisplayName { get; }
        public Angle Longitude { get; }
        public Range VisibleRange { get;  }
        public double Height { get; }
        public ImageOffset ImageOffset { get; }

        /// <param name="filePrefix"></param>
        /// <param name="displayName"></param>
        /// <param name="longitude"></param>
        /// <param name="visibleRange"></param>
        /// <param name="imageOffset"></param>
        /// <param name="height">Satellite height above ellipsoid (metres)</param>
        public SatelliteDefinition(string filePrefix, string displayName, Angle longitude, Range visibleRange, ImageOffset imageOffset, double height = 35786023)
        {
            FilePrefix = filePrefix;
            DisplayName = displayName;
            VisibleRange = visibleRange;
            ImageOffset = imageOffset;
            Height = height;

            // Convert satellite longitude to Mercator scale of -180 to 180 degrees
            Longitude = longitude.NormaliseLongitude();
        }
    }
}