using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Funhouse.Exceptions;
using Funhouse.Extensions;
using Funhouse.Extensions.Images;
using Funhouse.ImageProcessing.Projection;
using Funhouse.Models;
using Funhouse.Models.Projections;
using Funhouse.Services;
using Funhouse.Services.Equirectangular;
using Serilog;
using ShellProgressBar;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Funhouse.Compositors
{
    public interface IEquirectangularCompositor
    {
        Task<Image<Rgba32>?> ComposeAsync(CancellationToken cancellationToken);
        Task<Image<Rgba32>?> ComposeAsync(Activity activity, CancellationToken cancellationToken);
        Activity CreateActivity();
    }

    public class EquirectangularCompositor : IEquirectangularCompositor
    {
        private readonly RenderOptions _options;
        private readonly IProgressBar _progressBar;
        private readonly IProjectionOverlapCalculator _projectionOverlapCalculator;
        private readonly ISatelliteImageLoader _satelliteImageLoader;
        private readonly IEquirectangularImageRenderer _equirectangularImageRenderer;
        private readonly IFileService _fileService;
        private readonly IImageStitcher _imageStitcher;

        public EquirectangularCompositor(
            RenderOptions options,
            IProgressBar progressBar,
            IProjectionOverlapCalculator projectionOverlapCalculator,
            ISatelliteImageLoader satelliteImageLoader,
            IEquirectangularImageRenderer equirectangularImageRenderer,
            IFileService fileService,
            IImageStitcher imageStitcher)
        {
            _options = options;
            _progressBar = progressBar;
            _projectionOverlapCalculator = projectionOverlapCalculator;
            _satelliteImageLoader = satelliteImageLoader;
            _equirectangularImageRenderer = equirectangularImageRenderer;
            _fileService = fileService;
            _imageStitcher = imageStitcher;
        }

        public async Task<Image<Rgba32>?> ComposeAsync(CancellationToken cancellationToken)
        {
            var activity = CreateActivity();
            return await ComposeAsync(activity, cancellationToken);
        }

        public async Task<Image<Rgba32>?> ComposeAsync(Activity activity, CancellationToken cancellationToken)
        {
            _progressBar.MaxTicks = activity.Registrations.Count * 2 + 2;
            _progressBar.Message = "Loading source images";

            var rendered = 0;
            await activity.Registrations.ForEachAsync(async registration =>
            {
                // Stop if requested
                if (cancellationToken.IsCancellationRequested)
                {
                    _progressBar.Message = $"Cancelled after rendering {rendered} images";
                    return;
                }

                _progressBar.Tick($"Rendering {Path.GetFileName(registration.Path)}");
                await registration.LoadAsync();

                // Normalise image characteristics
                registration.Normalise(_options);
                rendered++;
            }, _options.NumImagesParallel);

            return await StitchImagesAsync(activity, cancellationToken);
        }

        public async Task<Image<Rgba32>?> StitchImagesAsync(Activity activity, CancellationToken cancellationToken)
        {
            // Reproject all images to equirectangular
            ToEquirectangular(activity, cancellationToken);
            if (cancellationToken.IsCancellationRequested) return null;

            // Stitch images if required
            var longitudeRange = activity.GetVisibleLongitudeRange();

            // Combine reprojected images

            _progressBar.Tick("Stitching images");
            using var stitched = _imageStitcher.Stitch(activity);

            // Calculate crop region if required
            Rectangle? cropRectangle = null;
            if (_options.EquirectangularRender?.AutoCrop == true)
            {
                cropRectangle = stitched.GetAutoCropBounds();
                if (cropRectangle == null) Log.Error("Unable to determine autocrop bounds");
                else Log.Information("Cropped image size: {width} x {height} px", cropRectangle.Value.Width, cropRectangle.Value.Height);
            }

            // Perform stitching
            var target = await _equirectangularImageRenderer.StitchImagesAsync(stitched, activity);

            // Render if not reprojecting
            // TODO referencing geostationary stuff here is a bit mucky
            if (_options.GeostationaryRender?.Longitude == null)
            {
                // Crop underlay to fit satellite imagery
                if (!_options.NoUnderlay)
                {
                    var xPixelRange = longitudeRange.ToPixelRangeX(target.Width);
                    if (xPixelRange.Range > 0)
                    {
                        target.Mutate(c => c.Crop(new Rectangle(0, 0, xPixelRange.Range, c.GetCurrentSize().Height)));
                    }
                }

                // Crop composited image
                if (cropRectangle != null)
                {
                    Log.Information("Cropping");
                    target.Mutate(ctx => ctx.Crop(cropRectangle.Value));
                }

                Log.Information("Saving stitched output");
                _progressBar.Tick("Saving output");
                await target.SaveWithExifAsync(_options.OutputPath);
            }

            return target;
        }

        public Activity CreateActivity()
        {
            // Load source images
            Log.Information("Loading source images");
            var activity = _satelliteImageLoader.RegisterImages();

            // Initialise activity
            if (!_fileService.ShouldWrite(_options.OutputPath))
            {
                Log.Information("Output file {outputFilename} exists; not overwriting", _options.OutputPath);
                throw new ValidationException($"Output file {_options.OutputPath} exists; not overwriting");
            }

            // If combining satellite images, calculate the overlap between images so they can be appropriately cropped
            if (_options.StitchImages) CalculateOverlap(activity);

            return activity;
        }

        /// <summary>
        ///     Calculates overlapping regions between satellites.
        /// </summary>
        private void CalculateOverlap(Activity activity)
        {
            _projectionOverlapCalculator.Initialise(activity.Registrations.Select(p => p.Definition));

            // Set latitude and longitude ranges based on overlapping satellite ranges
            activity.Registrations.ForEach(image =>
            {
                image.LongitudeRange = _projectionOverlapCalculator.GetNonOverlappingRange(image.Definition);
                image.LatitudeRange = image.Definition.LatitudeRange;
            });
        }

        private void ToEquirectangular(Activity activity, CancellationToken cancellationToken)
        {
            // Offset all images by the minimum longitude
            var globalOffset = -activity.Registrations.Select(p => p.LongitudeRange.UnwrapLongitude().NormaliseLongitude().Start).Min();

            activity.Registrations.ForEach(registration =>
            {
                _progressBar.Tick($"Reprojecting {Path.GetFileName(registration.Path)}");
                
                if (cancellationToken.IsCancellationRequested) return;

                Guard.Against.Null(registration.Definition, nameof(registration.Definition));

                // Reproject geostationary image into equirectangular
                LogStatistics(registration);
                registration.Image = registration.Reproject(_options);

                // Overlap range relative the satellite's visible range and convert to a equirectangular map
                // offset with a pixel range of -180 to 180 degrees
                var longitude = (registration.Definition.LongitudeRange.Start + globalOffset).NormaliseLongitude();
                registration.OffsetX = longitude.ScaleToWidth(_options.ImageSize * 2);
            });
        }

        private static void LogStatistics(Registration registration)
        {
            Guard.Against.Null(registration.Definition, nameof(registration.Definition));

            var definition = registration.Definition;
            var longitudeCrop = registration.LongitudeRange;

            Log.Information("{definition:l0} range {startRange:F2} to {endRange:F2} degrees",
                definition.DisplayName,
                Angle.FromRadians(definition.LongitudeRange.Start).Degrees,
                Angle.FromRadians(definition.LongitudeRange.End).Degrees);

            Log.Information("{definition:l0} crop {startRange:F2} to {endRange:F2} degrees",
                definition.DisplayName,
                Angle.FromRadians(longitudeCrop.Start).Degrees,
                Angle.FromRadians(longitudeCrop.End).Degrees);
        }
    }
}