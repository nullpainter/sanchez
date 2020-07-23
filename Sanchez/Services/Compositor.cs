using Sanchez.Extensions;
using Sanchez.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Normalization;
using Color = System.Drawing.Color;

namespace Sanchez.Services
{
    internal static class Compositor
    {
        /// <summary>
        ///     Blends the underlay image with the satellite image.
        /// </summary>
        internal static IImageProcessingContext BlendUnderlayImage(this IImageProcessingContext context, Image satellite)
        {
            return context.DrawImage(satellite, PixelColorBlendingMode.Screen, 1.0f);
        }

        internal static IImageProcessingContext PostProcess(this IImageProcessingContext context, CommandLineOptions options)
        {
            context
                .Brightness(options.Brightness)
                .Saturate(options.Saturation);

            return context;
        }

        internal static void TintSatelliteImage(this Image satellite, Color tint)
        {
            var originalSatellite = satellite.Clone(c => { c.HistogramEqualization(new HistogramEqualizationOptions { Method = HistogramEqualizationMethod.Global }); });

            satellite.Mutate(satelliteContext =>
            {
                // Apply a tint to the satellite image and blend with original in order to remove
                // grey tint but preserve cloud colours.
                satelliteContext
                    .Tint(tint)
                    .DrawImage(originalSatellite, PixelColorBlendingMode.HardLight, 1.0f);
            });
        }

        /// <summary>
        ///     Optionally adds a full disc mask image in order to add textual information or to
        ///     mask discrepancies between the full disk image and the underlay.
        /// </summary>
        internal static IImageProcessingContext AddMask(this IImageProcessingContext context, CommandLineOptions options)
        {
            if (options.MaskPath == null) return context;

            using var mask = Image.Load(options.MaskPath);
            context.DrawImage(mask, PixelColorBlendingMode.Multiply, 1.0f);

            return context;
        }
    }
}