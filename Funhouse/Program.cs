using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Funhouse.Extensions;
using Funhouse.ImageProcessing.Blur;
using Funhouse.ImageProcessing.Projection;
using Funhouse.Models.Angles;
using Funhouse.Models.Configuration;
using Funhouse.Services;
using MathNet.Spatial.Units;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Range = Funhouse.Models.Angles.Range;

namespace Funhouse
{
    internal static class Program
    {
        private static async Task Main()
        {
            const string targetPath = "out.jpg"; //"c:\\temp\\out.png";

            var configuration = new RenderOptions(InterpolationType.Bilinear, false);

            // TODO may need to fix this for self-contained executables
            // var applicationPath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName)!;
            // var definitionsPath = Path.Combine(applicationPath, Constants.DefinitionsPath);

            var definitionsPath = Constants.DefinitionsPath;

            if (!File.Exists(definitionsPath))
            {
                await Console.Error.WriteLineAsync($"Unable to find satellite definition file: {definitionsPath}");
                Environment.Exit(-1);
                return;
            }

            var registry = new SatelliteRegistry(definitionsPath);

            try
            {
                await registry.InitialiseAsync();
            }
            catch (JsonSerializationException e)
            {
                await Console.Error.WriteLineAsync($"Unable to parse satellite definition file: {e.Message}");
                Environment.Exit(-1);
                return;
            }

            const string goes17 = @"Resources\SampleImages\GOES17_FD_CH13_20200816T030031Z.jpg";
            const string goes16 = @"Resources\SampleImages\GOES16_FD_CH13_20200816T025020Z.jpg";

            var stopwatch = Stopwatch.StartNew();

            //
            // var outputWidth = goes17Projection.Image.Width + goes16Projection.Image.Width + 1000; // HACK
            //
            // var projections = new List<ProjectedImage> { goes16Projection, goes17Projection };
            // projections.ForEach(d => d.Definition.VisibleRange = d.Definition.VisibleRange.NormaliseLongitude());
            // projections = projections.OrderByDescending(d => d.Definition.VisibleRange.Start.Radians).ToList();
            //
            //
            var goes16Definition = registry.Locate(goes16);
            if (goes16Definition == null)
            {
                await Console.Error.WriteLineAsync("Unable to determine satellite based on file prefix");
                Environment.Exit(-1);
            }

            var goes17Definition = registry.Locate(goes17);

            if (goes17Definition == null)
            {
                await Console.Error.WriteLineAsync("Unable to determine satellite based on file prefix");
                Environment.Exit(-1);
            }

            var overlapCalculator = new ProjectionOverlapCalculator(goes16Definition, goes17Definition);

            var goes17Projected = await ReprojectAsync(overlapCalculator, goes17Definition, goes17, configuration);
            await goes17Projected.SaveAsync("goes17.jpg");

            var goes16Projected = await ReprojectAsync(overlapCalculator, goes16Definition, goes16,  configuration);
            await goes16Projected.SaveAsync("goes16.jpg");
            
            // TODO this is awful, also shoudl be automatic
            var target = new Image<Rgba32>(goes17Projected.Width + goes16Projected.Width, goes17Projected.Height); //FIXME work outprper width
            
            // TODO this is awful, also shoudl be automatic
            var offset = Angle.FromDegrees(-300);
            
            var minProjectedLongitude16 = new ProjectionAngle((goes16Definition.VisibleRange.Start + offset).NormaliseLongitude(), new Angle());
            var minProjectedLongitude17 = new ProjectionAngle((goes17Definition.VisibleRange.Start + offset).NormaliseLongitude(), new Angle());
            
            var minX16 = minProjectedLongitude16.ToPixelCoordinates(Constants.ImageSize * 2, 0);
            var minX17 = minProjectedLongitude17.ToPixelCoordinates(Constants.ImageSize * 2, 0);
            //
            // Draw image onto black background as we have applied transparency
            target.Mutate(c => c
                .Fill(Color.Black)
                .DrawImage(goes17Projected, new Point((int) Math.Round(minX17.X), 0), PixelColorBlendingMode.Screen, 1.0f)
                .DrawImage(goes16Projected, new Point((int) Math.Round(minX16.X), 0), PixelColorBlendingMode.Screen, 1.0f));
            
            
            // .HistogramEqualization());
            
            await target.SaveAsync(targetPath);
            
            
            Console.WriteLine($"Output written to {Path.GetFullPath(targetPath)}");
            
            Console.WriteLine($"Elapsed time: {stopwatch.Elapsed}");
        }

        private static async Task<Image<Rgba32>> ReprojectAsync(ProjectionOverlapCalculator overlapCalculator, SatelliteDefinition definition, string sourcePath, RenderOptions configuration)
        {
            var longitudeCrop = overlapCalculator.GetNonOverlappingRange(definition);
 
            Console.WriteLine($"{definition.DisplayName} range {definition.VisibleRange.Start.Degrees:F2} to {definition.VisibleRange.End.Degrees:F2} degrees");

            Console.WriteLine($"{definition.DisplayName} crop {longitudeCrop.Start.Degrees:F2} to {longitudeCrop.End.Degrees:F2} degrees");

            using var source = (await Image.LoadAsync(sourcePath)).CloneAs<Rgba32>();

            // Normalise to 2km spatial resolution
            source.Mutate(c => c.Resize(Constants.ImageSize, Constants.ImageSize));

            // Blur edges to avoid artifacts in projected output
            source.BlurEdges(0.995f);

            return source.Reproject(definition, configuration, longitudeCrop);
        }
    }
}