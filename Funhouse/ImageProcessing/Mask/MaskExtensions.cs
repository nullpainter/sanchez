using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;

namespace Funhouse.ImageProcessing.Mask
{
    public static class MaskExtensions
    {
        public static void MaskEarth(this Image<Rgba32> source, bool blur)
        {
            var operation = new MaskRowOperation(source, blur);
            ParallelRowIterator.IterateRows(Configuration.Default, source.Bounds(), in operation);
        }
    }
}