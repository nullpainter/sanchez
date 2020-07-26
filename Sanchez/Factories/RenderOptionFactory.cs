using Sanchez.Extensions;
using Sanchez.Models;

namespace Sanchez.Factories
{
    internal static class RenderOptionFactory
    {
        /// <summary>
        ///     Creates render options from command line options.
        /// </summary>
        internal static RenderOptions ToRenderOptions(CommandLineOptions options)
        {
            // Compositing options
            var renderMask = options.MaskPath != null;
            var renderOverlay = options.OverlayPath != null;

            return new RenderOptions(
                renderMask,
                renderOverlay,
                options.Brightness,
                options.Saturation,
                options.Tint.FromHexString());
        }
    }
}