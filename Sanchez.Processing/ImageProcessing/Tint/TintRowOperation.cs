using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;

namespace Sanchez.Processing.ImageProcessing.Tint;

public readonly struct TintRowOperation : IRowOperation
{
    private readonly Image<Rgba32> _source;
    private readonly Rgba32 _tint;
    private readonly float _tintLightness;

    public TintRowOperation(Image<Rgba32> source, Rgba32 tint, float tintLightness)
    {
        _source = source;
        _tint = tint;
        _tintLightness = tintLightness;
    }

    public void Invoke(int y)
    {
        var span = _source.GetPixelRowSpan(y);

        for (var x = 0; x < span.Length; x++)
        {
            var colour = span[x];
                
            var targetColour = ColorizeSinglePixel(colour, _tint);
                
            // Apply target colour, preserving alpha
            span[x].R = targetColour.R;
            span[x].G = targetColour.G;
            span[x].B = targetColour.B; 
        }
    }

    private Rgba32 ColorizeSinglePixel(Rgba32 pixel, Rgba32 tint)
    {
        // Assume greyscale
        var value = pixel.R / 255f;

        // Always tint; never shade
        return Blend3(Color.Black, tint, Color.White, _tintLightness * (value - 1) + 1);
    }

    private static Rgba32 Blend2(Rgba32 left, Rgba32 right, float pos)
    {
        return new(
            left.R / 255f * (1 - pos) + right.R / 255f * pos,
            left.G / 255f * (1 - pos) + right.G / 255f * pos,
            left.B / 255f * (1 - pos) + right.B / 255f * pos);
    }

    private static Rgba32 Blend3(Rgba32 left, Rgba32 main, Rgba32 right, float pos)
    {
        if (pos < 0) return Blend2(left, main, pos + 1);
        if (pos > 0) return Blend2(main, right, pos);

        return main;
    }
}