using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Sanchez.Exceptions;
using Sanchez.Extensions;
using Sanchez.Extensions.Images;
using Sanchez.ImageProcessing.Projection;
using Sanchez.Models;
using Sanchez.Models.Projections;
using Sanchez.Services;
using Sanchez.Services.Equirectangular;
using Serilog;
using ShellProgressBar;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Range = Sanchez.Models.Angles.Range;

namespace Sanchez.Compositors
{
    public interface IEquirectangularCompositor
    {
        Task ComposeAsync(Activity activity, int registrationCount, CancellationToken cancellationToken);
        Activity CreateActivity();
        List<Activity> CreateActivities();
        Task<Image<Rgba32>?> ComposeStitchedAsync(Activity activity, bool saveOutput, CancellationToken cancellationToken);
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

        public async Task ComposeAsync(Activity activity, int registrationCount, CancellationToken cancellationToken)
        {
            _progressBar.MaxTicks = registrationCount * 4;
            _progressBar.Message = "Loading source images";

            // Offset all images by the minimum longitude
            var globalOffset = -activity.Registrations.Select(p => p.LongitudeRange.UnwrapLongitude().NormaliseLongitude().Start).Min();

            var rendered = 0;
            await activity.Registrations.ForEachAsync(async registration =>
            {
                try
                {
                    // Stop if requested
                    if (cancellationToken.IsCancellationRequested)
                    {
                        _progressBar.Message = $"Cancelled after rendering {rendered} images"; 
                        return;
                    }

                    _progressBar.Tick($"Rendering {Path.GetFileName(registration.Path)}");

                    if (!await LoadImage(registration)) return;

                    // Project satellite image to equirectangular
                    ToEquirectangular(registration, globalOffset, cancellationToken);

                    Guard.Against.Null(registration.OutputPath, nameof(registration.OutputPath));
                    Guard.Against.Null(registration.Image, nameof(registration.Image));

                    var cropRectangle = GetCropBounds(registration.Image);

                    _progressBar.Tick($"Projecting {Path.GetFileName(registration.Path)}");
                    using var target = await _equirectangularImageRenderer.ComposeAsync(registration.Image, activity);
                    CropToSatelliteImagery(registration.LongitudeRange.UnwrapLongitude(), target);
                    Crop(cropRectangle, target);

                    _progressBar.Tick("Saving output");
                    await target.SaveWithExifAsync(registration.OutputPath);

                    activity.Rendered++;
                    rendered++;
                }
                finally
                {
                    // Free image resources after render, as the image is no longer required
                    if (!_options.StitchImages) registration.Dispose();
                }
            }, _options.NumImagesParallel);
        }

        public async Task<Image<Rgba32>?> ComposeStitchedAsync(Activity activity, bool saveOutput, CancellationToken cancellationToken)
        {
            _progressBar.MaxTicks = activity.Registrations.Count * 2 + 2;
            _progressBar.Message = "Loading source images";

            // Offset all images by the minimum longitude
            var globalOffset = -activity.Registrations.Select(p => p.LongitudeRange.UnwrapLongitude().NormaliseLongitude().Start).Min();

            await activity.Registrations.ForEachAsync(async registration =>
            {
                try
                {
                    // Stop if requested
                    if (cancellationToken.IsCancellationRequested)
                    {
                        _progressBar.Message = $"Cancelled after rendering {activity.Rendered} images";
                        return;
                    }

                    _progressBar.Tick($"Rendering {Path.GetFileName(registration.Path)}");

                    if (await LoadImage(registration))
                    {
                        ToEquirectangular(registration, globalOffset, cancellationToken);
                        activity.Rendered++;
                    }
                }
                finally
                {
                    // Free image resources after render, as the image is no longer required
                    if (!_options.StitchImages) registration.Dispose();
                }
            }, _options.NumImagesParallel);

            if (_options.StitchImages) return await StitchImagesAsync(activity, saveOutput, cancellationToken);
            return null;
        }

        private async Task<bool> LoadImage(Registration registration)
        {
            // Verify that the output file can be written
            registration.OutputPath = _fileService.GetOutputFilename(registration.Path);

            if (!_fileService.ShouldWrite(registration.OutputPath))
            {
                Log.Information("Output file {outputFilename} exists; not overwriting", registration.OutputPath);
                return false;
            }

            // Load image
            await registration.LoadAsync();

            // Normalise image characteristics
            registration.Normalise(_options);
            return true;
        }

        private async Task<Image<Rgba32>?> StitchImagesAsync(Activity activity, bool saveOutput, CancellationToken cancellationToken)
        {
            var longitudeRange = activity.GetVisibleLongitudeRange();

            // Reproject all images to equirectangular
            if (cancellationToken.IsCancellationRequested) return null;

            // Combine reprojected images
            _progressBar.Tick("Stitching images");
            using var stitched = _imageStitcher.DrawImages(activity);

            var cropRectangle = GetCropBounds(stitched);
            var target = await _equirectangularImageRenderer.ComposeAsync(stitched, activity);

            // Render if not reprojecting
            if (saveOutput)
            {
                // Crop underlay to fit satellite imagery
                CropToSatelliteImagery(longitudeRange, target); 
                Crop(cropRectangle, target);

                Log.Information("Saving stitched output");
                _progressBar.Tick("Saving output");
                await target.SaveWithExifAsync(_options.OutputPath);
            }

            return target;
        }

        private static void Crop(Rectangle? cropRectangle, Image<Rgba32> target)
        {
            if (cropRectangle == null) return;

            Log.Information("Cropping");
            target.Mutate(ctx => ctx.Crop(cropRectangle.Value));
        }

        private Rectangle? GetCropBounds(Image<Rgba32> stitched)
        {
            var autoCrop = _options.EquirectangularRender?.AutoCrop ?? false;
            var extents = _options.EquirectangularRender?.Extents;

            if (!autoCrop && extents == null) return null;

            if (autoCrop)
            {
                var cropRectangle = stitched.GetAutoCropBounds();

                if (cropRectangle == null) Log.Error("Unable to determine autocrop bounds");
                else Log.Information("Cropped image size: {width} x {height} px", cropRectangle.Value.Width, cropRectangle.Value.Height);

                return cropRectangle;
            }

            var xPixelRange = extents!.Value.Longitude.UnwrapLongitude().ToPixelRangeX(stitched.Width);
            var yPixelRange = extents!.Value.Latitude.ToPixelRangeY(stitched.Height);
            
            Log.Information("Cropped image size: {width} x {height} px", xPixelRange.Range, yPixelRange.Range);

            return new Rectangle(0, xPixelRange.Range, yPixelRange.Start, yPixelRange.Range);
        }

        private void CropToSatelliteImagery(Range longitudeRange, Image<Rgba32> target)
        {
            if (_options.NoUnderlay) return;

            var xPixelRange = longitudeRange.ToPixelRangeX(target.Width);
            if (xPixelRange.Range > 0)
            {
                target.Mutate(c => c.Crop(new Rectangle(0, 0, xPixelRange.Range, c.GetCurrentSize().Height)));
            }
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

            // Calculate the visible range of images so they can be cropped to size
            SetLatLongRange(activity);
            return activity;
        }

        public List<Activity> CreateActivities()
        {
            // Load source images
            Log.Information("Loading source images");

            var masterActivity = _satelliteImageLoader.RegisterImages();

            var activities = masterActivity.Registrations.Select(registration => new Activity(new List<Registration> { registration })).ToList();

            // Calculate the visible range of images so they can be cropped to size
            activities.ForEach(SetLatLongRange);

            return activities;
        }

        /// <summary>
        ///     Calculates overlapping regions between satellites, or visible region if not stitching images.
        /// </summary>
        private void SetLatLongRange(Activity activity)
        {
            _projectionOverlapCalculator.Initialise(activity.Registrations.Select(p => p.Definition));

            // Set latitude and longitude ranges based on overlapping satellite ranges
            activity.Registrations.ForEach(registration =>
            {
                registration.LatitudeRange = registration.Definition.LatitudeRange;

                registration.LongitudeRange =
                    _options.StitchImages
                        ? _projectionOverlapCalculator.GetNonOverlappingRange(registration.Definition)
                        : registration.Definition.LongitudeRange.UnwrapLongitude();
            });
        }

        private void ToEquirectangular(Registration registration, double globalOffset, CancellationToken cancellationToken)
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