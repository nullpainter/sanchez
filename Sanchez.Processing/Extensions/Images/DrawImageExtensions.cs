using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Sanchez.Processing.Extensions.Images;

public static class DrawImageExtensions
{
    /// <summary>
    ///     Draws <see cref="source"/> onto <see cref="target"/>, wrapping <see cref="source"/> horizontally if part of the
    ///     image would exceed the bounds of the source image.
    /// </summary>
    public static void DrawImageCylindrical(this Image<Rgba32> target, Image<Rgba32> source, Point location, PixelColorBlendingMode colourBlending, float opacity)
    {
        target.Mutate(context =>
        {
            // Identify maximum pixel after compositing source with target
            var maxX = location.X + source!.Width;
                
            // If image overlaps bounds of target, split into two
            if (maxX > target.Width)
            {
                // Create crop for overlapping region
                var crop = new Rectangle(target.Width - location.X, 0, maxX - target.Width, target.Height);
                using var right = source.Clone(c => c.Crop(crop));

                // Draw overlapping region on the left of the target image
                context.DrawImage(right, new Point(0, 0), PixelColorBlendingMode.Normal, 1.0f);
                    
                // Crop source to non-overlapping region
                source.Mutate(c => c.Crop(new Rectangle(0, 0, target.Width - location.X, target.Height)));
            }

            // Composite source and target
            context.DrawImage(source, location, colourBlending, opacity);
        });
    }
}