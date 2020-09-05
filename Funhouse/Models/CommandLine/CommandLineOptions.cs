using CommandLine;
using JetBrains.Annotations;

namespace Funhouse.Models.CommandLine
{
    /// <summary>
    ///     All command-line options supported by Sanchez.
    /// </summary>
    [UsedImplicitly]
    public class CommandLineOptions
    {
        [Option('a', "autocrop", HelpText = "Whether to create a cropped image. Only applicable when stitching.", Required = false, Default = false)]
        public bool AutoCrop { get; [UsedImplicitly] set; }

        [Option('b', "brightness", HelpText = "Brightness adjustment", Required = false, Default = 1.0f)]
        public float Brightness { get; [UsedImplicitly] set; }
        
        [Option('h', "haze", HelpText = "Amount of haze to apply to image; valid values are between 0 (no haze) and 1 (full haze)", Required = false, Default = 0.3f)]
        public float HazeAmount { get; [UsedImplicitly] set; }

        [Option('i', "interpolation", HelpText = "Interpolation type. Valid values are N (nearest neighbour), B (bilinear)", Required = false, Default = InterpolationOptions.B)]
        public InterpolationOptions InterpolationType { get; [UsedImplicitly] set; }
        
        // TODO validate this is being used when stitching/equirectangular
        [Option('l', "longitude", HelpText = "Target longitude for geostationary satellite projection", Required = false)]
        public double? Longitude { get; [UsedImplicitly] set; } = null!;

        [Option('m', "mode", HelpText = "Render mode. Valid values are G (geostationary), E (equirectangular)", Required = false, Default = ProjectionOptions.G)]
        public ProjectionOptions ProjectionType { get; [UsedImplicitly] set; }

        [Option('q', "quiet", HelpText = "Don't provide any console output", Required = false, Default = false)]
        public bool Quiet { get; [UsedImplicitly] set; }

        [Option('o', "output", HelpText = "Path to output file or folder", Required = true)]
        public string OutputPath { get; [UsedImplicitly] set; } = null!;

        [Option('r', "resolution", HelpText = "Output spatial resolution in km; valid values are 2 or 4", Default = Constants.Satellite.SpatialResolution.FourKm)]
        public int SpatialResolution { get; [UsedImplicitly] set; }

        [Option('s', "source", HelpText = "Path to IR satellite image(s)", Required = true)]
        public string? SourcePath { get; [UsedImplicitly] set; }

        [Option('S', "saturation", HelpText = "Saturation adjustment", Required = false, Default = 0.7f)]
        public float Saturation { get; [UsedImplicitly] set; }

        [Option('t', "tint", HelpText = "Tint to apply to satellite image", Required = false, Default = "1b3f66")]
        public string Tint { get; [UsedImplicitly] set; } = null!;

        [Option('T', "stitch", HelpText = "Combine multiple satellite images", Required = false, Default = false)]
        public bool Stitch { get; [UsedImplicitly] set; }

        [Option('u', "underlay", HelpText = "Path to custom full-colour underlay image", Required = false)]
        public string? UnderlayPath { get; [UsedImplicitly] set; }
        
        [Option('U', "nounderlay", HelpText = "If no underlay should be rendered", Required = false)]
        public bool NoUnderlay { get; [UsedImplicitly] set; }

        [Option('v', "verbose", HelpText = "Verbose console output", Required = false, Default = false)]
        public bool Verbose { get; [UsedImplicitly] set; }
    }
}