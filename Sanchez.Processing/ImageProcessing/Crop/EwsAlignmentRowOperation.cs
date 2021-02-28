using System.Collections.Concurrent;
using System.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;

namespace Sanchez.Processing.ImageProcessing.Crop
{
    /// <summary>
    ///     Identifies whether an EWS-G1 image requires cropping on its left or right edge, as these images aren't centered in the frame and the alignment
    ///     shifts every 12 hours.
    /// </summary>
    public readonly struct EwsAlignmentRowOperation : IRowOperation
    {
        private readonly Image<Rgba32> _image;
        private readonly ConcurrentBag<int> _matchedPositions;
        
        private const int Threshold = 50;
        
        public EwsAlignmentRowOperation(Image<Rgba32> image)
        {
            _image = image;
            _matchedPositions = new ConcurrentBag<int>();
        }

        public bool IsRightCrop() => _matchedPositions.Average() < _image.Width / 2f;

        public void Invoke(int y)
        {
            var span = _image.GetPixelRowSpan(y);

            for (var x = 0; x < _image.Width; x++)
            {
                var intensity = span[x].R;
                if (intensity > Threshold) _matchedPositions.Add(x);
            }
        }
    }
}