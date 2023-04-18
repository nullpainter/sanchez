using System.Collections.ObjectModel;
using System.Numerics;
using Sanchez.Processing.Models;
using SixLabors.ImageSharp.ColorSpaces;
using SixLabors.ImageSharp.ColorSpaces.Conversion;

namespace Sanchez.Processing.ImageProcessing.Atmosphere;

public class AtmosphereRowOperation
{
    /// <summary>
    ///     Size to increase image by due to atmosphere rendering.
    /// </summary>
    public const float ImageScaleFactor = 1.05f;
    
    private const float InnerOpacityScale = 0.7f;
    
    private readonly Image<Rgba32> _source;
    private readonly ReadOnlyCollection<CieLch> _gradient;

    /// <summary>
    ///     Semi-minor axis, squared.
    /// </summary>
    private readonly double _semiMinor2;

    /// <summary>
    ///     Semi-major axis, squared.
    /// </summary>
    private readonly double _semiMajor2;

    private readonly float _atmosphere;
    private readonly ColorSpaceConverter _converter;

    /// <summary>
    ///     Maximum distance from centre of Earth.
    /// </summary>
    private readonly double _maxDistance;
    private readonly float _opacity;

    /// <summary>
    ///     Adjustment factor for whether a point is inside the Earth, necessary to avoid single-pixel rounding errors
    ///     with Himawari-8's white background.
    /// </summary>
    private const double BorderRatio = 0.001d;

    /// <summary>
    ///     Amount of gradient outside Earth.
    /// </summary>
    private const float OuterGradientScale = 0.05f;

    /// <param name="source"></param>
    /// <param name="atmosphere">Amount of atmosphere to apply, from 0.0 - 1.0</param>
    /// <param name="opacity">Atmosphere opacity; from 0.0 - 1.0</param>
    /// <param name="gradient">Rayleigh gradient</param>
    public AtmosphereRowOperation(Image<Rgba32> source, float atmosphere, float opacity, ReadOnlyCollection<CieLch> gradient)
    {
        _source = source;
        _gradient = gradient;
        _opacity = opacity;

        var semiMajor = (int)Math.Round(source.Width / ImageScaleFactor) / 2d;
        var semiMinor = (int)Math.Round(source.Width / ImageScaleFactor) * (Constants.Earth.RadiusPolar / Constants.Earth.RadiusEquator) / 2d;

        _semiMinor2 = semiMinor * semiMinor;
        _semiMajor2 = semiMajor * semiMajor;

        _atmosphere = atmosphere;
        _converter = new ColorSpaceConverter();
        _maxDistance = Distance(_source.Width, _source.Height);
    }

    public void Invoke(Span<Vector4> row, Point value)
    {
        for (var x = 0; x < row.Length; x++)
        {
            var distance = Distance(x, value.Y);

            if (distance < 1 - _atmosphere) continue;

            var gradientValue = _gradient[(int)Math.Round(distance / _maxDistance * 255f)];
            var rgb = _converter.ToRgb(gradientValue);

            row[x].X = rgb.R;
            row[x].Y = rgb.G;
            row[x].Z = rgb.B;

            // Apply different alpha based on whether we're inside the Earth
            var alpha = distance <= 1
                ? (1 - (1 - distance) / _atmosphere) * _opacity * InnerOpacityScale
                : (1 - (float)(distance - 1) / OuterGradientScale) * _opacity;
            
            row[x].W = (float)alpha;
        }
    }

    /// <summary>
    ///     Distance from point to Earth's circumference. A value of 1 is on the circumference, and less
    ///     than 1 is inside the Earth.
    /// </summary>
    private double Distance(int x, int y)
    {
        var xDistance = x - _source.Width / 2d;
        var yDistance = y - _source.Height / 2d;

        return xDistance * xDistance / _semiMajor2 + yDistance * yDistance / _semiMinor2 + BorderRatio;
    }
}