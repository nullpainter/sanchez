using Ardalis.GuardClauses;
using Funhouse.Extensions;
using Funhouse.Extensions.Images;
using Funhouse.Models;
using Funhouse.Projections;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;

namespace Funhouse.ImageProcessing.Underlay
{
    internal readonly struct GeostationaryProjectionRowOperation : IRowOperation
    {
        private readonly ImageBuffer _sourceBuffer;
        private readonly Image<Rgba32> _target;
        private readonly double _satelliteLongitude;
        private readonly double _satelliteHeight;
        private readonly RenderOptions _options;
        private readonly int _sourceWidth, _sourceHeight;

        public GeostationaryProjectionRowOperation(
            Image<Rgba32> underlay,
            Image<Rgba32> target,
            double satelliteLongitude,
            double satelliteHeight,
            RenderOptions options)
        {
            _sourceBuffer = ImageBuffer.ToBuffer(underlay);
            _target = target;
            _satelliteLongitude = satelliteLongitude;
            _satelliteHeight = satelliteHeight;
            _options = options;

            _sourceWidth = _sourceBuffer.Size.Width;
            _sourceHeight = _sourceBuffer.Size.Height;

            Guard.Against.Null(_options.ImageOffset, nameof(_options.ImageOffset));
        }

        public void Invoke(int y)
        {
            var span = _target.GetPixelRowSpan(y);
            var scanningY = _options.ImageOffset!.ToVerticalScanningAngle(y);

            var verticalScanningCalculations = ReverseGeostationaryProjection.VerticalScanningCalculations(scanningY, _satelliteHeight);

            for (var x = 0; x < span.Length; x++)
            {
                var scanningX = _options.ImageOffset.ToHorizontalScanningAngle(x);
                ReverseGeostationaryProjection.ToLatitudeLongitude(scanningX, verticalScanningCalculations, _satelliteLongitude, out var latitude, out var longitude);

                if (double.IsNaN(longitude) || double.IsNaN(latitude))
                {
                    span[x] = Color.Transparent;
                    continue;
                }

                var x1 = longitude.ScaleToWidthD(_sourceWidth).Limit(0, _sourceWidth);
                var y1 = _sourceHeight - latitude.ScaleToHeightD(_sourceHeight);

                span[x] = _sourceBuffer.GetInterpolatedPixel(x1, y1, _options.InterpolationType);
            }
        }
    }
}