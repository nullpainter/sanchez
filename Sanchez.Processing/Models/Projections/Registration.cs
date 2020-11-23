using System;
using System.Threading.Tasks;
using Sanchez.Processing.Models.Configuration;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Sanchez.Processing.Models.Projections
{
    /// <summary>
    ///     Wrapper around satellite imagery, containing the image itself and associated metadata used
    ///     for interpreting and reprojecting the image.
    /// </summary>
    public sealed class Registration : IDisposable
    {
        public Registration(string path, SatelliteDefinition definition, DateTime? timestamp)
        {
            Path = path;
            Definition = definition;
            Timestamp = timestamp;
        }

        public SatelliteDefinition Definition { get; }

        /// <summary>
        ///     Timestamp of image, extracted from filename.
        /// </summary>
        public DateTime? Timestamp { get; }

        private Image<Rgba32>? _image;

        public Image<Rgba32>? Image
        {
            get => _image;
            set
            {
                _image?.Dispose();
                _image = value;
            }
        }

        public int Width => Image!.Width;
        public int Height => Image!.Height;

        /// <summary>
        ///     Full path to source image.
        /// </summary>
        public string Path { get; }

        /// <summary>
        ///     Horizontal offset when rendering on an equirectangular map.
        /// </summary>
        public int OffsetX { get; set; }

        /// <summary>
        ///     Visible longitude range of satellite, minus overlapping ranges if required.
        /// </summary>
        public ProjectionRange? LongitudeRange { get; set; }

        /// <summary>
        ///     Visible latitude range of satellite.
        /// </summary>
        public ProjectionRange? LatitudeRange { get; set; }

        /// <summary>
        ///     Path to output file, if saving an image per satellite image.
        /// </summary>
        public string? OutputPath { get; set; }

        public void Dispose()
        {
            Image?.Dispose();
            Image = null;
        }

        public async Task LoadAsync()
        {
            if (Image != null) throw new InvalidOperationException("Image has already been loaded");
            Image = await SixLabors.ImageSharp.Image.LoadAsync<Rgba32>(Path);
        }
    }
}