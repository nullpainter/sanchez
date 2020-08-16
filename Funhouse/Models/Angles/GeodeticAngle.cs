using MathNet.Spatial.Units;

namespace Funhouse.Models.Angles
{ 
    /// <summary>
    ///     Convenience wrapper around an angle representing a latitude and longitude.
    /// </summary>
    public struct GeodeticAngle
    {
        public Angle Latitude { get; set; }
        public Angle Longitude { get; set; }

        public GeodeticAngle(Angle latitude, Angle longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }
    }
}