using Sanchez.Models;
using Sanchez.Models.Angles;

namespace Sanchez.Helpers
{
    public static class ExtentsHelper
    {
        public static Extents? ParseExtentsString(string? extents)
        {
            if (extents == null) return null;
            
            var components = extents.Split(':');
            if (components.Length != 4) return null;

            if (!double.TryParse(components[0], out var minLatitude)) return null;
            if (!double.TryParse(components[1], out var maxLatitude)) return null;
            if (!double.TryParse(components[2], out var minLongitude)) return null;
            if (!double.TryParse(components[3], out var maxLongitude)) return null;
            
            return new Extents(
                new Range(Angle.FromDegrees(minLatitude), Angle.FromDegrees(maxLatitude)),
                new Range(Angle.FromDegrees(minLongitude), Angle.FromDegrees(maxLongitude)));
        }
    }
}