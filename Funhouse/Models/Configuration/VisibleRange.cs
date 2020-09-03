using Newtonsoft.Json;

namespace Funhouse.Models.Configuration
{
    public class VisibleRange
    {
        [JsonProperty("MinLongitude", Required = Required.Always)]
        public double MinLongitude { get; set; }

        [JsonProperty("MaxLongitude", Required = Required.Always)]
        public double MaxLongitude { get; set; }

        [JsonProperty("MinLatitude")] public double MinLatitude { get; set; } = Constants.Satellite.VisibleRange.DefaultMinLatitude;

        [JsonProperty("MaxLatitude")]
        public double MaxLatitude { get; set; } = Constants.Satellite.VisibleRange.DefaultMaxLatitude;
    }
}