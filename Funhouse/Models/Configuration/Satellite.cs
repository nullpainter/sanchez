using Funhouse.Extensions;
using Funhouse.Models.Angles;

namespace Funhouse.Models.Configuration
{
    public class SatelliteDefinition
    {
        /// <param name="filePrefix"></param>
        /// <param name="displayName"></param>
        /// <param name="longitude"></param>
        /// <param name="latitudeRange"></param>
        /// <param name="longitudeRange"></param>
        /// <param name="height">Satellite height above ellipsoid (metres)</param>
        /// <param name="brightness"></param>
        public SatelliteDefinition(
            string filePrefix, string displayName, double longitude,
            Range latitudeRange,
            Range longitudeRange,
            double height = Constants.Satellite.DefaultHeight,
            float brightness = 1.0f)
        {
            FilePrefix = filePrefix;
            DisplayName = displayName;
            LatitudeRange = latitudeRange;
            LongitudeRange = longitudeRange;
            Height = height;
            Brightness = brightness;

            // Convert satellite longitude to lat/long scale of -180 to 180 degrees
            Longitude = longitude.NormaliseLongitude();
        }

        public string FilePrefix { get; }
        public string DisplayName { get; }
        public Range LatitudeRange { get; }
        public Range LongitudeRange { get; }
        public double Longitude { get; }
        public double Height { get; }
        public float Brightness { get; }
    }
}