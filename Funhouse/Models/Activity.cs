using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Funhouse.Models.Projections;
using Range = Funhouse.Models.Angles.Range;

namespace Funhouse.Models
{
    public sealed class Activity : IDisposable
    {
        public List<Registration> Registrations { get; }
        private bool _disposed;

        public Activity(IEnumerable<Registration> registrations) => Registrations = registrations.ToList();

        public void GetCropRange(out Range latitudeRange, out Range longitudeRange)
        {
            latitudeRange = new Range(
                Registrations.Min(a => a.LatitudeRange.Start),
                Registrations.Max(a => a.LatitudeRange.End));

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

        public void Dispose()
        {
            if (!_disposed) return;
            Registrations.ForEach(registration => registration.Dispose());

            _disposed = true;
        }
    }
}