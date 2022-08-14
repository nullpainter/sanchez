using System.Collections.ObjectModel;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.ColorSpaces;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Sanchez.Processing.ImageProcessing.Atmosphere;

public static class AtmosphereExtensions
{
    public static void ApplyAtmosphere(this Image<Rgba32> image, float amount, float opacity, ReadOnlyCollection<CieLch> gradient)
    {
        var mask = new Image<Rgba32>(image.Width, image.Height);
        
        var operation = new AtmosphereRowOperation(mask, amount, opacity, gradient);
        mask.Mutate(c => c.ProcessPixelRowsAsVector4((row, point) => operation.Invoke(row, point)));
        
        image.Mutate(context => context.DrawImage(mask, PixelColorBlendingMode.Add, 1f));
    }
}