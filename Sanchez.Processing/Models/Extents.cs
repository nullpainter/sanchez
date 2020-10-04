using Sanchez.Processing.Models.Angles;

namespace Sanchez.Processing.Models
{
    public readonly struct Extents
    {
        public Range Latitude { get; }
        public Range Longitude { get; }

        public Extents(Range latitude, Range longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }
    }
}