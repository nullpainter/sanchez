using CommandLine;
using JetBrains.Annotations;

namespace Sanchez.Models.CommandLine
{
    [Verb("reproject", HelpText = "Reproject one or more satellite images to equirectangular projection")]
    public class EquirectangularOptions : CommandLineOptions
    {
        [Option('a', "autocrop", HelpText = "Whether to create an automatically cropped image. Only applicable when stitching.", Required = false, Default = false)]
        public bool AutoCrop { get; [UsedImplicitly] set; }

        // [Option('e', "extents", HelpText = "Latitude/longitude extents. Format is minlat:maxlat:minlon:maxlon; e.g. -100:100:-45:45", Required = false)]
        // public string? Extents { get; [UsedImplicitly] set; }
        
        [Option('m', "mode", HelpText = "Whether source images are stitched together or rendered individually in a batch", Default = EquirectangularMode.Batch)]
        public EquirectangularMode Mode { get; [UsedImplicitly] set; }
    }

    public enum EquirectangularMode
    {
        Stitch,
        Batch
    }
}