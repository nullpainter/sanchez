using Sanchez.Processing.Helpers;
using Sanchez.Processing.Models.Configuration;
using SixLabors.ImageSharp.PixelFormats;

namespace Sanchez.Processing.Models
{
    public static class Constants
    {
        /// <summary>
        ///     Suffix applied to output filenames.
        /// </summary>
        public const string OutputFileSuffix = "-FC";

        public static string DefaultDefinitionsPath => PathHelper.ResourcePath("Satellites.json");
        public static string DefaultUnderlayPath => PathHelper.ResourcePath(@"world.200411.3x10848x5424.jpg");
        public static string DefaultGradientPath => PathHelper.ResourcePath(@"Gradients/DarkRed-Blue.json");

        internal static readonly Rgba32 Transparent = new Rgba32(0, 0, 0, 0);
        
        /// <summary>
        ///     The number Pi * 2.
        /// </summary>
        public const double Pi2 = 6.28318530717959;
        
        /// <summary>
        ///     The number pi/2.
        /// </summary>
        public const double PiOver2 = 1.5707963267949;

        /// <summary>
        ///     GOES-R satellite defaults. Non GOES-R satellite images are modified so these values
        ///     and associated calculations can be applied universally.
        /// </summary>
        public static class Satellite
        {
            public const double DefaultHeight = 35786023.0;

            /// <summary>
            ///     Supported spatial resolutions of output images.
            /// </summary>
            public static class SpatialResolution
            {
                public const int OneKm = 1;
                public const int TwoKm = 2;
                public const int FourKm = 4; 
            }

            public static class ImageSize
            {
                /// <summary>
                ///     Width and height of target images, representing 2km spatial resolution.
                /// </summary>
                public const int OneKm = 10848;
                
                /// <summary>
                ///     Width and height of target images, representing 2km spatial resolution.
                /// </summary>
                public const int TwoKm = 5424;

                /// <summary>
                ///     Width and height of target images, representing 4km spatial resolution.
                /// </summary>
                public const int FourKm = 2712;
            }

            internal static class CropRange
            {
                internal const double MinLatitude = 81.3282;
                internal const double MaxLatitude = -81.3282;
            }

            public static class Offset
            {
                /// <summary>
                ///     1km spatial resolution.
                /// </summary>
                public static readonly ImageOffset OneKm = new ImageOffset(-0.151858, 0.151858, 0.000028);
                
                /// <summary>
                ///     2km spatial resolution.
                /// </summary>
                public static readonly ImageOffset TwoKm = new ImageOffset(-0.151844, 0.151844, 0.000056);

                /// <summary>
                ///     4km spatial resolution.
                /// </summary>
                public static readonly ImageOffset FourKm = new ImageOffset(-0.151816, 0.151816, 0.000112);
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