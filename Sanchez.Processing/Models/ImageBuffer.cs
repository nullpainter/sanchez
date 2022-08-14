using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;

namespace Sanchez.Processing.Models;

public class ImageBuffer
{
    public Rgba32[] Buffer { get; }
    public Size Size { get; }

    private ImageBuffer(Rgba32[] buffer, Size size)
    {
        Buffer = buffer;
        Size = size;
    }

    public static ImageBuffer ToBuffer(Image<Rgba32> image) => new(GetBuffer(image), image.Size());

    private static Rgba32[] GetBuffer(Image<Rgba32> image)
    {
        // Return single span if present
        if (image.DangerousTryGetSinglePixelMemory(out var pixelSpan))
        {
            return pixelSpan.ToArray();
        }

        var memoryGroup = image.GetPixelMemoryGroup();

        // Combine spans for larger images
        var bufferSize = memoryGroup.Select(g => g.Span.Length).Sum();
        var buffer = new Rgba32[bufferSize];

        var offset = 0;
        foreach (var group in memoryGroup)
        {
            var span = group.Span;
            Array.Copy(span.ToArray(), 0, buffer, offset, span.Length);

            offset += span.Length;
        }

        return buffer;
    }
}