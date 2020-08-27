using Newtonsoft.Json;

namespace Funhouse.Models.Configuration
{
    public class VisibleRange
    {
        [JsonProperty("MinLongitude", Required = Required.Always)]
        public double MinLongitude { get; set; }

        [JsonProperty("MaxLongitude", Required = Required.Always)]
        public double MaxLongitude { get; set; }
        
        [JsonProperty("MinLatitude", Required = Required.Always)]
        public double MinLatitude { get; set; }

        [JsonProperty("MaxLatitude", Required = Required.Always)]
        public double MaxLatitude { get; set; }
    }
}