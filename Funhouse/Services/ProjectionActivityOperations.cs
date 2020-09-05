using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Funhouse.Extensions;
using Funhouse.Extensions.Images;
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
        void Initialise(ProjectionActivities activities);
        ProjectionActivityOperations CalculateOverlap();
        void ToEquirectangular();
        Task RenderGeostationaryUnderlayAsync(ProjectionActivity activity);
    }

    public class ProjectionActivityOperations : IProjectionActivityOperations
    {
        private readonly IImageProjector _imageProjector;
        private readonly IProjectionOverlapCalculator _projectionOverlapCalculator;
        private readonly IUnderlayService _underlayService;
        private ProjectionActivities _activities = null!;
        private bool _initialised;
        private readonly RenderOptions _renderOptions;
        private readonly CommandLineOptions _commandLineOptions;

        public ProjectionActivityOperations(
            RenderOptions renderOptions,
            CommandLineOptions commandLineOptions,
            IImageProjector imageProjector,
            IProjectionOverlapCalculator projectionOverlapCalculator,
            IUnderlayService underlayService)
        {
            _renderOptions = renderOptions;
            _commandLineOptions = commandLineOptions;
            _imageProjector = imageProjector;
            _projectionOverlapCalculator = projectionOverlapCalculator;
            _underlayService = underlayService;
        }

        public void Initialise(ProjectionActivities activities)
        {
            _activities = activities;
            _initialised = true;
        }
   
        public async Task RenderGeostationaryUnderlayAsync(ProjectionActivity activity)
        {
            Guard.Against.Null(activity.Definition, nameof(activity.Definition));
            Guard.Against.Null(activity.Source, nameof(activity.Source));

            // Get or generate projected underlay
            var underlayOptions = new UnderlayProjectionOptions(
                _renderOptions.ProjectionType,
                _renderOptions.InterpolationType,
                _renderOptions.ImageSize,
                _commandLineOptions.UnderlayPath);

            Log.Information("Retrieving underlay");
            var underlay = await _underlayService.GetUnderlayAsync(underlayOptions, activity.Definition);

            activity.Target = activity.Source.Clone();

            Log.Information("Tinting and normalising IR imagery");
            activity.Target.Mutate(c => c.HistogramEqualization());
            activity.Target.Tint(_renderOptions.Tint);

            Log.Information("Blending with underlay");
            activity.Target.Mutate(ctx => ctx.Resize(_renderOptions.ImageSize, _renderOptions.ImageSize));
            activity.Target.Mutate(ctx => ctx.DrawImage(underlay, PixelColorBlendingMode.Screen, 1.0f));

            if (_renderOptions.HazeAmount > 0) activity.Target.ApplyHaze(_renderOptions.Tint, _renderOptions.HazeAmount);

            // Perform global colour correction
            activity.Target.ColourCorrect(_renderOptions);
            activity.Target.Mutate();

            await activity.SaveAsync("-FC", _commandLineOptions);
        }

        /// <summary>
        ///     Calculates overlapping regions between satellites.
        /// </summary>
        public ProjectionActivityOperations CalculateOverlap()
        {
            EnsureInitialised();
            if (_activities.GetUnmapped().Any()) throw new InvalidOperationException("Not all images have valid satellite definitions");

            _projectionOverlapCalculator.Initialise(_activities.Activities.Select(p => p.Definition!));

            // Set latitude and longitude ranges based on overlapping satellite ranges
            _activities.Activities.ForEach(activity =>
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
            var globalOffset = -_activities.Activities.Select(p => p.LongitudeRange.UnwrapLongitude().NormaliseLongitude().Start).Min();

            _activities.Activities.ForEach(activity =>
            {
                Guard.Against.Null(activity.Definition, nameof(activity.Definition));

                
                // Reproject geostationary image into equirectangular
                activity.Target = _imageProjector.Reproject(activity, _renderOptions);

                // Overlap range relative the satellite's visible range and convert to a equirectangular map
                // offset with a pixel range of -180 to 180 degrees
                var longitude = (activity.Definition.LongitudeRange.Start + globalOffset).NormaliseLongitude();
                activity.OffsetX = longitude.ScaleToWidth(_renderOptions.ImageSize * 2);
            });
        }
    }
}