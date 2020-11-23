using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Sanchez.Processing.ImageProcessing.Intensity
{
    public static class IntensityRangeHelper
    {
        // TODO comment
        public static Range GetIntensityRange(this Image<Rgba32> source)
        {
            var minIntensity = 255;
            var maxIntensity = 0;

            // TODO can we paralleise this?
            for (var y = 0; y < source.Height; y++)
            {
                var sourceSpan = source.GetPixelRowSpan(y);

                for (var x = 0; x < sourceSpan.Length; x++)
                {
                    var intensity = sourceSpan[x].R;

                    if (intensity < minIntensity) minIntensity = intensity;
                    if (intensity > maxIntensity) maxIntensity = intensity;
                }
            }

            return new Range(minIntensity, maxIntensity);
        }
    }
}