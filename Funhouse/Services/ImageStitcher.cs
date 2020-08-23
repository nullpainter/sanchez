using System;
using System.Collections.Generic;
using System.Linq;
using Funhouse.Models;
using Funhouse.Models.Projections;
using Serilog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Funhouse.Services
{
    public interface IImageStitcher
    {
        Image<Rgba32> Stitch(List<ProjectionActivity> projections);
    }

    public class ImageStitcher : IImageStitcher
    {
        public Image<Rgba32> Stitch(List<ProjectionActivity> projections)
        {
            // Identify minimum horizontal offset im source images
            var minOffset = projections.Select(p => p.Offset.X).Min();
            var target = InitialiseTarget(projections, minOffset);

            Log.Information("Output image size: {width} x {height}px", target.Width, target.Height);
            Log.Information("Compositing");

            // Composite all images. Images will have their horizontal offsets pre-calculated and overlaps
            // blended, so compositing just involves combining them in the correct stacking order.
            target.Mutate(context =>
            {
                // Render all images in correct stacking order
                foreach (var projection in projections.OrderByDescending(p => p.Offset.X))
                {
                    var location = new Point((int) Math.Round(projection.Offset.X - minOffset), 0);
                    context.DrawImage(projection.Output, location, PixelColorBlendingMode.Normal, 1.0f);
                }
            });

            return target;
        }

        /// <summary>
        ///     Initialises the target image, calculating image size based on size of source images and
        ///     adjusting for image offsets.
        /// </summary>
        private static Image<Rgba32> InitialiseTarget(List<ProjectionActivity> projections, float minOffset)
        {
            var outputWidth = projections.Select(p => p.Offset.X).Max() + projections[0].Output.Width - minOffset;
            return new Image<Rgba32>((int) Math.Round(outputWidth), Constants.ImageSize);
        }
    }
}