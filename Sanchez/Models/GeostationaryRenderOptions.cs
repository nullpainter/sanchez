namespace Sanchez.Models
{
    public class GeostationaryRenderOptions
    {
        public GeostationaryRenderOptions(double? longitude, float hazeAmount)
        {
            Longitude = longitude;
            HazeAmount = hazeAmount;
        }

        /// <summary>
        ///     Target longitude for geostationary satellite projection.
        /// </summary>
        public double? Longitude { get; }

        /// <summary>
        ///     Amount of haze to apply to image; valid values are between 0 (no haze) and 1 (full haze).
        /// </summary>
        public float HazeAmount { get; }
    }
}