using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Sanchez.Extensions
{
    public static class TintExtensions
    {
        /// <summary>
        ///     Applies a tint to an image.
        /// </summary>
        public static IImageProcessingContext Tint(this IImageProcessingContext context, Color colour)
        {
            var tintMatrix = CreateTintMatrix(colour);
            return context.Filter(tintMatrix);
        }

        /// <summary>
        ///     Creates a colour matrix used to apply a tint to an image.
        /// </summary>
        /// <param name="colour">colour to tint by</param>
        public static ColorMatrix CreateTintMatrix(Color colour)
        {
            var colourVector = colour.ToPixel<Rgba32>().ToScaledVector4();

            // Create a identity matrix with R-R', G-G' and B-B' scaled by the tint colour
            var tintMatrix = new ColorMatrix
            {
                M11 = colourVector.X,
                M22 = colourVector.Y,
                M33 = colourVector.Z,
                M44 = colourVector.W
            };

            return tintMatrix;
        }
    }
}