using System.Collections.Generic;
using System.Linq;
using Funhouse.Models.Angles;
using Funhouse.Models.Projections;

namespace Funhouse.Models
{
    public class ProjectionActivities
    {
        public List<ProjectionActivity> Activities { get; }

        public ProjectionActivities(List<ProjectionActivity> activities) => Activities = activities;
            
        public void GetVisibleRange(out Range latitudeRange, out Range longitudeRange)
        {
            latitudeRange = new Range(
                Activities.Min(a => a.LatitudeRange.Start),
                Activities.Max(a => a.LatitudeRange.End));

            longitudeRange = GetVisibleLongitudeRange();
        } 
        
        public Range GetVisibleLongitudeRange()
        {
            var sortedActivities = Activities.OrderBy(p => p.OffsetX).ToList();

            return new Range(
                sortedActivities.First().LongitudeRange.Start,
                sortedActivities.Last().LongitudeRange.End
            ).UnwrapLongitude();
        } 
        
        
        /// <summary>
        ///     Gets projection activities which don't have an associated satellite definition.
        /// </summary>
        /// <returns></returns>
        public List<ProjectionActivity> GetUnmapped() => Activities!.Where(p => p.Definition == null).ToList();
    }
}