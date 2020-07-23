using Sanchez.Models;
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
    internal static class CompositorExtensions
    {
        /// <summary>
        ///     Blends the underlay image with the satellite image.
        /// </summary>
        internal static IImageProcessingContext BlendUnderlayImage(this IImageProcessingContext context, Image satellite)
        {
            return context.DrawImage(satellite, PixelColorBlendingMode.Screen, 1.0f);
        }

        /// <summary>
        ///     Performs global post-processing on the composited image.
        /// </summary>
        internal static IImageProcessingContext PostProcess(this IImageProcessingContext context, CommandLineOptions options)
        {
            context
                .Brightness(options.Brightness)
                .Saturate(options.Saturation);

            return context;
        }

        /// <summary>
        ///     Adds a colour tint to the IR image. Without a tint, darker colours in the image result in a
        ///     grey shade on the underlay.
        /// </summary>
        internal static void TintSatelliteImage(this Image satellite, Color tint)
        {
            // IR satellite image with equalised histogram in order to enhance cloud contrast
            var originalSatellite = satellite.Clone(c =>
            {
                c.HistogramEqualization(new HistogramEqualizationOptions
                {
                    Method = HistogramEqualizationMethod.Global
                });
            });

            // Apply transformations to the satellite image, tinting it and blending with the original
            // satellite image to result in an image which removes the overall grey tint while preserving
            // white clouds.
            satellite.Mutate(satelliteContext =>
            {
                satelliteContext
                    .Tint(tint)
                    .DrawImage(originalSatellite, PixelColorBlendingMode.HardLight, 1.0f);
            });
        }

        /// <summary>
        ///     Adds a full disc mask image if required in order to mask discrepancies between the full disk image
        ///     and the underlay or to add shadows.
        /// </summary>
        /// <remarks>
        ///    This mask is multiplied with the composite image so isn't entirely suitable for adding textual information
        ///     or other graphics to the image.
        /// </remarks>
        internal static IImageProcessingContext AddMask(this IImageProcessingContext context, CommandLineOptions options)
        {
            if (options.MaskPath == null) return context;

            using var mask = Image.Load(options.MaskPath);
            context.DrawImage(mask, PixelColorBlendingMode.Multiply, 1.0f);

            return context;
        }
    }
}