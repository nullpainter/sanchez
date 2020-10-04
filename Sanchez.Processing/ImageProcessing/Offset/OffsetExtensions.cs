using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;

namespace Sanchez.Processing.ImageProcessing.Offset
{
    public static class OffsetExtensions
    {
        public static void HorizontalOffset(this Image<Rgba32> image, int amount)
        {
            var operation = new OffsetRowOperation(image, amount);
            ParallelRowIterator.IterateRows<OffsetRowOperation, Rgba32>(Configuration.Default, image.Bounds(), in operation);
        }
    }
}