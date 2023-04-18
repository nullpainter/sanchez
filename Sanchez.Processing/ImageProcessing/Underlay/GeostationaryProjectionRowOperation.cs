using System.Numerics;
using Sanchez.Processing.Extensions;
using Sanchez.Processing.Extensions.Images;
using Sanchez.Processing.Models;
using Sanchez.Processing.Projections;

namespace Sanchez.Processing.ImageProcessing.Underlay;

internal class GeostationaryProjectionRowOperation
{
    private readonly ImageBuffer _sourceBuffer;
    private readonly double _satelliteLongitude;
    private readonly double _satelliteHeight;
    private readonly RenderOptions _options;
    private readonly int _sourceWidth, _sourceHeight;

    public GeostationaryProjectionRowOperation(
        Image<Rgba32> underlay,
        double satelliteLongitude,
        double satelliteHeight,
        RenderOptions options)
    {
        _sourceBuffer = ImageBuffer.ToBuffer(underlay);
        _satelliteLongitude = satelliteLongitude;
        _satelliteHeight = satelliteHeight;
        _options = options;

        _sourceWidth = _sourceBuffer.Size.Width;
        _sourceHeight = _sourceBuffer.Size.Height;

        ArgumentNullException.ThrowIfNull(_options.ImageOffset);
    }


    public void Invoke(Span<Vector4> row, Point point)
    {
        var scanningY = _options.ImageOffset!.ToVerticalScanningAngle(point.Y);
        var verticalScanningCalculations = ReverseGeostationaryProjection.VerticalScanningCalculations(scanningY, _satelliteHeight);

        for (var x = 0; x < row.Length; x++)
        {
            var scanningX = _options.ImageOffset.ToHorizontalScanningAngle(x);
            ReverseGeostationaryProjection.ToLatitudeLongitude(scanningX, verticalScanningCalculations, _satelliteLongitude, out var latitude, out var longitude);

            if (double.IsNaN(longitude) || double.IsNaN(latitude))
            {
                row[x] = Vector4.Zero;
                continue;
            }

            var x1 = longitude.ScaleToWidthD(_sourceWidth).Limit(0, _sourceWidth);
            var y1 = latitude.ScaleToHeightD(_sourceHeight);

            row[x] = _sourceBuffer.GetInterpolatedPixel(x1, y1, _options.InterpolationType).ToVector4();
        }
    }
}