using System;
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
        /// <summary>
        ///     Projected X angle, from -180 to 180 degrees.
        /// </summary>
        public Angle X { get; }

        /// <summary>
        ///     Projected Y angle, from -90 to 90 degrees.
        /// </summary>
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
                FromX(x, width),
                FromY(y, height)
            );
        }

        /// <summary>
        ///     Converts a pixel y coordinate to a vertical projection angle.
        /// </summary>
        /// <param name="y"></param>
        /// <param name="height">height of image</param>
        /// <returns>projection angle</returns>
        public static Angle FromY(int y, int height) => Angle.FromRadians(y / (double) height * PI - MathNet.Numerics.Constants.PiOver2);

        public static int ToY(Angle angle, int height) => (int)Round((angle.Radians + MathNet.Numerics.Constants.PiOver2) * height / PI);


        /// <summary>
        ///     Converts a pixel x coordinate to a horizontal projection angle.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="width">width of image</param>
        /// <returns>projection angle</returns>
        public static Angle FromX(int x, int width) => Angle.FromRadians(x / (double) width * MathNet.Numerics.Constants.Pi2 - PI);

        public static int ToX(Angle angle, int width) => (int) Round((angle.Radians + PI) * width / MathNet.Numerics.Constants.Pi2);

        public PointF ToPixelCoordinates(int width, int height)
        {
            return new PointF(
                (float) (width * (X.Radians + PI) / (PI * 2)),
                (float) (height * (Y.Radians + MathNet.Numerics.Constants.PiOver2) / PI)
            );
        }

        public override string ToString()
        {
            return $"{X.Degrees}, {Y.Degrees}";
        }
    }
}