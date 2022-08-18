using System.Numerics;
using SixLabors.ImageSharp.PixelFormats;

namespace Sanchez.Processing.ImageProcessing.Tint;

public class TintRowOperation
{
    private readonly Vector4 _tint;
    private readonly float _tintLightness;

    public TintRowOperation(Rgba32 tint, float tintLightness)
    {
        _tint = tint.ToVector4();
        _tintLightness = tintLightness;
    }

    public void Invoke(Span<Vector4> row)
    {
        for (var x = 0; x < row.Length; x++)
        {
            var targetColour = ColorizeSinglePixel(row[x], _tint);

            // Apply target colour, preserving alpha
            row[x].X = targetColour.X;
            row[x].Y = targetColour.Y;
            row[x].Z = targetColour.Z;
        }
    }

    private Vector4 ColorizeSinglePixel(Vector4 pixel, Vector4 tint)
    {
        // Assume greyscale
        var value = pixel.X;

        // Always tint; never shade
        return Blend3(Vector4.Zero, tint, Vector4.One, _tintLightness * (value - 1) + 1);
    }

    private static Vector4 Blend2(Vector4 left, Vector4 right, float pos)
    {
        return new Vector4(
            left.X * (1 - pos) + right.X * pos,
            left.Y * (1 - pos) + right.Y * pos,
            left.Z * (1 - pos) + right.Z * pos, 1);
    }

    private static Vector4 Blend3(Vector4 left, Vector4 main, Vector4 right, float pos)
    {
        return pos switch
        {
            < 0 => Blend2(left, main, pos + 1),
            > 0 => Blend2(main, right, pos),
            _ => main
        };
    }
}