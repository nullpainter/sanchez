using System;
using System.Collections.Generic;
using System.Linq;
using Funhouse.Extensions;
using Funhouse.Models.Configuration;
using MathNet.Spatial.Units;
using Range = Funhouse.Models.Angles.Range;

namespace Funhouse
{
    public class ProjectionOverlapCalculator
    {
        private readonly Dictionary<SatelliteDefinition, Range> _definitions;

        public ProjectionOverlapCalculator(params SatelliteDefinition[] definitions)
        {
            _definitions = definitions
                .Select(entry => new
                {
                    Definition = entry,
                    Range = entry.VisibleRange
                })
                .ToDictionary(entry => entry.Definition, key => key.Range);
        }

     
        // note only works for simple overlap on one or other side
        // only handles one satellite overlapping each side
        public Range GetNonOverlappingRange(SatelliteDefinition definition)
        {
            var minLongitude = definition.VisibleRange.Start;
            var maxLongitude = definition.VisibleRange.End;
            
            // Iterate over other satellites
            foreach (var other in _definitions.Where(entry => entry.Key != definition).Select(entry => entry.Value))
            {
                var range = definition.VisibleRange;
                var otherRange = other;
                var offset = Angle.FromRadians(0);

                // Apply an offset to both satellites being compared if either wraps around -180 to 180 longitude
                if (definition.VisibleRange.End < definition.VisibleRange.Start || other.End < other.Start)
                {
                    offset = Angle.FromRadians(-Math.PI - Math.Max(definition.VisibleRange.Start.Radians, other.Start.Radians));
                    range = (definition.VisibleRange + offset).UnwrapLongitude().NormaliseLongitude();
                    otherRange = (other + offset).UnwrapLongitude().NormaliseLongitude();

                    minLongitude += offset;
                    maxLongitude += offset;
                }

                if (range.Start < otherRange.Start && range.End > otherRange.Start)
                {
                    maxLongitude = (range.End - otherRange.Start) / 2 + otherRange.Start;
                }
                else if (range.End > otherRange.End && range.Start < otherRange.End)
                {
                    minLongitude = (otherRange.End - range.Start) / 2 + range.Start;
                }
                
                maxLongitude -= offset;
                minLongitude -= offset;
            }
            
            return new Range(minLongitude, maxLongitude).NormaliseLongitude();
        }
    }
}