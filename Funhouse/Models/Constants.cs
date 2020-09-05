using Funhouse.Models.Configuration;
using SixLabors.ImageSharp.PixelFormats;

namespace Funhouse.Models
{
    public static class Constants
    {
        internal const string DefinitionsPath = @"Resources\Satellites.json";
        internal const string DefaultUnderlayPath = @"Resources\world.200411.3x10848x5424.jpg";

        internal static readonly Rgba32 Transparent = new Rgba32(0, 0, 0, 0);
        public const double Pi2 = 6.28318530717959;
        /// <summary>The number pi/2</summary>
        public const double PiOver2 = 1.5707963267949;
        

        internal static class Satellite
        {
            internal const double DefaultHeight = 35786023.0;

            /// <summary>
            ///     Supported spatial resolutions of output images.
            /// </summary>
            internal static class SpatialResolution
            {
                internal const int TwoKm = 2;
                internal const int FourKm = 4; 
            }

            internal static class ImageSize
            {
                /// <summary>
                ///     Width and height of target images, representing 2km spatial resolution.
                /// </summary>
                internal const int TwoKm = 5424;

                /// <summary>
                ///     Width and height of target images, representing 4km spatial resolution.
                /// </summary>
                internal const int FourKm = 2712;
            }

            internal static class VisibleRange
            {
                internal const double DefaultMinLatitude = -81.3282;
                internal const double DefaultMaxLatitude = 81.3282;
            }

            internal static class Offset
            {
                /// <summary>
                ///     2km spatial resolution.
                /// </summary>
                internal static readonly ImageOffset TwoKm = new ImageOffset(-0.151844, 0.151844, 0.000056);

                /// <summary>
                ///     4km spatial resolution.
                /// </summary>
                internal static readonly ImageOffset FourKm = new ImageOffset(-0.151816, 0.151816, 0.000112);
            }
        }

        internal static class Earth
        {
            /// <summary>
            ///     GRS80 semi-major axis of earth (metres)
            /// </summary>
            internal const double RadiusEquator = 6378137;

            /// <summary>
            ///     GRS80 semi-minor axis of earth (metres)
            /// </summary>
            internal const double RadiusPolar = 6356752.31414;

            internal const double Eccentricity = 0.0818191910435;
        }
    }
}