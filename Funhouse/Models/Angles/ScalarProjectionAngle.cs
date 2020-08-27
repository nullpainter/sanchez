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

        public ScalarProjectionAngle(Angle angle) => Angle = angle;

        public float ScaleToWidth(int width) => (float) (width * (Angle.Radians + Math.PI) / MathNet.Numerics.Constants.Pi2);
        public float ScaleToHeight(int height) => (float) (height * (Angle.Radians + MathNet.Numerics.Constants.PiOver2) / Math.PI);
        
        public override string ToString() => $"{Angle.Degrees}";
    }
}