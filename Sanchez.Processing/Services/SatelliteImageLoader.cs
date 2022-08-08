using Sanchez.Processing.Models;
using Sanchez.Processing.Models.Projections;
using Sanchez.Processing.Services.Filesystem;

namespace Sanchez.Processing.Services;

public interface ISatelliteImageLoader
{
    Activity RegisterImages(List<Registration> sourceRegistrations, DateTime? timestamp = null);
}

public class SatelliteImageLoader : ISatelliteImageLoader
{
    private readonly IImageMatcher _imageMatcher;

    public SatelliteImageLoader(IImageMatcher imageMatcher) => _imageMatcher = imageMatcher;

    public Activity RegisterImages(List<Registration> sourceRegistrations, DateTime? timestamp = null)
    {
        // If we are combining files by timestamp, locate all matching files based on the timestamp and tolerance
        var registrations = timestamp == null ? sourceRegistrations : _imageMatcher.FilterMatchingRegistrations(sourceRegistrations, timestamp.Value);
        return new Activity(registrations);
    }
}