using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Funhouse.Extensions;
using Funhouse.Extensions.Images;
using Funhouse.ImageProcessing.Projection;
using Funhouse.ImageProcessing.ShadeEdges;
using Funhouse.ImageProcessing.Tint;
using Funhouse.Models;
using Funhouse.Models.Projections;
using Funhouse.Services.Underlay;
using Serilog;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Funhouse.Services
{
    // TODO this class is a mess. Unsure what it's doing. maybe split into separate classes. we have stuff doing bunches of things
    // for multiple images in other places and it's not obvious where code should go.
    public interface IProjectionActivityOperations
    {
        void Initialise(SatelliteImages images);
        ProjectionActivityOperations CalculateOverlap();
        void ToEquirectangular(CancellationToken cancellationToken);
        Task RenderGeostationaryUnderlayAsync(SatelliteImage image);
    }

    public class ProjectionActivityOperations : IProjectionActivityOperations
    {
        private readonly IProjectionOverlapCalculator _projectionOverlapCalculator;
        private readonly IUnderlayService _underlayService;
        private SatelliteImages _images = null!;
        private bool _initialised;
        private readonly RenderOptions _options;

        public ProjectionActivityOperations(
            RenderOptions options,
            IProjectionOverlapCalculator projectionOverlapCalculator,
            IUnderlayService underlayService)
        {
            _options = options;
            _projectionOverlapCalculator = projectionOverlapCalculator;
            _underlayService = underlayService;
        }

        public void Initialise(SatelliteImages images)
        {
            _images = images;
            _initialised = true;
            
            foreach (var image in images.Images)
            {
                Log.Information("{definition:l0} loaded {path}", image.Definition.DisplayName, image.Path);
            }
        }
   
        public async Task RenderGeostationaryUnderlayAsync(SatelliteImage image)
        {
            Guard.Against.Null(_options.GeostationaryRender, nameof(_options.GeostationaryRender));

            // Get or generate projected underlay
            var underlayOptions = new UnderlayProjectionOptions(
                ProjectionType.Geostationary,
                _options.InterpolationType,
                _options.ImageSize,
                _options.UnderlayPath);

            Log.Information("Retrieving underlay");
            var underlay = await _underlayService.GetUnderlayAsync(underlayOptions, image.Definition);

            Log.Information("Tinting and normalising IR imagery");
            image.Image.Mutate(c => c.HistogramEqualization());
            image.Image.Tint(_options.Tint);

            Log.Information("Blending with underlay");
            image.Image.Mutate(ctx => ctx.Resize(_options.ImageSize, _options.ImageSize));
            image.Image.Mutate(ctx => ctx.DrawImage(underlay, PixelColorBlendingMode.Screen, 1.0f));

            var hazeAmount = _options.GeostationaryRender.HazeAmount;
            if (hazeAmount > 0) image.Image.ApplyHaze(_options.Tint, hazeAmount);

            // Perform global colour correction
            image.Image.ColourCorrect(_options);
            image.Image.Mutate();
        }

        /// <summary>
        ///     Calculates overlapping regions between satellites.
        /// </summary>
        public ProjectionActivityOperations CalculateOverlap()
        {
            EnsureInitialised();

            _projectionOverlapCalculator.Initialise(_images.Images.Select(p => p.Definition));

            // Set latitude and longitude ranges based on overlapping satellite ranges
            _images.Images.ForEach(image =>
            {
                image.LongitudeRange = _projectionOverlapCalculator.GetNonOverlappingRange(image.Definition);
                image.LatitudeRange = image.Definition.LatitudeRange;
            });

            return this;
        }

        private void EnsureInitialised()
        {
            if (!_initialised) throw new InvalidOperationException($"Not initialised; please call {nameof(Initialise)} first");
        }

        public void ToEquirectangular(CancellationToken cancellationToken)
        {
            EnsureInitialised();

            // Offset all images by the minimum longitude
            var globalOffset = -_images.Images.Select(p => p.LongitudeRange.UnwrapLongitude().NormaliseLongitude().Start).Min();

            _images.Images.ForEach(image =>
            {
                if (cancellationToken.IsCancellationRequested) return;
                
                Guard.Against.Null(image.Definition, nameof(image.Definition));

                // Reproject geostationary image into equirectangular
                LogStatistics(image);
                image.Image = image.Reproject(_options);

                // Overlap range relative the satellite's visible range and convert to a equirectangular map
                // offset with a pixel range of -180 to 180 degrees
                var longitude = (image.Definition.LongitudeRange.Start + globalOffset).NormaliseLongitude();
                image.OffsetX = longitude.ScaleToWidth(_options.ImageSize * 2);
            });
        }
        
        
        private static void LogStatistics(SatelliteImage image)
        {
            Guard.Against.Null(image.Definition, nameof(image.Definition));

            var definition = image.Definition;
            var longitudeCrop = image.LongitudeRange;

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