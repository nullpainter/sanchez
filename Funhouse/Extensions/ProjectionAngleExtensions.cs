using Funhouse.Models.Angles;

namespace Funhouse.Extensions
{
    public static class ProjectionAngleExtensions
    {
        public static int ToX(this double angle, int width) => ProjectionAngle.ToX(angle, width);
        public static int ToY(this double angle, int height) => ProjectionAngle.ToY(angle, height);
    }
}