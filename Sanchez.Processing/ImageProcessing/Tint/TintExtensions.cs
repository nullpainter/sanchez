using SixLabors.ImageSharp.ColorSpaces.Conversion;

namespace Sanchez.Processing.ImageProcessing.Tint;

public static class TintExtensions
{
    /// <summary>
    ///     Applies a tint to an image.
    /// </summary>
    public static void Tint(this Image<Rgba32> image, Color colour)
    {
        var tint = colour.ToPixel<Rgba32>();
        var lightness = 2 * (1 - (ColorSpaceConverter.ToHsl(tint).L * 2 - 1));

        var operation = new TintRowOperation(tint, lightness);
        image.Mutate(c => c.ProcessPixelRowsAsVector4(row => operation.Invoke(row)));
    }
}