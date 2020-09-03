using Funhouse.Extensions;
using Funhouse.Extensions.Images;
using Funhouse.Models;
using Funhouse.Models.Configuration;
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
        private readonly SatelliteDefinition _definition;
        private readonly RenderOptions _options;
        private readonly int _sourceWidth, _sourceHeight;

        public GeostationaryProjectionRowOperation(Image<Rgba32> underlay,
            Image<Rgba32> target,
            SatelliteDefinition definition,
            RenderOptions options)
        {
            _sourceBuffer = ImageBuffer.ToBuffer(underlay);
            _target = target;
            _definition = definition;
            _options = options;

            _sourceWidth = _sourceBuffer.Size.Width;
            _sourceHeight = _sourceBuffer.Size.Height;
        }

        public void Invoke(int y)
        {
            var span = _target.GetPixelRowSpan(y);
            var scanningY = _options.ImageOffset.ToVerticalScanningAngle(y);
            
            var verticalScanningCalculations = GeostationaryProjection.VerticalScanningCalculations(scanningY, _definition);

            for (var x = 0; x < span.Length; x++)
            {
                var scanningX = _options.ImageOffset.ToHorizontalScanningAngle(x);
                GeostationaryProjection.ToGeodetic(verticalScanningCalculations, scanningX, _definition, _options, out var lat, out var lon);

                if (double.IsNaN(lon) || double.IsNaN(lat))
                {
                    span[x] = Color.Transparent;
                    continue;
                }

                // TODO why do we need to negate the y coordinate?
                var x1 = lon.ScaleToWidthF(_sourceWidth).Limit(0, _sourceWidth);
                var y1 = _sourceHeight - lat.ScaleToHeight(_sourceHeight);

                span[x] =  _sourceBuffer.GetInterpolatedPixel(x1, y1, _options.InterpolationType);
            }
        }
    }
}