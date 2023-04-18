using Sanchez.Processing.Extensions;

namespace Sanchez.Processing.Models.Angles;

/// <summary>
///     Convenience wrapper around an angle range.
/// </summary>
/// <param name="Start">start angle, in radians</param>
/// <param name="End">end angle, in radians</param>
public readonly record struct AngleRange(double Start, double End)
{
    /// <summary>
    ///     Constructs a angle range.
    /// </summary>
    /// <param name="start">start angle</param>
    /// <param name="end">end angle</param>
    public AngleRange(Angle start, Angle end) : this(start.Radians, end.Radians)
    {
    }

    /// <summary>
    ///     Unwraps a longitude range so the end angle is always greater than the start angle.
    ///     This is to simplify maths for projections which wrap around the standard -180 to 180 degrees.
    /// </summary>
    public AngleRange UnwrapLongitude() => End < Start ? this with { End = End + Constants.Pi2 } : this;

    public AngleRange NormaliseLongitude() => new(Start.NormaliseLongitude(), End.NormaliseLongitude());

    public static AngleRange operator +(AngleRange range, double amount) => new(range.Start + amount, range.End + amount);
    public static AngleRange operator -(AngleRange range, double amount) => new(range.Start - amount, range.End - amount);

    public override string ToString() => $"{Start} to {End}";
}