using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Funhouse.Extensions.Images;
using Funhouse.ImageProcessing.Tint;
using Funhouse.ImageProcessing.Underlay;
using Funhouse.Models;
using Funhouse.Models.CommandLine;
using Funhouse.Models.Configuration;
using Funhouse.Models.Projections;
using Funhouse.Services.Underlay;
using MathNet.Spatial.Units;
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
        private readonly CommandLineOptions _commandLineOptions;
        private readonly RenderOptions _renderOptions;
        private readonly IImageStitcher _imageStitcher;
        private readonly IImageLoader _imageLoader;
        private readonly IUnderlayService _underlayService;
        private readonly IProjectionActivityOperations _activityOperations;

        public Compositor(
            CommandLineOptions commandLineOptions,
            RenderOptions renderOptions,
            IImageStitcher imageStitcher,
            IImageLoader imageLoader,
            IUnderlayService underlayService,
            IProjectionActivityOperations activityOperations)
        {
            _commandLineOptions = commandLineOptions;
            _renderOptions = renderOptions;
            _imageStitcher = imageStitcher;
            _imageLoader = imageLoader;
            _underlayService = underlayService;
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

            var activities = satelliteImageLoadTasks.Select(task => task.Result).ToList();
            _activityOperations.Initialise(activities);

            // Verify that all images have an associated projection definition
            var unmappedProjections = _activityOperations.GetUnmapped();
            if (unmappedProjections.Any())
            {
                unmappedProjections.ForEach(p => Console.Error.WriteLineAsync($"Unable to determine satellite based on file prefix: {p.Path}"));
                Environment.Exit(-1);
            }

            // Calculate crop for each image based on visible range and image overlaps
            Log.Information("Calculating crop");
            _activityOperations.CalculateOverlap();

            Log.Information("Scaling and normalising IR colour");
            _activityOperations.NormaliseSize();
            _activityOperations.NormaliseHistogram();

            // TODO this needs beefing up. Should be able to stitch and then reproject,e tc.
            switch (_renderOptions.ProjectionType)
            {
                case ProjectionType.Geostationary:
                {
                    await _activityOperations.RenderGeostationaryUnderlayAsync();

                    // TODO we need to stick this in the otuput dircetory - currently same directory as source  
                    await _activityOperations.SaveAsync("-FC", _commandLineOptions);
                }

                    break;
                case ProjectionType.Equirectangular:
                {
                    // Reproject all images to equirectangular
                    _activityOperations.Reproject();

                    // Stitch images if required
                    if (_commandLineOptions.Stitch)
                    {
                        // Combine reprojected images
                        var target = await StitchImagesAsync(activities);

                        Log.Information("Reprojecting to geostationary");

                        // FIXME hack hack hack
                        var lon = -42; // Also, wrong longitude because map has been wrapped

                        var definition = new SatelliteDefinition("", "", Angle.FromDegrees(lon).Radians,
                            new Range(Angle.FromDegrees(Constants.Satellite.VisibleRange.DefaultMinLatitude).Radians, 
                                Angle.FromDegrees(Constants.Satellite.VisibleRange.DefaultMaxLatitude).Radians),
                            new Range(Angle.FromDegrees(lon - 80).Radians, Angle.FromDegrees(lon + 80).Radians)); // HACK


                        // TEMP
                        var arse = target.ToGeostationaryProjection(definition, _renderOptions);
                        await arse.SaveAsync("reprojected.jpg");

                        // Save output
                        Log.Information("Saving stitched output");
                        await target.SaveAsync(_commandLineOptions.OutputPath);

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
            }
        }

        private async Task<Image<Rgba32>> StitchImagesAsync(List<ProjectionActivity> activities)
        {
            var target = _imageStitcher.Stitch(activities);

            // Calculate crop region if required
            Rectangle? cropRectangle = null;
            if (_commandLineOptions.AutoCrop)
            {
                cropRectangle = target.AutoCrop();
                if (cropRectangle == null) Log.Error("Unable to autocrop bounds");
                else Log.Information("Cropped image size: {width} x {height} px", cropRectangle.Value.Width, cropRectangle.Value.Height);
            }

            // Determine visible range of all satellite imagery
            GetVisibleRange(activities, out var latitudeRange, out var longitudeRange);

            // Load underlay
            var underlayOptions = new UnderlayProjectionOptions(
                _renderOptions.ProjectionType,
                _renderOptions.InterpolationType,
                target.Size(),
                latitudeRange, longitudeRange);

            // Retrieve underlay
            var underlay = await _underlayService.GetUnderlayAsync(underlayOptions, underlayPath: _commandLineOptions.UnderlayPath);

            Log.Information("Tinting and normalising IR imagery");

            var clone = target.Clone();
            clone.Mutate(c => c.HistogramEqualization());
            target.Tint(_renderOptions.Tint);

            target.Mutate(c => c.DrawImage(clone, PixelColorBlendingMode.HardLight, 0.5f));

            // Render underlay and optionally crop to size
            Log.Information("Blending with underlay");
            target.Mutate(ctx => ctx.DrawImage(underlay, PixelColorBlendingMode.Screen, 1.0f));

            // Crop composited image
            if (cropRectangle != null)
            {
                Log.Information("Cropping");
                target.Mutate(ctx => ctx.Crop(cropRectangle.Value));
            }

            // Perform global colour correction
            target.ColourCorrect(_renderOptions);

            return target;
        }

        // TODO move to service - some sort of grouped activity service?
        private void GetVisibleRange(List<ProjectionActivity> activities, out Range latitudeRange, out Range longitudeRange)
        {
            var sortedActivities = activities.OrderBy(p => p.Offset.X).ToList();

            latitudeRange = new Range(
                activities.Min(a => a.LatitudeRange.Start),
                activities.Max(a => a.LatitudeRange.End));

            longitudeRange = new Range(
                sortedActivities.First().LongitudeRange.Start,
                sortedActivities.Last().LongitudeRange.End
            ).UnwrapLongitude();
        }
    }
}