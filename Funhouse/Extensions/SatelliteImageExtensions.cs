using System;
using System.IO;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Funhouse.Extensions.Images;
using Funhouse.ImageProcessing.Mask;
using Funhouse.Models.CommandLine;
using Funhouse.Models.Projections;
using Serilog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace Funhouse.Extensions
{
    public static class SatelliteImageExtensions
    {
        /// <summary>
        ///     Mask all pixels outside the Earth to assist image stitching of projected images.
        /// </summary>
        public static SatelliteImage RemoveBackground(this SatelliteImage activity)
        {
            Guard.Against.Null(activity.Image, nameof(activity.Image));
            activity.Image.RemoveBackground();

            return activity;
        }
        
        public static SatelliteImage CropBorders(this SatelliteImage activity)
        {
            Guard.Against.Null(activity.Image, nameof(activity.Image));
            Guard.Against.Null(activity.Definition, nameof(activity.Definition));
            
            // Perform crop
            if (activity.Definition.Crop != null) activity.Image.AutoCropBorder(activity.Definition.Crop);
            return activity;
        }
        
        public static SatelliteImage NormaliseHistogram(this SatelliteImage activity)
        {
            Guard.Against.Null(activity.Image, nameof(activity.Image));
            Guard.Against.Null(activity.Definition, nameof(activity.Definition));

            // Normalise brightness and contrast
            activity.Image.Mutate(c => c.HistogramEqualization());
            activity.Image.Mutate(c => c.Brightness(activity.Definition.Brightness));
            activity.Image.Mutate(c => c.Brightness(1.1f));

            return activity;
        }

        /// <summary>
        ///     Normalises the source image size to match the specified output spatial resolution. Doing
        ///     so simplifies maths when blending multiple pages.
        /// </summary>
        public static SatelliteImage NormaliseSize(this SatelliteImage image, int imageSize)
        {
            Guard.Against.Null(image.Image, nameof(image.Image));

            if (image.Image.Width != imageSize || image.Image.Height != imageSize)
            {
                // TODO test results of different interpolation types
                image.Image.Mutate(c => c.Resize(imageSize, imageSize, KnownResamplers.Welch));
            }

            return image;
        }

        public static async Task SaveWithExifAsync(this SatelliteImage image, string suffix, CommandLineOptions options)
        {
            var filename = $"{Path.GetFileNameWithoutExtension(image.Path)}{suffix}.jpg";
            var outputPath = Path.Combine(Path.GetDirectoryName(image.Path)!, filename);
            await image.Image.SaveWithExifAsync(outputPath);

            if (options.Verbose)
            {
                Log.Information("Output written to {path}", Path.GetFullPath(outputPath));
            }
            else
            {
                Console.WriteLine($"Output written to {Path.GetFullPath(outputPath)}");
            } 
        }
    }
}