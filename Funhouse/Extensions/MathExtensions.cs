using System;
using System.Collections.Generic;
using System.Linq;

namespace Funhouse.Extensions
{
    public static class MathExtensions
    {
        public static double Limit(this double value, double min, double max) => ((value - min) % (max - min) + (max - min)) % (max - min) + min;
        public static float Limit(this float value, float min, float max) => ((value - min) % (max - min) + (max - min)) % (max - min) + min;

        public static float ClosestTo(this List<float> collection, float target)
        {
            if (!collection.Any()) throw new InvalidOperationException("Collection is empty");

            var closest = float.MaxValue;
            var minDifference = float.MaxValue;

            foreach (var element in collection)
            {
                var difference = Math.Abs(element - target);
                if (minDifference <= difference) continue;
                minDifference = difference;
                closest = element;
            }

            return closest;
        }
    }
}