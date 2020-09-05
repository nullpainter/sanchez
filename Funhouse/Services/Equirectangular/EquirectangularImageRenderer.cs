using System.Collections.Generic;
using System.Threading.Tasks;
using Funhouse.Extensions.Images;
using Funhouse.ImageProcessing.Tint;
using Funhouse.Models;
using Funhouse.Models.CommandLine;
using Funhouse.Models.Projections;
using Funhouse.Services.Underlay;
using Serilog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Funhouse.Services.Equirectangular
{
    public interface IEquirectangularImageRenderer
    {
        Task<Image<Rgba32>> StitchImagesAsync(ProjectionActivities activities);
    }

    public class EquirectangularImageRenderer : IEquirectangularImageRenderer
    {
        private readonly IProjectionActivityOperations _projectionActivityOperations;
        private readonly CommandLineOptions _commandLineOptions;
        private readonly RenderOptions _renderOptions;
        private readonly IImageStitcher _imageStitcher;
        private readonly IUnderlayService _underlayService;

        public EquirectangularImageRenderer(
            IProjectionActivityOperations projectionActivityOperations,
            CommandLineOptions commandLineOptions,
            RenderOptions renderOptions,
            IImageStitcher imageStitcher,
            IUnderlayService underlayService)
        {
            _projectionActivityOperations = projectionActivityOperations;
            _commandLineOptions = commandLineOptions;
            _renderOptions = renderOptions;
            _imageStitcher = imageStitcher;
            _underlayService = underlayService;
        } 
        
        public async Task<Image<Rgba32>> StitchImagesAsync(ProjectionActivities activities)
        {
            var stitched = _imageStitcher.Stitch(activities.Activities);

            // Calculate crop region if required
            Rectangle? cropRectangle = null;
            if (_commandLineOptions.AutoCrop)
            {
                cropRectangle = stitched.AutoCrop();
                if (cropRectangle == null) Log.Error("Unable to autocrop bounds");
                else Log.Information("Cropped image size: {width} x {height} px", cropRectangle.Value.Width, cropRectangle.Value.Height);
            }

            // Determine visible range of all satellite imagery
            activities.GetVisibleRange(out var latitudeRange, out var longitudeRange);

            // Load underlay
            var underlayOptions = new UnderlayProjectionOptions(
                _renderOptions.ProjectionType,
                _renderOptions.InterpolationType,
                _renderOptions.ImageSize,
                _commandLineOptions.UnderlayPath,
                stitched.Height,
                latitudeRange, longitudeRange);

            Log.Information("Retrieving underlay");
            var underlay = await _underlayService.GetUnderlayAsync(underlayOptions);

            Log.Information("Tinting and normalising IR imagery");

            var clone = stitched.Clone();
            clone.Mutate(c => c.HistogramEqualization());
            stitched.Tint(_renderOptions.Tint);

            stitched.Mutate(c => c.DrawImage(clone, PixelColorBlendingMode.HardLight, 0.5f));

            // Render underlay and optionally crop to size
            Log.Information("Blending with underlay");
            underlay.Mutate(ctx => ctx.DrawImage(stitched, PixelColorBlendingMode.Screen, 1.0f));

            // Crop composited image
            if (cropRectangle != null)
            {
                Log.Information("Cropping");
                underlay.Mutate(ctx => ctx.Crop(cropRectangle.Value));
            }

            // Perform global colour correction
            underlay.ColourCorrect(_renderOptions);

            return underlay;
        }
    }
}