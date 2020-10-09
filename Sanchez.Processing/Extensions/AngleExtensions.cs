using System;
using Sanchez.Processing.Models;

namespace Sanchez.Processing.Extensions
{
    public static class AngleExtensions
    {
        /// <summary>
        ///     Returns an angle which is within -180 and 180 degrees longitude, wrapping as required.
        /// </summary>
        /// <param name="angle">angle in radians</param>
        public static double NormaliseLongitude(this double angle) => angle.Limit(-Math.PI, Math.PI);
        
        /// <summary>
        ///     Scales an angle to a fractional pixel width, based on an equirectangular projection.
        /// </summary>
        /// <param name="angle">angle in radians</param>
        /// <param name="width">width</param>
        /// <returns>scaled angle</returns>
        public static double ScaleToWidthD(this double angle, int width) => width * (angle + Math.PI) / Constants.Pi2;
        
        /// <summary>
        ///     Scales an angle to an integer pixel width, based on an equirectangular projection.
        /// </summary>
        /// <param name="angle">angle in radians</param>
        /// <param name="width">width</param>
        /// <returns>scaled angle</returns>
        public static int ScaleToWidth(this double angle, int width) => (int) Math.Round(ScaleToWidthD(angle, width));
        
        /// <summary>
        ///     Scales an angle to a fractional pixel height, based on an equirectangular projection.
        /// </summary>
        /// <param name="angle">angle in radians</param>
        /// <param name="height">height</param>
        /// <returns>scaled angle</returns>
        public static double ScaleToHeightD(this double angle, int height) => height - height * (angle + Constants.PiOver2) / Math.PI;
        
        /// <summary>
        ///     Scales an angle to an integer pixel height, based on an equirectangular projection.
        /// </summary>
        /// <param name="angle">angle in radians</param>
        /// <param name="height">height</param>
        /// <returns>scaled angle</returns> 
        public static int ScaleToHeight(this double angle, int height) => (int) Math.Round(ScaleToHeightD(angle, height));
    }
}