using Sanchez.Processing.Models.Configuration;
using Range = Sanchez.Processing.Models.Angles.Range;

namespace Sanchez.Processing.Models
{
    public class UnderlayProjectionData : ProjectionData
    {
        public int? TargetHeight { get; }
        public Range? LatitudeCrop { get; }
        public Range? LongitudeCrop { get; }

        public UnderlayProjectionData(
            ProjectionType projection, 
            InterpolationType interpolation,
            int imageSize,
            int? targetHeight = null,
            Range? latitudeCrop = null, 
            Range? longitudeCrop = null) : base(projection, interpolation, imageSize)
        {
            TargetHeight = targetHeight;
            LatitudeCrop = latitudeCrop;
            LongitudeCrop = longitudeCrop;
        }

        public bool CropSpecified => LatitudeCrop != null && LongitudeCrop != null;
    }
}