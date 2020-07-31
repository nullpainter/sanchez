using System.Linq;
using Sanchez.Models;
using Serilog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Sanchez.Builders
{
    /// <summary>
    ///     Primary class to compose satellite images.
    /// </summary>
    internal class CompositorBuilder
    {
        /// <summary>
        ///     Source images
        /// </summary>
        private readonly ImageStack _stack;
        private readonly RenderOptions _options;

        /// <summary>
        ///     Image being composited
        /// </summary>
        private readonly Image _image;
        
        /// <summary>
        ///     Creates a new builder.
        /// </summary>
        /// <param name="stack">image to compose</param>
        /// <param name="options">render options</param>
        public CompositorBuilder(ImageStack stack, RenderOptions options)
        {
            _stack = stack;
            _options = options;

            _image = _stack.Underlay!;
        }

        /// <summary>
        ///     Blends the underlay image with the satellite image.
        /// </summary>
        public CompositorBuilder AddUnderlay()
        {
            _image.Mutate(context => context.DrawImage(_stack.Satellite, PixelColorBlendingMode.Screen, 1.0f));
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
        public CompositorBuilder AddMask()
        {
            if (_stack.Mask == null) return this;

            Compose(_stack.Mask, PixelColorBlendingMode.Multiply);
            return this;
        }

        /// <summary>
        ///     Adds an overlay image.
        /// </summary>
        public CompositorBuilder AddOverlay()
        {
            if (_stack.Overlay == null) return this;

            Compose(_stack.Overlay);
            return this;
        }

        /// <summary>
        ///     Composes the underlay with a target image.
        /// </summary>
        private void Compose(Image target, PixelColorBlendingMode blendingMode = PixelColorBlendingMode.Normal)
        {
            _image.Mutate(context => context.DrawImage(target, blendingMode, 1.0f));
        }

        /// <summary>
        ///     Saves the composited image.
        /// </summary>
        public void Save(string outputFilename) => _image.Save(outputFilename);

        /// <summary>
        ///     Resizes each image in a collection to the largest image in the collection.
        /// </summary>
        /// <remarks>
        ///    It is assumed that each image has the same aspect ratio. If not, warping will occur.
        /// </remarks>
        public CompositorBuilder Scale()
        {
            var images = _stack.All.ToList();
            
            var maxWidth = images.Max(image => image.Width);
            var maxHeight = images.Max(image => image.Height);

            // Find the image which has the largest width and height
            var largestImage = images.FirstOrDefault(i => i.Width == maxWidth && i.Height == maxHeight);
            if (largestImage == null) return this;

            // Don't do anything if all images are the same size
            if (images.Min(image => image.Width) >= maxWidth && images.Min(image => image.Height) >= maxHeight) return this;
            
            images.ForEach(image =>
            {
                Log.Information("Resizing to max dimension of {width}x{height}", maxWidth, maxHeight);
 
                image.Mutate(context => context.Resize(largestImage.Width, largestImage.Height));
            });

            return this;
        }
    }
}