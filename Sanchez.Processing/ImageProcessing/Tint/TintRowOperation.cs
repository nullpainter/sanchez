using System.Numerics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Sanchez.Processing.ImageProcessing.Tint;

public class TintRowOperation
{
    private readonly Rgba32 _tint;
    private readonly float _tintLightness;

    public TintRowOperation(Rgba32 tint, float tintLightness)
    {
        _tint = tint;
        _tintLightness = tintLightness;
    }

    public void Invoke(Span<Vector4> row)
    {
        for (var x = 0; x < row.Length; x++)
        {
            var targetColour = ColorizeSinglePixel(row[x], _tint);

            // Apply target colour, preserving alpha
            row[x].X = targetColour.R / 255f;
            row[x].Y = targetColour.G / 255f;
            row[x].Z = targetColour.B / 255f;
        }
    }

    private Rgba32 ColorizeSinglePixel(Vector4 pixel, Rgba32 tint)
    {
        // Assume greyscale
        var value = pixel.X;

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