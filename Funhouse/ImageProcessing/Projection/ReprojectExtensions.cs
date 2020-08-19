using System;
using Funhouse.Models.Angles;
using Funhouse.Models.Configuration;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using Range = Funhouse.Models.Angles.Range;

namespace Funhouse.ImageProcessing.Projection
{
    public static class ReprojectExtensions
    {
        public static Image<Rgba32> Reproject(this Image<Rgba32> source, SatelliteDefinition definition, RenderOptions options, Range longitudeCrop)
        {
            // Determine pixel ranges of projected image so we can limit our processing to longitudes visible to satellite
            var minProjectedLongitude = new ScalarProjectionAngle(definition.VisibleRange.Start);
            var maxProjectedLongitude = new ScalarProjectionAngle(definition.VisibleRange.End);

            // Preserve 2:1 Mercator aspect ratio
            const int maxWidth = Constants.ImageSize * 2;

            // Unwrap the longitude range to simplify maths
            var longitudeRange = new Range(
                minProjectedLongitude.Angle,
                maxProjectedLongitude.Angle).UnwrapLongitude();
            
            Console.WriteLine($"{definition.DisplayName} unwrapped range {longitudeRange.Start.Degrees:F2} to {longitudeRange.End.Degrees:F2} degrees");

            // Get size of projection in pixels
            var minX = new ScalarProjectionAngle(longitudeRange.Start).ScaleToWidth(maxWidth);
            var maxX = new ScalarProjectionAngle(longitudeRange.End).ScaleToWidth(maxWidth);

            Console.WriteLine($"{definition.DisplayName} pixel range: {minX:F0} - {maxX:F0}");

            var targetWidth = maxX - minX;
            Console.WriteLine($"{definition.DisplayName} width: {targetWidth:F0}px");

            // Create target image with the correct dimensions for the projected satellite image
            var target = new Image<Rgba32>((int) Math.Round(targetWidth), Constants.ImageSize);

            Console.WriteLine($"{definition.DisplayName} Reprojecting...");

            // Perform reprojection
            var operation = new ReprojectRowOperation(source, target, definition, minX, options, longitudeCrop.UnwrapLongitude());
            ParallelRowIterator.IterateRows(Configuration.Default, target.Bounds(), in operation);

            return target;
        }
    }
}