using System;
using System.Drawing;

namespace Sanchez.Extensions
{
    public static class ColorExtensions
    {
        public static Color? FromHexTriplet(this string triplet)
        {
            try
            {
                return ColorTranslator.FromHtml(triplet.PadLeft(7, '#'));
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}