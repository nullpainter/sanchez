using Funhouse.Extensions;

namespace Funhouse.Models.Angles
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
        ///     Unwraps a longitude range so the end angle is always greater than the start angle.
        ///     This is to simplify maths for projections which wrap around the standard -180 to 180 degrees.
        /// </summary>
        public Range UnwrapLongitude() => End < Start ? new Range(Start, End+ MathNet.Numerics.Constants.Pi2) : this;

        public Range NormaliseLongitude() => new Range(Start.NormaliseLongitude(), End.NormaliseLongitude());

        public static Range operator +(Range range, double amount) => new Range(range.Start + amount, range.End + amount);

        public override string ToString()
        {
            return $"{Start} to {End}";
        }
    }
}