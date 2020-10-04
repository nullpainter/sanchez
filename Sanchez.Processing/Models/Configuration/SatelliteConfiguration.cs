using Newtonsoft.Json;
using Sanchez.Processing.Services.Filesystem;

namespace Sanchez.Processing.Models.Configuration
{
    public class SatelliteConfiguration
    {
        [JsonProperty("DisplayName", Required = Required.Always)]
        public string DisplayName { get; set; } = null!;
        
        [JsonProperty("FilenamePrefix", Required = Required.Always)]
        public string FilenamePrefix { get; set; } = null!;
        
        [JsonProperty("FilenameParser", Required = Required.Always)]
        public FilenameParserType FilenameParserType { get; set; }
        
        [JsonProperty("Longitude", Required = Required.Always)]
        public double Longitude { get; set; }

        /// <summary>
        ///     Offset added to the stated longitude, for visual correction to satellite imagery which hasn't originated
        ///     from GOES-R.
        /// </summary>
        [JsonProperty("LongitudeAdjustment")]
        public double? LongitudeAdjustment { get; set; }
        
        [JsonProperty("CropRange", Required = Required.Always)]
        public CropRange CropRange { get; set; } = new CropRange();

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