using System;
using Funhouse.Extensions;

namespace Funhouse.Models.Angles
{
    // TODO document
    public readonly struct PixelRange
    {
        public int Start { get; }
        public int End { get; }

        public PixelRange(int start, int end)
        {
            Start = start;
            End = end;
        }
        
        public PixelRange(Range range, Func<double, int> transform)
        {
            Start = transform(range.Start);
            End = transform(range.End);
        }

        public static PixelRange ToPixelRangeX(Range range, int width) => new PixelRange(range.Start.ToX(width), range.End.ToX(width));
        public static PixelRange ToPixelRangeY(Range range, int height) => new PixelRange(range.Start.ToY(height), range.End.ToY(height));
        
        public int Range => End - Start;
    }
}