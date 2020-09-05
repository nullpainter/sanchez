using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Funhouse.Extensions;
using Funhouse.Extensions.Images;
using Funhouse.ImageProcessing.Mask;
using Funhouse.ImageProcessing.ShadeEdges;
using Funhouse.ImageProcessing.Tint;
using Funhouse.Models;
using Funhouse.Models.CommandLine;
using Funhouse.Models.Configuration;
using Funhouse.Models.Projections;
using Funhouse.Services.Equirectangular;
using Funhouse.Services.Underlay;
using Serilog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Range = Funhouse.Models.Angles.Range;

namespace Funhouse.Services
{
    // TODO this class is a mess. Unsure what it's doing. maybe split into separate classes. we have stuff doing bunches of things
    // for multiple images in other places and it's not obvious where code should go.
    public interface IProjectionActivityOperations
    {
        void Initialise(List<ProjectionActivity> activities);
        ProjectionActivityOperations CalculateOverlap();
        void ReprojectToEquirectangular();
        List<ProjectionActivity> GetUnmapped();
        Task RenderGeostationaryUnderlayAsync();
        void GetVisibleRange(out Range latitudeRange, out Range longitudeRange);
        Range GetVisibleLongitudeRange();
    }

    public class ProjectionActivityOperations : IProjectionActivityOperations
    {
        private readonly IImageProjector _imageProjector;
        private readonly IProjectionOverlapCalculator _projectionOverlapCalculator;
        private readonly IUnderlayService _underlayService;
        private List<ProjectionActivity> _activities = null!;
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

        public void Initialise(List<ProjectionActivity> activities)
        {
            _activities = activities;
            _initialised = true;
        }
        
        public void GetVisibleRange(out Range latitudeRange, out Range longitudeRange)
        {
            latitudeRange = new Range(
                _activities.Min(a => a.LatitudeRange.Start),
                _activities.Max(a => a.LatitudeRange.End));

            longitudeRange = GetVisibleLongitudeRange();
        } 
        
        public Range GetVisibleLongitudeRange()
        {
            var sortedActivities = _activities.OrderBy(p => p.Offset.X).ToList();

            return new Range(
                sortedActivities.First().LongitudeRange.Start,
                sortedActivities.Last().LongitudeRange.End
            ).UnwrapLongitude();
        } 

        public List<ProjectionActivity> GetUnmapped()
        {
            EnsureInitialised();
            return _activities!.Where(p => p.Definition == null).ToList();
        }

        /// <summary>
        ///     Mask all pixels outside the Earth to assist image stitching of projected images.
        /// </summary>
        public ProjectionActivityOperations RemoveBackground()
        {
            _activities.ForEach(activity => 
            {
                Guard.Against.Null(activity.Source, nameof(activity.Source));
                activity.Source.RemoveBackground();
            });

            return this;
        }

        public async Task RenderGeostationaryUnderlayAsync()
        {
            foreach (var activity in _activities)
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

                await SaveAsync(activity, "-FC", _commandLineOptions);
            }
        }

        public ProjectionActivityOperations CropBorders()
        {
            _activities.ForEach(activity =>
            {
                Guard.Against.Null(activity.Source, nameof(activity.Source));
                Guard.Against.Null(activity.Definition, nameof(activity.Definition));

                if (activity.Definition.Crop != null) activity.Source.AutoCropBorder(activity.Definition.Crop);
            });

            return this;
        }

        private static async Task SaveAsync(ProjectionActivity activity, string suffix, CommandLineOptions options)
        {
            var filename = $"{Path.GetFileNameWithoutExtension(activity.Path)}{suffix}.jpg";
            var outputPath = Path.Combine(Path.GetDirectoryName(activity.Path)!, filename);
            await activity.Target.SaveAsync(outputPath);

            if (options.Verbose)
            {
                Log.Information("Output written to {path}", Path.GetFullPath(outputPath));
            }
            else
            {
                Console.WriteLine($"Output written to {Path.GetFullPath(outputPath)}");
            }
        }

        public ProjectionActivityOperations NormaliseHistogram()
        {
            _activities.ForEach(activity =>
            {
                Guard.Against.Null(activity.Source, nameof(activity.Source));
                Guard.Against.Null(activity.Definition, nameof(activity.Definition));

                // Normalise brightness and contrast
                activity.Source.Mutate(c => c.HistogramEqualization());
                activity.Source.Mutate(c => c.Brightness(activity.Definition.Brightness));
                activity.Source.Mutate(c => c.Brightness(1.1f));
            });

            return this;
        }

        /// <summary>
        ///     Calculates overlapping regions between satellites.
        /// </summary>
        public ProjectionActivityOperations CalculateOverlap()
        {
            EnsureInitialised();
            if (GetUnmapped().Any()) throw new InvalidOperationException("Not all images have valid satellite definitions");

            _projectionOverlapCalculator.Initialise(_activities.Select(p => p.Definition!));

            // Set latitude and longitude ranges based on overlapping satellite ranges
            _activities.ForEach(activity =>
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

        /// <summary>
        ///     Normalises the source image size to match the specified output spatial resolution. Doing
        ///     so simplifies maths when blending multiple pages.
        /// </summary>
        public ProjectionActivityOperations NormaliseSize()
        {
            var imageSize = _renderOptions.ImageSize;

            _activities.ForEach(activity =>
            {
                Guard.Against.Null(activity.Source, nameof(activity.Source));

                if (activity.Source.Width != imageSize || activity.Source.Height != imageSize)
                {
                    // TODO test results of different interpolation types
                    activity.Source.Mutate(c => c.Resize(imageSize, imageSize, KnownResamplers.Welch));
                }
            });

            return this;
        }

        public void ReprojectToEquirectangular()
        {
            EnsureInitialised();

            // Offset all images by the minimum longitude
            var globalOffset = -_activities.Select(p => p.LongitudeRange.UnwrapLongitude().NormaliseLongitude().Start).Min();

            _activities.ForEach(activity =>
            {
                // Reproject geostationary image into equirectangular
                activity.Target = _imageProjector.Reproject(activity, _renderOptions);

                // Overlap range relative the satellite's visible range
                activity.Offset = GetOffset(activity.Definition!, globalOffset);
            });
        }

        /// <summary>
        ///     Returns the horizontal offset of an image in equirectangular projection, based on the longitude of the
        ///     associated satellite and a global offset.
        /// </summary>
        private Point GetOffset(SatelliteDefinition definition, double globalOffset)
        {
            var longitude = (definition.LongitudeRange.Start + globalOffset).NormaliseLongitude();

            // Convert to a equirectangular map offset, with a pixel range of -180 to 180 degrees
            var offset = longitude.ScaleToWidth(_renderOptions.ImageSize * 2);
            return new Point(offset, 0);
        }
    }
}