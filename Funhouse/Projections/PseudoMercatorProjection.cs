using Funhouse.Models;
using Funhouse.Models.Angles;
using MathNet.Spatial.Units;
using static System.Math;

namespace Funhouse.Projections
{
    public class PseudoMercatorProjection : IProjection
    {
        public GeodeticAngle ToGeodetic(ProjectionAngle angle) => new GeodeticAngle(ToLatitude(angle.Y), ToLongitude(angle.X));

        public Angle ToLatitude(Angle y)
        {
            var phi = Constants.HalfPi - 2 * Atan(Exp(-y.Radians / Constants.Earth.RadiusEquator));
            return Angle.FromRadians(phi * Constants.Earth.RadiusEquator);
        }

        public Angle ToLongitude(Angle x) => x;
    }
}