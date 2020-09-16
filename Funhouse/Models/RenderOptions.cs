using System;
using System.IO;
using Funhouse.Models.Configuration;
using SixLabors.ImageSharp;

namespace Funhouse.Models
{
    /// <summary>
    ///     Rendering options used to composite the image.
    /// </summary>
    public sealed class RenderOptions
    {
        public EquirectangularRenderOptions? EquirectangularRender { get; set; }
        public GeostationaryRenderOptions? GeostationaryRender { get; set; }

        public ProjectionType Projection => GeostationaryRender != null ? ProjectionType.Geostationary : ProjectionType.Equirectangular;
        public bool StitchImages => Projection == ProjectionType.Equirectangular || GeostationaryRender?.Longitude != null;

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
                
                // Equirectangular projection always composites to a single image
                if (Projection == ProjectionType.Equirectangular) return false;
                
                // Multiple output files are written for geostationary render if images aren't being composited
                return GeostationaryRender?.Longitude == null;
            }
        }

        public DateTime? TargetTimestamp { get; set; }
        public TimeSpan Tolerance { get; set; }
        public int NumImagesParallel { get; set; }
    }
}
