using Funhouse.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Funhouse.Extensions.Images
{
    public static class LevelsExtensions
    {
        public static Image<Rgba32> ColourCorrect(this Image<Rgba32> image, RenderOptions options)
        {
            // Correct brightness and saturation
            image.Mutate(context =>
            {
                context
                    .Brightness(options.Brightness)
                    .Saturate(options.Saturation);
            });
  
            return image;
        }
 
    }
}