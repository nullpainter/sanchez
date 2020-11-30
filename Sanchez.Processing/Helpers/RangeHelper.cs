using Extend;
using Sanchez.Processing.Models;
using Sanchez.Processing.Models.Angles;

namespace Sanchez.Processing.Helpers
{
    public static class RangeHelper
    {
        /// <summary>
        ///     Parses a string in the form of min:max angle range in degrees into a <see cref="Range"/>,
        ///     or <c>null</c> if no range is specified or could be parsed.
        /// </summary>
        public static Range? ParseRange(string? range)
        {
            if (range.IsNullOrEmpty()) return null;

            var components = range!.Split(':');
            if (components.Length != 2) return null;

            if (!double.TryParse(components[0], out var min)) return null;
            if (!double.TryParse(components[1], out var max)) return null;

            return new Range(
                Angle.FromDegrees(min),
                Angle.FromDegrees(max)
            );
        }
    }
}