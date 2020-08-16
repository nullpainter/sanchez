using Newtonsoft.Json;

namespace Funhouse.Models.Configuration
{
    public class VisibleRange
    {
        [JsonProperty("MinLongitude", Required = Required.Always)]
        public double MinLongitude { get; set; }

        [JsonProperty("MaxLongitude", Required = Required.Always)]
        public double MaxLongitude { get; set; }
    }
}