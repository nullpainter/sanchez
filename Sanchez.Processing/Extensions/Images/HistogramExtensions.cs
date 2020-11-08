using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Normalization;

namespace Sanchez.Processing.Extensions.Images
{
    public static class HistogramExtensions
    {
        private static HistogramEqualizationOptions AdaptiveTileOptions = new HistogramEqualizationOptions
        {
            Method = HistogramEqualizationMethod.AdaptiveTileInterpolation,
            ClipHistogram = true,
            LuminanceLevels = 65536
        };

        public static void AdjustLevels(this Image<Rgba32> image, bool applyClahe = true)
        {
            if (applyClahe) image.Mutate(c => c.HistogramEqualization(AdaptiveTileOptions));

            
            // Apply global histogram equalisation to normalise images from different satellites, and adaptive equalisation
            // to bring out regions of interest.
            image.Mutate(c => c.HistogramEqualization());
        }
    }
}