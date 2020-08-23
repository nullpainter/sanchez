using Funhouse.Models;
using Funhouse.Models.Angles;
using MathNet.Spatial.Units;
using static System.Math;

namespace Funhouse.Projections
{
    public class MercatorProjection : IProjection
    {
        private const double Com = 0.5 * Constants.Earth.Eccentricity;

        public GeodeticAngle ToGeodetic(ProjectionAngle angle) => new GeodeticAngle(ToLatitude(angle.Y), ToLongitude(angle.X));

        public Angle ToLongitude(Angle x) => x;

        public Angle ToLatitude(Angle y)
        {
            var ts = Exp(-y.Radians / Constants.Earth.RadiusEquator);
            var phi = Constants.HalfPi - 2 * Atan(ts);

            var dphi = 1.0;
            var i = 0;

            while (Abs(dphi) > 0.000000001 && i < 15)
            {
                var con = Constants.Earth.Eccentricity * Sin(phi);
                dphi = Constants.HalfPi - 2 * Atan(ts * Pow((1.0 - con) / (1.0 + con), Com)) - phi;
                phi += dphi;
                i++;
            }

            return Angle.FromRadians(phi * Constants.Earth.RadiusEquator);
        }
    }
}