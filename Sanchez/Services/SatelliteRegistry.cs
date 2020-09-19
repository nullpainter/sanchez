using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Sanchez.Models;
using Sanchez.Models.Configuration;
using Newtonsoft.Json;
using Angle = Sanchez.Models.Angle;
using Range = Sanchez.Models.Angles.Range;

namespace Sanchez.Services
{
    public interface ISatelliteRegistry
    {
        Task InitialiseAsync();
        SatelliteDefinition? Locate(string pattern);
    }

    public class SatelliteRegistry : ISatelliteRegistry
    {
        private readonly RenderOptions _options;
        private List<SatelliteDefinition>? _definitions;
        private bool _initialised;

        public SatelliteRegistry(RenderOptions options) => _options = options;

        public async Task InitialiseAsync()
        {
            var json = await File.ReadAllTextAsync(_options.DefinitionsPath);
            var definitions = JsonConvert.DeserializeObject<List<SatelliteConfiguration>>(json);

            _definitions = definitions.Select(d => new SatelliteDefinition(
                d.DisplayName!,
                d.FilenamePrefix!,
                d.FilenameParserType,
                Angle.FromDegrees(d.Longitude + d.LongitudeAdjustment.GetValueOrDefault()).Radians,
                
                new Range(
                    Angle.FromDegrees(d.CropRange.MinLatitude),
                    Angle.FromDegrees(d.CropRange.MaxLatitude)),
                
                new Range(
                    Angle.FromDegrees(d.CropRange.MinLongitude),
                    Angle.FromDegrees(d.CropRange.MaxLongitude)),
                d.Height,
                d.Crop,
                d.Brightness)).ToList();

            _initialised = true;
        }

        public SatelliteDefinition? Locate(string pattern)
        {
            if (!_initialised) throw new InvalidOperationException($"Registry not initialised; call {nameof(InitialiseAsync)} before use");

            return _definitions!.FirstOrDefault(d =>
                Path.GetFileName(pattern).StartsWith(d.FilenamePrefix, StringComparison.CurrentCultureIgnoreCase));
        }
    }
}