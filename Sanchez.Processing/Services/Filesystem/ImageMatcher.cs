using Microsoft.Extensions.Logging;
using Sanchez.Processing.Models;
using Sanchez.Processing.Models.Projections;

namespace Sanchez.Processing.Services.Filesystem;

public interface IImageMatcher
{
    List<Registration> FilterMatchingRegistrations(IEnumerable<Registration> registrations, DateTime targetTimestamp);
}

public class ImageMatcher : IImageMatcher
{
    private readonly ILogger<ImageMatcher> _logger;
    private readonly RenderOptions _options;

    public ImageMatcher(ILogger<ImageMatcher> logger, RenderOptions options)
    {
        _logger = logger;
        _options = options;
    }

    public List<Registration> FilterMatchingRegistrations(IEnumerable<Registration> registrations, DateTime targetTimestamp)
    {
        _logger.LogInformation("Searching for images in {Path} with a timestamp of {Minutes} minutes tolerance to {Target}",
            _options.SourcePath, _options.Tolerance.TotalMinutes, targetTimestamp);

        // Return the closest match per satellite
        var matched = GetMatchedFiles(registrations, targetTimestamp);
        return matched
            .GroupBy(m => m.Registration.Definition)
            .DistinctBy(entry => entry.Key.Longitude)
            .Select(entry => entry
                .OrderBy(e => e.Deviation)
                .First()
                .Registration)
            .ToList();
    }

    private IEnumerable<SatelliteMatch> GetMatchedFiles(IEnumerable<Registration> registrations, DateTime targetTimestamp)
    {
        var tolerance = _options.Tolerance;

        // Add files that have a timestamp within the tolerance of the selected time
        var matches =
            from registration in registrations.Where(r => r.Timestamp != null)
            let deviation = Math.Abs((targetTimestamp - registration.Timestamp!.Value).TotalMilliseconds)
            where deviation < tolerance.TotalMilliseconds
            select new SatelliteMatch(registration, deviation);

        return matches;
    }

    private sealed class SatelliteMatch
    {
        public SatelliteMatch(Registration registration, double deviation)
        {
            Registration = registration;
            Deviation = deviation;
        }

        internal Registration Registration { get; }
        internal double Deviation { get; }
    }
}