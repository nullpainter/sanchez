using System.Collections.Generic;
using System.Linq;
using Funhouse.Models.Angles;
using Funhouse.Models.Projections;

namespace Funhouse.Models
{
    public class SatelliteImages
    {
        public List<SatelliteImage> Images { get; }

        public SatelliteImages(List<SatelliteImage> activities) => Images = activities;
            
        public void GetVisibleRange(out Range latitudeRange, out Range longitudeRange)
        {
            latitudeRange = new Range(
                Images.Min(a => a.LatitudeRange.Start),
                Images.Max(a => a.LatitudeRange.End));

            longitudeRange = GetVisibleLongitudeRange();
        } 
        
        public Range GetVisibleLongitudeRange()
        {
            var sortedActivities = Images.OrderBy(p => p.OffsetX).ToList();

            return new Range(
                sortedActivities.First().LongitudeRange.Start,
                sortedActivities.Last().LongitudeRange.End
            ).UnwrapLongitude();
        } 
        
        
        /// <summary>
        ///     Gets projection activities which don't have an associated satellite definition.
        /// </summary>
        /// <returns></returns>
        public List<SatelliteImage> GetUnmapped() => Images!.Where(p => p.Definition == null).ToList();
    }
}