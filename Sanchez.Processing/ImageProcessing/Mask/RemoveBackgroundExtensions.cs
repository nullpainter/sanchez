using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;

namespace Sanchez.Processing.ImageProcessing.Mask;

public static class RemoveBackgroundExtensions
{
    /// <summary>
    ///     Sets the background outside the Earth on geostationary images to be transparent in order to facilitate blending.
    /// </summary>
    /// <param name="image">source image</param>
    public static void RemoveBackground(this Image<Rgba32> image)
    {
        var operation = new RemoveBackgroundRowOperation(image);
        ParallelRowIterator.IterateRows(Configuration.Default, image.Bounds(), in operation);
    }
}