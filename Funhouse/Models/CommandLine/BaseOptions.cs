using System;
using System.IO;
using CommandLine;
using JetBrains.Annotations;

namespace Funhouse.Models.CommandLine
{
    public abstract class BaseOptions
    {
        [Option('b', "brightness", HelpText = "Brightness adjustment", Required = false, Default = 1.0f)]
        public float Brightness { get; [UsedImplicitly] set; }

        [Option('d', "tolerance", HelpText = "Time tolerance in minutes in identifying suitable satellite images when combining", Required = false, Default = 30)]
        public int ToleranceMinutes { get; set; }
        
        [Option('i', "interpolation", HelpText = "Interpolation type. Valid values are N (nearest neighbour), B (bilinear)", Required = false, Default = InterpolationOptions.B)]
        public InterpolationOptions InterpolationType { get; [UsedImplicitly] set; }

        [Option('f', "force", HelpText = "Force overwrite existing output file", Required = false, Default = false)]
        public bool Force { get; [UsedImplicitly] set; }

        [Option('q', "quiet", HelpText = "Don't provide any console output", Required = false, Default = false)]
        public bool Quiet { get; [UsedImplicitly] set; }

        [Option('o', "output", HelpText = "Path to output file or folder", Required = true)]
        public string OutputPath { get; [UsedImplicitly] set; } = null!;

        [Option('r', "resolution", HelpText = "Output spatial resolution in km; valid values are 2 or 4", Default = Constants.Satellite.SpatialResolution.FourKm)]
        public int SpatialResolution { get; [UsedImplicitly] set; }

        [Option('s', "source", HelpText = "Path to IR satellite image(s)", Required = true)]
        public string? SourcePath { get; [UsedImplicitly] set; } = null!;

        [Option('S', "saturation", HelpText = "Saturation adjustment", Required = false, Default = 0.7f)]
        public float Saturation { get; [UsedImplicitly] set; }

        [Option('t', "tint", HelpText = "Tint to apply to satellite image", Required = false, Default = "1b3f66")]
        public string Tint { get; [UsedImplicitly] set; } = null!;
        
        [Option('T', "timestamp", HelpText = "Target timestamp in UTC if combining multiple files; e.g. 2020-12-20T23:00:30", Required = false)]
        public DateTime? TargetTimestamp { get; set; }
        
        [Option('u', "underlay", HelpText = "Path to custom full-colour underlay image", Required = false)]
        public string? UnderlayPath { get; [UsedImplicitly] set; }

        [Option('U', "nounderlay", HelpText = "If no underlay should be rendered", Required = false)]
        public bool NoUnderlay { get; [UsedImplicitly] set; }

        [Option('v', "verbose", HelpText = "Verbose console output", Required = false, Default = false)]
        public bool Verbose { get; [UsedImplicitly] set; }

        /// <summary>
        ///     Identifies whether <see cref="SourcePath"/> is referring to a directory or a file.
        /// </summary>
        public bool MultipleSources => SourcePath?.Contains('*') == true || Directory.Exists(SourcePath);
    }
}