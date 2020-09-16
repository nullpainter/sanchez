using Funhouse.Extensions;
using Funhouse.Models;
using Funhouse.Models.Angles;
using Funhouse.Models.Projections;
using Serilog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;

namespace Funhouse.ImageProcessing.Projection
{
    public static class ReprojectExtensions
    {
        public static Image<Rgba32> Reproject(this Registration registration, RenderOptions options)
        {
            var definition = registration.Definition;

            // Preserve 2:1 equirectangular aspect ratio
            var maxWidth = options.ImageSize * 2;
            var maxHeight = options.ImageSize;

            // Determine pixel ranges of projected image so we can limit our processing to longitudes visible to satellite
            // Unwrap the longitude range to simplify maths
            var longitudeRange = new Range(
                definition.LongitudeRange.Start,
                definition.LongitudeRange.End).UnwrapLongitude();

            var latitudeRange = new Range(definition.LatitudeRange.Start, definition.LatitudeRange.End);

            Log.Information("{definition:l0} latitude range {startRange:F2} to {endRange:F2} degrees",
                definition.DisplayName,
                Angle.FromRadians(latitudeRange.Start).Degrees,
                Angle.FromRadians(latitudeRange.End).Degrees);

            Log.Information("{definition:l0} unwrapped longitude range {startRange:F2} to {endRange:F2} degrees",
                definition.DisplayName,
                Angle.FromRadians(longitudeRange.Start).Degrees,
                Angle.FromRadians(longitudeRange.End).Degrees);

            // Get size of projection in pixels
            var xRange = new PixelRange(longitudeRange, a => a.ScaleToWidth(maxWidth));
            var yRange = new PixelRange(latitudeRange, a => a.ScaleToHeight(maxHeight));

            Log.Information("{definition:l0} pixel range X: {minX} - {maxX} px", definition.DisplayName, xRange.Start, xRange.End);
            Log.Information("{definition:l0} pixel range Y: {minY} - {maxY} px", definition.DisplayName, yRange.Start, yRange.End);

            Log.Information("{definition:l0} width: {targetWidth} px", definition.DisplayName, xRange.Range);
            Log.Information("{definition:l0} height: {targetWidth} px", definition.DisplayName, yRange.Range);

            // Create target image with the correct dimensions for the projected satellite image
            var target = new Image<Rgba32>(xRange.Range, yRange.Range);
            Log.Information("{definition:l0} Reprojecting", definition.DisplayName);

            // Perform reprojection
            var operation = new ReprojectRowOperation(registration, target, xRange.Start, yRange.Start, options);
            ParallelRowIterator.IterateRows(Configuration.Default, target.Bounds(), in operation);

            return target;
        }
    }
}