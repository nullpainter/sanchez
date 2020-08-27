using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;

namespace Funhouse.ImageProcessing.Mask
{
    public static class BlurExtensions
    {
        public static void BlurEdges(this Image<Rgba32> source)
        {
            var operation = new BlurEdgeRowOperation(source);
            ParallelRowIterator.IterateRows(Configuration.Default, source.Bounds(), in operation);
        }
    }
}