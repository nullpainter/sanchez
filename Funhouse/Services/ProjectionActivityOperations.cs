using System;
using System.Collections.Generic;
using System.Linq;
using Ardalis.GuardClauses;
using Funhouse.Extensions;
using Funhouse.Models;
using Funhouse.Models.Configuration;
using Funhouse.Models.Projections;
using MathNet.Spatial.Units;
using SixLabors.ImageSharp;

namespace Funhouse.Services
{
    public interface IProjectionActivityOperations
    {
        void Initialise(List<ProjectionActivity> activities);
        void CalculateCrop();
        void Reproject();
        List<ProjectionActivity> GetUnmapped();
    }

    public class ProjectionActivityOperations : IProjectionActivityOperations
    {
        private readonly IImageProjector _imageProjector;
        private readonly IProjectionOverlapCalculator _projectionOverlapCalculator;
        private List<ProjectionActivity> _activities = null!;
        private bool _initialised;
        private readonly CommandLineOptions _options;

        public ProjectionActivityOperations(
            CommandLineOptions options,
            IImageProjector imageProjector,
            IProjectionOverlapCalculator projectionOverlapCalculator)
        {
            _options = options;
            _imageProjector = imageProjector;
            _projectionOverlapCalculator = projectionOverlapCalculator;
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

        public void Reproject()
        {
            EnsureInitialised();

            // Determine satellite's visible range
            var globalOffset = -_activities.Select(p => p.LongitudeRange.UnwrapLongitude().Start).Min();

            foreach (var projection in _activities)
            {
                // Reproject geostationary image into equirectangular
                projection.Output = _imageProjector.Reproject(projection, _options);

                // Overlap range relative the satellite's visible range
                projection.Offset = GetOffset(projection.Definition!, globalOffset);
            }
        }

        private static Point GetOffset(SatelliteDefinition definition, Angle globalOffset)
        {
            var longitude = (definition.LongitudeRange.Start + globalOffset).NormaliseLongitude();
            
            // Convert to a equirectangular map offset, with a pixel range of -180 to 180 degrees
            var offset = longitude.ScaleToWidth(Constants.ImageSize * 2);
            return new Point(offset, 0);
        }
    }
}