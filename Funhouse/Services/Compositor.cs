using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Funhouse.Extensions;
using Funhouse.ImageProcessing.Crop;
using Funhouse.Models;
using Funhouse.Models.Projections;
using Serilog;
using SixLabors.ImageSharp;

namespace Funhouse.Services
{
    public interface ICompositor
    {
        Task ComposeAsync(CancellationTokenSource cancellationToken);
    }

    public class Compositor : ICompositor
    {
        private readonly CommandLineOptions _options;
        private readonly IImageStitcher _imageStitcher;
        private readonly IImageLoader _imageLoader;
        private readonly IProjectionActivityOperations _activityOperations;

        public Compositor(
            CommandLineOptions options,
            IImageStitcher imageStitcher,
            IImageLoader imageLoader,
            IProjectionActivityOperations activityOperations)
        {
            _options = options;
            _imageStitcher = imageStitcher;
            _imageLoader = imageLoader;
            _activityOperations = activityOperations;
        }

        public async Task ComposeAsync(CancellationTokenSource cancellationToken)
        {
            if (_options.AutoCrop)
            {
                _options.BlurEdges = false;
                Log.Information("Disabling edge blurring as auto crop enabled");
            }

            // TODO may need to fix this for self-contained executables
            // var applicationPath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName)!;
            // var definitionsPath = Path.Combine(applicationPath, Constants.DefinitionsPath);

            var underlay = Image.LoadAsync(@"Resources\world.200411.3x21600x10800.jpg");

            string[] paths =
            {
                @"Resources\SampleImages\GOES17_FD_CH13_20200816T030031Z.jpg",
                @"Resources\SampleImages\GOES16_FD_CH13_20200816T025020Z.jpg"
            };

            var activities = new List<ProjectionActivity>();
            foreach (var path in paths)
            {
                if (cancellationToken.IsCancellationRequested) return;
                
                var activity = await _imageLoader.LoadAsync(path);
                activities.Add(activity);
            }

            _activityOperations.Initialise(activities, cancellationToken);


            // Verify that all images have an associated projection definition
            var unmappedProjections = _activityOperations.GetUnmapped();
            if (unmappedProjections.Any())
            {
                unmappedProjections.ForEach(p => Console.Error.WriteLineAsync($"Unable to determine satellite based on file prefix: {p.Path}"));
                Environment.Exit(-1);
            }

            var stopwatch = Stopwatch.StartNew();

            _activityOperations.CalculateCrop();
            await _activityOperations.ReprojectAsync();

            if (cancellationToken.IsCancellationRequested) return;

            // Stitch
            if (_options.Stitch)
            {
                var target = _imageStitcher.Stitch(activities);

                // Save uncropped stitched image for debugging purposes
                if (_options.Debug)
                {
                    var targetFilename = Path.GetFileNameWithoutExtension(_options.OutputPath) + "-stitched.png"; 
                    await target.SaveAsync(targetFilename);
                }
                
                if (_options.AutoCrop) target.AutoCrop();
                
                // TODO only do if no underlay
                target.AddBackgroundColour(Color.Black);

                Log.Information("Saving output");
                await target.SaveAsync(_options.OutputPath);

                Console.WriteLine($"Output written to {Path.GetFullPath(_options.OutputPath)}");
            }

            Log.Information("Elapsed time: {elapsed}", stopwatch.Elapsed);
        }
    }
}