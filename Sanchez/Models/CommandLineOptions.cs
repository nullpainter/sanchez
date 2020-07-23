using CommandLine;
using JetBrains.Annotations;

namespace Sanchez.Models
{
    [UsedImplicitly]
    public class CommandLineOptions
    {
        [Option('u', "underlay", HelpText = "Path to full-colour underlay image", Required = true)]
        public string? UnderlayPath { get; [UsedImplicitly] set; }

        [Option('s', "source", HelpText = "Path to satellite image being composed", Required = true)]
        public string? SourceImagePath { get; [UsedImplicitly] set; }

        [Option('m', "mask", HelpText = "Optional path to mask image", Required = false)]
        public string? MaskPath { get; [UsedImplicitly] set; }

        [Option('o', "output", HelpText = "Path to output file", Required = true)]
        public string? OutputFile { get; [UsedImplicitly] set; }

        [Option('t', "tint", HelpText = "Hex triplet tint to apply to satellite image", Required = false, Default = "5ebfff")]
        public string Tint { get; [UsedImplicitly] set; } = null!;

        [Option('b', "brightness", HelpText = "Brightness adjustment", Required = false, Default = 1.2f)]
        public float Brightness { get; [UsedImplicitly] set; }

        [Option('S', "saturation", HelpText = "Saturation adjustment", Required = false, Default = 0.7f)]
        public float Saturation { get; [UsedImplicitly] set; }
    }
}