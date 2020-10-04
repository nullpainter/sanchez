using Sanchez.Processing.Models.Angles;
using Sanchez.Processing.Models.Configuration;

namespace Sanchez.Processing.Models
{
    public class UnderlayProjectionOptions : ProjectionOptions
    {
        public string? UnderlayPath { get; }
        public int? TargetHeight { get; }
        public Range? LatitudeCrop { get; }
        public Range? LongitudeCrop { get; }

        public UnderlayProjectionOptions(
            ProjectionType projection, 
            InterpolationType interpolation,
            int imageSize,
            string underlayPath,
            int? targetHeight = null,
            Range? latitudeCrop = null, 
            Range? longitudeCrop = null) : base(projection, interpolation, imageSize)
        {
            UnderlayPath = underlayPath;
            TargetHeight = targetHeight;
            LatitudeCrop = latitudeCrop;
            LongitudeCrop = longitudeCrop;
        }

        public bool CropSpecified => LatitudeCrop != null && LongitudeCrop != null;
    }
}