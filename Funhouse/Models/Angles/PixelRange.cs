using System;
using Funhouse.Extensions;
using MathNet.Spatial.Units;

namespace Funhouse.Models.Angles
{
    public readonly struct PixelRange
    {
        public int Start { get; }
        public int End { get; }

        public PixelRange(int start, int end)
        {
            Start = start;
            End = end;
        }
        
        public PixelRange(Range range, Func<Angle, int> transform)
        {
            Start = transform(range.Start);
            End = transform(range.End);
        }

        public static PixelRange ToPixelRangeX(Range range, int width) => new PixelRange(range.Start.ToX(width), range.End.ToX(width));
        public static PixelRange ToPixelRangeY(Range range, int height) => new PixelRange(range.Start.ToY(height), range.End.ToY(height));
        
        public int Range => End - Start;
    }
}