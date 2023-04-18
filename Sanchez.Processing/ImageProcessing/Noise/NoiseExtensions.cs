namespace Sanchez.Processing.ImageProcessing.Noise;

public static class NoiseExtensions
{
    public static void ApplyGaussianNoise(this Image<Rgba32> image)
    {
        var noiseImage = image.Clone();

        var operation = new GaussianNoiseRowOperation();
        noiseImage.Mutate(c => c.ProcessPixelRowsAsVector4(row => operation.Invoke(row)));
        image.Mutate(c => c.DrawImage(noiseImage, PixelColorBlendingMode.Overlay, 0.1f));
    }
}