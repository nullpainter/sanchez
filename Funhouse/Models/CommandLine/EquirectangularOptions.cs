using CommandLine;
using JetBrains.Annotations;

namespace Funhouse.Models.CommandLine
{
    [Verb("reproject", HelpText = "Reproject one or more satellite images to equirectangular projection")]
    public class EquirectangularOptions : BaseOptions
    {
        [Option('a', "autocrop", HelpText = "Whether to create a cropped image. Only applicable when stitching.", Required = false, Default = false)]
        public bool AutoCrop { get; [UsedImplicitly] set; }
    }
}