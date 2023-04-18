using Sanchez.Processing.Models.Angles;

namespace Sanchez.Processing.ImageProcessing.Intensity;

public static class IntensityRangeHelper
{
    /// <summary>
    ///     Identifies the minimum and maximum pixel intensity of a monochrome image.
    /// </summary>
    public static AngleRange GetIntensityRange(this Image<Rgba32> source)
    {
        var minIntensity = 1.0;
        var maxIntensity = 0.0;

        source.Mutate(c => c.ProcessPixelRowsAsVector4(row =>
        {
            for (var x = 0; x < row.Length; x++)
            {
                // Image is assumed to be monochrome, so we can arbitrarily select a single channel for the intensity
                var intensity = row[x].X;

                if (intensity < minIntensity) minIntensity = intensity;
                if (intensity > maxIntensity) maxIntensity = intensity;
            }
        }));

        return new AngleRange(minIntensity, maxIntensity);
    }
}