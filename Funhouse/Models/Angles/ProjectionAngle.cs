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

        /// <summary>
        ///     Converts a pixel y coordinate to a vertical projection angle.
        /// </summary>
        /// <param name="y"></param>
        /// <param name="height">height of image</param>
        /// <returns>projection angle</returns>
        public static double FromY(int y, int height) => y / (double) height * PI - Constants.PiOver2;

        public static int ToY(double angle, int height) => (int) Round((angle + Constants.PiOver2) * height / PI);


        /// <summary>
        ///     Converts a pixel x coordinate to a horizontal projection angle.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="width">width of image</param>
        /// <returns>projection angle</returns>
        public static double FromX(int x, int width) => x / (double) width * Constants.Pi2 - PI;

        public static int ToX(double angle, int width) => (int) Round((angle + PI) * width / Constants.Pi2);

        public PointF ToPixelCoordinates(int width, int height)
        {
            return new PointF(
                (float) (width * (X.Radians + PI) / (PI * 2)),
                (float) (height * (Y.Radians + Constants.PiOver2) / PI)
            );
        }

    }
}