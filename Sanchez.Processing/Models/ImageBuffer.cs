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
        var buffer = new Rgba32[image.Width * image.Height];
        image.CopyPixelDataTo(buffer);
     
        return buffer;
    }
}