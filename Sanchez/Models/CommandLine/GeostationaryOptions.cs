using CommandLine;
using JetBrains.Annotations;

namespace Sanchez.Models.CommandLine;

/// <summary>
///     All command-line options supported by Sanchez.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
[Verb("geostationary", true, HelpText = "Composite a full-colour underlay with geostationary IR satellite images")]
public record GeostationaryOptions : CommandLineOptions
{
    [Option('l', "longitude", HelpText = "Target longitude for geostationary satellite projection", Required = false)]
    public double? LongitudeDegrees { get; set; }

    [Option('E', "endlongitude", HelpText = "Target end longitude for timelapse geostationary satellite projection", Required = false)]
    public double? EndLongitudeDegrees { get; set; }

    [Option("inverse", HelpText = "If an end longitude is specified, invert standard Earth rotation", Required = false, Default = false)]
    public bool InverseRotation { get; set; }

    [Option('h', "haze", HelpText = "Amount of atmosphere to apply to image; valid values are between 0 (no haze) and 1 (full haze)", Required = false, Default = 0.5f)]
    public float AtmosphereAmount { get; set; }

    [Option('O', "hazeopacity", HelpText = "Opacity of atmosphere; valid values are between 0 (transparent) and 1 (opaque)", Required = false, Default = 0.5f)]
    public float AtmosphereOpacity { get; set; }
}