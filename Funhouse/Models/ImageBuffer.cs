using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Funhouse.Models
{
    public class ImageBuffer
    {
        public Rgba32[] Buffer { get; }
        public Size Size { get; }

        private ImageBuffer(Rgba32[] buffer, Size size)
        {
            Buffer = buffer;
            Size = size;
        }

        public static ImageBuffer ToBuffer(Image<Rgba32> image) => new ImageBuffer(GetBuffer(image), image.Size());

        private static Rgba32[] GetBuffer(Image<Rgba32> image)
        {
            if (!image.TryGetSinglePixelSpan(out var pixelSpan)) throw new InvalidOperationException("Unable to retrieve image buffer");
            return pixelSpan.ToArray();
        }
    }
}