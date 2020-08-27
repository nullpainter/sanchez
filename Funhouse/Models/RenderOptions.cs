using SixLabors.ImageSharp;

 namespace Funhouse.Models
{
    /// <summary>
    ///     Rendering options used to composite the image.
    /// </summary>
    public class RenderOptions
    {
        internal RenderOptions(float brightness, float saturation, Color? tint)
        {
            Brightness = brightness;
            Saturation = saturation;
            Tint = tint;
        }

        internal float Brightness { get; }
        internal float Saturation { get; }
        internal Color? Tint { get; }
    }
}