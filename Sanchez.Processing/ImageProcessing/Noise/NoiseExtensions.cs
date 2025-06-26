using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Sanchez.Processing.ImageProcessing.Noise;

public static class NoiseExtensions
{
    public static void ApplyGaussianNoise(this Image<Rgba32> image)
    {
        var noiseImage = image.Clone();

        noiseImage.Mutate(c => c.ProcessPixelRowsAsVector4(GaussianNoiseRowOperation.Invoke));
        image.Mutate(c => c.DrawImage(noiseImage, PixelColorBlendingMode.Overlay, 0.1f));
    }
}