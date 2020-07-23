using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using Color = System.Drawing.Color;

namespace Sanchez.Extensions
{
    public static class TintExtensions
    {
        public static IImageProcessingContext Tint(this IImageProcessingContext context, Color colour)
        {
            var tintMatrix = CreateTintMatrix(colour);
            return context.Filter(tintMatrix);
        }

        public static ColorMatrix CreateTintMatrix(Color colour)
        {
            static float ToMatrixValue(byte value) => (float) value / 255;
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