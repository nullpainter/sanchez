using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Funhouse.Models;
using Funhouse.Models.Configuration;
using Funhouse.Models.Projections;
using Funhouse.Services.Filesystem;
using Serilog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Funhouse.Services
{
    public interface IImageLoader
    {
        Task<SatelliteImages> LoadImagesAsync();
    }

    public class ImageLoader : IImageLoader
    {
        private readonly ISatelliteRegistry _satelliteRegistry;
        private readonly IImageLocator _imageLocator;
        private readonly RenderOptions _options;

        public ImageLoader(
            ISatelliteRegistry satelliteRegistry, 
            IImageLocator imageLocator, 
            RenderOptions options)
        {
            _satelliteRegistry = satelliteRegistry;
            _imageLocator = imageLocator;
            _options = options;
        }

        public async Task<SatelliteImages> LoadImagesAsync()
        {
            var paths = _imageLocator.LocateImages(_options.SourcePath);

            var unmappedProjections = false;
            var satelliteImageLoadTasks = new List<Task<SatelliteImage>>();
            foreach (var path in paths)
            {
                var definition = _satelliteRegistry.Locate(path);

                if (definition == null)
                {
                    unmappedProjections = true;
                    await Console.Error.WriteLineAsync($"Unable to determine satellite based on file prefix: {path}");
                    continue;
                }

                satelliteImageLoadTasks.Add(LoadAsync(path, definition));
            }

            // Verify that all images have an associated projection definition
            if (unmappedProjections)
            {
                Log.Error("Exiting because of unmapped satellite definitions");
                Environment.Exit(-1);
            }

            await Task.WhenAll(satelliteImageLoadTasks);
            return new SatelliteImages(satelliteImageLoadTasks.Select(t => t.Result));
        } 
        
        // TODO rename this class/method  - suggests we are just loading any old image
        private async Task<SatelliteImage> LoadAsync(string path, SatelliteDefinition definition)
        {
            var image = await Image.LoadAsync<Rgba32>(path);
            return new SatelliteImage(path, image, definition);
        }
    }
}