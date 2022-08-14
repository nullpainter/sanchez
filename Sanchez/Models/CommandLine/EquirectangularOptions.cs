using CommandLine;
using JetBrains.Annotations;

namespace Sanchez.Models.CommandLine;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
[Verb("reproject", HelpText = "Reproject one or more satellite images to equirectangular projection")]
public record EquirectangularOptions : CommandLineOptions
{
    [Option('a', "autocrop", HelpText = "Whether to create an automatically cropped image.", Required = false, Default = false)]
    public bool AutoCrop { get; set; }

    [Option("nocrop", HelpText = "If no cropping should be performed", Required = false, Default = false)]
    public bool NoCrop { get; set; }
        
    [Option("lon", HelpText = "Longitude range in degrees. format is min:max; e.g., 165.1:179.3", Required = false)]
    public string? LongitudeRange { get; set; }
        
    [Option("lat", HelpText = "Latitude range in degrees. format is min:max; e.g., -33.6:-48", Required = false)]
    public string? LatitudeRange { get; set; }
}