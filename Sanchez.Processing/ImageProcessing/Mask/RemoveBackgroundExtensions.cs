using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

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
        image.Mutate(c => c.ProcessPixelRowsAsVector4((row, point) => operation.Invoke(row, point)));
    }
}