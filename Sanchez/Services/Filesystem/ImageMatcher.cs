using System;
using System.Collections.Generic;
using System.Linq;
using Ardalis.GuardClauses;
using Sanchez.Models;
using Sanchez.Models.Configuration;
using Serilog;

namespace Sanchez.Services.Filesystem
{
    public interface IImageMatcher
    {
        List<string> LocateMatchingImages(List<string> candidateFiles);
    }

    public class ImageMatcher : IImageMatcher
    {
        private readonly RenderOptions _options;
        private readonly IFileService _fileService;
        private readonly FilenameParserProvider _filenameParserProvider;
        private readonly ISatelliteRegistry _registry;

        public ImageMatcher(
            RenderOptions options,
            IFileService fileService,
            FilenameParserProvider filenameParserProvider,
            ISatelliteRegistry registry)
        {
            _options = options;
            _fileService = fileService;
            _filenameParserProvider = filenameParserProvider;
            _registry = registry;
        }

        public List<string> LocateMatchingImages(List<string> candidateFiles)
        {
            Log.Information("Searching for images in {path} with a timestamp of {minutes} minutes tolerance to {target}", 
                _options.SourcePath, _options.Tolerance.TotalMinutes, _options.TargetTimestamp);

            // Return the closest match per satellite
            var matched = GetMatchedFiles(candidateFiles);
            return matched.GroupBy(m => m.Definition).Select(entry => entry.OrderBy(e => e.Deviation).First().Path).ToList();
        }

        private IEnumerable<SatelliteMatch> GetMatchedFiles(IEnumerable<string> candidateFiles)
        {
            var matched = new List<SatelliteMatch>();

            Guard.Against.Null(_options.TargetTimestamp, nameof(_options.TargetTimestamp));
            Guard.Against.Null(_options.Tolerance, nameof(_options.Tolerance));

            var targetTimestamp = _options.TargetTimestamp.Value;
            var tolerance = _options.Tolerance;

            foreach (var file in candidateFiles)
            {
                var definition = _registry.Locate(file);
                if (definition == null) continue;

                var parser = _filenameParserProvider.GetParser(definition.FilenameParserType);
                if (parser == null)
                {
                    Log.Warning("Unable to find parser for type {type}", definition.FilenameParserType);
                    continue;
                }

                var timestamp = parser.GetTimestamp(file);
                if (timestamp == null) continue;

                // Add files that have a timestamp within the tolerance of the selected time
                var deviation = Math.Abs((targetTimestamp - timestamp.Value).TotalMilliseconds);
                if (deviation < tolerance.TotalMilliseconds) matched.Add(new SatelliteMatch(definition, file, deviation));
            }

            return matched;
        }

        private class SatelliteMatch
        {
            public SatelliteMatch(SatelliteDefinition definition, string path, double deviation)
            {
                Definition = definition;
                Path = path;
                Deviation = deviation;
            }

            internal SatelliteDefinition Definition { get; }
            public string Path { get; }
            internal double Deviation { get; }
        }
    }
}