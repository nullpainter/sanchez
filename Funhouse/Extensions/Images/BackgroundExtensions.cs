using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Funhouse.Extensions.Images
{
    public static class BackgroundExtensions
    {
        internal static Image<Rgba32> AddBackgroundColour(this Image<Rgba32> source, Color backgroundColour)
        {
            
            source.Mutate(context =>
            {
                using var original = source.Clone();

                context.Fill(backgroundColour);
                context.DrawImage(original, PixelColorBlendingMode.Normal, 1.0f);
            });

            return source;
        } 
    }
}