using System.Runtime.CompilerServices;
using Sanchez.Processing.Models.Configuration;
using static System.Math;
using static Sanchez.Processing.Models.Constants.Earth;

namespace Sanchez.Processing.Projections;

/// <remarks>
///     Calculations taken from https://www.goes-r.gov/users/docs/PUG-L1b-vol3.pdf, section 5.1.2.8.1
/// </remarks>
public static class GeostationaryProjection
{
    private const double RadiusPolarSquared = RadiusPolar * RadiusPolar;
    private const double RadiusEquatorSquared = RadiusEquator * RadiusEquator;
    
    // Pre-computed constant to avoid division in hot path
    private const double RadiusRatio = RadiusEquatorSquared / RadiusPolarSquared;
    
    // Pre-computed constant for latitude calculations
    private const double EccentricitySquared = Eccentricity * Eccentricity;
    private const double LatitudeScaleFactor = RadiusPolarSquared / RadiusEquatorSquared;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LatitudeCalculations LatitudeCalculations(double latitude)
    {
        // Pre-compute values that are used multiple times
        var geocentricLatitude = Atan(LatitudeScaleFactor * Tan(latitude));
        var cosLatitude = Cos(geocentricLatitude);
        var sinLatitude = Sin(geocentricLatitude);
        
        // Cache the cosine squared value which is used in multiple places
        var cosLatitudeSquared = cosLatitude * cosLatitude;

        var rc = RadiusPolar / Sqrt(1 - EccentricitySquared * cosLatitudeSquared);
        var sz = rc * sinLatitude;
        var sz2 = sz * sz;

        // Fill out the struct all at once to avoid unnecessary assignments
        return new LatitudeCalculations
        {
            CosLatitude = cosLatitude,
            Rc = rc,
            Sz = sz,
            Sz2 = sz2,
            RcCosLatitude = rc * cosLatitude,
            RadiusRatioSz2 = RadiusRatio * sz2
        };
    }

    /// <summary>
    ///     Converts a latitude and longitude to a scanning angle.
    /// </summary>
    /// <remarks>
    ///    The <see cref="o:ToScanningAngle(LatitudeCalculations,longitude,Sanchez.Shared.Configuration.SatelliteDefinition,out double,out double)"/> method with
    ///     latitude calculations should be used in preference if performing these calculations in bulk, as it avoids duplicate calculations.
    /// </remarks>
    /// <param name="latitude">latitude in radians</param>
    /// <param name="longitude">longitude in radians</param>
    /// <param name="definition">satellite definition</param>
    /// <param name="scanningX">calculated horizontal scanning angle in radians</param>
    /// <param name="scanningY">calculated vertical scanning angle in radians</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ToScanningAngle(double latitude, double longitude, SatelliteDefinition definition, out double scanningX, out double scanningY)
    {
        var latitudeCalculations = LatitudeCalculations(latitude);
        ToScanningAngle(latitudeCalculations, longitude, definition, out scanningX, out scanningY);
    }

    /// <summary>
    ///     Converts a latitude and longitude to a geostationary image scanning angle.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    public static void ToScanningAngle(LatitudeCalculations latitudeCalculations, double longitude, SatelliteDefinition definition, out double scanningX, out double scanningY)
    {
        var satelliteLongitude = definition.Longitude;
        var satelliteHeight = definition.Height + RadiusEquator;

        // Calculate longitude difference just once
        var longitudeDifference = longitude - satelliteLongitude;
        var cosLongDiff = Cos(longitudeDifference);
        var sinLongDiff = Sin(longitudeDifference);
        
        var rcCosLat = latitudeCalculations.RcCosLatitude;
        
        var sx = satelliteHeight - rcCosLat * cosLongDiff;
        var sy = -rcCosLat * sinLongDiff;
        var sy2 = sy * sy;

        // Check if geodetic angle is visible from satellite 
        if (satelliteHeight * (satelliteHeight - sx) < sy2 + latitudeCalculations.RadiusRatioSz2)
        {
            scanningX = scanningY = double.NaN;
            return;
        }

        // Calculate (x,y) scanning angle
        // Pre-compute the denominator for scanning angle calculations
        var sqrtTerm = Sqrt(sx * sx + sy2 + latitudeCalculations.Sz2);
        
        scanningX = Asin(-sy / sqrtTerm);
        scanningY = Atan(latitudeCalculations.Sz / sx);
    }
}

/// <summary>
///     Intermediary calculations for scanning angle calculations.
/// </summary>
public struct LatitudeCalculations
{
    public double Rc { get; init; }
    public double RcCosLatitude { get; set; }
    public double CosLatitude { get; init; }
    public double Sz { get; init; }
    public double Sz2 { get; init; }
    public double RadiusRatioSz2 { get; set; }
}