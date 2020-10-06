namespace Sanchez.Processing.Models
{
    public class GeostationaryRenderOptions
    {
        public GeostationaryRenderOptions(double? longitude, double? endLongitude, float hazeAmount)
        {
            Longitude = longitude;
            EndLongitude = endLongitude;
            HazeAmount = hazeAmount;
        }

        /// <summary>
        ///     Target longitude for geostationary satellite projection.
        /// </summary>
        public double? Longitude { get; }

        /// <summary>
        ///     End longitude for timelapse geostationary satellite projection.
        /// </summary>
        public double? EndLongitude { get; }

        /// <summary>
        ///     Amount of haze to apply to image; valid values are between 0 (no haze) and 1 (full haze).
        /// </summary>
        public float HazeAmount { get; }
    }
}