using System;
using Ardalis.GuardClauses;
using Funhouse.Models;
using Funhouse.Models.Angles;
using Funhouse.Models.Projections;
using Serilog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using Range = Funhouse.Models.Angles.Range;

namespace Funhouse.ImageProcessing.Projection
{
    public static class ReprojectExtensions
    {
        public static Image<Rgba32> Reproject(this ProjectionActivity activity, CommandLineOptions options)
        {
            Guard.Against.Null(activity.Definition, nameof(activity.Definition));
            var definition = activity.Definition;

            // Determine pixel ranges of projected image so we can limit our processing to longitudes visible to satellite
            var minProjectedLongitude = new ScalarProjectionAngle(definition.LongitudeRange.Start);
            var maxProjectedLongitude = new ScalarProjectionAngle(definition.LongitudeRange.End);

            var minProjectedLatitude = new ScalarProjectionAngle(definition.LatitudeRange.Start);
            var maxProjectedLatitude = new ScalarProjectionAngle(definition.LatitudeRange.End);

            // Preserve 2:1 equirectangular aspect ratio
            const int maxWidth = Constants.ImageSize * 2;
            const int maxHeight = Constants.ImageSize;

            // Unwrap the longitude range to simplify maths
            var longitudeRange = new Range(
                minProjectedLongitude.Angle,
                maxProjectedLongitude.Angle).UnwrapLongitude();

            var latitudeRange = new Range(
                minProjectedLatitude.Angle,
                maxProjectedLatitude.Angle);

            Log.Information("{definition:l0} latitude range {startRange:F2} to {endRange:F2} degrees",
                definition.DisplayName,
                latitudeRange.Start.Degrees,
                latitudeRange.End.Degrees);

            Log.Information("{definition:l0} unwrapped longitude range {startRange:F2} to {endRange:F2} degrees",
                definition.DisplayName,
                longitudeRange.Start.Degrees,
                longitudeRange.End.Degrees);

            // Get size of projection in pixels
            var minX = (int)Math.Round(new ScalarProjectionAngle(longitudeRange.Start).ScaleToWidth(maxWidth));
            var maxX = (int)Math.Round(new ScalarProjectionAngle(longitudeRange.End).ScaleToWidth(maxWidth));

            // FIXME can we simplify this shit? Refactor into something which does both this and the stuff above so we 
            // don't have four variables?
            var minY = (int)Math.Round(new ScalarProjectionAngle(latitudeRange.Start).ScaleToHeight(maxHeight));
            var maxY = (int)Math.Round(new ScalarProjectionAngle(latitudeRange.End).ScaleToHeight(maxHeight));

            Log.Information("{definition:l0} pixel range X: {minX} - {maxX} px", definition.DisplayName, minX, maxX);
            Log.Information("{definition:l0} pixel range Y: {minY} - {maxY} px", definition.DisplayName, minY, maxY);

            var targetWidth = maxX - minX;
            var targetHeight = maxY - minY;

            Log.Information("{definition:l0} width: {targetWidth} px", definition.DisplayName, targetWidth);
            Log.Information("{definition:l0} height: {targetWidth} px", definition.DisplayName, targetHeight);

            // Create target image with the correct dimensions for the projected satellite image
            var target = new Image<Rgba32>(targetWidth, targetHeight);
            Log.Information("{definition:l0} Reprojecting", definition.DisplayName);

            // Perform reprojection
            var operation = new ReprojectRowOperation(activity, target, minX, minY, options);
            ParallelRowIterator.IterateRows(Configuration.Default, target.Bounds(), in operation);

            return target;
        }
    }
}