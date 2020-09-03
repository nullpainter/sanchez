using Funhouse.Models.Angles;
using Funhouse.Models.Configuration;
using SixLabors.ImageSharp;

namespace Funhouse.Models
{
    public class UnderlayProjectionOptions : ProjectionOptions
    {
        public Size? TargetSize { get; }
        public Range? LatitudeCrop { get; }
        public Range? LongitudeCrop { get; }

        public UnderlayProjectionOptions(
            ProjectionType projection, 
            InterpolationType interpolation,
            Size? targetSize = null,
            Range? latitudeCrop = null, 
            Range? longitudeCrop = null) : base(projection, interpolation)
        {
            TargetSize = targetSize;
            LatitudeCrop = latitudeCrop;
            LongitudeCrop = longitudeCrop;
        }

        public bool CropSpecified => LatitudeCrop != null && LongitudeCrop != null;
    }
}