using System.Collections.Concurrent;
using System.Numerics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Sanchez.Processing.ImageProcessing.Crop;

/// <summary>
///     Identifies whether an EWS-G1 image requires cropping on its left or right edge, as these images aren't centered in the frame and the alignment
///     shifts every 12 hours.
/// </summary>
// FIXME in later versions of the encoding software, EXIF tags are present to indicate alignment. This should be used in preference to this hac.
public class EwsAlignmentRowOperation
{
    private readonly ConcurrentBag<int> _matchedPositions;

    private const float Threshold = 0.20f;

    public EwsAlignmentRowOperation() => _matchedPositions = new ConcurrentBag<int>();

    public bool IsRightCrop(Image<Rgba32> image) => _matchedPositions.Average() < image.Width / 2f;

    public void Invoke(Span<Vector4> row)
    {
        for (var x = 0; x < row.Length; x++)
        {
            var intensity = row[x].X;
            if (intensity > Threshold) _matchedPositions.Add(x);
        }
    }
}