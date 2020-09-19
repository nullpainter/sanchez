using System;
using Serilog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Sanchez.Extensions.Images
{
    public static class BorderCropExtensions
    {
        /// <summary>
        ///     Crops the border of an image.
        /// </summary>
        /// <param name="image">image to crop</param>
        /// <param name="cropRatios">ratio to crop for each edge. This is expected to be a four element array, with crop values {top,right,bottom,left}</param>
        /// <exception cref="InvalidOperationException">if an incorrect number of crop ratios has been specified</exception>
        public static void CropBorder(this Image<Rgba32> image, double[] cropRatios)
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
        }
    }
}