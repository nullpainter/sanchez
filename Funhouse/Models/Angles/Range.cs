using MathNet.Spatial.Units;

namespace Funhouse.Models.Angles
{
    /// <summary>
    ///     Convenience wrapper around an angle range.
    /// </summary>
    public struct Range
    {
        public Angle Start { get; set; }
        public Angle End { get; set; }

        public Range(Angle start, Angle end)
        {
            Start = start;
            End = end;
        }
    }
}