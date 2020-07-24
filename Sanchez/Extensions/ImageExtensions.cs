using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Normalization;
using Color = System.Drawing.Color;

namespace Sanchez.Extensions
{
    /// <summary>
    ///     Methods to composite IR satellite images with an underlay, together with blending and post-processing operations.
    /// </summary>
    internal static class ImageExtensions
    {
        /// <summary>
        ///     Adds a colour tint to the IR image. Without a tint, darker colours in the image result in a
        ///     grey shade on the underlay.
        /// </summary>
        internal static void TintAndBlend(this Image image, Color tint)
        {
            // IR satellite image with equalised histogram in order to enhance cloud contrast
            var originalImage = image.Clone(context =>
            {
                context.HistogramEqualization(new HistogramEqualizationOptions
                {
                    Method = HistogramEqualizationMethod.Global
                });
            });

            // Apply transformations to the satellite image, tinting it and blending with the original
            // satellite image to result in an image which removes the overall grey tint while preserving
            // white clouds.
            image.Mutate(context =>
            {
                context
                    .Tint(tint)
                    .DrawImage(originalImage, PixelColorBlendingMode.HardLight, 1.0f);
            });
        }
    }
}