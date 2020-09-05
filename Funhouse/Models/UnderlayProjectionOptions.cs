using Funhouse.Models.Angles;
using Funhouse.Models.Configuration;
using SixLabors.ImageSharp;

namespace Funhouse.Models
{
    public class UnderlayProjectionOptions : ProjectionOptions
    {
        public string? UnderlayPath { get; }
        public Size? TargetSize { get; }
        public Range? LatitudeCrop { get; }
        public Range? LongitudeCrop { get; }

        public UnderlayProjectionOptions(
            ProjectionType projection, 
            InterpolationType interpolation,
            int imageSize,
            string? underlayPath = null,
            Size? targetSize = null,
            Range? latitudeCrop = null, 
            Range? longitudeCrop = null) : base(projection, interpolation, imageSize)
        {
            UnderlayPath = underlayPath ?? Constants.DefaultUnderlayPath;
            TargetSize = targetSize;
            LatitudeCrop = latitudeCrop;
            LongitudeCrop = longitudeCrop;
        }

        public bool CropSpecified => LatitudeCrop != null && LongitudeCrop != null;
    }
}