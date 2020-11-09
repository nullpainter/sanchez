using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;

namespace Sanchez.Processing.Models
{
    public readonly struct ImageBuffer
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
            // Return single span if present
            if (image.TryGetSinglePixelSpan(out var pixelSpan))
            {
                return pixelSpan.ToArray();
            }

            // Combine spans for larger images
            var buffer = new Rgba32[image.Width * image.Height];

       
            var offset = 0;
            var memoryGroup = image.GetPixelMemoryGroup();
            for (var i = 0; i < memoryGroup.Count; i++)
            {
                var span = memoryGroup[i].Span;
                Array.Copy(span.ToArray(), 0, buffer, offset, span.Length);
                offset += (i + 1) * span.Length;
            }

            return buffer;
        }
    }
}