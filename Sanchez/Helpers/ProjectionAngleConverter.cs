using Sanchez.Models;
using static System.Math;

namespace Sanchez.Helpers
{
    public static class ProjectionAngleConverter
    {
        /// <summary>
        ///     Converts a pixel y coordinate to a vertical projection angle.
        /// </summary>
        /// <param name="y">y co-ordinate</param>
        /// <param name="height">height of image</param>
        /// <returns>projection angle</returns>
        public static double FromY(int y, int height) => (height - y) / (double) height * PI - Constants.PiOver2;

        /// <summary>
        ///     Converts a pixel x coordinate to a horizontal projection angle.
        /// </summary>
        /// <param name="x">x co-ordinate</param>
        /// <param name="width">width of image</param>
        /// <returns>projection angle</returns>
        public static double FromX(int x, int width) => x / (double) width * Constants.Pi2 - PI;

    }
}