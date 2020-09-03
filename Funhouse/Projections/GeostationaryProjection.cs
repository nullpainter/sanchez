using System.Runtime.CompilerServices;
using Funhouse.Models;
using Funhouse.Models.Configuration;
using static System.Math;
using static Funhouse.Models.Constants.Earth;

namespace Funhouse.Projections
{
    public struct LatitudeCalculations
    {
        public double Rc { get; set; }
        public double RcCosLatitude { get; set; }
        public double CosLatitude { get; set; }
        public double GeocentricLatitude { get; set; }
        public double SinLatitude { get; set; }
        public double Sz { get; set; }
        public double Sz2 { get; set; }
        public double RadiusRatioSz2 { get; set; }
    }

    public struct VerticalScanningCalculations
    {
        // TODO define what this is (what is it?)
        public double T { get; set; }
        public double CosY { get; set; }
        public double SinY { get; set; }
        public double C { get; set; }
        public double SatelliteHeight { get; set; }
    }

    /// <remarks>
    ///     Calculations taken from https://www.goes-r.gov/users/docs/PUG-L1b-vol3.pdf, section 5.1.2.8.1
    /// </remarks>
    public class GeostationaryProjection
    {
        private const double RadiusPolarSquared = RadiusPolar * RadiusPolar;
        private const double RadiusEquatorSquared = RadiusEquator * RadiusEquator;

        public static LatitudeCalculations LatitudeCalculations(double latitude)
        {
            var geocentricLatitude = Atan(RadiusPolarSquared / RadiusEquatorSquared * Tan(-latitude));
            var cosLatitude = Cos(geocentricLatitude);
            var sinLatitude = Sin(geocentricLatitude);

            var rc = RadiusPolar / Sqrt(1 - Eccentricity * Eccentricity * cosLatitude * cosLatitude);
            var sz = rc * sinLatitude;

            var calculations = new LatitudeCalculations
            {
                GeocentricLatitude = geocentricLatitude,
                CosLatitude = cosLatitude,
                SinLatitude = sinLatitude,
                Rc = rc,
                Sz = sz,
                Sz2 = sz * sz,
            };

            calculations.RcCosLatitude = calculations.Rc * calculations.CosLatitude;
            calculations.RadiusRatioSz2 = RadiusEquatorSquared / RadiusPolarSquared * calculations.Sz2;

            return calculations;
        }

        public void FromGeodetic(double lat, double lon, SatelliteDefinition definition, out double scanningX, out double scanningY)
            => ToScanningAngle(LatitudeCalculations(lat), lon, definition, out scanningX, out scanningY);

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


//  TODO add note to everywhere that scanning angle is in radians
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static void ToGeodetic(VerticalScanningCalculations verticalScanningCalculations, double scanningX, SatelliteDefinition definition, RenderOptions renderOptions, out double lat,
            out double lon)
        {
            var satelliteLongitude = definition.Longitude;
            var satelliteHeight = definition.Height + RadiusEquator;

            var l0 = satelliteLongitude;

            // TODO extract out the y coalculations are we're doing this all the time
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

            // TODO bodge. Not sure why we need to negate these. Is something inadvertently negated earlier?
            lat = -Atan(RadiusEquatorSquared / RadiusPolarSquared * (sz / Sqrt((satelliteHeight - sx) * (satelliteHeight - sx) + sy * sy)));
            lon = l0 - Atan(sy / (satelliteHeight - sx));
        }
    }
}