using Sanchez.Processing.Extensions;
using Sanchez.Processing.Models.Angles;

namespace Sanchez.Processing.Models.Configuration
{
    public class SatelliteDefinition
    {
        /// <param name="displayName"></param>
        /// <param name="filenamePrefix"></param>
        /// <param name="filenameSuffix"></param>
        /// <param name="invert"></param>
        /// <param name="longitude"></param>
        /// <param name="latitudeRange"></param>
        /// <param name="longitudeRange"></param>
        /// <param name="height">Satellite height above ellipsoid (metres)</param>
        /// <param name="crop"></param>
        /// <param name="brightness"></param>
        public SatelliteDefinition(
            string displayName,
            string? filenamePrefix,
            string? filenameSuffix,
            bool invert,
            double longitude,
            Range latitudeRange,
            Range longitudeRange,
            double height = Constants.Satellite.DefaultHeight,
            double[]? crop = null,
            float brightness = 1.0f)
        {
            FilenamePrefix = filenamePrefix;
            FilenameSuffix = filenameSuffix;
            Invert = invert;
            DisplayName = displayName;
            LatitudeRange = latitudeRange;
            LongitudeRange = longitudeRange;
            Height = height;
            Crop = crop;
            Brightness = brightness;

            // Convert satellite longitude to lat/long scale of -180 to 180 degrees
            Longitude = longitude.NormaliseLongitude();
        }

        public string? FilenamePrefix { get; }
        public string? FilenameSuffix { get; }

        /// <summary>
        ///     Whether pixel intensities in IR images should be inverted to match GOES-R.
        /// </summary>
        public bool Invert { get; set; }

        public string DisplayName { get; }
        public Range LatitudeRange { get; }
        public Range LongitudeRange { get; }
        public double Longitude { get; }
        public double Height { get; }

        /// <summary>
        ///     Image crop ratio. This is expected to be a four element array of {top, right, bottom, left}.
        /// </summary>
        public double[]? Crop { get; }

        public float Brightness { get; }
    }
}