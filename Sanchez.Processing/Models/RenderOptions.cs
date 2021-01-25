using System;
using System.IO;
using Sanchez.Processing.Models.Configuration;
using Sanchez.Processing.Models.Projections;
using SixLabors.ImageSharp;

namespace Sanchez.Processing.Models
{
    /// <summary>
    ///     Rendering options used to composite the image.
    /// </summary>
    public sealed class RenderOptions
    {
        public EquirectangularRenderOptions? EquirectangularRender { get; set; }
        public GeostationaryRenderOptions? GeostationaryRender { get; set; }

        public ProjectionType Projection => GeostationaryRender != null ? ProjectionType.Geostationary : ProjectionType.Equirectangular;
        public bool StitchImages => EquirectangularRender?.StitchImages == true || GeostationaryRender?.Longitude != null;

        /// <summary>
        ///     Global brightness adjustment.
        /// </summary>
        public float Brightness { get; set; }

        /// <summary>
        ///    Interpolation type.
        /// </summary>
        public InterpolationType InterpolationType { get; set; }

        /// <summary>
        ///     Whether logs should be output to console.
        /// </summary>
        public bool Quiet { get; set; }

        /// <summary>
        ///     Path to output file or folder.
        /// </summary>
        public string OutputPath { get; set; } = null!;

        /// <summary>
        ///    Spatial resolution. 
        /// </summary>
        public int SpatialResolution { get; set; }

        /// <summary>
        ///     Path to IR satellite image(s).
        /// </summary>
        public string SourcePath { get; set; } = null!;

        /// <summary>
        ///     Saturation adjustment.
        /// </summary>
        public float Saturation { get; set; }

        /// <summary>
        ///     Tint to apply to satellite image.
        /// </summary>
        public Color Tint { get; set; }

        /// <summary>
        ///     Path to custom full-colour underlay image.
        /// </summary>
        public string UnderlayPath { get; set; } = Constants.DefaultUnderlayPath;

        /// <summary>
        ///     Path to satellite definitions.
        /// </summary>
        public string DefinitionsPath { get; set; } = Constants.DefaultDefinitionsPath;

        /// <summary>
        ///     If no underlay should be rendered.
        /// </summary>
        public bool NoUnderlay { get; set; }

        /// <summary>
        ///     Verbose console output.
        /// </summary>
        public bool Verbose { get; set; }

        /// <summary>
        ///     Target edge length in pixels of normalised satellite imagery.
        /// </summary>
        public int ImageSize { get; set; }

        /// <summary>
        ///    Satellite image offset factors for mapping pixels to angles, based on selected spatial resolution. 
        /// </summary>
        public ImageOffset? ImageOffset { get; set; }

        /// <summary>
        ///     Whether existing files should be overwritten.
        /// </summary>
        public bool Force { get; set; }

        /// <summary>
        ///     Identifies whether <see cref="SourcePath"/> is referring to a directory or a file.
        /// </summary>
        public bool MultipleSources => SourcePath?.Contains('*') == true || Directory.Exists(SourcePath);

        /// <summary>
        ///     Indicates whether the output should be a single file or a directory.
        /// </summary>
        public bool MultipleTargets
        {
            get
            {
                if (!MultipleSources) return false;

                // If an interval is provided, we are in timelapse mode
                if (Interval != null) return true;

                // Stitching multiple images in non-batch mode results in a single image
                if (Projection == ProjectionType.Equirectangular && EquirectangularRender!.StitchImages) return false;

                // Multiple output files are written for geostationary render if images aren't being composited
                return GeostationaryRender?.Longitude == null;
            }
        }

        /// <summary>
        ///     Target timestamp if combining multiple files.
        /// </summary>
        public DateTime? Timestamp { get; set; }

        /// <summary>
        ///     Optional end timestamp if combining multiple files.
        /// </summary>
        public DateTime? EndTimestamp { get; set; }

        /// <summary>
        ///     Interval between frames when performing batch conversion.
        /// </summary>
        public TimeSpan? Interval { get; set; }

        /// <summary>
        ///     Tolerance from <see cref="Timestamp"/> in identifying suitable satellite images when combining.
        /// </summary>
        public TimeSpan Tolerance { get; set; }

        /// <summary>
        ///     Whether the satellite histogram should be equalised. This should be disabled when creating animations with satellite
        ///     imagery which has large variance in brightness per frame.
        /// </summary>
        public bool AutoAdjustLevels { get; set; }

        /// <summary>
        ///     Minimum number of satellites in images when stitching. If there are less satellites than this number when stitching,
        ///     no image is produced.
        /// </summary>
        public int? MinSatellites { get; set; }

        public OverlayOptions Overlay { get; } = new();
    }
}