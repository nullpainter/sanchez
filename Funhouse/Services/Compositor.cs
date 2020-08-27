using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Funhouse.Extensions.Images;
using Funhouse.ImageProcessing.Offset;
using Funhouse.Models;
using Funhouse.Models.Angles;
using Funhouse.Models.Projections;
using Serilog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Range = Funhouse.Models.Angles.Range;

namespace Funhouse.Services
{
    public interface ICompositor
    {
        Task ComposeAsync();
    }

    public class Compositor : ICompositor
    {
        private readonly CommandLineOptions _options;
        private readonly RenderOptions _renderOptions;
        private readonly IImageStitcher _imageStitcher;
        private readonly IImageLoader _imageLoader;
        private readonly IProjectionActivityOperations _activityOperations;

        public Compositor(
            CommandLineOptions options,
            RenderOptions renderOptions,
            IImageStitcher imageStitcher,
            IImageLoader imageLoader,
            IProjectionActivityOperations activityOperations)
        {
            _options = options;
            _renderOptions = renderOptions;
            _imageStitcher = imageStitcher;
            _imageLoader = imageLoader;
            _activityOperations = activityOperations;
        }

        public async Task ComposeAsync()
        {
            if (_options.AutoCrop)
            {
                _options.BlurEdges = false;
                Log.Information("Disabling edge blurring as auto crop enabled");
            }

            // TODO may need to fix this for self-contained executables
            // var applicationPath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName)!;
            // var definitionsPath = Path.Combine(applicationPath, Constants.DefinitionsPath);

            string[] paths =
            {
                @"Resources\SampleImages\GOES17_FD_CH13_20200816T030031Z.jpg",
                @"Resources\SampleImages\GOES16_FD_CH13_20200816T025020Z.jpg"
            };

            // Load source images
            var loadTasks = paths.Select(path => _imageLoader.LoadAsync(path)).ToList();
            await Task.WhenAll(loadTasks);

            var activities = loadTasks.Select(task => task.Result).ToList();

            _activityOperations.Initialise(activities);

            // Verify that all images have an associated projection definition
            var unmappedProjections = _activityOperations.GetUnmapped();
            if (unmappedProjections.Any())
            {
                unmappedProjections.ForEach(p => Console.Error.WriteLineAsync($"Unable to determine satellite based on file prefix: {p.Path}"));
                Environment.Exit(-1);
            }

            var stopwatch = Stopwatch.StartNew();

            // Calculate crop for each image based on visible range and image overlaps
            _activityOperations.CalculateCrop();
            
            // Reproject all images to equirectangular
            _activityOperations.Reproject();

            // Stitch images if required
            if (_options.Stitch)
            {
                // Combine reprojected images
                var target = await StitchImages(activities);

                // Save output
                if (target != null)
                {
                    Log.Information("Saving stitched output");
                    await target.SaveAsync(_options.OutputPath);

                    Console.WriteLine($"Output written to {Path.GetFullPath(_options.OutputPath)}");
                }
            }

            Log.Information("Elapsed time: {elapsed}", stopwatch.Elapsed);
        }

        private async Task<Image<Rgba32>?> StitchImages(List<ProjectionActivity> activities)
        {
            var target = _imageStitcher.Stitch(activities);

            var underlay = await Image.LoadAsync<Rgba32>(@"Resources\world.200411.3x21600x10800.jpg");
            underlay = OffsetAndWrap(underlay, activities);

            underlay.Mutate(c => c.Resize(target.Width, target.Height));

            // Calculate crop region if required
            Rectangle? cropRectange = null;
            if (_options.AutoCrop)
            {
                cropRectange = target.AutoCrop();
                if (cropRectange == null) Log.Error("Unable to autocrop bounds");
                else Log.Information("Cropped image size: {width} x {height} px", cropRectange.Value.Width, cropRectange.Value.Height);
            }
            // Remove grey tint from satellite image
            target.TintAndBlend(_renderOptions);

            // Render underlay and optionally  crop to size
            target.Mutate(ctx => ctx.DrawImage(underlay, PixelColorBlendingMode.Screen, 1.0f));
            if (cropRectange != null) target.Mutate(ctx => ctx.Crop(cropRectange.Value));
      
            // Perform global colour correction
            target.ColourCorrect(_renderOptions);

            return target;
        }

        private static Image<Rgba32> OffsetAndWrap(Image<Rgba32> underlay, List<ProjectionActivity> activities)
        {
            var sortedActivities = activities.OrderBy(p => p.Offset.X).ToList();

            var longitudeRange = new Range(
                sortedActivities.First().LongitudeRange.Start,
                sortedActivities.Last().LongitudeRange.End
            ).UnwrapLongitude();

            var latitudeRange = new Range(
                activities.Min(a => a.LatitudeRange.Start),
                activities.Max(a => a.LatitudeRange.End));

            var xPixelRange = PixelRange.ToPixelRangeX(longitudeRange, underlay.Width);
            var yPixelRange = PixelRange.ToPixelRangeY(latitudeRange, underlay.Height);

            Log.Information("Cropping underlay to {min} - {max} px width", xPixelRange.Start, xPixelRange.End);
            Log.Information("Cropping underlay to {min} - {max} px height", yPixelRange.Start, yPixelRange.End);

            // Offset and wrap underlay if required to match projection
            if (xPixelRange.End > underlay.Width)
            {
                underlay.HorizontalOffset(-xPixelRange.Start);
                xPixelRange = new PixelRange(0, xPixelRange.Range);
            }

            // Crop underlay
            underlay.Mutate(c => c.Crop(new Rectangle(xPixelRange.Start, yPixelRange.Start, xPixelRange.Range, yPixelRange.Range)));
            return underlay;
        }
    }
}