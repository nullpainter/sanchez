using CommandLine;
using Funhouse.Models.Configuration;
using Funhouse.Models.Projections;
using JetBrains.Annotations;

namespace Funhouse.Models
{
    /// <summary>
    ///     All command-line options supported by Sanchez.
    /// </summary>
    [UsedImplicitly]
    public class CommandLineOptions
    {
     
        
        [Option('p', "projection", HelpText = "Output projection type. Valid values are Mercator, PseudoMercator", Required = false, Default = ProjectionType.PseudoMercator)]
        public ProjectionType ProjectionType { get; [UsedImplicitly] set; }
        
        [Option('q', "quiet", HelpText = "Don't provide any console output", Required = false, Default = false)]
        public bool Quiet { get; [UsedImplicitly] set; }
        
        [Option('s', "source", HelpText = "Path to IR satellite image(s)", Required = true)]
        public string? SourcePath { get; [UsedImplicitly] set; }
        
        [Option('o', "output", HelpText = "Path to output file or folder", Required = true)]
        public string OutputPath { get; [UsedImplicitly] set; } = null!;
        
        /// <summary>
        ///     Whether to blur edges to minimise artifacts in projected output.
        /// </summary>
        [Option('B', "blur", HelpText = "Blur edges for non-cropped images", Required = false, Default = false)]
        public bool BlurEdges { get; [UsedImplicitly] set; }
        
        [Option('a', "autocrop", HelpText = "Whether to create a cropped image. Only applicable when stitching.", Required = false, Default = false)]
        public bool AutoCrop { get; [UsedImplicitly] set; }
        
        [Option('T', "stitch", HelpText = "Combine multiple satellite images", Required = false, Default = false)]
        public bool Stitch { get; [UsedImplicitly] set; }
        
        [Option('v', "verbose", HelpText = "Verbose console output", Required = false, Default = false)]
        public bool Verbose { get; [UsedImplicitly] set; }
        
        [Option('i', "interpolation", HelpText = "Interpolation type. Valid values are NearestNeighbour, Bilinear", Required = false, Default = InterpolationType.NearestNeighbour)]
        public InterpolationType InterpolationType { get; [UsedImplicitly] set; }

        // FIXME doesn't belong here
        public bool Debug { get;  } = false;
    }
}