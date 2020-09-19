using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sanchez.Exceptions;
using Sanchez.Models;
using Sanchez.Models.Projections;
using Sanchez.Services.Filesystem;
using Serilog;

namespace Sanchez.Services
{
    public interface ISatelliteImageLoader
    {
        Activity RegisterImages();
    }

    public class SatelliteImageLoader : ISatelliteImageLoader
    {
        private readonly FilenameParserProvider _filenameParserProvider;
        private readonly IFileService _fileService;
        private readonly ISatelliteRegistry _satelliteRegistry;
        private readonly IImageMatcher _imageMatcher;
        private readonly RenderOptions _options;

        public SatelliteImageLoader(
            FilenameParserProvider filenameParserProvider,
            IFileService fileService,
            ISatelliteRegistry satelliteRegistry,
            IImageMatcher imageMatcher,
            RenderOptions options)
        {
            _filenameParserProvider = filenameParserProvider;
            _fileService = fileService;
            _satelliteRegistry = satelliteRegistry;
            _imageMatcher = imageMatcher;
            _options = options;
        }

        public Activity RegisterImages()
        {
            // Retrieve all file details matching source pattern
            var sourceFiles = _fileService.GetSourceFiles();
            
            // If we are combining files by timestamp, locate all matching files based on the timestamp and tolerance
            List<string> matchedFiles;
            try
            {
                matchedFiles = _options.TargetTimestamp == null ? sourceFiles : _imageMatcher.LocateMatchingImages(sourceFiles);

                // Verify images were found
                if (!matchedFiles.Any())
                {
                    throw new ValidationException("No matching source images found");
                }
            }
            catch (DirectoryNotFoundException e)
            {
                throw new ValidationException("Source directory not found", e);
            }

            var registrations = new List<Registration>();
            foreach (var path in matchedFiles)
            {
                var definition = _satelliteRegistry.Locate(path);

                if (definition == null)
                {
                    Log.Warning($"Unable to determine satellite for file: {path}; ignoring");
                    continue;
                }
                
                var parser = _filenameParserProvider.GetParser(definition.FilenameParserType);
                if (parser == null)
                {
                    Log.Warning("Unable to find parser for type {type}", definition.FilenameParserType);
                    continue;
                }

                if (parser.GetTimestamp(path) != null)
                {
                    registrations.Add(new Registration(path, definition));
                }
                else
                {
                    Log.Warning($"Unable to match file: {path}; ignoring");
                }
            }

            // Verify that all images have an associated projection definition
            if (!registrations.Any())
            {
                Log.Error("No valid images found");
                throw new ValidationException("No valid images found");
            }

            return new Activity(registrations);
        }
    }
}