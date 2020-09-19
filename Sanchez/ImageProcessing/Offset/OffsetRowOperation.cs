using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;

namespace Sanchez.ImageProcessing.Offset
{
    public readonly struct OffsetRowOperation : IRowOperation<Rgba32>
    {
        private readonly Image<Rgba32> _target;
        private readonly int _amount;

        public OffsetRowOperation(Image<Rgba32> target, int amount)
        {
            _target = target;
            _amount = amount;
        }

        public void Invoke(int y, Span<Rgba32> span)
        {
            var row = _target.GetPixelRowSpan(y);
            row.CopyTo(span);

            for (var x = 0; x < row.Length; x++)
            {
                var targetOffset = (x - _amount) % row.Length;
                row[x] = span[targetOffset];
            }
        }
    }
}