namespace Funhouse.Models
{
    public class GeostationaryRenderOptions
    {
        public GeostationaryRenderOptions(double? longitude, float hazeAmount, bool autoAdjustLevels)
        {
            Longitude = longitude;
            HazeAmount = hazeAmount;
            AutoAdjustLevels = autoAdjustLevels;
        }

        /// <summary>
        ///     Target longitude for geostationary satellite projection.
        /// </summary>
        public double? Longitude { get; }

        /// <summary>
        ///     Amount of haze to apply to image; valid values are between 0 (no haze) and 1 (full haze).
        /// </summary>
        public float HazeAmount { get; }

        /// <summary>
        ///     Whether the satellite histogram should be equalised. This should be disabled when creating animations with satellite
        ///     imagery which has large variance in brightness per frame.
        /// </summary>
        public bool AutoAdjustLevels { get; }
    }
}