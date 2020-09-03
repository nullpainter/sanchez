using System;
using Funhouse.Models;
using Funhouse.Models.Configuration;
using SixLabors.ImageSharp.PixelFormats;

namespace Funhouse.Extensions.Images
{
    public static class InterpolationExtensions
    {
        internal static Rgba32 GetInterpolatedPixel(this ImageBuffer image, double targetX, double targetY, InterpolationType interpolationType)
        {
            // Interpolate fractional source pixel to target image
            var targetPixel = interpolationType switch
            {
                InterpolationType.NearestNeighbour => image.NearestNeighbour(targetX, targetY),
                InterpolationType.Bilinear => image.Bilinear(targetX, targetY),
                _ => throw new ArgumentOutOfRangeException($"Unhandled interpolation type: {interpolationType}")
            };

            return targetPixel;
        }

        private static Rgba32 NearestNeighbour(this ImageBuffer image, double targetX, double targetY)
        {
            var roundedX = (int) Math.Round(targetX);
            var roundedY = (int) Math.Round(targetY);

            var index = roundedY * image.Size.Width + roundedX;
            if (index < 0 || index >= image.Buffer.Length) return Constants.Transparent;
            
            return image.Buffer[index];
        }

        private static Rgba32 Bilinear(this ImageBuffer image, double targetX, double targetY)
        {
            var xInt = (int) targetX;
            var yInt = (int) targetY;
             
            // Lazy approach for image edge
            if (xInt + 1 == image.Size.Width || yInt + 1 == image.Size.Height) return NearestNeighbour(image, xInt, yInt);

            var width = image.Size.Width;
            var baseIndex = yInt * width + xInt;
            
            var c00 = image.Buffer[baseIndex];
            var c01 = image.Buffer[baseIndex + width];
            var c10 = image.Buffer[baseIndex + 1];
            var c11 = image.Buffer[baseIndex + 1 + width];
            
            var amountX = targetX - xInt;
            var amountY = targetY - yInt;
            
            // Even though the IR images are greyscale, calculate RGB separately in case a colour image is passed
            var red = Interpolation.Blerp(c00.R, c10.R, c01.R, c11.R, amountX, amountY);
            var green = Interpolation.Blerp(c00.G, c10.G, c01.G, c11.G, amountX, amountY);
            var blue = Interpolation.Blerp(c00.B, c10.B, c01.B, c11.B, amountX, amountY);
            var alpha = Interpolation.Blerp(c00.A, c10.A, c01.A, c11.A, amountX, amountY);
            
            return new Rgba32((byte) red, (byte) green, (byte) blue, (byte) alpha);
        }
    }
}