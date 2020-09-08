using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Funhouse.Models.Configuration;
using Serilog;

namespace Funhouse.Services.Filesystem
{
    public interface IImageLocator
    {
        List<string> LocateImages(string path);
    }

    public class ImageLocator : IImageLocator
    {
        private readonly FilenameParserProvider _filenameParserProvider;
        private readonly ISatelliteRegistry _registry;

        public ImageLocator(
            FilenameParserProvider filenameParserProvider,
            ISatelliteRegistry registry)
        {
            _filenameParserProvider = filenameParserProvider;
            _registry = registry;
        }

        public List<string> LocateImages(string path)
        {
            // TEMP HACK
            var targetTimestamp = new DateTime(2020, 08, 30, 03, 30, 00, DateTimeKind.Utc);
            var tolerance = TimeSpan.FromMinutes(30);

            var matched = new List<SatelliteMatch>();

            foreach (var file in GetFilenames(path))
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

            // Return the closest match per satellite
            return matched.GroupBy(m => m.Definition).Select(entry => entry.OrderBy(e => e.Deviation).First().Path).ToList();
        }

        private static IEnumerable<string> GetFilenames(string path)
        {
            if (File.Exists(path)) return new List<string> { path };
            if (Directory.Exists(path))
            {
                return Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories);
            }

            Console.Error.WriteLine($"{path} is neither a file nor a directory");
            Environment.Exit(-1);

            return new[] { "" };
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