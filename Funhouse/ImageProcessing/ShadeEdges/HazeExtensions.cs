using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Funhouse.ImageProcessing.ShadeEdges
{
    public static class HazeExtensions
    {
        public static void ApplyHaze(this Image<Rgba32> image, Color tint, float hazeAmount)
        {
            var mask = new Image<Rgba32>(image.Width, image.Height);
            var operation = new HazeRowOperation(mask, tint, hazeAmount);
            ParallelRowIterator.IterateRows(Configuration.Default, mask.Bounds(), in operation);
            
            image.Mutate(context => context.DrawImage(mask, PixelColorBlendingMode.Screen, 1.0f));
        }
    }
}