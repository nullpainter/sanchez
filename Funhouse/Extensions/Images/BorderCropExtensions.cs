using System;
using Serilog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Funhouse.Extensions.Images
{
    public static class BorderCropExtensions
    {
        public static Image<Rgba32> AutoCropBorder(this Image<Rgba32> image, double[] cropRatios)
        {
            if (cropRatios.Length != 4) throw new InvalidOperationException("Invalid number of crop ratios");

            image.Mutate(context => context.Crop(
                new Rectangle(
                    (int) Math.Round(cropRatios[3] * image.Width),
                    (int) Math.Round(cropRatios[0] * image.Height),
                    (int) Math.Round(image.Width - cropRatios[3] * image.Width - cropRatios[1] * image.Width),
                    (int) Math.Round(image.Height - cropRatios[0] * image.Height - cropRatios[2] * image.Height))
            ));
            
            Log.Information("Cropped image to {width} x {height} px", image.Width, image.Height);

            return image;
        }
    }
}