using Sanchez.Extensions;
using Sanchez.Models.Angles;
using Sanchez.Services.Filesystem;

namespace Sanchez.Models.Configuration
{
    public class SatelliteDefinition
    {
        /// <param name="filenamePrefix"></param>
        /// <param name="displayName"></param>
        /// <param name="filenameParserType"></param>
        /// <param name="longitude"></param>
        /// <param name="latitudeRange"></param>
        /// <param name="longitudeRange"></param>
        /// <param name="height">Satellite height above ellipsoid (metres)</param>
        /// <param name="crop"></param>
        /// <param name="brightness"></param>
        public SatelliteDefinition(
            string displayName, 
            string filenamePrefix, 
            FilenameParserType filenameParserType,
            double longitude,
            Range latitudeRange,
            Range longitudeRange,
            double height = Constants.Satellite.DefaultHeight,
            double[]? crop = null,
            float brightness = 1.0f)
        {
            FilenamePrefix = filenamePrefix;
            FilenameParserType = filenameParserType;
            DisplayName = displayName;
            LatitudeRange = latitudeRange;
            LongitudeRange = longitudeRange;
            Height = height;
            Crop = crop;
            Brightness = brightness;

            // Convert satellite longitude to lat/long scale of -180 to 180 degrees
            Longitude = longitude.NormaliseLongitude();
        }

        public string FilenamePrefix { get; }
        public FilenameParserType FilenameParserType { get; }
        public string DisplayName { get; }
        public Range LatitudeRange { get; }
        public Range LongitudeRange { get; }
        public double Longitude { get; }
        public double Height { get; }
        
        /// <summary>
        ///     Image crop ratio. This is expected to be a four element array of form {top ratio, right
        /// </summary>
        public double[]? Crop { get; }
        public float Brightness { get; }
    }
}