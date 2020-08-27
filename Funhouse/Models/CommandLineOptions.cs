using CommandLine;
using Funhouse.Models.Configuration;
using JetBrains.Annotations;

namespace Funhouse.Models
{
    /// <summary>
    ///     All command-line options supported by Sanchez.
    /// </summary>
    [UsedImplicitly]
    public class CommandLineOptions
    {
        [Option('a', "autocrop", HelpText = "Whether to create a cropped image. Only applicable when stitching.", Required = false, Default = false)]
        public bool AutoCrop { get; [UsedImplicitly] set; }

        /// <summary>
        ///     Whether to blur edges to minimise artifacts in projected output.
        /// </summary>
        [Option('B', "blur", HelpText = "Blur edges for non-cropped images", Required = false, Default = false)]
        public bool BlurEdges { get; [UsedImplicitly] set; }
        
        [Option('b', "brightness", HelpText = "Brightness adjustment", Required = false, Default = 1.0f)]
        public float Brightness { get; [UsedImplicitly] set; }
        
        [Option('i', "interpolation", HelpText = "Interpolation type. Valid values are NearestNeighbour, Bilinear", Required = false, Default = InterpolationType.Bilinear)]
        public InterpolationType InterpolationType { get; [UsedImplicitly] set; }

        [Option('q', "quiet", HelpText = "Don't provide any console output", Required = false, Default = false)]
        public bool Quiet { get; [UsedImplicitly] set; }
       
        [Option('o', "output", HelpText = "Path to output file or folder", Required = true)]
        public string OutputPath { get; [UsedImplicitly] set; } = null!;
        
        [Option('s', "source", HelpText = "Path to IR satellite image(s)", Required = true)]
        public string? SourcePath { get; [UsedImplicitly] set; }
        
        [Option('S', "saturation", HelpText = "Saturation adjustment", Required = false, Default = 0.7f)]
        public float Saturation { get; [UsedImplicitly] set; }
 
        [Option('t', "tint", HelpText = "Tint to apply to satellite image", Required = false, Default = "5ebfff")]
        public string Tint { get; [UsedImplicitly] set; } = null!;

        [Option('T', "stitch", HelpText = "Combine multiple satellite images", Required = false, Default = false)]
        public bool Stitch { get; [UsedImplicitly] set; }
        
        [Option('u', "underlay", HelpText = "Path to custom full-colour underlay image", Required = false)]
        public string? UnderlayPath { get; [UsedImplicitly] set; }

        [Option('v', "verbose", HelpText = "Verbose console output", Required = false, Default = false)]
        public bool Verbose { get; [UsedImplicitly] set; }
    }
}