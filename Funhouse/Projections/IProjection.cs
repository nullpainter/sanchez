using Funhouse.Models.Angles;
using MathNet.Spatial.Units;

namespace Funhouse.Projections
{
    public interface IProjection
    {
        GeodeticAngle ToGeodetic(ProjectionAngle angle);
        Angle ToLongitude(Angle x);
        Angle ToLatitude(Angle y);
    }
}