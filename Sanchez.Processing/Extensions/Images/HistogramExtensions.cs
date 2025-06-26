using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Normalization;

namespace Sanchez.Processing.Extensions.Images;

public static class HistogramExtensions
{
    /// <summary>
    ///     Options for performing adaptive tile histogram equalisation.
    /// </summary>
    private static readonly HistogramEqualizationOptions AdaptiveTileOptions = new()
    {
        Method = HistogramEqualizationMethod.AdaptiveTileInterpolation,
        ClipHistogram = true,
        LuminanceLevels = 65536
    };

    /// <summary>
    ///     Performs global histogram equalisation on a satellite image in order to increase contrast and
    ///     to assist in blending imagery from multiple satellites. Adaptive equalisation can also be performed
    ///     to increase contrast on localised regions of interest. 
    /// </summary>
    /// <param name="image">image to equalise</param>
    /// <param name="adaptive">whether adaptive tile histogram equalisation should also be performed</param>
    public static void AdjustLevels(this Image<Rgba32> image, bool adaptive)
    {
        if (adaptive) image.Mutate(c => c.HistogramEqualization(AdaptiveTileOptions));
        image.Mutate(c => c.HistogramEqualization());
    }
}