using Funhouse.Models;
using static System.Math;

namespace Funhouse.Helpers
{
    public static class ProjectionAngleConverter
    {
        /// <summary>
        ///     Converts a pixel y coordinate to a vertical projection angle.
        /// </summary>
        /// <param name="y"></param>
        /// <param name="height">height of image</param>
        /// <returns>projection angle</returns>
        public static double FromY(int y, int height) => y / (double) height * PI - Constants.PiOver2;

        /// <summary>
        ///     Converts a pixel x coordinate to a horizontal projection angle.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="width">width of image</param>
        /// <returns>projection angle</returns>
        public static double FromX(int x, int width) => x / (double) width * Constants.Pi2 - PI;

    }
}