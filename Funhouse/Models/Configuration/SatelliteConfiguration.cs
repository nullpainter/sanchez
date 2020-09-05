using Newtonsoft.Json;

namespace Funhouse.Models.Configuration
{
    public class SatelliteConfiguration
    {
        [JsonProperty("FilePrefix", Required = Required.Always)]
        public string FilePrefix { get; set; } = null!;

        [JsonProperty("DisplayName", Required = Required.Always)]
        public string DisplayName { get; set; } = null!;

        [JsonProperty("Longitude", Required = Required.Always)]
        public double Longitude { get; set; }

        [JsonProperty("VisibleRange", Required = Required.Always)]
        public VisibleRange VisibleRange { get; set; } = new VisibleRange();

        [JsonProperty("Height")]
        public double Height { get; set; } = Constants.Satellite.DefaultHeight;

        /// <summary>
        ///     Optional border crop ratios. Values are expected to be top, right, bottom, left.
        /// </summary>
        [JsonProperty("Crop")]
        public double[]? Crop { get; set; }

        [JsonProperty("Brightness")]
        public float Brightness { get; set; } = 1.0f;

        [JsonProperty("Contrast")]
        public float Contrast { get; set; } = 1.0f;


        [JsonProperty("Saturation")]
        public float Saturation { get; set; } = 1.0f;
    }
}