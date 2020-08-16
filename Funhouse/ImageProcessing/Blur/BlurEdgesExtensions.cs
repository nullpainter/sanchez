using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;

namespace Funhouse.ImageProcessing.Blur
{
    public static class BlurEdgesExtensions
    {
        public static void BlurEdges(this Image<Rgba32> source, float amount)
        {
            var operation = new BlurEdgesRowOperation(amount, source);
            ParallelRowIterator.IterateRows(Configuration.Default, source.Bounds(), in operation);
        }
    }
}