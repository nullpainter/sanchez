using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Sanchez.Processing.ImageProcessing.Haze
{
    public static class HazeExtensions
    {
        public static void ApplyHaze(this Image<Rgba32> image, Color tint, float amount, float opacity)
        {
            var mask = new Image<Rgba32>(image.Width, image.Height);
            var operation = new HazeRowOperation(mask, tint, amount, opacity);
            
            ParallelRowIterator.IterateRows(Configuration.Default, mask.Bounds(), in operation);
            
            image.Mutate(context => context.DrawImage(mask, PixelColorBlendingMode.Screen, 1f));
        }
    }
}