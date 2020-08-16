using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Funhouse.Extensions
{
    internal static class ImageExtensions
    {
        internal static Rgba32 NearestNeighbour(this Image<Rgba32> source, PointF target)
        {
            var targetX = (int) Math.Round(target.X);
            var targetY = (int) Math.Round(target.Y);

            return source[targetX, targetY];
        }

        internal static Rgba32 Bilinear(this Image<Rgba32> source, PointF target)
        {
            var xInt = (int) target.X;
            var yInt = (int) target.Y;

            // Lazy approach for image edge
            if (xInt + 1 == source.Width || yInt + 1 == source.Height) return source[xInt, yInt];

            var c00 = source[xInt, yInt];
            var c01 = source[xInt, yInt + 1];
            var c10 = source[xInt + 1, yInt];
            var c11 = source[xInt + 1, yInt + 1];

            var amount = new PointF(target.X - xInt, target.Y - yInt);

            // Even though the IR images are greyscale, calculate RGB separately in case a colour image is passed
            var red = Interpolation.Blerp(c00.R, c10.R, c01.R, c11.R, amount);
            var green = Interpolation.Blerp(c00.G, c10.G, c01.G, c11.G, amount);
            var blue = Interpolation.Blerp(c00.B, c10.B, c01.B, c11.B, amount);
            var alpha = Interpolation.Blerp(c00.A, c10.A, c01.A, c11.A, amount);

            return new Rgba32(red / 255, green / 255, blue / 255, alpha / 255);
        }

    }
}