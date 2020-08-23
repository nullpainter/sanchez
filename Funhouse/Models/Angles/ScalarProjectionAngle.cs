using System;
using MathNet.Spatial.Units;

namespace Funhouse.Models.Angles
{
    /// <summary>
    ///     Convenience wrapper around an angle representing a one-dimensional projection.
    /// </summary>
    public readonly struct ScalarProjectionAngle
    {
        public Angle Angle { get; }

        public ScalarProjectionAngle(Angle angle)
        {
            Angle = angle;
        }

        public float ScaleToWidth(int width)
        {
            return (float) (width * (Angle.Radians + Math.PI) / (Math.PI * 2));
        }

        public float ScaleToHeight(int height)
        {
            return (float) (height * (Angle.Radians + Constants.HalfPi) / Math.PI);
        }

        public override string ToString()
        {
            return $"{Angle.Degrees}";
        }
    }
}