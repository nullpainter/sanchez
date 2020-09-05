using System;

namespace Funhouse.Models
{
    /// <summary>
    ///     Angle utility struct, extracted from Math.NET Numerics. 
    /// </summary>
    /// <remarks>
    ///    This was copied from Math.NET because the package isn't small, and we are only currently using this one struct,
    ///     and only in a few places. If and when we use the package in anger, we can remove this.
    /// </remarks>
    public readonly struct Angle : IEquatable<Angle>
    {
        /// <summary>
        ///     The value in radians.
        /// </summary>
        public readonly double Radians;

        /// <summary>
        ///     Initializes a new instance of the <see cref="T:Funhouse.Models.Angle" /> struct.
        /// </summary>
        /// <param name="radians">The value in Radians</param>
        private Angle(double radians) => Radians = radians;

        /// <summary>
        ///     Gets the value in degrees.
        /// </summary>
        public double Degrees => Radians * (180.0 / Math.PI);

        /// <summary>
        ///     Returns a value that indicates whether two specified Angles are equal.
        /// </summary>
        /// <param name="left">The first angle to compare</param>
        /// <param name="right">The second angle to compare</param>
        /// <returns>True if the angles are the same; otherwise false.</returns>
        public static bool operator ==(Angle left, Angle right) => left.Equals(right);

        /// <summary>
        ///     Returns a value that indicates whether two specified Angles are not equal.
        /// </summary>
        /// <param name="left">The first angle to compare</param>
        /// <param name="right">The second angle to compare</param>
        /// <returns>True if the angles are different; otherwise false.</returns>
        public static bool operator !=(Angle left, Angle right) => !left.Equals(right);

        /// <summary>
        ///     Returns a value that indicates if a specified Angles is less than another.
        /// </summary>
        /// <param name="left">The first angle to compare</param>
        /// <param name="right">The second angle to compare</param>
        /// <returns>True if the first angle is less than the second angle; otherwise false.</returns>
        public static bool operator <(Angle left, Angle right) => left.Radians < right.Radians;

        /// <summary>
        ///     Returns a value that indicates if a specified Angles is greater than another.
        /// </summary>
        /// <param name="left">The first angle to compare</param>
        /// <param name="right">The second angle to compare</param>
        /// <returns>True if the first angle is greater than the second angle; otherwise false.</returns>
        public static bool operator >(Angle left, Angle right) => left.Radians > right.Radians;

        /// <summary>
        ///     Returns a value that indicates if a specified Angles is less than or equal to another.
        /// </summary>
        /// <param name="left">The first angle to compare</param>
        /// <param name="right">The second angle to compare</param>
        /// <returns>True if the first angle is less than or equal to the second angle; otherwise false.</returns>
        public static bool operator <=(Angle left, Angle right) => left.Radians <= right.Radians;

        /// <summary>
        ///     Returns a value that indicates if a specified Angles is greater than or equal to another.
        /// </summary>
        /// <param name="left">The first angle to compare</param>
        /// <param name="right">The second angle to compare</param>
        /// <returns>True if the first angle is greater than or equal to the second angle; otherwise false.</returns>
        public static bool operator >=(Angle left, Angle right) => left.Radians >= right.Radians;

        /// <summary>
        ///     Multiplies an Angle by a scalar.
        /// </summary>
        /// <param name="left">The scalar.</param>
        /// <param name="right">The angle.</param>
        /// <returns>A new angle equal to the product of the angle and the scalar.</returns>
        public static Angle operator *(double left, Angle right) => new Angle(left * right.Radians);

        /// <summary>
        ///     Multiplies an Angle by a scalar.
        /// </summary>
        /// <param name="left">The angle.</param>
        /// <param name="right">The scalar.</param>
        /// <returns>A new angle equal to the product of the angle and the scalar.</returns>
        public static Angle operator *(Angle left, double right) => new Angle(left.Radians * right);

        /// <summary>
        ///     Divides an Angle by a scalar.
        /// </summary>
        /// <param name="left">The angle.</param>
        /// <param name="right">The scalar.</param>
        /// <returns>A new angle equal to the division of the angle by the scalar.</returns>
        public static Angle operator /(Angle left, double right) => new Angle(left.Radians / right);

        /// <summary>
        ///     Adds two angles together.
        /// </summary>
        /// <param name="left">The first angle.</param>
        /// <param name="right">The second angle.</param>
        /// <returns>A new Angle equal to the sum of the provided angles.</returns>
        public static Angle operator +(Angle left, Angle right) => new Angle(left.Radians + right.Radians);

        /// <summary>
        ///     Subtracts the second angle from the first.
        /// </summary>
        /// <param name="left">The first angle.</param>
        /// <param name="right">The second angle.</param>
        /// <returns>A new Angle equal to the difference of the provided angles.</returns>
        public static Angle operator -(Angle left, Angle right) => new Angle(left.Radians - right.Radians);

        /// <summary>Negates the angle</summary>
        /// <param name="angle">The angle to negate.</param>
        /// <returns>The negated angle.</returns>
        public static Angle operator -(Angle angle) => new Angle(-1.0 * angle.Radians);

        /// <summary>Creates a new instance of Angle.</summary>
        /// <param name="value">The value in degrees.</param>
        /// <returns> A new instance of the <see cref="T:Funhouse.Models.Angle" /> struct.</returns>
        public static Angle FromDegrees(double value) => new Angle(value * (Math.PI / 180.0));

        /// <summary>Creates a new instance of Angle.</summary>
        /// <param name="value">The value in radians.</param>
        /// <returns> A new instance of the <see cref="T:Funhouse.Models.Angle" /> struct.</returns>
        public static Angle FromRadians(double value) => new Angle(value);


        public override string ToString() => $"{Radians} rad, {Degrees} degrees";

        public bool Equals(Angle other) => Radians.Equals(other.Radians);

        public override bool Equals(object? obj) => obj is Angle other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(Radians);
    }
}