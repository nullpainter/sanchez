namespace Sanchez.Processing.Extensions.Images;

public static class BackgroundExtensions
{
    public static Image<Rgba32> AddBackgroundColour(this Image<Rgba32> source, Color backgroundColour)
    {
        source.Mutate(context =>
        {
            using var original = source.Clone();

            context
                .BackgroundColor(backgroundColour)
                .DrawImage(original, PixelColorBlendingMode.Normal, 1.0f);
        });

        return source;
    } 
}