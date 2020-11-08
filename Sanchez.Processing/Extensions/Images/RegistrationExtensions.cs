using System;
using Ardalis.GuardClauses;
using Sanchez.Processing.ImageProcessing.Mask;
using Sanchez.Processing.Models;
using Sanchez.Processing.Models.Projections;
using SixLabors.ImageSharp.Processing;

namespace Sanchez.Processing.Extensions.Images
{
    public static class RegistrationExtensions
    {
        public static void Normalise(this Registration registration, RenderOptions options)
        {
            registration
                .CropBorders()
                .NormaliseSize(options.ImageSize)
                .RemoveBackground();

            if (options.AutoAdjustLevels)
            {
                registration.Image!.AdjustLevels();
                registration.Image.Mutate(c => c.Brightness(registration.Definition.Brightness));
            }
        }

        private static void Mutate(this Registration registration, Action<IImageProcessingContext> operation)
        {
            Guard.Against.Null(registration.Image, nameof(registration.Image));
            registration.Image.Mutate(operation);
        }

        /// <summary>
        ///     Mask all pixels outside the Earth to assist image stitching of projected images.
        /// </summary>
        private static void RemoveBackground(this Registration registration)
        {
            Guard.Against.Null(registration.Image, nameof(registration.Image));
            registration.Image.RemoveBackground();
        }

        private static Registration CropBorders(this Registration registration)
        {
            Guard.Against.Null(registration.Image, nameof(registration.Image));
            if (registration.Definition.Crop != null)
            {
                registration.Image.CropBorder(registration.Definition.Crop);
            }

            return registration;
        }

        /// <summary>
        ///     Normalises the source image size to match the specified output spatial resolution. Doing
        ///     so simplifies maths when blending multiple pages.
        /// </summary>
        public static Registration NormaliseSize(this Registration registration, int imageSize)
        {
            Guard.Against.Null(registration.Image, nameof(registration.Image));
            if (registration.Width != imageSize || registration.Height != imageSize)
            {
                registration.Mutate(c => c.Resize(imageSize, imageSize, KnownResamplers.Welch));
            }

            return registration;
        }
    }
}