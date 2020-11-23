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

        /// <summary>
        ///     Whether satellite imagery is covering the entire Earth. This is performed by verifying that all satellites
        ///     have overlapping visible coverage.
        /// </summary>
        public bool IsFullEarthCoverage() => Registrations.All(r => r.LongitudeRange!.OverlappingLeft && r.LongitudeRange.OverlappingRight);

        public void GetCropRange(out Range latitudeRange, out Range longitudeRange)
        {
            latitudeRange = new Range(
                Registrations.Max(a => a.LatitudeRange!.Range.Start),
                Registrations.Min(a => a.LatitudeRange!.Range.End));

            longitudeRange = new Range(Angle.FromDegrees(-180), Angle.FromDegrees(180));
        }

        public async Task LoadAllAsync()
        {
            var tasks = Registrations.Select(i => i.LoadAsync());
            await Task.WhenAll(tasks);
        }

        public void Dispose() => Registrations.ForEach(registration => registration.Dispose());
    }
}