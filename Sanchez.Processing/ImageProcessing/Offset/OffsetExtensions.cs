namespace Sanchez.Processing.ImageProcessing.Offset;

public static class OffsetExtensions
{
    public static void HorizontalOffset(this Image<Rgba32> image, int amount)
    {
        var operation = new OffsetRowOperation(amount);
        image.Mutate(c => c.ProcessPixelRowsAsVector4(row => operation.Invoke(row)));
    }
}