using System;
using System.Collections.Generic;
using System.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Sanchez.Extensions.Images
{
    public static class AutoCropExtensions
    {
        public static Rectangle? GetAutoCropBounds(this Image<Rgba32> source)
        {
            var thresholds = new List<CropDetails>();

            for (var x = source.Width / 2; x < source.Width; x++)
            {
                var (cropWidth, cropHeight) = GetCrop(source, x);

                // Skip if for some reason we were unable to determine an edge pixel
                if (cropWidth == 0 || cropHeight == 0) continue;

                var xRatio = (cropWidth - source.Width) / (float) source.Width;
                var yRatio = (cropHeight - source.Height) / (float) source.Height;

                thresholds.Add(new CropDetails(xRatio / yRatio, cropWidth, cropHeight));
            }

            if (!thresholds.Any()) return null;

            var middleThreshold = thresholds.Select(v => v.Ratio).ToList().ClosestTo(1.0);
            var cropThreshold = thresholds.First(k => Math.Abs(k.Ratio - middleThreshold) < 0.00001);

            var croppedHeight = (cropThreshold.CropHeight - source.Height / 2) * 2;
            var croppedWidth = (cropThreshold.CropWidth - source.Width / 2) * 2;

            // Fail if we were unable to find a sensible crop region
            if (croppedWidth <= 0 || croppedHeight <= 0) return null;

            return new Rectangle(
                source.Width / 2 - croppedWidth / 2,
                source.Height / 2 - croppedHeight / 2,
                croppedWidth, croppedHeight);
        }

        private static Point GetCrop(Image<Rgba32> target, int x)
        {
            var cropHeight = 0;
            var cropWidth = 0;

            // Scan down, finding edge pixel. We are taking advantage of symmetry in only scanning half the image
            for (var y = target.Height / 2; y < target.Height; y++)
            {
                // Skip over pixels with full opacity
                if (target[x, y].A == 255) continue;

                // We've found the vertical edge pixel
                cropHeight = y - 1;

                // Scan across to find the horizontal edge pixel
                for (var x1 = x; x1 < target.Width; x1++)
                {
                    if (target[x1, cropHeight].A == 255) continue;
                    cropWidth = x1 - 1;
                    break;
                }

                break;
            }

            return new Point(cropWidth, cropHeight);
        }

        private readonly struct CropDetails
        {
            public double Ratio { get; }
            public int CropWidth { get; }
            public int CropHeight { get; }

            public CropDetails(float ratio, int cropWidth, int cropHeight)
            {
                Ratio = ratio;
                CropWidth = cropWidth;
                CropHeight = cropHeight;
            }
        }
    }
}