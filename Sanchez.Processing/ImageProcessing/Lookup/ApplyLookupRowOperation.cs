using Sanchez.Processing.ImageProcessing.Intensity;
using Sanchez.Processing.Models.Angles;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Sanchez.Processing.ImageProcessing.Lookup;

public class ApplyLookupRowOperation
{
    private readonly List<Rgba32> _lookup;
    private readonly AngleRange _intensityRange;

    public ApplyLookupRowOperation(Image<Rgba32> source, List<Rgba32> lookup)
    {
        _lookup = lookup;
        _intensityRange = source.GetIntensityRange();
    }

    public void Invoke(PixelAccessor<Rgba32> sourceAccessor, PixelAccessor<Rgba32> targetAccessor)
    {
        for (var y = 0; y < sourceAccessor.Height; y++)
        {
            var sourceRow = sourceAccessor.GetRowSpan(y);
            var targetRow = targetAccessor.GetRowSpan(y);

            for (var x = 0; x < sourceRow.Length; x++)
            {
                // Ignore transparent areas
                if (sourceRow[x].A == 0) continue;

                var intensity = (int)Math.Round((sourceRow[x].R - _intensityRange.Start) / (_intensityRange.End - _intensityRange.Start));
                targetRow[x] = _lookup[intensity];
            }
        }
    }
}