using System.Runtime.CompilerServices;
using Funhouse.Models.Configuration;
using static System.Math;
using static Funhouse.Models.Constants.Earth;

namespace Funhouse.Projections
{
    /// <remarks>
    ///     Calculations taken from https://www.goes-r.gov/users/docs/PUG-L1b-vol3.pdf, section 5.1.2.8.1
    /// </remarks>
    public static class GeostationaryProjection
    {
        private const double RadiusPolarSquared = RadiusPolar * RadiusPolar;
        private const double RadiusEquatorSquared = RadiusEquator * RadiusEquator;

        public static LatitudeCalculations LatitudeCalculations(double latitude)
        {
            var geocentricLatitude = Atan(RadiusPolarSquared / RadiusEquatorSquared * Tan(latitude));
            var cosLatitude = Cos(geocentricLatitude);
            var sinLatitude = Sin(geocentricLatitude);

            var rc = RadiusPolar / Sqrt(1 - Eccentricity * Eccentricity * cosLatitude * cosLatitude);
            var sz = rc * sinLatitude;

            var calculations = new LatitudeCalculations
            {
                CosLatitude = cosLatitude,
                Rc = rc,
                Sz = sz,
                Sz2 = sz * sz,
            };

            calculations.RcCosLatitude = calculations.Rc * calculations.CosLatitude;
            calculations.RadiusRatioSz2 = RadiusEquatorSquared / RadiusPolarSquared * calculations.Sz2;

            return calculations;
        }

        /// <summary>
        ///     Converts a latitude and longitude to a scanning angle.
        /// </summary>
        /// <remarks>
        ///    The <see cref="o:ToScanningAngle(LatitudeCalculations,longitude,Funhouse.Models.Configuration.SatelliteDefinition,out double,out double)"/> method with
        ///     latitude calculations should be used in preference if performing these calculations in bulk, as it avoids duplicate calculations.
        /// </remarks>
        /// <param name="latitude">latitude in radians</param>
        /// <param name="longitude">longitude in radians</param>
        /// <param name="definition">satellite definition</param>
        /// <param name="scanningX">calculated horizontal scanning angle in radians</param>
        /// <param name="scanningY">calculated vertical scanning angle in radians</param>
        public static void ToScanningAngle(double latitude, double longitude, SatelliteDefinition definition, out double scanningX, out double scanningY)
        {
            var latitudeCalculations = LatitudeCalculations(latitude);
            ToScanningAngle(latitudeCalculations, longitude, definition, out scanningX, out scanningY);
        }

        /// <summary>
        ///     Converts a latitude and longitude to a geostationary image scanning angle.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static void ToScanningAngle(LatitudeCalculations latitudeCalculations, double longitude, SatelliteDefinition definition, out double scanningX, out double scanningY)
        {
            var satelliteLongitude = definition.Longitude;
            var satelliteHeight = definition.Height + RadiusEquator;

            var sx = satelliteHeight - latitudeCalculations.RcCosLatitude * Cos(longitude - satelliteLongitude);
            var sy = -latitudeCalculations.RcCosLatitude * Sin(longitude - satelliteLongitude);
            var sy2 = sy * sy;

            // Check if geodetic angle is visible from satellite 
            if (satelliteHeight * (satelliteHeight - sx) < sy2 + latitudeCalculations.RadiusRatioSz2)
            {
                scanningX = scanningY = double.NaN;
                return;
            }

            // Calculate (x,y) scanning angle
            scanningX = Asin(-sy / Sqrt(sx * sx + sy2 + latitudeCalculations.Sz2));
            scanningY = Atan(latitudeCalculations.Sz / sx);
        }
    }

    /// <summary>
    ///     Intermediary calculations for scanning angle calculations.
    /// </summary>
    public struct LatitudeCalculations
    {
        public double Rc { get; set; }
        public double RcCosLatitude { get; set; }
        public double CosLatitude { get; set; }
        public double Sz { get; set; }
        public double Sz2 { get; set; }
        public double RadiusRatioSz2 { get; set; }
    }
}