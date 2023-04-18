using System.Collections.Concurrent;
using System.Numerics;
using Sanchez.Processing.Extensions.Images;
using Sanchez.Processing.Helpers;
using Sanchez.Processing.Models;
using Sanchez.Processing.Models.Angles;
using Sanchez.Processing.Models.Configuration;
using Sanchez.Processing.Models.Projections;
using Sanchez.Processing.Projections;

namespace Sanchez.Processing.ImageProcessing.Projection;

public class ReprojectRowOperation
{
    
    private static readonly ConcurrentDictionary<int, LatitudeCalculations> LatitudeCalculationCache = new();
    
    private readonly Registration _registration;
    private readonly ImageBuffer _sourceBuffer;
    private readonly ImageOffset _imageOffset;
    private readonly RenderOptions _options;
    private readonly Image<Rgba32> _source;
    private readonly Image<Rgba32> _target;
    private readonly int _xOffset, _yOffset;
    private readonly AngleRange _latitudeRange, _longitudeRange;

    /// <summary>
    ///     Longitude outside of the longitude range where the image is blended.
    /// </summary>
    private readonly double _blendStartLongitude;

    private readonly double _blendEndLongitude;

    /// <summary>
    ///     Ratio of source image which has an alpha mask applied to blend it with overlapping images.
    /// </summary>
    private const float BlendRatio = 0.10f;

    public ReprojectRowOperation(
        Registration registration,
        Image<Rgba32> source,
        Image<Rgba32> target,
        int xOffset,
        int yOffset,
        RenderOptions options)
    {
        _registration = registration;
        _source = source;
        _target = target;
        _xOffset = xOffset;
        _yOffset = yOffset;
        _options = options;

        ArgumentNullException.ThrowIfNull(options.ImageOffset);
        ArgumentNullException.ThrowIfNull(registration.LongitudeRange);

        _sourceBuffer = ImageBuffer.ToBuffer(source);
        _imageOffset = options.ImageOffset;

        // Normalise longitude range so it doesn't wrap around the map
        _longitudeRange = registration.LongitudeRange.Value.Range.UnwrapLongitude();
        _latitudeRange = registration.Definition.LatitudeRange;

        // Calculate longitude range for blend
        var overlap = BlendRatio * (_longitudeRange.End - _longitudeRange.Start);
        _blendEndLongitude = _longitudeRange.End + overlap;
        _blendStartLongitude = _longitudeRange.Start - overlap;
    }
    public void Invoke(Span<Vector4> row, Point value)
    {
        var y = value.Y;
        
        // Calculate or retrieve the latitude calculation component of geostationary projection
        var latitudeCalculations = CalculateGeostationaryLatitude(y + _yOffset);

        // Convert image x,y to Mercator projection angle
        var targetWidth = _source.Width * 2;
        var projectionY = ProjectionAngleConverter.FromY(y + _yOffset, _source.Height);

        for (var x = 0; x < row.Length; x++)
        {
            var projectionX = ProjectionAngleConverter.FromX(x + _xOffset, targetWidth);

            // Convert latitude/longitude to geostationary scanning angle
            GeostationaryProjection.ToScanningAngle(latitudeCalculations, projectionX, _registration.Definition, out var scanningX, out var scanningY);

            // Map pixel from satellite image back to target image
            row[x] = GetTargetColour(scanningX, scanningY, projectionY, projectionX).ToVector4();
        }
    }

    /// <summary>
    ///     Performs latitude calculations as part of lat/long to scanning angle. These are cached in order to avoid
    ///     repeating latitude calculations for every pixel.
    /// </summary>
    private LatitudeCalculations CalculateGeostationaryLatitude(int y)
    {
        var target = _target;
        var yOffset = _yOffset;

        return LatitudeCalculationCache.GetOrAdd(y, _ =>
        {
            // Convert pixel row to latitude
            var projectionY = ProjectionAngleConverter.FromY(y, target.Height + yOffset * 2);

            // Perform and cache intermediary geostationary latitude calculations
            return GeostationaryProjection.LatitudeCalculations(projectionY);
        });
    }

    /// <summary>
    ///     Gets the target pixel colour for a given scanning angle and longitude. Images are blended on their right-hand
    ///     side with an alpha gradient in order to smooth the transition between overlapping images.
    /// </summary>
    /// <param name="scanningX">geostationary scanning angle</param>
    /// <param name="scanningY">geostationary scanning angle</param>
    /// <param name="latitude"></param>
    /// <param name="longitude"></param>
    /// <returns>interpolated pixel value from source image at the given scanning angle</returns>
    private Rgba32 GetTargetColour(double scanningX, double scanningY, double latitude, double longitude)
    {
        // Ignore pixels outside of disc and outside of crop region
        if (double.IsNaN(scanningX)
            || double.IsNaN(scanningY)
            || longitude < _blendStartLongitude
            || longitude > _blendEndLongitude
            || latitude > _latitudeRange.Start
            || latitude < _latitudeRange.End) return Constants.Transparent;

        // Map pixel from source image if not blending
        if (longitude <= _longitudeRange.End && longitude > _longitudeRange.Start) return InterpolatePixel(scanningX, scanningY);

        var pixel = InterpolatePixel(scanningX, scanningY);
        double alpha;

        // Blending, so identify target alpha
        if (longitude > _longitudeRange.End)
        {
            // Right blend
            alpha = 1 - (longitude - _longitudeRange.End) / (_blendEndLongitude - _longitudeRange.End);
        }
        else
        {
            // Left blend    
            alpha = 1 - (_longitudeRange.Start - longitude) / (_longitudeRange.Start - _blendStartLongitude);
        }

        // Calculate target pixel and blend
        pixel.A = (byte)Math.Round(alpha * pixel.A);

        return pixel;
    }

    /// <summary>
    ///     Returns the interpolated pixel at a specified scanning angle.
    /// </summary>
    private Rgba32 InterpolatePixel(double scanningX, double scanningY)
    {
        // Convert geostationary scanning angle to image coordinates
        _imageOffset.ToImageCoordinates(scanningX, scanningY, out var targetX, out var targetY);
        return _sourceBuffer.GetInterpolatedPixel(targetX, targetY, _options.InterpolationType);
    }
}