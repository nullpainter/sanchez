using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ExifLibrary;
using Funhouse.Extensions;
using Funhouse.Extensions.Images;
using Funhouse.ImageProcessing.ShadeEdges;
using Funhouse.ImageProcessing.Underlay;
using Funhouse.Models;
using Funhouse.Models.Angles;
using Funhouse.Models.CommandLine;
using Funhouse.Models.Configuration;
using Funhouse.Services.Equirectangular;
using Serilog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Angle = Funhouse.Models.Angle;
using Range = Funhouse.Models.Angles.Range;

namespace Funhouse.Services
{
    public interface ICompositor
    {
        Task ComposeAsync();
    }

    public class Compositor : ICompositor
    {
        private readonly CommandLineOptions _commandLineOptions;
        private readonly RenderOptions _renderOptions;
        private readonly IImageLoader _imageLoader;
        private readonly IEquirectangularImageRenderer _equirectangularImageRenderer;
        private readonly IImageStitcher _imageStitcher;
        private readonly IProjectionActivityOperations _activityOperations;

        public Compositor(
            CommandLineOptions commandLineOptions,
            RenderOptions renderOptions,
            IImageLoader imageLoader,
            IEquirectangularImageRenderer equirectangularImageRenderer,
            IImageStitcher imageStitcher,
            IProjectionActivityOperations activityOperations)
        {
            _commandLineOptions = commandLineOptions;
            _renderOptions = renderOptions;
            _imageLoader = imageLoader;
            _equirectangularImageRenderer = equirectangularImageRenderer;
            _imageStitcher = imageStitcher;
            _activityOperations = activityOperations;
        }

        public async Task ComposeAsync()
        {
            // TODO TEMP
            string[] paths =
            {
                @"Resources\SampleImages\GOES16_FD_CH13_20200830T035020Z.jpg",
                @"Resources\SampleImages\Himawari8_FD_IR_20200830T035100Z.jpg",
                @"Resources\SampleImages\GOES17_FD_CH13_20200830T033031Z.jpg",
            };

            // Load source images
            Log.Information("Loading source images");

            var satelliteImageLoadTasks = paths.Select(path => _imageLoader.LoadAsync(path)).ToList();
            await Task.WhenAll(satelliteImageLoadTasks);

            Log.Information("Images loaded");

            var images = new SatelliteImages(satelliteImageLoadTasks.Select(task => task.Result).ToList());
            _activityOperations.Initialise(images);

            // Verify that all images have an associated projection definition
            var unmappedProjections = images.GetUnmapped();
            if (unmappedProjections.Any())
            {
                unmappedProjections.ForEach(p => Console.Error.WriteLineAsync($"Unable to determine satellite based on file prefix: {p.Path}"));
                Environment.Exit(-1);
            }

            // Calculate crop for each image based on visible range and image overlaps
            Log.Information("Processing IR image");

            _activityOperations.CalculateOverlap();

            foreach (var activity in images.Images)
            {
                activity
                    .CropBorders()
                    .RemoveBackground()
                    .NormaliseSize(_renderOptions.ImageSize)
                    .NormaliseHistogram();

                if (_renderOptions.ProjectionType == ProjectionType.Geostationary)
                {
                    await _activityOperations.RenderGeostationaryUnderlayAsync(activity);
                }
            }

            // TODO this needs beefing up. Should be able to stitch and then reproject,e tc.
            switch (_renderOptions.ProjectionType)
            {
                case ProjectionType.Geostationary:

                    break;
                case ProjectionType.Equirectangular:
                {
                    // Reproject all images to equirectangular
                    _activityOperations.ToEquirectangular();

                    // Stitch images if required
                    if (_commandLineOptions.Stitch)
                    {
                        var longitudeRange = images.GetVisibleLongitudeRange();
                        
                        // Combine reprojected images
                        var stitched = _imageStitcher.Stitch(images.Images);
                        
                        // Calculate crop region if required
                        Rectangle? cropRectangle = null;
                        if (_commandLineOptions.AutoCrop)
                        {
                            cropRectangle = stitched.GetAutoCropBounds();
                            if (cropRectangle == null) Log.Error("Unable to determine autocrop bounds");
                            else Log.Information("Cropped image size: {width} x {height} px", cropRectangle.Value.Width, cropRectangle.Value.Height);
                        }
                        
                        // Perform stitching
                        var target = await _equirectangularImageRenderer.StitchImagesAsync(stitched, images);

                        // Reproject to geostationary
                        if (_commandLineOptions.Longitude != null)
                        {
                            var geostationary = ToGeostationary(longitudeRange, _commandLineOptions.Longitude.Value, target);
                            
                            // TODO fix filename
                            await geostationary.SaveWithExifAsync("reprojected.jpg");
                        }

                        // Crop composited image
                        if (cropRectangle != null)
                        {
                            Log.Information("Cropping");
                            target.Mutate(ctx => ctx.Crop(cropRectangle.Value));
                        }

                        // Crop underlay to fit satellite imagery
                        if (!_commandLineOptions.NoUnderlay)
                        {
                            var xPixelRange = PixelRange.ToPixelRangeX(longitudeRange, target.Width);
                            target.Mutate(c => c.Crop(new Rectangle(0, 0, xPixelRange.Range, target.Height)));
                        }

                        // Save output
                        Log.Information("Saving stitched output");
                        await target.SaveWithExifAsync(_commandLineOptions.OutputPath);

                        if (_commandLineOptions.Verbose)
                        {
                            Log.Information("Output written to {path}", Path.GetFullPath(_commandLineOptions.OutputPath));
                        }
                        else
                        {
                            Console.WriteLine($"Output written to {Path.GetFullPath(_commandLineOptions.OutputPath)}");
                        }
                    }

                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException($"Unhandled projection type: {_renderOptions.ProjectionType}");
            }
        }

        private Image<Rgba32> ToGeostationary(Range longitudeRange, double longitude, Image<Rgba32> target)
        {
            Log.Information("Reprojecting to geostationary with longitude {longitude} degrees", longitude);

            // Determine visible range of all satellite imagery
            var targetLongitude = Angle.FromDegrees(longitude).Radians;

            // Adjust longitude based on the underlay wrapping for visible satellites
            var adjustedLongitude = -Math.PI - longitudeRange.Start + targetLongitude;

            var definition = new SatelliteDefinition("", "", adjustedLongitude,
                new Range(
                    Angle.FromDegrees(Constants.Satellite.VisibleRange.DefaultMinLatitude),
                    Angle.FromDegrees(Constants.Satellite.VisibleRange.DefaultMaxLatitude)),
                new Range());

            // Render geostationary image
            var geostationary = target.ToGeostationaryProjection(definition, _renderOptions);
            
            // Apply haze
            if (_renderOptions.HazeAmount > 0 && !_commandLineOptions.NoUnderlay)
            {
                geostationary.ApplyHaze(_renderOptions.Tint, _renderOptions.HazeAmount);
            }

            return geostationary;
        }
    }
}