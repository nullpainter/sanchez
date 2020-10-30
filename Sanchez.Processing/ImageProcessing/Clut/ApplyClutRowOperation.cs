using System.Collections.Generic;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;

namespace Sanchez.Processing.ImageProcessing.Clut
{
    public readonly struct ApplyClutRowOperation : IRowOperation
    {
        private readonly Image<Rgba32> _source;
        private readonly Image<Rgba32> _target;
        private readonly List<Rgba32> _clut;

        public ApplyClutRowOperation(Image<Rgba32> source, Image<Rgba32> target, List<Rgba32> clut)
        {
            _source = source;
            _target = target;
            _clut = clut;
        }

        public void Invoke(int y)
        {
            var sourceSpan = _source.GetPixelRowSpan(y);
            var targetSpan = _target.GetPixelRowSpan(y);

            for (var x = 0; x < sourceSpan.Length; x++)
            {
                // Ignore transparent areas
                if (sourceSpan[x].A == 0) continue;

                var intensity = sourceSpan[x].R;
                targetSpan[x] = _clut[intensity];
            }
        }
    }
}