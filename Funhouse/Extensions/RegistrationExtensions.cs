using System;
using Ardalis.GuardClauses;
using Funhouse.Extensions.Images;
using Funhouse.ImageProcessing.Mask;
using Funhouse.Models;
using Funhouse.Models.Projections;
using SixLabors.ImageSharp.Processing;

namespace Funhouse.Extensions
{
    public static class RegistrationExtensions
    {
        public static void Normalise(this Registration registration, RenderOptions options, bool autoAdjustLevels = true)
        {
            registration
                .CropBorders()
                .NormaliseSize(options.ImageSize)
                .RemoveBackground();

            if (autoAdjustLevels)
            {
                registration.Mutate(c => c
                    .HistogramEqualization()
                    .Brightness(registration.Definition.Brightness));
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
        private static Registration RemoveBackground(this Registration registration)
        {
            Guard.Against.Null(registration.Image, nameof(registration.Image));
            registration.Image.RemoveBackground();

            return registration;
        }

        private static Registration CropBorders(this Registration registration)
        {
            Guard.Against.Null(registration.Image, nameof(registration.Image));
            if (registration.Definition.Crop != null) registration.Image.AutoCropBorder(registration.Definition.Crop);

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