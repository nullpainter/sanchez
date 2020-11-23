using System;
using System.Collections.Generic;
using System.Linq;
using Sanchez.Processing.Models.Configuration;
using Sanchez.Processing.Models.Projections;
using Range = Sanchez.Processing.Models.Angles.Range;

namespace Sanchez.Processing.Services
{
    public interface IProjectionOverlapCalculator
    {
        void Initialise(IEnumerable<SatelliteDefinition> definitions);
        ProjectionRange GetNonOverlappingRange(SatelliteDefinition definition);
    }

    public class ProjectionOverlapCalculator : IProjectionOverlapCalculator
    {
        private Dictionary<SatelliteDefinition, Range>? _definitions;
        private bool _initialised;

        public void Initialise(IEnumerable<SatelliteDefinition> definitions)
        {
            _definitions = definitions
                .Select(entry => new
                {
                    Definition = entry,
                    Range = entry.LongitudeRange
                })
                .ToDictionary(entry => entry.Definition, key => key.Range);

            _initialised = true;
        }

        // note only works for simple overlap on one or other side
        // only handles one satellite overlapping each side
        public ProjectionRange GetNonOverlappingRange(SatelliteDefinition definition)
        {
            if (!_initialised) throw new InvalidOperationException($"Please call {nameof(Initialise)} before performing range calculations");

            var minLongitude = definition.LongitudeRange.UnwrapLongitude().Start;
            var maxLongitude = definition.LongitudeRange.UnwrapLongitude().End;
            var overlappingLeft = false;
            var overlappingRight = false;

            // Iterate over other satellites
            foreach (var other in _definitions!.Where(entry => entry.Key != definition).Select(entry => entry.Value))
            {
                var range = definition.LongitudeRange.UnwrapLongitude();
                var otherRange = other.UnwrapLongitude();
                var offset = 0.0;

                // Apply an offset to both satellites being compared if either wraps around -180 to 180 longitude
                if (definition.LongitudeRange.End < definition.LongitudeRange.Start || other.End < other.Start)
                {
                    if (other.End < other.Start)
                    {
                        offset = -Math.PI - Math.Min(definition.LongitudeRange.Start, other.Start);
                    }
                    else
                    {
                        offset = -Math.PI - Math.Max(definition.LongitudeRange.Start, other.Start);
                    }

                    range = (range + offset).NormaliseLongitude();
                    otherRange = (other + offset).NormaliseLongitude();

                    minLongitude += offset;
                    maxLongitude += offset;
                }

                if (range.Start < otherRange.Start && range.End > otherRange.Start)
                {
                    var midPoint = (range.End - otherRange.Start) / 2 + otherRange.Start;
                    if (midPoint < maxLongitude) maxLongitude = midPoint;
                    overlappingRight = true;
                }
                else if (range.End > otherRange.End && range.Start < otherRange.End)
                {
                    var midPoint = (otherRange.End - range.Start) / 2 + range.Start;
                    if (midPoint > minLongitude) minLongitude = midPoint;
                    overlappingLeft = true;
                }

                // Remove offset which was added to simplify calculations
                maxLongitude -= offset;
                minLongitude -= offset;
            }

            return new ProjectionRange(new Range(minLongitude, maxLongitude), overlappingLeft, overlappingRight);
        }
    }
}