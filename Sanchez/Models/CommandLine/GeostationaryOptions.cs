using CommandLine;
using JetBrains.Annotations;

namespace Sanchez.Models.CommandLine;

/// <summary>
///     All command-line options supported by Sanchez.
/// </summary>
[UsedImplicitly]
[Verb("geostationary", true, HelpText = "Composite a full-colour underlay with geostationary IR satellite images")]
public class GeostationaryOptions : CommandLineOptions
{
    [Option('l', "longitude", HelpText = "Target longitude for geostationary satellite projection", Required = false)]
    public double? LongitudeDegrees { get; [UsedImplicitly] set; }
        
    [Option('E', "endlongitude", HelpText = "Target end longitude for timelapse geostationary satellite projection", Required = false)]
    public double? EndLongitudeDegrees { get; [UsedImplicitly] set; }
        
    [Option("inverse", HelpText = "If an end longitude is specified, invert standard Earth rotation", Required = false, Default = false)]
    public bool InverseRotation { get; [UsedImplicitly] set; }

    [Option('h', "haze", HelpText = "Amount of haze to apply to image; valid values are between 0 (no haze) and 1 (full haze)", Required = false, Default = 0.2f)]
    public float HazeAmount { get; [UsedImplicitly] set; }
        
    [Option('O', "hazeopacity", HelpText = "Opacity of haze; valid values are between 0 (transparent) and 1 (opaque)", Required = false, Default = 0.9f)]
    public float HazeOpacity { get; [UsedImplicitly] set; }
}