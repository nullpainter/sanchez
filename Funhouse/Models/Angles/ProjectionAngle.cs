using MathNet.Spatial.Units;
using SixLabors.ImageSharp;
using static System.Math;

namespace Funhouse.Models.Angles
{
    /// <summary>
    ///     Convenience wrapper around an angle representing a two-dimensional projection.
    /// </summary>
    public readonly struct ProjectionAngle
    {
        public Angle X { get; }
        public Angle Y { get; }

        public ProjectionAngle(Angle x, Angle y)
        {
            X = x;
            Y = y;
        }

        public static ProjectionAngle FromPixelCoordinates(Point pixel, int width, int height)
        {
            var (x, y) = pixel;

            return new ProjectionAngle(
                Angle.FromRadians(x / (double) width * PI * 2 - PI),
                Angle.FromRadians(y / (double) height * PI - PI / 2)
            );
        }

        public PointF ToPixelCoordinates(int width, int height)
        {
            return new PointF(
                (float) (width * (X.Radians + PI) / (PI * 2)),
                (float) (height * (Y.Radians + PI / 2) / PI)
            );
        }
    }
}