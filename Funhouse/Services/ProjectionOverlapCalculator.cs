using System;
using System.Collections.Generic;
using System.Linq;
using Funhouse.Models.Configuration;
using MathNet.Spatial.Units;
using Range = Funhouse.Models.Angles.Range;

namespace Funhouse.Services
{
    public interface IProjectionOverlapCalculator
    {
        void Initialise(IEnumerable<SatelliteDefinition> definitions);
        Range GetNonOverlappingRange(SatelliteDefinition definition);
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
        public Range GetNonOverlappingRange(SatelliteDefinition definition)
        {
            if (!_initialised) throw new InvalidOperationException($"Please call {nameof(Initialise)} before performing range calculations");

            var minLongitude = definition.LongitudeRange.Start;
            var maxLongitude = definition.LongitudeRange.End;

            // Iterate over other satellites
            foreach (var other in _definitions!.Where(entry => entry.Key != definition).Select(entry => entry.Value))
            {
                var range = definition.LongitudeRange;
                var otherRange = other;
                var offset = Angle.FromRadians(0);

                // Apply an offset to both satellites being compared if either wraps around -180 to 180 longitude
                if (definition.LongitudeRange.End < definition.LongitudeRange.Start || other.End < other.Start)
                {
                    offset = Angle.FromRadians(-Math.PI - Math.Max(definition.LongitudeRange.Start.Radians, other.Start.Radians));
                    range = (definition.LongitudeRange + offset).UnwrapLongitude().NormaliseLongitude();
                    otherRange = (other + offset).UnwrapLongitude().NormaliseLongitude();

                    minLongitude += offset;
                    maxLongitude += offset;
                }

                if (range.Start < otherRange.Start && range.End > otherRange.Start)
                    maxLongitude = (range.End - otherRange.Start) / 2 + otherRange.Start;
                else if (range.End > otherRange.End && range.Start < otherRange.End) minLongitude = (otherRange.End - range.Start) / 2 + range.Start;

                maxLongitude -= offset;
                minLongitude -= offset;
            }

            return new Range(minLongitude, maxLongitude).NormaliseLongitude();
        }
    }
}