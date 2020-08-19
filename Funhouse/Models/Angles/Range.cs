using System;
using Funhouse.Extensions;
using MathNet.Spatial.Units;

namespace Funhouse.Models.Angles
{
    /// <summary>
    ///     Convenience wrapper around an angle range.
    /// </summary>
    public readonly struct Range
    {
        public Angle Start { get; }
        public Angle End { get; }

        public Range(Range other)
        {
            Start = Angle.FromRadians(other.Start.Radians);
            End = Angle.FromRadians(other.End.Radians);
        }

        public Range(Angle start, Angle end)
        {
            Start = start;
            End = end;
        }
       
        /// <summary>
        ///     Normalises a longitude range so the end angle is always greater than the start angle.
        ///     This is to simplify maths for projections which wrap around the standard -180 to 180 degrees.
        ///     
        /// </summary>
        public Range UnwrapLongitude() => End < Start ? new Range(Start, Angle.FromRadians(End.Radians + Math.PI * 2)) : this;

        public Range NormaliseLongitude() => new Range(Start.NormaliseLongitude(), End.NormaliseLongitude());
        
        public static Range operator +(Range range, Angle amount) => new Range(range.Start + amount, range.End + amount);
    }
}