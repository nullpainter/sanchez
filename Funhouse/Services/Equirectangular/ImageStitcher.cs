using System.Linq;
using Funhouse.Models;
using Serilog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Funhouse.Services.Equirectangular
{
    public interface IImageStitcher
    {
        /// <summary>
        ///     Stitches equirectangular satellite IR images into a single image
        /// </summary>
        Image<Rgba32> Stitch(Activity activity);
    }

    public class ImageStitcher : IImageStitcher
    {
        public Image<Rgba32> Stitch(Activity activity)
        {
            // Identify minimum horizontal offset im source images
            var minOffset = activity.Registrations.Select(p => p.OffsetX).Min();
            var target = NewTargetImage(activity, minOffset);

            Log.Information("Output image size: {width} x {height} px", target.Width, target.Height);

            // Composite all images. Images will have their horizontal offsets pre-calculated and overlaps
            // blended, so compositing just involves combining them in the correct stacking order.
            target.Mutate(context =>
            {
                // Render all images in correct stacking order
                foreach (var projection in activity.Registrations.OrderByDescending(p => p.OffsetX))
                {
                    var projectionImage = projection.Image;
                    
                    // Identify horizontal offset of each image
                    var location = new Point(projection.OffsetX - minOffset, 0);
                    context.DrawImage(projectionImage, location, PixelColorBlendingMode.Normal, 1.0f);
                }
            });
            
            return target;
        }

        /// <summary>
        ///     Initialises the target image, calculating image size based on size of source images and
        ///     adjusting for image offsets.
        /// </summary>
        private static Image<Rgba32> NewTargetImage(Activity activity, int minOffset)
        {
            // As we know the horizontal offsets of all images being composed, the output width is the 
            // maximum offset plus the width of the final image, minus the minimum offset.
            var finalProjection = activity.Registrations.OrderBy(p => p.OffsetX).Last();

            var outputWidth = finalProjection.OffsetX + finalProjection.Width - minOffset;
            return new Image<Rgba32>(outputWidth, finalProjection.Height);
        }
    }
}