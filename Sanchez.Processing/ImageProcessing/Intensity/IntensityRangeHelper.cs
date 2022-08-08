using Sanchez.Processing.Models.Angles;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Sanchez.Processing.ImageProcessing.Intensity;

public static class IntensityRangeHelper
{
    /// <summary>
    ///     Identifies the minimum and maximum pixel intensity of a monochrome image.
    /// </summary>
    public static AngleRange GetIntensityRange(this Image<Rgba32> source)
    {
        var minIntensity = 255;
        var maxIntensity = 0;

        for (var y = 0; y < source.Height; y++)
        {
            var sourceSpan = source.GetPixelRowSpan(y);

            for (var x = 0; x < sourceSpan.Length; x++)
            {
                // Image is assumed to be monochrome, so we can arbitrarily select a single channel for the intensity
                var intensity = sourceSpan[x].R;

                if (intensity < minIntensity) minIntensity = intensity;
                if (intensity > maxIntensity) maxIntensity = intensity;
            }
        }

        return new AngleRange(minIntensity, maxIntensity);
    }
}