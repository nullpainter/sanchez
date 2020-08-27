using System;
using MathNet.Spatial.Units;

namespace Funhouse.Extensions
{
    public static class AngleExtensions
    {
        /// <summary>
        ///     Returns an angle which is within -180 and 180 degrees longitude, wrapping as required.
        /// </summary>
        public static Angle NormaliseLongitude(this Angle angle)
        {
            return Angle.FromRadians(angle.Radians.Limit(-Math.PI, Math.PI));
        }
        
        
        public static int ScaleToWidth(this Angle angle, int width) => (int) Math.Round(width * (angle.Radians + Math.PI) / MathNet.Numerics.Constants.Pi2);
        public static int ScaleToHeight(this Angle angle, int height) => (int) Math.Round(height * (angle.Radians + MathNet.Numerics.Constants.PiOver2) / Math.PI);

    }
}