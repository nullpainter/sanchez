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
    }
}