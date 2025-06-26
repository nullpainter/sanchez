using Microsoft.Extensions.Logging;
using Sanchez.Processing.Models;
using Sanchez.Processing.Models.Projections;

namespace Sanchez.Processing.Services.Filesystem;

public interface IImageMatcher
{
    List<Registration> FilterMatchingRegistrations(IEnumerable<Registration> registrations, DateTime targetTimestamp);
}

public class ImageMatcher(ILogger<ImageMatcher> logger, RenderOptions options) : IImageMatcher
{
    public List<Registration> FilterMatchingRegistrations(IEnumerable<Registration> registrations, DateTime targetTimestamp)
    {
        logger.LogInformation("Searching for images in {Path} with a timestamp of {Minutes} minutes tolerance to {Target}",
            options.SourcePath, options.Tolerance.TotalMinutes, targetTimestamp);

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
        var tolerance = options.Tolerance;

        // Add files that have a timestamp within the tolerance of the selected time
        var matches =
            from registration in registrations.Where(r => r.Timestamp != null)
            let deviation = Math.Abs((targetTimestamp - registration.Timestamp!.Value).TotalMilliseconds)
            where deviation < tolerance.TotalMilliseconds
            select new SatelliteMatch(registration, deviation);

        return matches;
    }

    private sealed class SatelliteMatch(Registration registration, double deviation)
    {
        internal Registration Registration { get; } = registration;
        internal double Deviation { get; } = deviation;
    }
}