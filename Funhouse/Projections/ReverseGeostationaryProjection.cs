using System.Runtime.CompilerServices;
using Funhouse.Extensions;
using Funhouse.Models.Configuration;
using static System.Math;
using static Funhouse.Models.Constants.Earth;

namespace Funhouse.Projections
{
    /// <remarks>
    ///     Calculations taken from https://www.goes-r.gov/users/docs/PUG-L1b-vol3.pdf, section 5.1.2.8.1
    /// </remarks>
    public static class ReverseGeostationaryProjection
    {
        private const double RadiusPolarSquared = RadiusPolar * RadiusPolar;
        private const double RadiusEquatorSquared = RadiusEquator * RadiusEquator;

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static VerticalScanningCalculations VerticalScanningCalculations(double scanningY, SatelliteDefinition definition)
        {
            var calculations = new VerticalScanningCalculations
            {
                CosY = Cos(scanningY),
                SinY = Sin(scanningY),
                SatelliteHeight = definition.Height + RadiusEquator
            };

            calculations.C = calculations.SatelliteHeight * calculations.SatelliteHeight - RadiusEquatorSquared;
            calculations.T = calculations.CosY * calculations.CosY + RadiusEquatorSquared / RadiusPolarSquared * calculations.SinY * calculations.SinY;

            return calculations;
        }

        /// <summary>
        ///     Converts a scanning angle to latitude and longitude.
        /// </summary>
        /// <remarks>
        ///    The <see cref="o:ToGeodetic(double,VerticalScanningCalculations,Funhouse.Models.Configuration.SatelliteDefinition,out double,out double)"/> method with
        ///     vertical scanning calculations should be used in preference if performing these calculations in bulk, as it avoids duplicate calculations.
        /// </remarks>
        /// <param name="scanningX">horizontal scanning angle in radians</param>
        /// <param name="scanningY">vertical scanning angle in radians</param>
        /// <param name="definition">satellite definition</param>
        /// <param name="latitude">calculated latitude in radians</param>
        /// <param name="longitude">calculated longitude in radians</param>
        public static void ToLatitudeLongitude(double scanningX, double scanningY, SatelliteDefinition definition, out double latitude, out double longitude)
        {
            var verticalCalculations = VerticalScanningCalculations(scanningY, definition);
            ToLatitudeLongitude(scanningX, verticalCalculations, definition, out latitude, out longitude);
        }

        /// <summary>
        ///     Converts a scanning angle to latitude and longitude.
        /// </summary>
        /// <param name="scanningX">horizontal scanning angle in radians</param>
        /// <param name="verticalScanningCalculations">vertical scanning calculations</param>
        /// <param name="definition">satellite definition</param>
        /// <param name="latitude">calculated latitude in radians</param>
        /// <param name="longitude">calculated longitude in radians</param>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static void ToLatitudeLongitude(
            double scanningX, VerticalScanningCalculations verticalScanningCalculations, SatelliteDefinition definition, out double latitude, out double longitude)
        {
            var satelliteLongitude = definition.Longitude;
            var satelliteHeight = definition.Height + RadiusEquator;

            var l0 = satelliteLongitude;

            var cosX = Cos(scanningX);
            var sinX = Sin(scanningX);

            var cosY = verticalScanningCalculations.CosY;
            var sinY = verticalScanningCalculations.SinY;
            var t = verticalScanningCalculations.T;
            var c = verticalScanningCalculations.C;

            var a = sinX * sinX + cosX * cosX * t;
            var b = -2 * satelliteHeight * cosX * cosY;

            var rs = (-b - Sqrt(b * b - 4 * a * c)) / (2 * a);

            var sx = rs * cosX * cosY;
            var sy = -rs * sinX;
            var sz = rs * cosX * sinY;

            latitude = Atan(RadiusEquatorSquared / RadiusPolarSquared * (sz / Sqrt((satelliteHeight - sx) * (satelliteHeight - sx) + sy * sy)));
            longitude = (l0 - Atan(sy / (satelliteHeight - sx))).NormaliseLongitude();
        }
    }

    /// <summary>
    ///     Intermediary calculations for latitude/longitude calculations.
    /// </summary>
    public struct VerticalScanningCalculations
    {
        public double T { get; set; }
        public double CosY { get; set; }
        public double SinY { get; set; }
        public double C { get; set; }
        public double SatelliteHeight { get; set; }
    }
}