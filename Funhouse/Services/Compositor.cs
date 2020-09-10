using System;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Funhouse.Extensions;
using Funhouse.Extensions.Images;
using Funhouse.ImageProcessing.ShadeEdges;
using Funhouse.ImageProcessing.Tint;
using Funhouse.ImageProcessing.Underlay;
using Funhouse.Models;
using Funhouse.Services.Equirectangular;
using Serilog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Angle = Funhouse.Models.Angle;
using Range = Funhouse.Models.Angles.Range;

namespace Funhouse.Services
{
    public interface ICompositor
    {
        Task ComposeAsync(CancellationToken cancellationToken);
    }

    public class Compositor : ICompositor
    {
        private readonly RenderOptions _options;
        private readonly IImageLoader _imageLoader;
        private readonly IEquirectangularImageRenderer _equirectangularImageRenderer;
        private readonly IFileService _fileService;
        private readonly IImageStitcher _imageStitcher;
        private readonly IProjectionActivityOperations _activityOperations;

        public Compositor(
            RenderOptions options,
            IImageLoader imageLoader,
            IEquirectangularImageRenderer equirectangularImageRenderer,
            IFileService fileService,
            IImageStitcher imageStitcher,
            IProjectionActivityOperations activityOperations)
        {
            _options = options;
            _imageLoader = imageLoader;
            _equirectangularImageRenderer = equirectangularImageRenderer;
            _fileService = fileService;
            _imageStitcher = imageStitcher;
            _activityOperations = activityOperations;
        }

        public async Task ComposeAsync(CancellationToken cancellationToken)
        {
            _fileService.PrepareOutput();

            // Load source images
            Log.Information("Loading source images");
            var images = await _imageLoader.LoadImagesAsync();

            Log.Information("Images loaded");

            _activityOperations.Initialise(images);

            // Calculate crop for each image based on visible range and image overlaps
            Log.Information("Processing IR image");
            
            var targetLongitude = _options.GeostationaryRender?.Longitude;
            
            // If combining satellite images, calculate the overlap between images so they can be appropriately cropped
            if (_options.StitchImages) _activityOperations.CalculateOverlap();
            
            foreach (var image in images.Images)
            {
                if (cancellationToken.IsCancellationRequested) return;
                
                image
                    .CropBorders()
                    .RemoveBackground()
                    .NormaliseSize(_options.ImageSize)
                    .NormaliseHistogram();

                if (_options.Projection == ProjectionType.Geostationary)
                {
                    await _activityOperations.RenderGeostationaryUnderlayAsync(image);
                    
                    // If no target longitude is specified, save each image
                    if (targetLongitude == null)
                    {
                        var outputFilename = _fileService.GetOutputFilename(image.Path);
                        await image.Image.SaveWithExifAsync(outputFilename, _options);
                    }
                }
            }

            // Render equirectangular projection if the target longitude is specified for geostationary projection,
            // as we need to stitch images together and reproject in order to support an arbitrary target longitude.
            if (_options.StitchImages)
            {
                // Reproject all images to equirectangular
                _activityOperations.ToEquirectangular(cancellationToken);
                if (cancellationToken.IsCancellationRequested) return;

                // Stitch images if required
                var longitudeRange = images.GetVisibleLongitudeRange();

                // Combine reprojected images
                var stitched = _imageStitcher.Stitch(images.Images);

                // Calculate crop region if required
                Rectangle? cropRectangle = null;
                if (_options.EquirectangularRender?.AutoCrop == true)
                {
                    cropRectangle = stitched.GetAutoCropBounds();
                    if (cropRectangle == null) Log.Error("Unable to determine autocrop bounds");
                    else Log.Information("Cropped image size: {width} x {height} px", cropRectangle.Value.Width, cropRectangle.Value.Height);
                }

                // Perform stitching
                var target = await _equirectangularImageRenderer.StitchImagesAsync(stitched, images);

                // Reproject to geostationary if required
                if (targetLongitude != null)
                {
                    target = ToGeostationary(longitudeRange, targetLongitude.Value, target);
                }
                else
                {
                    // Crop underlay to fit satellite imagery
                    if (!_options.NoUnderlay)
                    {
                        var xPixelRange = longitudeRange.ToPixelRangeX(target.Width);
                        if (xPixelRange.Range > 0)
                        {
                            target.Mutate(c => c.Crop(new Rectangle(0, 0, xPixelRange.Range, target.Height)));
                        }
                    }

                    // Crop composited image
                    if (cropRectangle != null)
                    {
                        Log.Information("Cropping");
                        target.Mutate(ctx => ctx.Crop(cropRectangle.Value));
                    }
                }

                Log.Information("Saving stitched output");
                await target.SaveWithExifAsync(_options.OutputPath, _options);
            }
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
    }
}