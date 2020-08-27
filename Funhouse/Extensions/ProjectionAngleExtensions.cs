using Funhouse.Models.Angles;
using MathNet.Spatial.Units;

namespace Funhouse.Extensions
{
    public static class ProjectionAngleExtensions
    {
        public static int ToX(this Angle angle, int width) => ProjectionAngle.ToX(angle, width);
        public static int ToY(this Angle angle, int height) => ProjectionAngle.ToY(angle, height);
    }
}