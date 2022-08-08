namespace Sanchez.Processing.Models.Angles;

/// <summary>
///     Pixel coordinate range.
/// </summary>
public readonly record struct PixelRange(int Start, int End)
{
    /// <summary>
    ///     Construct a pixel range from an angle range, utilising a custom transformation function
    ///     to convert.
    /// </summary>
    public PixelRange(AngleRange range, Func<double, int> transform) : this(transform(range.Start), transform(range.End))
    {
        // Ensure end pixel is larger than start pixel. 
        if (Start > End)
        {
            (Start, End) = (End, Start);
        }
    }

    public int Range => End - Start;

    public override string ToString() => $"{nameof(Start)}: {Start}, {nameof(End)}: {End}";
}