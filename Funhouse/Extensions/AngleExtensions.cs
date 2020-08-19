using System;
using MathNet.Spatial.Units;

namespace Funhouse.Extensions
{
    public static class AngleExtensions
    {
        public static Angle NormaliseLatitude(this Angle angle)
            => Angle.FromRadians(angle.Radians.Limit(-Math.PI / 2, Math.PI / 2));

        // TODO RENAME ME
        public static Angle NormaliseLongitude(this Angle angle)
            => Angle.FromRadians(angle.Radians.Limit(-Math.PI, Math.PI));

    }
}