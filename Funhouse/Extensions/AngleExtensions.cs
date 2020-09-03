using System;
using MathNet.Numerics;

namespace Funhouse.Extensions
{
    public static class AngleExtensions
    {
        /// <summary>
        ///     Returns an angle which is within -180 and 180 degrees longitude, wrapping as required.
        /// </summary>
        /// <param name="angle">angle in radians</param>
        public static double NormaliseLongitude(this double angle) => angle.Limit(-Math.PI, Math.PI);

        // TODO note that these are all in radians
        public static float ScaleToWidthF(this double angle, int width) => (float) (width * (angle + Math.PI) / Constants.Pi2);
        public static float ScaleToHeightF(this double angle, int height) => (float) (height * (angle + Constants.PiOver2) / Math.PI);
        public static int ScaleToWidth(this double angle, int width) => (int) Math.Round(ScaleToWidthF(angle, width));
        public static int ScaleToHeight(this double angle, int height) => (int) Math.Round(ScaleToHeightF(angle, height));
    }
}