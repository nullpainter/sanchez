using Ardalis.GuardClauses;
using Sanchez.Processing.Models;
using Sanchez.Processing.Models.Angles;
using Sanchez.Processing.Projections;

namespace Sanchez.Processing.Services
{
    public interface IVisibleRangeService
    {
        Range GetVisibleRange(Angle satelliteLongitude);
    }

    public class VisibleRangeService : IVisibleRangeService
    {
        private readonly RenderOptions _options;

        public VisibleRangeService(RenderOptions options) => _options = options;

        public Range GetVisibleRange(Angle satelliteLongitude)
        {
            Guard.Against.Null(_options.ImageOffset, nameof(_options.ImageOffset));

            // TOOD remove latitude stuff
            var scanningY = _options.ImageOffset!.ToVerticalScanningAngle(_options.ImageSize / 2); //FIXME

            // TODO remove satellite height from json / models - really not necessary 
            var verticalScanningCalculations = ReverseGeostationaryProjection.VerticalScanningCalculations(scanningY, Constants.Satellite.DefaultHeight);

            var minLongitude = GetMinLongitude(satelliteLongitude, verticalScanningCalculations);
            var maxLongitude = GetMaxLongitude(satelliteLongitude, verticalScanningCalculations);

            return new Range(minLongitude, maxLongitude);
        }

        private double GetMaxLongitude(Angle satelliteLongitude, VerticalScanningCalculations verticalScanningCalculations)
        {
            double maxLongitude = 0;
            for (var x = _options.ImageSize - 1; x > 0; x--)
            {
                var scanningX = _options.ImageOffset!.ToHorizontalScanningAngle(x);
                ReverseGeostationaryProjection.ToLatitudeLongitude(scanningX, verticalScanningCalculations, satelliteLongitude.Radians, out _, out var longitude);
                maxLongitude = longitude;

                if (!double.IsNaN(maxLongitude)) break;
            }

            return maxLongitude;
        }

        private double GetMinLongitude(Angle satelliteLongitude, VerticalScanningCalculations verticalScanningCalculations)
        {
            double minLongitude = 0;
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
}