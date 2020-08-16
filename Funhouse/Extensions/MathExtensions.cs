using System;

namespace Funhouse.Extensions
{
    public static class MathExtensions
    {
        public static double Deg2Rad(this double degrees) => Math.PI / 180 * degrees;
        public static double RadToDeg(this double radians) => 180 / Math.PI * radians;
        public static double Limit(this double value, double min, double max) => ((value - min) % (max - min) + (max - min)) % (max - min) + min;
        public static double Limit(this float value, float min, float max) => ((value - min) % (max - min) + (max - min)) % (max - min) + min;
    }
}