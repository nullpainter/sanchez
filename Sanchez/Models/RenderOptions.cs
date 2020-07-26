using SixLabors.ImageSharp;

namespace Sanchez.Models
{
    /// <summary>
    ///     Rendering options used to composite the image.
    /// </summary>
    public class RenderOptions
    {
        internal RenderOptions(bool renderMask, bool renderOverlay, float brightness, float saturation, Color? tint)
        {
            RenderMask = renderMask;
            RenderOverlay = renderOverlay;
            Brightness = brightness;
            Saturation = saturation;
            Tint = tint;
        }

        internal bool RenderMask { get; }
        internal bool RenderOverlay { get; }
        internal float Brightness { get; }
        internal float Saturation { get; }
        internal Color? Tint { get; }
    }
}