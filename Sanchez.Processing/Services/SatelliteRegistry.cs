using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Sanchez.Processing.Models;
using Sanchez.Processing.Models.Configuration;
using Range = Sanchez.Processing.Models.Angles.Range;

namespace Sanchez.Processing.Services
{
    public interface ISatelliteRegistry
    {
        Task InitialiseAsync();
        (SatelliteDefinition? definition, DateTime? timestamp) Locate(string path);
    }

    public class SatelliteRegistry : ISatelliteRegistry
    {
        private readonly IVisibleRangeService _visibleRangeService;
        private readonly ILogger<SatelliteRegistry> _logger;
        private readonly RenderOptions _options;
        private List<SatelliteDefinition>? _definitions;
        private bool _initialised;

        public SatelliteRegistry(
            IVisibleRangeService visibleRangeService,
            ILogger<SatelliteRegistry> logger,
            RenderOptions options)
        {
            _visibleRangeService = visibleRangeService;
            _logger = logger;
            _options = options;
        }

        public async Task InitialiseAsync()
        {
            var json = await File.ReadAllTextAsync(_options.DefinitionsPath);
            var definitions = JsonConvert.DeserializeObject<List<SatelliteConfiguration>>(json);

            _definitions = definitions.Select(d => new SatelliteDefinition(
                d.DisplayName!,
                d.FilenamePrefix,
                d.FilenameSuffix,
                d.FilenameParserType,
                d.Invert,
                Angle.FromDegrees(d.Longitude + d.LongitudeAdjustment.GetValueOrDefault()).Radians,
                new Range(
                    Angle.FromDegrees(Constants.Satellite.CropRange.MinLatitude),
                    Angle.FromDegrees(Constants.Satellite.CropRange.MaxLatitude)),
                _visibleRangeService.GetVisibleRange(Angle.FromDegrees(d.Longitude)),
                d.Height,
                d.Crop,
                d.Brightness)).ToList();

            _initialised = true;
        }

        public (SatelliteDefinition? definition, DateTime? timestamp) Locate(string path)
        {
            if (!_initialised) throw new InvalidOperationException($"Registry not initialised; call {nameof(InitialiseAsync)} before use");

            var filename = Path.GetFileName(path);

            foreach (var definition in _definitions!)
            {
                var timestamp = definition.FilenameParser.GetTimestamp(filename);
                if (timestamp == null) continue;
                
                _logger.LogDebug("Matched {Definition} handler for {Filename}", definition.DisplayName, filename);
                return (definition, timestamp);
            }

            _logger.LogWarning("Unable to find filename parser for {Filename}", filename);

            return (null, null);
        }
    }
}