using System;
using System.Threading.Tasks;
using Sanchez.Models.Configuration;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Range = Sanchez.Models.Angles.Range;

namespace Sanchez.Models.Projections
{
    /// <summary>
    ///     Wrapper around satellite imagery, containing the image itself and associated metadata used
    ///     for interpreting and reprojecting the image.
    /// </summary>
    public sealed class Registration : IDisposable
    {
        private bool _disposed;

        public Registration(string path, SatelliteDefinition definition)
        {
            Path = path;
            Definition = definition;
        }

        public SatelliteDefinition Definition { get; }

        public Image<Rgba32>? Image { get; set; }

        public int Width => Image!.Width;
        public int Height => Image!.Height;

        /// <summary>
        ///     Full path to image.
        /// </summary>
        public string Path { get; }

        /// <summary>
        ///     Horizontal offset when rendering on an equirectangular map.
        /// </summary>
        public int OffsetX { get; set; }

        /// <summary>
        ///     Visible longitude range of satellite, minus overlapping ranges if required.
        /// </summary>
        public Range LongitudeRange { get; set; }

        /// <summary>
        ///     Visible latitude range of satellite.
        /// </summary>
        public Range LatitudeRange { get; set; }

        /// <summary>
        ///     Path to output file, if saving an image per satellite image.
        /// </summary>
        public string? OutputPath { get; set; }

        public void Dispose()
        {
            if (_disposed) return;
            Image?.Dispose();

            _disposed = true;
        }

        public async Task LoadAsync()
        {
            if (Image != null) throw new InvalidOperationException("Image has already been loaded");
            Image = await SixLabors.ImageSharp.Image.LoadAsync<Rgba32>(Path);
        }
    }
}