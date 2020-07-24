using System;
using System.IO;
using Ardalis.GuardClauses;
using Sanchez.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Sanchez.Builders
{
    /// <summary>
    ///     Primary class to compose satellite images.
    /// </summary>
    internal class CompositorBuilder
    {
        private readonly Image _image;
        private readonly RenderOptions _options;

        /// <summary>
        ///     Creates a new builder.
        /// </summary>
        /// <param name="image">image to compose</param>
        /// <param name="options">render options</param>
        public CompositorBuilder(Image image, RenderOptions options)
        {
            _image = image;
            _options = options;
        }

        /// <summary>
        ///     Blends the underlay image with the satellite image.
        /// </summary>
        public CompositorBuilder AddUnderlay(Image satellite)
        {
            _image.Mutate(context => context.DrawImage(satellite, PixelColorBlendingMode.Screen, 1.0f));
            return this;
        }

        /// <summary>
        ///     Performs global post-processing on the composited image.
        /// </summary>
        public CompositorBuilder PostProcess()
        {
            _image.Mutate(context =>
            {
                context
                    .Brightness(_options.Brightness)
                    .Saturate(_options.Saturation);
            });

            return this;
        }

        /// <summary>
        ///     Adds a full disc mask image if required in order to mask discrepancies between the full disk image
        ///     and the underlay or to add shadows.
        /// </summary>
        /// <remarks>
        ///     This mask is multiplied with the composite image so isn't entirely suitable for adding textual information
        ///     or other graphics to the image.
        /// </remarks>
        public CompositorBuilder AddMask(string? path)
        {
            if (!_options.RenderMask) return this;
            Guard.Against.Null(path, nameof(path));

            Compose(path!, PixelColorBlendingMode.Multiply);
            return this;
        }

        /// <summary>
        ///     Adds an overlay image.
        /// </summary>
        public CompositorBuilder AddOverlay(string? path)
        {
             if (!_options.RenderOverlay) return this;
            Guard.Against.Null(path, nameof(path));

            Compose(path!);
            return this;
        }

        private void Compose(string path, PixelColorBlendingMode blendingMode = PixelColorBlendingMode.Normal)
        {
            _image.Mutate(context =>
            {
                using var target = Image.Load(path);
                context.DrawImage(target, blendingMode, 1.0f);
            });
        }

        /// <summary>
        ///     Saves the composited image.
        /// </summary>
        /// <param name="file">path to output file</param>
        /// <returns>full path to output file</returns>
        public string Save(string file)
        {
            using var outputStream = new FileStream(file, FileMode.Create);
            _image.Save(outputStream, GetEncoder());

            return outputStream.Name;
        }

        /// <summary>
        ///     Returns the image encoder based for the selected image format.
        /// </summary>
        private IImageEncoder GetEncoder()
        {
            return _options.OutputFormat!switch
            {
                ImageFormat.Jpeg => new JpegEncoder { Quality = 85 },
                ImageFormat.Png => new PngEncoder(),
                _ => throw new InvalidOperationException($"Unhandled output format: {_options.OutputFormat}")
            };
        }
    }
}