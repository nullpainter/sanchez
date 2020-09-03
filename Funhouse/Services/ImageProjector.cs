using Ardalis.GuardClauses;
using Funhouse.ImageProcessing.Mask;
using Funhouse.ImageProcessing.Projection;
using Funhouse.Models;
using Funhouse.Models.Projections;
using MathNet.Spatial.Units;
using Serilog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Funhouse.Services
{
    public interface IImageProjector
    {
        Image<Rgba32> Reproject(ProjectionActivity activity, RenderOptions options);
    }

    public class ImageProjector : IImageProjector
    {
        public Image<Rgba32> Reproject(ProjectionActivity activity, RenderOptions options)
        {
            LogStatistics(activity);

            using var source = activity.Source!;

            // Mask all pixels outside the Earth to assist image stitching of projected images
            source.RemoveBackground();

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
                Angle.FromRadians(definition.LongitudeRange.Start).Degrees,
                Angle.FromRadians(definition.LongitudeRange.End).Degrees);

            Log.Information("{definition:l0} crop {startRange:F2} to {endRange:F2} degrees",
                definition.DisplayName,
                Angle.FromRadians(longitudeCrop.Start).Degrees,
                Angle.FromRadians(longitudeCrop.End).Degrees);
        }
    }
}