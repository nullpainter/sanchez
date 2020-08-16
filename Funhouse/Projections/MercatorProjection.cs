using Funhouse.Models.Angles;
using MathNet.Spatial.Units;
using static System.Math;

namespace Funhouse.Projections
{
    public static class MercatorProjection
    {
        private static readonly double Com = 0.5 * Constants.Earth.Eccentricity;

        private static readonly double Pi2 = PI / 2.0;

        public static GeodeticAngle ToGeodetic(ProjectionAngle angle) => new GeodeticAngle(ToLatitude(angle.Y), ToLongitude(angle.X));

        private static Angle ToLongitude(Angle x) => Angle.FromDegrees(x.Degrees / Constants.Earth.RadiusEquator);

        private static Angle ToLatitude(Angle y)
        {
            var ts = Exp(-y.Radians / Constants.Earth.RadiusEquator);
            var phi = Pi2 - 2 * Atan(ts);
            var dphi = 1.0;
            var i = 0;
            while (Abs(dphi) > 0.000000001 && i < 15)
            {
                var con = Constants.Earth.Eccentricity * Sin(phi);
                dphi = Pi2 - 2 * Atan(ts * Pow((1.0 - con) / (1.0 + con), Com)) - phi;
                phi += dphi;
                i++;
            }

            return Angle.FromRadians(phi);
        }
    }
}