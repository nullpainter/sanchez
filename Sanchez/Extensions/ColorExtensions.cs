﻿using SixLabors.ImageSharp;

 namespace Sanchez.Extensions
{
    public static class ColorExtensions
    {
        /// <summary>
        ///     Converts a HTML hex string to a <see cref="Color" />.
        ///
        /// Supported ImageSharp triplet formats are #xxx, #xxxxxx, and #xxxxxxxx hex formats with or without the leading #.
        /// </summary>
        /// <param name="hex">triplet to convert</param>
        /// <returns>colour, or <c>null</c> if unable to parse</returns>
        public static Color? FromHexString(this string hex)
        {
            if (Color.TryParseHex(hex, out var result)) return result;
            return null;
        }
    }
}