using Sanchez.Processing.Models;
using Sanchez.Processing.Models.Angles;
using Sanchez.Processing.Projections;

namespace Sanchez.Processing.Services;

public interface IVisibleRangeService
{
    /// <summary>
    ///     Calculates the minimum and maximum longitude of imagery visible to a geostationary satellite.
    /// </summary>
    /// <param name="satelliteLongitude">longitude of satellite</param>
    AngleRange GetVisibleRange(Angle satelliteLongitude);
}

public class VisibleRangeService : IVisibleRangeService
{
    private readonly RenderOptions _options;

    public VisibleRangeService(RenderOptions options) => _options = options;

    /// <summary>
    ///     Calculates the minimum and maximum longitude of imagery visible to a geostationary satellite.
    /// </summary>
    /// <param name="satelliteLongitude">longitude of satellite</param>
    public AngleRange GetVisibleRange(Angle satelliteLongitude)
    {
        ArgumentNullException.ThrowIfNull(_options.ImageOffset);

        var scanningY = _options.ImageOffset!.ToVerticalScanningAngle(_options.ImageSize / 2);
        var verticalScanningCalculations = ReverseGeostationaryProjection.VerticalScanningCalculations(scanningY, Constants.Satellite.DefaultHeight);

        var minLongitude = GetMinLongitude(satelliteLongitude, verticalScanningCalculations);
        var maxLongitude = GetMaxLongitude(satelliteLongitude, verticalScanningCalculations);

        return new AngleRange(minLongitude, maxLongitude);
    }

    /// <summary>
    ///     Brute force calculation of maximum longitude visible to a geostationary satellite.
    /// </summary>
    private double GetMaxLongitude(Angle satelliteLongitude, VerticalScanningCalculations verticalScanningCalculations)
    {
        double maxLongitude = 0;

        // Projection returns double.NaN if a scanning angle doesn't correspond to a latitude and longitude visible to the
        // satellite. The maximum longitude is calculated in a brute force manner by scanning backwards from the maximum x
        // coordinate until a valid longitude is identified.
        for (var x = _options.ImageSize - 1; x > 0; x--)
        {
            var scanningX = _options.ImageOffset!.ToHorizontalScanningAngle(x);
            ReverseGeostationaryProjection.ToLatitudeLongitude(scanningX, verticalScanningCalculations, satelliteLongitude.Radians, out _, out var longitude);
            maxLongitude = longitude;

            if (!double.IsNaN(maxLongitude)) break;
        }

        return maxLongitude;
    }

    /// <summary>
    ///     Brute force calculation of minimum longitude visible to a geostationary satellite.
    /// </summary>
    private double GetMinLongitude(Angle satelliteLongitude, VerticalScanningCalculations verticalScanningCalculations)
    {
        double minLongitude = 0;

        // Projection returns double.NaN if a scanning angle doesn't correspond to a latitude and longitude visible to the
        // satellite. The minimum longitude is calculated in a brute force manner by scanning from the minimum x coordinate
        // until a valid longitude is identified.
        for (var x = 0; x < _options.ImageSize; x++)
        {
            var scanningX = _options.ImageOffset!.ToHorizontalScanningAngle(x);
            ReverseGeostationaryProjection.ToLatitudeLongitude(scanningX, verticalScanningCalculations, satelliteLongitude.Radians, out _, out var longitude);
            minLongitude = longitude;

            if (!double.IsNaN(longitude)) break;
        }

        return minLongitude;
    }
}