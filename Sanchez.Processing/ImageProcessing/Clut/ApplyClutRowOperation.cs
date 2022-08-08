using Sanchez.Processing.ImageProcessing.Intensity;
using Sanchez.Processing.Models.Angles;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;

namespace Sanchez.Processing.ImageProcessing.Clut;

public readonly struct ApplyClutRowOperation : IRowOperation
{
    private readonly Image<Rgba32> _source;
    private readonly Image<Rgba32> _target;
    private readonly List<Rgba32> _clut;
    private readonly AngleRange _intensityRange;

    public ApplyClutRowOperation(Image<Rgba32> source, Image<Rgba32> target, List<Rgba32> clut)
    {
        _source = source;
        _target = target;
        _clut = clut;

        _intensityRange = source.GetIntensityRange();
    }

    public void Invoke(int y)
    {
        var sourceSpan = _source.GetPixelRowSpan(y);
        var targetSpan = _target.GetPixelRowSpan(y);

        for (var x = 0; x < sourceSpan.Length; x++)
        {
            // Ignore transparent areas
            if (sourceSpan[x].A == 0) continue;

            var intensity = (int)Math.Round((sourceSpan[x].R - _intensityRange.Start) / (_intensityRange.End - _intensityRange.Start) * 255);
            targetSpan[x] = _clut[intensity];
        }
    }
}