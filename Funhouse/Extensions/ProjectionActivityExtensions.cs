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
    public static class ProjectionActivityExtensions
    {
        /// <summary>
        ///     Mask all pixels outside the Earth to assist image stitching of projected images.
        /// </summary>
        public static ProjectionActivity RemoveBackground(this ProjectionActivity activity)
        {
            Guard.Against.Null(activity.Source, nameof(activity.Source));
            activity.Source.RemoveBackground();

            return activity;
        }
        
        public static ProjectionActivity CropBorders(this ProjectionActivity activity)
        {
            Guard.Against.Null(activity.Source, nameof(activity.Source));
            Guard.Against.Null(activity.Definition, nameof(activity.Definition));
            
            // Perform crop
            if (activity.Definition.Crop != null) activity.Source.AutoCropBorder(activity.Definition.Crop);
            return activity;
        }
        
        public static ProjectionActivity NormaliseHistogram(this ProjectionActivity activity)
        {
            Guard.Against.Null(activity.Source, nameof(activity.Source));
            Guard.Against.Null(activity.Definition, nameof(activity.Definition));

            // Normalise brightness and contrast
            activity.Source.Mutate(c => c.HistogramEqualization());
            activity.Source.Mutate(c => c.Brightness(activity.Definition.Brightness));
            activity.Source.Mutate(c => c.Brightness(1.1f));

            return activity;
        }

        /// <summary>
        ///     Normalises the source image size to match the specified output spatial resolution. Doing
        ///     so simplifies maths when blending multiple pages.
        /// </summary>
        public static ProjectionActivity NormaliseSize(this ProjectionActivity activity, int imageSize)
        {
            Guard.Against.Null(activity.Source, nameof(activity.Source));

            if (activity.Source.Width != imageSize || activity.Source.Height != imageSize)
            {
                // TODO test results of different interpolation types
                activity.Source.Mutate(c => c.Resize(imageSize, imageSize, KnownResamplers.Welch));
            }

            return activity;
        }

        public static async Task SaveAsync(this ProjectionActivity activity, string suffix, CommandLineOptions options)
        {
            var filename = $"{Path.GetFileNameWithoutExtension(activity.Path)}{suffix}.jpg";
            var outputPath = Path.Combine(Path.GetDirectoryName(activity.Path)!, filename);
            await activity.Target.SaveAsync(outputPath);

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