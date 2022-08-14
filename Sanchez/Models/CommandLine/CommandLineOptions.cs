using CommandLine;
using JetBrains.Annotations;
using Sanchez.Processing.Models;
using Sanchez.Processing.Models.Options;

namespace Sanchez.Models.CommandLine;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public abstract record CommandLineOptions
{
    [Option('b', "brightness", HelpText = "Brightness adjustment", Required = false, Default = 1.0f)]
    public float Brightness { get; set; }

    [Option('c', "clut", HelpText = "Apply CLUT to IR image for intensity range; e.g. 0.0-1.0", Required = false)]
    public string? ClutRange { get; set; }

    [Option('d', "tolerance", HelpText = "Time tolerance in minutes in identifying suitable satellite images when combining", Required = false, Default = 30)]
    public int ToleranceMinutes { get; set; }

    [Option('D', "definitions", HelpText = "Path to custom satellite definitions", Required = false)]
    public string? DefinitionsPath { get; set; }

    [Option('e', "endtimestamp", HelpText = "End timestamp in UTC if stitching multiple files; e.g. 2020-12-20T23:00:30", Required = false)]
    public DateTimeOffset? EndTimestamp { get; set; }

    [Option('i', "interpolation", HelpText = "Interpolation type. Valid values are N (nearest neighbour), B (bilinear)", Required = false, Default = InterpolationOptions.B)]
    public InterpolationOptions InterpolationType { get; set; }

    [Option('I', "interval", HelpText = "Time interval in minutes between images when stitching", Required = false)]
    public int? IntervalMinutes { get; set; }

    [Option('f', "force", HelpText = "Force overwrite existing output file", Required = false, Default = false)]
    public bool Force { get; set; }

    [Option('g', "gradient", HelpText = "Path to gradient configuration", Required = false)]
    public string? GradientPath { get; set; }

    [Option('L', "noadjustlevels", HelpText = "Don't perform histogram equalisation on satellite imagery", Required = false, Default = false)]
    public bool NoAutoAdjustLevels { get; set; }

    [Option('A', "adaptivelevels", HelpText = "Apply adaptive histogram equalisation", Required = false, Default = false)]
    public bool AdaptiveLevelAdjustment { get; set; }

    [Option('m', "minsatellites", HelpText = "Minimum number of satellites in images when stitching", Required = false)]
    public int? MinSatellites { get; set; }

    [Option('o', "output", HelpText = "Path to output file or folder", Required = true)]
    public string OutputPath { get; set; } = null!;

    [Option('F', "format", HelpText = "Output file format; valid values are png or jpg/jpeg", Required = false)]
    public string? OutputFormat { get; set; }

    [Option('n', HelpText = "Add gaussian noise to output image", Required = false)]
    public bool Noise { get; set; }

    [Option('q', "quiet", HelpText = "Don't perform console output", Required = false, Default = false)]
    public bool Quiet { get; set; }

    [Option('r', "resolution", HelpText = "Output spatial resolution in km; valid values are 0.5, 1, 2 or 4", Default = Constants.Satellite.SpatialResolution.FourKm)]
    public string SpatialResolution { get; set; } = null!;

    [Option('s', "source", HelpText = "Path to IR satellite image(s)", Required = true)]
    public string? SourcePath { get; set; }

    [Option('S', "saturation", HelpText = "Saturation adjustment", Required = false, Default = 0.7f)]
    public float Saturation { get; set; }

    [Option('t', "tint", HelpText = "Tint to apply to satellite image", Required = false, Default = "1b3f66")]
    public string Tint { get; set; } = null!;

    [Option('T', "timestamp", HelpText = "Target timestamp in UTC if stitching multiple files; e.g. 2020-12-20T23:00:30", Required = false)]
    public DateTimeOffset? Timestamp { get; set; }

    [Option('u', "underlay", HelpText = "Path to full-colour equirectangular underlay image", Required = false)]
    public string? UnderlayPath { get; set; }

    [Option('U', "nounderlay", HelpText = "If no underlay should be rendered", Required = false)]
    public bool NoUnderlay { get; set; }

    [Option('v', "verbose", HelpText = "Verbose console output", Required = false, Default = false)]
    public bool Verbose { get; set; }

    /// <summary>
    ///     Identifies whether <see cref="SourcePath"/> is referring to a directory or a file.
    /// </summary>
    public bool MultipleSources => SourcePath != null && SourcePath.Contains('*') || Directory.Exists(SourcePath);
}