using System;
using Funhouse.Models.Angles;
using Funhouse.Models.Configuration;
using MathNet.Spatial.Units;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;

namespace Funhouse.ImageProcessing.Projection
{
    public static class ReprojectExtensions
    {
        public static Image<Rgba32> Reproject(this Image<Rgba32> source, SatelliteDefinition definition, RenderOptions options)
        {
            // Determine pixel ranges of projected image so we can limit our processing to longitudes visible to satellite
            var minProjectedLongitude = new ProjectionAngle(definition.VisibleRange.Start, new Angle());
            var maxProjectedLongitude = new ProjectionAngle(definition.VisibleRange.End, new Angle());
            
            // Preserve 2:1 Mercator aspect ratio
            var maxWidth = source.Width * 2;

            var minX = minProjectedLongitude.ToPixelCoordinates(maxWidth, 0);
            var maxX = maxProjectedLongitude.ToPixelCoordinates(maxWidth, 0);

            // Get size of projection in pixels, ensuring correct calculations for projections wrapping around our
            // Mercator longitude projection of -180 to 180 degrees.
            var targetWidth = minX.X < maxX.X ? maxX.X - minX.X : maxWidth - (minX.X - maxX.X);

            // Create target image with the correct dimensions for the projected satellite image
            var target = new Image<Rgba32>((int) Math.Round(targetWidth), source.Height);

            // Perform reprojection
            var operation = new ReprojectRowOperation(source, target, definition, minX, options);
            ParallelRowIterator.IterateRows(Configuration.Default, target.Bounds(), in operation);

            return target;
        }
    }
}