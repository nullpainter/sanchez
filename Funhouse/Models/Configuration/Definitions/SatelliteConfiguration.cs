using Newtonsoft.Json;

namespace Funhouse.Models.Configuration.Definitions
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
        
        [JsonProperty("ImageOffset", Required = Required.Always)]
        public ImageOffsetConfiguration ImageOffset { get; } = new ImageOffsetConfiguration();

        [JsonProperty("Height")]
        public double Height { get; set; } = 35786023.0;
    }
}