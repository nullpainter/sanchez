using Newtonsoft.Json;
using Sanchez.Processing.Services.Filesystem;

namespace Sanchez.Processing.Models.Configuration;

public record SatelliteConfiguration
{
    [JsonProperty(Required = Required.Always)]
    public string DisplayName { get; init; } = null!;

    [JsonProperty]
    public string? FilenamePrefix { get; init; } 

    /// <summary>
    ///     Regular expression matching filename suffixes.
    /// </summary>
    [JsonProperty]
    public string? FilenameSuffix { get; init; } 
        
    [JsonProperty("FilenameParser", Required = Required.Always)]
    public FilenameParserType FilenameParserType { get; init; }

    /// <summary>
    ///     Whether pixel intensities in IR images should be inverted to match GOES-R.
    /// </summary>
    [JsonProperty]
    public bool Invert { get; init; }

    [JsonProperty(Required = Required.Always)]
    public double Longitude { get; init; }

    /// <summary>
    ///     Offset added to the stated longitude, for visual correction to satellite imagery which hasn't originated
    ///     from GOES-R.
    /// </summary>
    [JsonProperty]
    public double? LongitudeAdjustment { get; init; }

    [JsonProperty]
    public double Height { get; init; } = Constants.Satellite.DefaultHeight;

    /// <summary>
    ///     Optional border crop ratios. Values are expected to be top, right, bottom, left.
    /// </summary>
    [JsonProperty]
    public double[]? Crop { get; init; }

    [JsonProperty]
    public float Brightness { get; init; } = 1.0f;

    [JsonProperty]
    public float Contrast { get; init; } = 1.0f;

    [JsonProperty]
    public float Saturation { get; init; } = 1.0f;
}