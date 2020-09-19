using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Sanchez.Extensions;
using Sanchez.Extensions.Images;
using Sanchez.ImageProcessing.ShadeEdges;
using Sanchez.ImageProcessing.Tint;
using Sanchez.ImageProcessing.Underlay;
using Sanchez.Models;
using Sanchez.Models.Projections;
using Sanchez.Services;
using Sanchez.Services.Underlay;
using Serilog;
using ShellProgressBar;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Angle = Sanchez.Models.Angle;
using Range = Sanchez.Models.Angles.Range;

namespace Sanchez.Compositors
{
    public interface IGeostationaryCompositor
    {
        Task<Activity?> ComposeAsync(CancellationToken cancellationToken);
        Task RenderProjectedAsync(Activity activity, Image<Rgba32> target);
    }

    public class GeostationaryCompositor : IGeostationaryCompositor
    {
        private readonly RenderOptions _options;
        private readonly IProgressBar _progressBar;
        private readonly IUnderlayService _underlayService;
        private readonly ISatelliteImageLoader _satelliteImageLoader;
        private readonly IFileService _fileService;

        public GeostationaryCompositor(
            RenderOptions options,
            IProgressBar progressBar,
            IUnderlayService underlayService,
            ISatelliteImageLoader satelliteImageLoader,
            IFileService fileService)
        {
            _options = options;
            _progressBar = progressBar;
            _underlayService = underlayService;
            _satelliteImageLoader = satelliteImageLoader;
            _fileService = fileService;
        }

        public async Task<Activity?> ComposeAsync(CancellationToken cancellationToken)
        {
            Log.Information("Loading source images");
            var activity = _satelliteImageLoader.RegisterImages();

            Log.Information("Processing images");

            _progressBar.MaxTicks = activity.Registrations.Count + 1;
            _progressBar.Message = "Compositing files";

            // Process each image registration
            await activity.Registrations.ForEachAsync(async registration =>
            {
                try
                {
                    _progressBar.Tick($"Rendering {Path.GetFileName(registration.Path)}");

                    // Stop if requested
                    if (cancellationToken.IsCancellationRequested)
                    {
                        _progressBar.Message = $"Cancelled after rendering {activity.Rendered} images";
                        return;
                    }

                    if (await LoadImage(registration))
                    {
                        await RenderAsync(registration);
                        activity.Rendered++;
                    }
                }
                finally
                {
                    // Free image resources after render, as the image is no longer required
                    registration.Dispose();
                }
            }, _options.NumImagesParallel);

            _progressBar.Tick($"Completed rendering {activity.Registrations.Count} images");

            return activity;
        }

        private async Task<bool> LoadImage(Registration registration)
        {
            // Verify that the output file can be written
            var targetLongitude = _options.GeostationaryRender?.Longitude;
            registration.OutputPath = targetLongitude == null ? _fileService.GetOutputFilename(registration.Path) : _options.OutputPath;

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

        /// <summary>
        ///     Renders a geostationary projected underlay, combining with the source satellite image.
        /// </summary>
        /// <remarks>
        ///    This method is only used for when a target longitude is not specified. When using a target longitude,
        ///     the <see cref="EquirectangularCompositor"/> must be used to first render the underlay.
        /// </remarks>
        private async Task RenderAsync(Registration registration)
        {
            var targetLongitude = _options.GeostationaryRender?.Longitude;
            if (targetLongitude != null) throw new InvalidOperationException("Equirectangular composition should be used used when target longitude is provided");

            Guard.Against.Null(registration.OutputPath, nameof(registration.OutputPath));

            // If no target longitude is specified, save each image
            await RenderGeostationaryUnderlayAsync(registration);
            await registration.Image!.SaveWithExifAsync(registration.OutputPath);
        }

        /// <summary>
        ///     Renders an equirectangular image into geostationary projection.
        /// </summary>
        /// <remarks>
        ///    This method should be used when repositioning a geostationary image. To do this, the satellite
        ///    image is first projected to equirectangular projection and composited with the underlay.
        /// </remarks>
        public async Task RenderProjectedAsync(Activity activity, Image<Rgba32> target)
        {
            Guard.Against.Null(_options.GeostationaryRender!.Longitude, nameof(GeostationaryRenderOptions.Longitude));

            var longitudeRange = activity.GetVisibleLongitudeRange();
            var targetLongitude = _options.GeostationaryRender!.Longitude!;

            using var geostationary = ToGeostationary(longitudeRange, targetLongitude.Value, target);

            Log.Information("Saving geostationary output");
            _progressBar.Tick("Saving output");
            await geostationary.SaveWithExifAsync(_options.OutputPath);
        }

        private Image<Rgba32> ToGeostationary(Range longitudeRange, double longitude, Image<Rgba32> target)
        {
            Guard.Against.Null(_options.GeostationaryRender, nameof(_options.GeostationaryRender));
            Log.Information("Reprojecting to geostationary with longitude {longitude} degrees", longitude);

            // Determine visible range of all satellite imagery
            var targetLongitude = Angle.FromDegrees(longitude).Radians;

            // Adjust longitude based on the underlay wrapping for visible satellites
            var adjustedLongitude = -Math.PI - longitudeRange.Start + targetLongitude;

            // Render geostationary image
            var geostationary = target.ToGeostationaryProjection(adjustedLongitude, Constants.Satellite.DefaultHeight, _options);

            // Apply haze if required
            var hazeAmount = _options.GeostationaryRender.HazeAmount;
            if (hazeAmount > 0 && !_options.NoUnderlay)
            {
                geostationary.ApplyHaze(_options.Tint, hazeAmount);
            }

            return geostationary;
        }

        private async Task RenderGeostationaryUnderlayAsync(Registration registration)
        {
            Guard.Against.Null(_options.GeostationaryRender, nameof(_options.GeostationaryRender));
            Guard.Against.Null(registration.Image, nameof(registration.Image));

            // Get or generate projected underlay
            var underlayOptions = new UnderlayProjectionOptions(
                ProjectionType.Geostationary,
                _options.InterpolationType,
                _options.ImageSize,
                _options.UnderlayPath);

            Log.Information("Retrieving underlay");
            var underlay = await _underlayService.GetUnderlayAsync(underlayOptions, registration.Definition);

            Log.Information("Tinting and normalising IR imagery");
            if (_options.AutoAdjustLevels) registration.Image.Mutate(c => c.HistogramEqualization());
            
            registration.Image.Tint(_options.Tint);

            Log.Information("Blending with underlay");
            registration.Image.Mutate(c => c
                .Resize(_options.ImageSize, _options.ImageSize)
                .DrawImage(underlay, PixelColorBlendingMode.Screen, 1.0f));

            var hazeAmount = _options.GeostationaryRender.HazeAmount;
            if (hazeAmount > 0) registration.Image.ApplyHaze(_options.Tint, hazeAmount);

            // Perform global colour correction
            registration.Image.ColourCorrect(_options);
        }
    }
}