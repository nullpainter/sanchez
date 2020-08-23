using System;
using Ardalis.GuardClauses;
using Funhouse.Models;
using Funhouse.Models.Angles;
using Funhouse.Models.Projections;
using Funhouse.Projections;
using Serilog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using Range = Funhouse.Models.Angles.Range;

namespace Funhouse.ImageProcessing.Projection
{
    public static class ReprojectExtensions
    {
        public static Image<Rgba32> Reproject(
            this ProjectionActivity activity,
            CommandLineOptions options,
            IProjection projection)
        {
            Guard.Against.Null(activity.Definition, nameof(activity.Definition));
            var definition = activity.Definition;
            
            // Determine pixel ranges of projected image so we can limit our processing to longitudes visible to satellite
            var minProjectedLongitude = new ScalarProjectionAngle(definition.VisibleRange.Start);
            var maxProjectedLongitude = new ScalarProjectionAngle(definition.VisibleRange.End);

            // Preserve 2:1 Mercator aspect ratio
            const int maxWidth = Constants.ImageSize * 2;

            // Unwrap the longitude range to simplify maths
            var longitudeRange = new Range(
                minProjectedLongitude.Angle,
                maxProjectedLongitude.Angle).UnwrapLongitude();

            Log.Information("{definition:l0} unwrapped range {startRange:F2} to {endRange:F2} degrees", 
                definition.DisplayName, 
                longitudeRange.Start.Degrees, 
                longitudeRange.End.Degrees);

            // Get size of projection in pixels
            var minX = new ScalarProjectionAngle(longitudeRange.Start).ScaleToWidth(maxWidth) - 10;
            var maxX = new ScalarProjectionAngle(longitudeRange.End).ScaleToWidth(maxWidth) + 10;

            Log.Information("{definition:l0} pixel range: {minX:F0} - {maxX:F0}px", definition.DisplayName, minX, maxX);

            var targetWidth = maxX - minX;
            Log.Information("{definition:l0} width: {targetWidth:F0}px", definition.DisplayName, targetWidth);

            // Create target image with the correct dimensions for the projected satellite image
            var target = new Image<Rgba32>((int) Math.Ceiling(targetWidth), Constants.ImageSize);

            Log.Information("{definition:l0} Reprojecting", definition.DisplayName);

            // Perform reprojection
            var operation = new ReprojectRowOperation(activity, target, minX, options, projection);
            ParallelRowIterator.IterateRows(Configuration.Default, target.Bounds(), in operation);

            return target;
        }
    }
}