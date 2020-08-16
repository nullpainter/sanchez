using MathNet.Spatial.Units;

namespace Funhouse.Models.Angles
{
    /// <summary>
    ///     Convenience wrapper around a geostationary image scanning angle.
    /// </summary>
    public readonly struct ScanningAngle
    {
        public Angle X { get; }
        public Angle Y { get; }

        public ScanningAngle(Angle x, Angle y)
        {
            X = x;
            Y = y;
        }
    }
}