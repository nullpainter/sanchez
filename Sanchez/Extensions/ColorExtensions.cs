using SixLabors.ImageSharp;

namespace Sanchez.Extensions
{
    public static class ColorExtensions
    {
        /// <summary>
        ///     Converts a HTML hex triplet of the form <c>#ffffff</c> or <c>ffffff</c> to a
        ///     <see cref="Color" />.
        /// </summary>
        /// <param name="triplet">triplet to convert</param>
        /// <returns>colour, or <c>null</c> if unable to parse</returns>
        public static Color? FromHexTriplet(this string triplet)
        {
            if (Color.TryParseHex(triplet, out Color result))
            {
                return result;
            }

            return null;
        }
    }
}