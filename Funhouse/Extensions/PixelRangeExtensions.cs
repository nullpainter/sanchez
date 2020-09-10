using System;
using Funhouse.Models;
using Funhouse.Models.Angles;
using Range = Funhouse.Models.Angles.Range;

namespace Funhouse.Extensions
{
    public static class PixelRangeExtensions
    {
        /// <summary>
        ///     Converts an angle range to a X pixel range, assuming the target image has X pixel coordinates mapping from -180 to 180 degrees.
        /// </summary>
        public static PixelRange ToPixelRangeX(this Range range, int width) => new PixelRange(range, angle => ToX(angle, width));

        /// <summary>
        ///     Converts an angle range to a Y pixel range, assuming the target image has Y pixel coordinates mapping from -90 to 90 degrees.
        /// </summary>
        public static PixelRange ToPixelRangeY(this Range range, int height) => new PixelRange(range, angle => ToY(angle, height));

        private static int ToY(double angle, int height) => (int) Math.Round((angle + Constants.PiOver2) * height / Math.PI);
        private static int ToX(double angle, int width) => (int) Math.Round((angle + Math.PI) * width / Constants.Pi2);
    }
}