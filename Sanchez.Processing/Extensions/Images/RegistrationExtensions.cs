using Sanchez.Processing.ImageProcessing.Mask;
using Sanchez.Processing.Models;
using Sanchez.Processing.Models.Projections;
using SixLabors.ImageSharp.Processing;

namespace Sanchez.Processing.Extensions.Images;

public static class RegistrationExtensions
{
    public static void Normalise(this Registration registration, RenderOptions options)
    {
        registration
            .CropBorders()
            .NormaliseSize(options.ImageSize)
            .RemoveBackground();

        if (registration.Definition.Invert)
        {
            registration.Image.Mutate(c => c.Invert());
        }

        if (options.AutoAdjustLevels)
        {
            registration.Image!.AdjustLevels(options.AdaptiveLevelAdjustment);

            // Only apply brightness adjustment for stitched images
            if (options.StitchImages)
            {
                registration.Image.Mutate(c => c.Brightness(registration.Definition.Brightness));
            }
        }
    }

    private static void Mutate(this Registration registration, Action<IImageProcessingContext> operation)
    {
        ArgumentNullException.ThrowIfNull(registration.Image);
        registration.Image.Mutate(operation);
    }

    /// <summary>
    ///     Mask all pixels outside the Earth to assist image stitching of projected images.
    /// </summary>
    private static void RemoveBackground(this Registration registration)
    {
        ArgumentNullException.ThrowIfNull(registration.Image);
        registration.Image.RemoveBackground();
    }

    private static Registration CropBorders(this Registration registration)
    {
        ArgumentNullException.ThrowIfNull(registration.Image);
        if (registration.Definition.Crop == null) return registration;
            
        var crop = (double[])registration.Definition.Crop.Clone();
                
        // Swap left and right edge crop values if the image needs to be cropped on the left
        if (registration.FlipHorizontalCrop)
        {
            (crop[1], crop[3]) = (crop[3], crop[1]);
        }

        // Perform crop
        registration.Image.CropBorder(crop);

        return registration;
    }

    /// <summary>
    ///     Normalises the source image size to match the specified output spatial resolution. Doing
    ///     so simplifies maths when blending multiple pages.
    /// </summary>
    private static Registration NormaliseSize(this Registration registration, int imageSize)
    {
        ArgumentNullException.ThrowIfNull(registration.Image);
        if (registration.Width != imageSize || registration.Height != imageSize)
        {
            registration.Mutate(c => c.Resize(imageSize, imageSize, KnownResamplers.Welch));
        }

        return registration;
    }
}