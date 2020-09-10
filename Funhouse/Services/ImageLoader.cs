using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Funhouse.Exceptions;
using Funhouse.Helpers;
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
        private readonly IFileService _fileService;
        private readonly ISatelliteRegistry _satelliteRegistry;
        private readonly IImageMatcher _imageMatcher;
        private readonly RenderOptions _options;

        public ImageLoader(
            IFileService fileService,
            ISatelliteRegistry satelliteRegistry,
            IImageMatcher imageMatcher,
            RenderOptions options)
        {
            _fileService = fileService;
            _satelliteRegistry = satelliteRegistry;
            _imageMatcher = imageMatcher;
            _options = options;
        }

        public async Task<SatelliteImages> LoadImagesAsync()
        {
            // Retrieve all file details matching source pattern
            var sourceFiles = _fileService.GetSourceFiles();
            List<string> matchedFiles;

            // If we are combining files by timestamp, locate all matching files based on the timestamp and tolerance
            matchedFiles = _options.TargetTimestamp == null ? sourceFiles : _imageMatcher.LocateMatchingImages(sourceFiles);

            // Verify images were found
            if (!matchedFiles.Any())
            {
                ConsoleLog.Warning("No matching source images found");
                throw new ValidationException();
            }

            var satelliteImageLoadTasks = new List<Task<SatelliteImage>>();
            foreach (var path in matchedFiles)
            {
                var definition = _satelliteRegistry.Locate(path);

                if (definition == null)
                {
                    ConsoleLog.Warning($"Unable to determine satellite for file: {path}; ignoring");
                    continue;
                }

                satelliteImageLoadTasks.Add(LoadAsync(path, definition));
            }

            // Verify that all images have an associated projection definition
            if (!satelliteImageLoadTasks.Any())
            {
                Log.Error("No images found");
                throw new ValidationException();
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