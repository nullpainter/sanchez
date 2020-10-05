using Newtonsoft.Json;

namespace Sanchez.Processing.Models.Configuration
{
    public class CropRange
    {
        [JsonProperty("MinLongitude", Required = Required.Always)]
        public double MinLongitude { get; set; }

        [JsonProperty("MaxLongitude", Required = Required.Always)]
        public double MaxLongitude { get; set; }

        [JsonProperty("MinLatitude")] public double MinLatitude { get; set; } = Constants.Satellite.CropRange.DefaultMinLatitude;

        [JsonProperty("MaxLatitude")]
        public double MaxLatitude { get; set; } = Constants.Satellite.CropRange.DefaultMaxLatitude;
    }
}