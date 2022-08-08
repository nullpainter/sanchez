using CommandLine;
using JetBrains.Annotations;

namespace Sanchez.Models.CommandLine;

[Verb("reproject", HelpText = "Reproject one or more satellite images to equirectangular projection")]
public class EquirectangularOptions : CommandLineOptions
{
    [Option('a', "autocrop", HelpText = "Whether to create an automatically cropped image.", Required = false, Default = false)]
    public bool AutoCrop { get; [UsedImplicitly] set; }

    [Option("nocrop", HelpText = "If no cropping should be performed", Required = false, Default = false)]
    public bool NoCrop { get; [UsedImplicitly] set; }
        
    [Option("lon", HelpText = "Longitude range in degrees. format is min:max; e.g., 165.1:179.3", Required = false)]
    public string? LongitudeRange { get; [UsedImplicitly] set; }
        
    [Option("lat", HelpText = "Latitude range in degrees. format is min:max; e.g., -33.6:-48", Required = false)]
    public string? LatitudeRange { get; [UsedImplicitly] set; }
}