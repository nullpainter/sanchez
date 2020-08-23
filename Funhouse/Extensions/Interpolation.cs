using SixLabors.ImageSharp;

namespace Funhouse.Extensions
{
    public static class Interpolation
    {
        /// <summary>
        ///     Linear interpolation between two values.
        /// </summary>
        /// <param name="start">first value</param>
        /// <param name="end">second value</param>
        /// <param name="amount">amount between 0.0 - 1.0</param>
        /// <returns>interpolated value</returns>
        public static float Lerp(float start, float end, float amount)
        {
            return start + (end - start) * amount;
        }

        /// <summary>
        ///     Bilinear interpolation.
        /// </summary>
        public static float Blerp(float c00, float c10, float c01, float c11, PointF position)
        {
            return Lerp(Lerp(c00, c10, position.X), Lerp(c01, c11, position.X), position.Y);
        }
    }
}