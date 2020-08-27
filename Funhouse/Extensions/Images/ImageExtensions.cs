using Funhouse.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Normalization;

namespace Funhouse.Extensions.Images
{
    internal static class ImageExtensions
    {
        /// <summary>
        ///     Adds a colour tint to the IR image. Without a tint, darker colours in the image result in a
        ///     grey shade on the underlay.
        /// </summary>
        internal static void TintAndBlend(this Image image, RenderOptions options)
        {
            // Apply transformations to the satellite image, tinting it and blending with the original
            // satellite image to result in an image which removes the overall grey tint while preserving
            // white clouds.
            image.Mutate(context =>
            {
                // IR satellite image with equalised histogram in order to enhance cloud contrast
                using var originalImage = image.Clone(originalContext =>
                {
                    originalContext.HistogramEqualization(new HistogramEqualizationOptions
                    {
                        Method = HistogramEqualizationMethod.Global
                    });
                });
                
                context
                    .Tint(options.Tint!.Value)
                    .DrawImage(originalImage, PixelColorBlendingMode.HardLight, 1.0f);
            });
        }
    }
}