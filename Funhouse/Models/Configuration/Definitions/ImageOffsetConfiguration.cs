using Newtonsoft.Json;

namespace Funhouse.Models.Configuration.Definitions
{
    /// <summary>
    ///     Image offsets, normalised to 2km spatial resolution (5424px edge)
    /// </summary>
    public class ImageOffsetConfiguration
    {
        [JsonProperty("X", Required = Required.Always)]
        public double X { get; set; }

        [JsonProperty("Y", Required = Required.Always)]
        public double Y { get; set; }

        [JsonProperty("ScaleFactor", Required = Required.Always)]
        public double ScaleFactor { get; set; }
    }
}