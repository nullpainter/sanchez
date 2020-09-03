using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.ColorSpaces.Conversion;
using SixLabors.ImageSharp.PixelFormats;

namespace Funhouse.ImageProcessing.Tint
{
    public static class TintExtensions
    {
        private static readonly ColorSpaceConverter Converter = new ColorSpaceConverter(new ColorSpaceConverterOptions());

        /// <summary>
        ///     Applies a tint to an image.
        /// </summary>
        public static void Tint(this Image<Rgba32> image, Color colour)
        {
            var tint = colour.ToPixel<Rgba32>();
            var lightness = 2 * (1 - (Converter.ToHsl(tint).L * 2 - 1));

            var operation = new TintRowOperation(image, tint, lightness);
            ParallelRowIterator.IterateRows(Configuration.Default, image.Bounds(), in operation);
        }
    }
}