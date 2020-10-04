using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sanchez.Processing.Models.Projections;
using Range = Sanchez.Processing.Models.Angles.Range;

namespace Sanchez.Processing.Models
{
    public sealed class Activity : IDisposable
    {
        public List<Registration> Registrations { get; }
        
        /// <summary>
        ///     Path to output file, if saving one image per collection of satellite images.
        /// </summary>
        public string? OutputPath { get; set; }
       
        public Activity(IEnumerable<Registration> registrations) => Registrations = registrations.ToList();

        public void GetCropRange(out Range latitudeRange, out Range longitudeRange)
        {
            latitudeRange = new Range(
                Registrations.Max(a => a.LatitudeRange.Start),
                Registrations.Min(a => a.LatitudeRange.End));

            longitudeRange = GetVisibleLongitudeRange();
        }

        public Range GetVisibleLongitudeRange()
        {
            var sortedActivities = Registrations.OrderBy(p => p.OffsetX).ToList();

            return new Range(
                sortedActivities.First().LongitudeRange.Start,
                sortedActivities.Last().LongitudeRange.End
            ).UnwrapLongitude();
        }

        public async Task LoadAllAsync()
        {
            var tasks = Registrations.Select(i => i.LoadAsync());
            await Task.WhenAll(tasks);
        }

        public void Dispose() => Registrations.ForEach(registration => registration.Dispose());
    }
}