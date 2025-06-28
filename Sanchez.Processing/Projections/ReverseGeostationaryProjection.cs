using System.Runtime.CompilerServices;
using Sanchez.Processing.Extensions;
using static System.Math;
using static Sanchez.Processing.Models.Constants.Earth;

namespace Sanchez.Processing.Projections;

/// <remarks>
///     Calculations taken from https://www.goes-r.gov/users/docs/PUG-L1b-vol3.pdf, section 5.1.2.8.1
/// </remarks>
public static class ReverseGeostationaryProjection
{
    private const double RadiusPolarSquared = RadiusPolar * RadiusPolar;
    private const double RadiusEquatorSquared = RadiusEquator * RadiusEquator;
    
    // Pre-computed constant to avoid division in hot path
    private const double RadiusRatio = RadiusEquatorSquared / RadiusPolarSquared;

    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    public static VerticalScanningCalculations VerticalScanningCalculations(double scanningY, double satelliteHeight)
    {
        var cosY = Cos(scanningY);
        var sinY = Sin(scanningY);
        var adjustedHeight = satelliteHeight + RadiusEquator;

        // Calculate directly instead of multiple property assignments
        return new VerticalScanningCalculations
        {
            CosY = cosY,
            SinY = sinY,
            SatelliteHeight = adjustedHeight,
            C = adjustedHeight * adjustedHeight - RadiusEquatorSquared,
            T = cosY * cosY + RadiusRatio * sinY * sinY
        };
    }

    /// <summary>
    ///     Converts a scanning angle to latitude and longitude.
    /// </summary>
    /// <remarks>
    ///    The <see cref="o:ToGeodetic(double,VerticalScanningCalculations,Sanchez.Shared.Configuration.SatelliteDefinition,out double,out double)"/> method with
    ///     vertical scanning calculations should be used in preference if performing these calculations in bulk, as it avoids duplicate calculations.
    /// </remarks>
    /// <param name="scanningX">horizontal scanning angle in radians</param>
    /// <param name="scanningY">vertical scanning angle in radians</param>
    /// <param name="definition">satellite definition</param>
    /// <param name="latitude">calculated latitude in radians</param>
    /// <param name="longitude">calculated longitude in radians</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ToLatitudeLongitude(double scanningX, double scanningY, double satelliteLongitude, double satelliteHeight, out double latitude, out double longitude)
    {
        var verticalCalculations = VerticalScanningCalculations(scanningY, satelliteHeight);
        ToLatitudeLongitude(scanningX, verticalCalculations, satelliteLongitude, out latitude, out longitude);
    }

    /// <summary>
    ///     Converts a scanning angle to latitude and longitude.
    /// </summary>
    /// <param name="scanningX">horizontal scanning angle in radians</param>
    /// <param name="verticalScanningCalculations">vertical scanning calculations</param>
    /// <param name="satelliteLongitude">satellite longitude</param>
    /// <param name="latitude">calculated latitude in radians</param>
    /// <param name="longitude">calculated longitude in radians</param>
    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    public static void ToLatitudeLongitude(
        double scanningX, VerticalScanningCalculations verticalScanningCalculations, double satelliteLongitude, out double latitude, out double longitude)
    {
        var satelliteHeight = verticalScanningCalculations.SatelliteHeight;

        // Pre-compute trigonometric values
        var cosX = Cos(scanningX);
        var sinX = Sin(scanningX);
        var cosXSquared = cosX * cosX;

        var cosY = verticalScanningCalculations.CosY;
        var sinY = verticalScanningCalculations.SinY;
        var t = verticalScanningCalculations.T;
        var c = verticalScanningCalculations.C;

        // Calculate quadratic formula components
        var a = sinX * sinX + cosXSquared * t;
        var b = -2 * satelliteHeight * cosX * cosY;
        var discr = b * b - 4 * a * c;
        
        // Inline sqrt calculation for better performance
        var rs = (-b - Sqrt(discr)) / (2 * a);

        // Pre-compute these values once as they're used multiple times
        var sx = rs * cosX * cosY;
        var sy = -rs * sinX;
        var sz = rs * cosX * sinY;
        
        // Calculate distance term once
        var satMinusSx = satelliteHeight - sx;
        var distSqr = satMinusSx * satMinusSx + sy * sy;
        
        // Calculate latitude and longitude
        latitude = Atan(RadiusRatio * (sz / Sqrt(distSqr)));
        longitude = (satelliteLongitude - Atan(sy / satMinusSx)).NormaliseLongitude();
    }
}

/// <summary>
///     Intermediary calculations for latitude/longitude calculations.
/// </summary>
public struct VerticalScanningCalculations
{
    public double T { get; set; }
    public double CosY { get; init; }
    public double SinY { get; init; }
    public double C { get; set; }
    public double SatelliteHeight { get; init; }
}