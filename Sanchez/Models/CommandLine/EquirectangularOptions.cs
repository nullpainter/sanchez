using CommandLine;
using JetBrains.Annotations;

namespace Sanchez.Models.CommandLine
{
    [Verb("reproject", HelpText = "Reproject one or more satellite images to equirectangular projection")]
    public class EquirectangularOptions : CommandLineOptions
    {
        [Option('a', "autocrop", HelpText = "Whether to create an automatically cropped image.", Required = false, Default = false)]
        public bool AutoCrop { get; [UsedImplicitly] set; }
    }
}