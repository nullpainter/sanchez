using System.IO;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Funhouse.ImageProcessing.Mask;
using Funhouse.ImageProcessing.Projection;
using Funhouse.Models;
using Funhouse.Models.Projections;
using Serilog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Funhouse.Services
{
    public interface IImageProjector
    {
        Task<Image<Rgba32>> ReprojectAsync(ProjectionActivity activity, CommandLineOptions options);
    }

    public class ImageProjector : IImageProjector
    {
        public async Task<Image<Rgba32>> ReprojectAsync(ProjectionActivity activity, CommandLineOptions options)
        {
            LogStatistics(activity);

            using var source = activity.Source!;

            // Normalise to 2km spatial resolution to simplify maths
            source.Mutate(c => c.Resize(Constants.ImageSize, Constants.ImageSize));

            // Mask all pixels outside the Earth to assist image stitching of projected images
            if (options.BlurEdges) source.BlurEdges();

            // Save output image for debugging purposes
            if (options.Debug)
            {
                var targetFilename = Path.GetFileNameWithoutExtension(activity.Path) + "-masked.jpg";
                await source.SaveAsync(targetFilename);
            }

            // Perform target projection
            return activity.Reproject(options);
        }

        private static void LogStatistics(ProjectionActivity activity)
        {
            Guard.Against.Null(activity.Definition, nameof(activity.Definition));

            var definition = activity.Definition;
            var longitudeCrop = activity.LongitudeRange;

            Log.Information("{definition:l0} range {startRange:F2} to {endRange:F2} degrees",
                definition.DisplayName,
                definition.LongitudeRange.Start.Degrees,
                definition.LongitudeRange.End.Degrees);

            Log.Information("{definition:l0} crop {startRange:F2} to {endRange:F2} degrees",
                definition.DisplayName,
                longitudeCrop.Start.Degrees,
                longitudeCrop.End.Degrees);
        }
    }
}