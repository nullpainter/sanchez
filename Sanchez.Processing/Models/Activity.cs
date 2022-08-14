using Sanchez.Processing.Models.Angles;
using Sanchez.Processing.Models.Projections;

namespace Sanchez.Processing.Models;

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
    public bool IsFullEarthCoverage() => Registrations.All(r => r.LongitudeRange!.Value.OverlappingLeft && r.LongitudeRange.Value.OverlappingRight);

    public void GetCropRange(out AngleRange latitudeRange, out AngleRange longitudeRange)
    {
        latitudeRange = new AngleRange(
            Registrations.Max(a => a.LatitudeRange!.Value.Range.Start),
            Registrations.Min(a => a.LatitudeRange!.Value.Range.End));

        longitudeRange = new AngleRange(Angle.FromDegrees(-180), Angle.FromDegrees(180));
    }

    public Task LoadAllAsync(CancellationToken ct = default)
    {
        var tasks = Registrations.Select(i => i.LoadAsync(ct));
        return Task.WhenAll(tasks);
    }

    public void Dispose() => Registrations.ForEach(registration => registration.Dispose());
}