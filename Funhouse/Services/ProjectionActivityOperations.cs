using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Funhouse.Extensions;
using Funhouse.Extensions.Images;
using Funhouse.ImageProcessing.Tint;
using Funhouse.Models;
using Funhouse.Models.CommandLine;
using Funhouse.Models.Configuration;
using Funhouse.Models.Projections;
using Funhouse.Services.Underlay;
using Serilog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Funhouse.Services
{
    public interface IProjectionActivityOperations
    {
        void Initialise(List<ProjectionActivity> activities);
        void NormaliseSize();
        
        /// <summary>
        ///     Calculates overlapping regions between satellites.
        /// </summary>
        void CalculateOverlap();
        void Reproject();
        List<ProjectionActivity> GetUnmapped();
        Task RenderGeostationaryUnderlayAsync();
        Task SaveAsync(string suffix, CommandLineOptions options);
        void NormaliseHistogram();
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

        public List<ProjectionActivity> GetUnmapped()
        {
            EnsureInitialised();
            return _activities!.Where(p => p.Definition == null).ToList();
        }

        public async Task RenderGeostationaryUnderlayAsync()
        {
            foreach (var activity in _activities)
            {
                Guard.Against.Null(activity.Definition, nameof(activity.Definition));
                
                // Get projected underlay
                var underlayOptions = new UnderlayProjectionOptions(_renderOptions.ProjectionType, _renderOptions.InterpolationType);
                var underlay = await _underlayService.GetUnderlayAsync(underlayOptions, activity.Definition, _commandLineOptions.UnderlayPath);

                activity.Target = activity.Source!.Clone();

                Log.Information("Tinting and normalising IR imagery");
                // Remove grey tint from satellite image
                // TODO coopied in Compositor

                activity.Source.Mutate(c => c.HistogramEqualization());
                activity.Source.Mutate(c => c.Brightness(activity.Definition.Brightness));
                activity.Source.Mutate(c => c.Brightness(1.1f));
  
                var clone = activity.Target.Clone();
                clone.Mutate(c => c.HistogramEqualization());
                activity.Target.Tint(_renderOptions.Tint);

                activity.Target.Mutate(c => c.DrawImage(clone, PixelColorBlendingMode.HardLight, 0.5f));

                // Render underlay and optionally crop to size
                activity.Target.Mutate(ctx => ctx.Resize(_renderOptions.ImageSize, _renderOptions.ImageSize));
                activity.Target.Mutate(ctx => ctx.DrawImage(underlay, PixelColorBlendingMode.Screen, 1.0f));

                // TODO copied in compositor
                // Perform global colour correction
                activity.Target.ColourCorrect(_renderOptions);
            }
        }

        public async Task SaveAsync(string suffix, CommandLineOptions options)
        {
            var saveTasks = new List<Task>();
            _activities.ForEach(a =>
            {
                var filename = $"{Path.GetFileNameWithoutExtension(a.Path)}{suffix}.jpg";
                var outputPath = Path.Combine(Path.GetDirectoryName(a.Path)!, filename);
                saveTasks.Add(a.Target.SaveAsync(outputPath));


                if (options.Verbose)
                {
                    Log.Information("Output written to {path}", Path.GetFullPath(outputPath));
                }
                else
                {
                    Console.WriteLine($"Output written to {Path.GetFullPath(outputPath)}");
                }
            });

            await Task.WhenAll(saveTasks);
        }

        public void NormaliseHistogram()
        {
            _activities.ForEach(a =>
            {
                Guard.Against.Null(a.Source, nameof(a.Source));
                Guard.Against.Null(a.Definition, nameof(a.Definition));

                // Normalise brightness and contrast
                // TODO copied in compositor
                a.Source.Mutate(c => c.HistogramEqualization());
                a.Source.Mutate(c => c.Brightness(a.Definition.Brightness));
                a.Source.Mutate(c => c.Brightness(1.1f));
            });
        }

        /// <summary>
        ///     Calculates overlapping regions between satellites.
        /// </summary>
        public void CalculateOverlap()
        {
            EnsureInitialised();
            if (GetUnmapped().Any()) throw new InvalidOperationException("Not all images have valid satellite definitions");

            _projectionOverlapCalculator.Initialise(_activities.Select(p => p.Definition!));

            // Set latitude and longitude ranges based on overlapping satellite ranges
            _activities.ForEach(a =>
            {
                Guard.Against.Null(a.Definition, nameof(a.Definition));

                a.LongitudeRange = _commandLineOptions.Stitch ? _projectionOverlapCalculator.GetNonOverlappingRange(a.Definition!) : a.Definition.LongitudeRange;
                a.LatitudeRange = a.Definition.LatitudeRange;
            });
        }

        private void EnsureInitialised()
        {
            if (!_initialised) throw new InvalidOperationException($"Not initialised; please call {nameof(Initialise)} first");
        }

        public void NormaliseSize()
        {
            var imageSize = _renderOptions.ImageSize;
            
            _activities.ForEach(activity =>
            {
                Guard.Against.Null(activity.Source, nameof(activity.Source));
                
                // TODO test results of different interpolation types
                if (activity.Source.Width != imageSize || activity.Source.Height != imageSize)
                {
                    // Normalise to 2km spatial resolution to simplify maths
                    activity.Source.Mutate(c => c.Resize(imageSize, imageSize, KnownResamplers.Welch));
                }
            });
        }

        public void Reproject()
        {
            EnsureInitialised();

            // Determine satellite's visible range
            var globalOffset = -_activities.Select(p => p.LongitudeRange.UnwrapLongitude().NormaliseLongitude().Start).Min();

            _activities.ForEach(activity =>
            {
                // Reproject geostationary image into equirectangular
                activity.Target = _imageProjector.Reproject(activity, _renderOptions);

                // Overlap range relative the satellite's visible range
                activity.Offset = GetOffset(activity.Definition!, globalOffset);
            });
        }

        private Point GetOffset(SatelliteDefinition definition, double globalOffset)
        {
            var longitude = (definition.LongitudeRange.Start + globalOffset).NormaliseLongitude();

            // Convert to a equirectangular map offset, with a pixel range of -180 to 180 degrees
            var offset = longitude.ScaleToWidth(_renderOptions.ImageSize * 2);
            return new Point(offset, 0);
        }
    }
}