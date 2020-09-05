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
        Task<Image<Rgba32>> StitchImagesAsync(Image<Rgba32> stitched, SatelliteImages activities);
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

        public async Task<Image<Rgba32>> StitchImagesAsync(Image<Rgba32> stitched, SatelliteImages activities)
        {

            // Determine visible range of all satellite imagery
            activities.GetVisibleRange(out var latitudeRange, out var longitudeRange);

            // Load underlay
            Image<Rgba32> target;
            if (_commandLineOptions.NoUnderlay)
            {
                target = stitched;
            }
            else
            {
                Log.Information("Tinting and normalising IR imagery");

                var clone = stitched.Clone();
                clone.Mutate(c => c.HistogramEqualization());
                stitched.Tint(_renderOptions.Tint);

                stitched.Mutate(c => c.DrawImage(clone, PixelColorBlendingMode.HardLight, 0.5f));

                var underlayOptions = new UnderlayProjectionOptions(
                    _renderOptions.ProjectionType,
                    _renderOptions.InterpolationType,
                    _renderOptions.ImageSize,
                    _commandLineOptions.UnderlayPath,
                    stitched.Height,
                    latitudeRange, longitudeRange);

                Log.Information("Retrieving underlay");
                target = await _underlayService.GetUnderlayAsync(underlayOptions);

                // Render underlay and optionally crop to size
                Log.Information("Blending with underlay");
                target.Mutate(ctx => ctx.DrawImage(stitched, PixelColorBlendingMode.Screen, 1.0f));
                
                // Perform global colour correction
                target.ColourCorrect(_renderOptions);
            }

            return target;
        }
    }
}