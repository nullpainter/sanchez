using System;

namespace Sanchez.Models
{
    /// <summary>
    ///     Angle utility struct, extracted from Math.NET Numerics. 
    /// </summary>
    /// <remarks>
    ///    This was copied from Math.NET because the package isn't small, and we are only currently using this one struct,
    ///     and only in a few places. If and when we use the package in anger, we can remove this.
    /// </remarks>
    public readonly struct Angle 
    {
        /// <summary>
        ///     The value in radians.
        /// </summary>
        public readonly double Radians;

        /// <summary>
        ///     Initializes a new instance of the <see cref="T:Sanchez.Models.Angle" /> struct.
        /// </summary>
        /// <param name="radians">The value in Radians</param>
        private Angle(double radians) => Radians = radians;

        /// <summary>
        ///     Gets the value in degrees.
        /// </summary>
        public double Degrees => Radians * (180.0 / Math.PI);

        /// <summary>Creates a new instance of Angle.</summary>
        /// <param name="value">The value in degrees.</param>
        /// <returns> A new instance of the <see cref="T:Sanchez.Models.Angle" /> struct.</returns>
        public static Angle FromDegrees(double value) => new Angle(value * (Math.PI / 180.0));

        /// <summary>Creates a new instance of Angle.</summary>
        /// <param name="value">The value in radians.</param>
        /// <returns> A new instance of the <see cref="T:Sanchez.Models.Angle" /> struct.</returns>
        public static Angle FromRadians(double value) => new Angle(value);

        public override string ToString() => $"{Radians} rad, {Degrees} degrees";
    }
}