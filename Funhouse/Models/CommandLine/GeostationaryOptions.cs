using CommandLine;
using JetBrains.Annotations;

namespace Funhouse.Models.CommandLine
{
    /// <summary>
    ///     All command-line options supported by Sanchez.
    /// </summary>
    [UsedImplicitly]
    [Verb("geostationary", true, HelpText = "Composite a full-colour underlay with geostationary IR satellite images")]
    public class GeostationaryOptions : CommandLineOptions
    {
        [Option('l', "longitude", HelpText = "Target longitude for geostationary satellite projection", Required = false)]
        public double? Longitude { get; [UsedImplicitly] set; }

        [Option('h', "haze", HelpText = "Amount of haze to apply to image; valid values are between 0 (no haze) and 1 (full haze)", Required = false, Default = 0.2f)]
        public float HazeAmount { get; [UsedImplicitly] set; }

        [Option('L', "noadjustlevels", HelpText = "Don't perform histogram equalisation", Required = false, Default = false)]
        public bool NoAutoAdjustLevels { get; [UsedImplicitly] set; }
    }
}