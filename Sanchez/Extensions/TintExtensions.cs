using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using Color = System.Drawing.Color;

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
            static float ToMatrixValue(byte value) => (float) value / 255;

            // Create a identity matrix with R-R', G-G' and B-B' scaled by the tint colour
            var tintMatrix = new ColorMatrix
            {
                M11 = ToMatrixValue(colour.R),
                M22 = ToMatrixValue(colour.G),
                M33 = ToMatrixValue(colour.B),
                M44 = 1
            };

            return tintMatrix;
        }
    }
}