using Newtonsoft.Json;
using Sanchez.Processing.Services.Filesystem;

namespace Sanchez.Processing.Models.Configuration;

public record SatelliteConfiguration
{
    [JsonProperty("DisplayName", Required = Required.Always)]
    public string DisplayName { get; init; } = null!;

    [JsonProperty("FilenamePrefix")]
    public string? FilenamePrefix { get; init; } 

    /// <summary>
    ///     Regular expression matching filename suffixes.
    /// </summary>
    [JsonProperty("FilenameSuffix")]
    public string? FilenameSuffix { get; init; } 
        
    [JsonProperty("FilenameParser", Required = Required.Always)]
    public FilenameParserType FilenameParserType { get; init; }

    /// <summary>
    ///     Whether pixel intensities in IR images should be inverted to match GOES-R.
    /// </summary>
    [JsonProperty("Invert")]
    public bool Invert { get; init; }

    [JsonProperty("Longitude", Required = Required.Always)]
    public double Longitude { get; init; }

    /// <summary>
    ///     Offset added to the stated longitude, for visual correction to satellite imagery which hasn't originated
    ///     from GOES-R.
    /// </summary>
    [JsonProperty("LongitudeAdjustment")]
    public double? LongitudeAdjustment { get; init; }

    [JsonProperty("Height")]
    public double Height { get; init; } = Constants.Satellite.DefaultHeight;

    /// <summary>
    ///     Optional border crop ratios. Values are expected to be top, right, bottom, left.
    /// </summary>
    [JsonProperty("Crop")]
    public double[]? Crop { get; init; }

    [JsonProperty("Brightness")]
    public float Brightness { get; init; } = 1.0f;

    [JsonProperty("Contrast")]
    public float Contrast { get; init; } = 1.0f;

    [JsonProperty("Saturation")]
    public float Saturation { get; init; } = 1.0f;
}