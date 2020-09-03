using Funhouse.Models.Configuration;
using SixLabors.ImageSharp;

namespace Funhouse.Models
{
    /// <summary>
    ///     Rendering options used to composite the image.
    /// </summary>
    public class RenderOptions
    {
        internal RenderOptions(
            float brightness, 
            float saturation, 
            Color tint, 
            InterpolationType interpolationType, 
            ProjectionType projectionType,
            ImageOffset imageOffset, 
            int imageSize)
        {
            Brightness = brightness;
            Saturation = saturation;
            Tint = tint;
            InterpolationType = interpolationType;
            ProjectionType = projectionType;
            ImageOffset = imageOffset;
            ImageSize = imageSize;
        }

        internal float Brightness { get; }
        internal float Saturation { get; }
        internal Color Tint { get; }
        public InterpolationType InterpolationType { get; }
        public ProjectionType ProjectionType { get; }
        public ImageOffset ImageOffset { get; }
        public int ImageSize { get; }
    }
}