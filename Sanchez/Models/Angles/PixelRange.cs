using System;

namespace Sanchez.Models.Angles
{
    /// <summary>
    ///     Pixel coordinate range.
    /// </summary>
    public readonly struct PixelRange
    {
        public int Start { get; }
        public int End { get; }

        /// <summary>
        ///     Construct a pixel range from an angle range, utilising a custom transformation function
        ///     to convert.
        /// </summary>
        public PixelRange(Range range, Func<double, int> transform)
        {
            Start = transform(range.Start);
            End = transform(range.End);

            // Ensure end pixel is larger than start pixel. 
            if (Start > End)
            {
                var temp = Start;

                Start = End;
                End = temp;
            }
        }

        public int Range => End - Start;
    }
}