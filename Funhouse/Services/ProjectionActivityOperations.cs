using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Funhouse.Extensions;
using Funhouse.Models;
using Funhouse.Models.Angles;
using Funhouse.Models.Configuration;
using Funhouse.Models.Projections;
using MathNet.Spatial.Units;
using SixLabors.ImageSharp;

namespace Funhouse.Services
{
    public interface IProjectionActivityOperations
    {
        void Initialise(List<ProjectionActivity> activities, CancellationTokenSource cancellationTokenSource);
        void CalculateCrop();
        Task ReprojectAsync();
        List<ProjectionActivity> GetUnmapped();
    }

    public class ProjectionActivityOperations : IProjectionActivityOperations
    {
        private readonly IImageProjector _imageProjector;
        private readonly IProjectionOverlapCalculator _projectionOverlapCalculator;
        private List<ProjectionActivity> _activities = null!;
        private bool _initialised;
        private readonly CommandLineOptions _options;
        private CancellationTokenSource? _cancellationTokenSource;

        public ProjectionActivityOperations(
            CommandLineOptions options,
            IImageProjector imageProjector,
            IProjectionOverlapCalculator projectionOverlapCalculator)
        {
            _options = options;
            _imageProjector = imageProjector;
            _projectionOverlapCalculator = projectionOverlapCalculator;
        }

        public void Initialise(List<ProjectionActivity> activities, CancellationTokenSource cancellationTokenSource)
        {
            _activities = activities;
            _cancellationTokenSource = cancellationTokenSource;
            _initialised = true;
        }

        public List<ProjectionActivity> GetUnmapped()
        {
            EnsureInitialised();
            return _activities!.Where(p => p.Definition == null).ToList();
        }

        public void CalculateCrop()
        {
            EnsureInitialised();
            if (GetUnmapped().Any()) throw new InvalidOperationException("Not all images have valid satellite definitions");

            _projectionOverlapCalculator.Initialise(_activities.Select(p => p.Definition!));
            
            // Set latitude and longitude ranges based on overlapping satellite ranges
            _activities.ForEach(a =>
            {
                Guard.Against.Null(a.Definition, nameof(a.Definition));
                
                a.LongitudeRange = _options.Stitch ? _projectionOverlapCalculator.GetNonOverlappingRange(a.Definition!) : a.Definition.LongitudeRange;
                a.LatitudeRange = a.Definition.LatitudeRange;
            });
        }

        private void EnsureInitialised()
        {
            if (!_initialised) throw new InvalidOperationException($"Not initialised; please call {nameof(Initialise)} first");
        }

        public async Task ReprojectAsync()
        {
            EnsureInitialised();

            // Satellite's visible range
            var globalOffset = -_activities.Select(p => p.LongitudeRange.UnwrapLongitude().Start).Min();

            foreach (var projection in _activities)
            {
                if (_cancellationTokenSource?.IsCancellationRequested == true) return;

                projection.Output = await _imageProjector.ReprojectAsync(projection, _options);

                // Overlap range relative the satellite's visible range
                projection.Offset = GetOffset(projection.Definition!, globalOffset);

                // FIXME this is kinda batch so we need to honour outputPath directory
                if (!_options.Stitch || _options.Debug)
                {
                    var targetFilename = Path.GetFileNameWithoutExtension(projection.Path) + "-proj.jpg";

                    // Draw image onto a black background as we have applied transparency
                    var target = projection.Output.Clone();
                    target.AddBackgroundColour(Color.Black);

                    await target.SaveAsync(targetFilename);
                    Console.WriteLine($"Output written to {Path.GetFullPath(targetFilename)}");
                }
            }
        }

        private static PointF GetOffset(SatelliteDefinition definition, Angle globalOffset)
        {
            var longitude = (definition.LongitudeRange.Start + globalOffset).NormaliseLongitude();
            
            // Convert to a Mercator map offset, with a pixel range of -180 to 180 degrees
            var offset = Constants.ImageSize * 2 * ((longitude.Radians + Math.PI) / MathNet.Numerics.Constants.Pi2);
            return new PointF((float)offset, 0);
        }
    }
}