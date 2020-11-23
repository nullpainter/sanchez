using Sanchez.Processing.Models.Configuration;
using SixLabors.ImageSharp;
using Range = Sanchez.Processing.Models.Angles.Range;

namespace Sanchez.Processing.Models
{
    public class UnderlayProjectionData : ProjectionData
    {
        public Size? TargetSize { get; }
        public Range? LatitudeCrop { get; }

        public UnderlayProjectionData(
            ProjectionType projection, 
            InterpolationType interpolation,
            string underlayPath,
            int imageSize,
            Size? targetSize = null,
            Range? latitudeCrop = null, 
            double? minLongitude = null
            ) : base(projection, interpolation, imageSize, underlayPath)
        {
            TargetSize = targetSize;
            LatitudeCrop = latitudeCrop;
            MinLongitude = minLongitude;
        }

        public double? MinLongitude { get; }
    }
}