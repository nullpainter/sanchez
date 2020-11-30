using CommandLine;
using JetBrains.Annotations;

namespace Sanchez.Models.CommandLine
{
    [Verb("reproject", HelpText = "Reproject one or more satellite images to equirectangular projection")]
    public class EquirectangularOptions : CommandLineOptions
    {
        [Option('a', "autocrop", HelpText = "Whether to create an automatically cropped image.", Required = false, Default = false)]
        public bool AutoCrop { get; [UsedImplicitly] set; }

        [Option("nocrop", HelpText = "If no cropping should be performed", Required = false, Default = false)]
        public bool NoCrop { get; [UsedImplicitly] set; }
        
        // FIXME get proper longitude for nz
        [Option("lon", HelpText = "Longitude range in degrees. format is min:max; e.g., -174:180", Required = false)]
        public string? LongitudeRange { get; [UsedImplicitly] set; }
        
        // FIXME get proper lat for NZ
        [Option("lat", HelpText = "Latitude range in degrees. format is min:max; e.g., -174:180", Required = false)]
        public string? LatitudeRange { get; [UsedImplicitly] set; }
    }
}