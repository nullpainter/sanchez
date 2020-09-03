
using Funhouse.Models;
using Funhouse.Models.Configuration;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;

namespace Funhouse.ImageProcessing.Underlay
{
    public static class GeostationaryExtensions
    {
        /// <summary>
        ///     Reprojects an image in an equirectangular projection, such as an underlay or composited underlay with IR imagery,
        ///     into a geostationary projection. 
        /// </summary>
        public static Image<Rgba32> ToGeostationaryProjection(this Image<Rgba32> source, SatelliteDefinition definition, RenderOptions options)
        {
            // Projected image
            var target = new Image<Rgba32>(options.ImageSize, options.ImageSize);

            // Reproject image to match the given satellite
            var operation = new GeostationaryProjectionRowOperation(source, target, definition,  options);
            ParallelRowIterator.IterateRows(Configuration.Default, target.Bounds(), in operation);

            return target;
        } 
    }
}