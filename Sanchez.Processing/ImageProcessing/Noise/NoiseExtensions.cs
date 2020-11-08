using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Sanchez.Processing.ImageProcessing.Noise
{
    public static class NoiseExtensions
    {
        public static void ApplyGaussianNoise(this Image<Rgba32> image)
        {
            var noiseImage = image.Clone();

            var operation = new GaussianNoiseRowOperation(noiseImage);
            ParallelRowIterator.IterateRows(Configuration.Default, noiseImage.Bounds(), in operation);
            
            image.Mutate(c => c.DrawImage(noiseImage, PixelColorBlendingMode.Multiply, 0.1f));
        }
    }
}