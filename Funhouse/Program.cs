using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Funhouse.ImageProcessing.Blur;
using Funhouse.ImageProcessing.Projection;
using Funhouse.Models.Configuration;
using Funhouse.Services;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Funhouse
{
    internal static class Program
    {
        private static async Task Main()
        {
            const string sourcePath = @"Resources\SampleImages\GOES17_FD_CH13_20200816T030031Z.jpg";
            //const string sourcePath = @"Resources\SampleImages\GOES16_FD_CH13_20200816T025020Z.jpg";
            
            const string targetPath = "c:\\temp\\out.jpg";

            var configuration = new RenderOptions(InterpolationType.Bilinear, false);

            // TODO may need to fix this for self-contained executables
            // var applicationPath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName)!;
            // var definitionsPath = Path.Combine(applicationPath, Constants.DefinitionsPath);
            
            var definitionsPath =  Constants.DefinitionsPath;
            
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

            var definition = registry.Locate(sourcePath);

            if (definition == null)
            {
                await Console.Error.WriteLineAsync("Unable to determine satellite based on file prefix");
                Environment.Exit(-1);
                return;
            }

            Console.WriteLine($"Reprojecting {definition.DisplayName} image");

            var stopwatch = Stopwatch.StartNew();


            using var source = (await Image.LoadAsync(sourcePath)).CloneAs<Rgba32>();

            // Normalise to 2km spatial resolution
            source.Mutate(c => c.Resize(5424, 5424));

            // Blur edges to avoid artifacts in projected output
            source.BlurEdges(0.995f);

            var target = source.Reproject(definition, configuration);


            // Draw image onto black background as we we have applied transparency
            var background = new Image<Rgba32>(target.Width, target.Height);
            background.Mutate(c => c
                .Fill(Color.Black)
                .Vignette()
                .DrawImage(target, PixelColorBlendingMode.Normal, 1.0f));

            await background.SaveAsync(targetPath);

            Console.WriteLine($"Elapsed time: {stopwatch.Elapsed}");
            Console.WriteLine($"Output written to {targetPath}");
        }
    }
}