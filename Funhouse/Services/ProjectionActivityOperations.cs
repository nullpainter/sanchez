using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Funhouse.Extensions;
using Funhouse.Extensions.Images;
using Funhouse.ImageProcessing.Projection;
using Funhouse.ImageProcessing.ShadeEdges;
using Funhouse.ImageProcessing.Tint;
using Funhouse.Models;
using Funhouse.Models.CommandLine;
using Funhouse.Models.Projections;
using Funhouse.Services.Equirectangular;
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
        void ToEquirectangular();
        Task RenderGeostationaryUnderlayAsync(SatelliteImage image);
    }

    public class ProjectionActivityOperations : IProjectionActivityOperations
    {
        private readonly IProjectionOverlapCalculator _projectionOverlapCalculator;
        private readonly IUnderlayService _underlayService;
        private SatelliteImages _images = null!;
        private bool _initialised;
        private readonly RenderOptions _renderOptions;
        private readonly CommandLineOptions _commandLineOptions;

        public ProjectionActivityOperations(
            RenderOptions renderOptions,
            CommandLineOptions commandLineOptions,
            IProjectionOverlapCalculator projectionOverlapCalculator,
            IUnderlayService underlayService)
        {
            _renderOptions = renderOptions;
            _commandLineOptions = commandLineOptions;
            _projectionOverlapCalculator = projectionOverlapCalculator;
            _underlayService = underlayService;
        }

        public void Initialise(SatelliteImages images)
        {
            this._images = images;
            _initialised = true;
        }
   
        public async Task RenderGeostationaryUnderlayAsync(SatelliteImage image)
        {
            Guard.Against.Null(image.Definition, nameof(image.Definition));
            Guard.Against.Null(image.Image, nameof(image.Image));

            // Get or generate projected underlay
            var underlayOptions = new UnderlayProjectionOptions(
                _renderOptions.ProjectionType,
                _renderOptions.InterpolationType,
                _renderOptions.ImageSize,
                _commandLineOptions.UnderlayPath);

            Log.Information("Retrieving underlay");
            var underlay = await _underlayService.GetUnderlayAsync(underlayOptions, image.Definition);

            Log.Information("Tinting and normalising IR imagery");
            image.Image.Mutate(c => c.HistogramEqualization());
            image.Image.Tint(_renderOptions.Tint);

            Log.Information("Blending with underlay");
            image.Image.Mutate(ctx => ctx.Resize(_renderOptions.ImageSize, _renderOptions.ImageSize));
            image.Image.Mutate(ctx => ctx.DrawImage(underlay, PixelColorBlendingMode.Screen, 1.0f));

            if (_renderOptions.HazeAmount > 0) image.Image.ApplyHaze(_renderOptions.Tint, _renderOptions.HazeAmount);

            // Perform global colour correction
            image.Image.ColourCorrect(_renderOptions);
            image.Image.Mutate();

            await image.SaveAsync("-FC", _commandLineOptions);
        }

        /// <summary>
        ///     Calculates overlapping regions between satellites.
        /// </summary>
        public ProjectionActivityOperations CalculateOverlap()
        {
            EnsureInitialised();
            if (_images.GetUnmapped().Any()) throw new InvalidOperationException("Not all images have valid satellite definitions");

            _projectionOverlapCalculator.Initialise(_images.Images.Select(p => p.Definition!));

            // Set latitude and longitude ranges based on overlapping satellite ranges
            _images.Images.ForEach(activity =>
            {
                Guard.Against.Null(activity.Definition, nameof(activity.Definition));

                activity.LongitudeRange = _commandLineOptions.Stitch ? _projectionOverlapCalculator.GetNonOverlappingRange(activity.Definition!) : activity.Definition.LongitudeRange;
                activity.LatitudeRange = activity.Definition.LatitudeRange;
            });

            return this;
        }

        private void EnsureInitialised()
        {
            if (!_initialised) throw new InvalidOperationException($"Not initialised; please call {nameof(Initialise)} first");
        }

        public void ToEquirectangular()
        {
            EnsureInitialised();

            // Offset all images by the minimum longitude
            var globalOffset = -_images.Images.Select(p => p.LongitudeRange.UnwrapLongitude().NormaliseLongitude().Start).Min();

            _images.Images.ForEach(image =>
            {
                Guard.Against.Null(image.Definition, nameof(image.Definition));

                // Reproject geostationary image into equirectangular
                LogStatistics(image);
                image.Image = image.Reproject(_renderOptions);

                // Overlap range relative the satellite's visible range and convert to a equirectangular map
                // offset with a pixel range of -180 to 180 degrees
                var longitude = (image.Definition.LongitudeRange.Start + globalOffset).NormaliseLongitude();
                image.OffsetX = longitude.ScaleToWidth(_renderOptions.ImageSize * 2);
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