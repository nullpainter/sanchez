using Sanchez.Processing.Extensions;

namespace Sanchez.Processing.Models.Angles
{
    /// <summary>
    ///     Convenience wrapper around an angle range.
    /// </summary>
    public readonly struct Range
    {
        public double Start { get; }
        public double End { get; }

        /// <summary>
        ///     Constructs a angle range.
        /// </summary>
        /// <param name="start">start angle, in radians</param>
        /// <param name="end">end angle, in radians</param>
        public Range(double start, double end)
        {
            Start = start;
            End = end;
        }

        /// <summary>
        ///     Constructs a angle range.
        /// </summary>
        /// <param name="start">start angle</param>
        /// <param name="end">end angle</param>
        public Range(Angle start, Angle end)
        {
            Start = start.Radians;
            End = end.Radians;
        }

        /// <summary>
        ///     Unwraps a longitude range so the end angle is always greater than the start angle.
        ///     This is to simplify maths for projections which wrap around the standard -180 to 180 degrees.
        /// </summary>
        public Range UnwrapLongitude() => End < Start ? new Range(Start, End + Constants.Pi2) : this;
        
        public Range NormaliseLongitude() => new Range(Start.NormaliseLongitude(), End.NormaliseLongitude());

        public static Range operator +(Range range, double amount) => new Range(range.Start + amount, range.End + amount);
        public static Range operator -(Range range, double amount) => new Range(range.Start - amount, range.End - amount);
        
        public override string ToString() => $"{Start} to {End}";
    }
}