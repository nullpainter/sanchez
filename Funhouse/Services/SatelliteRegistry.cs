using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Funhouse.Models.Configuration;
using Newtonsoft.Json;
using Angle = Funhouse.Models.Angle;
using Range = Funhouse.Models.Angles.Range;

namespace Funhouse.Services
{
    public interface ISatelliteRegistry
    {
        Task InitialiseAsync(string definitionsPath);
        SatelliteDefinition? Locate(string pattern);
    }

    public class SatelliteRegistry : ISatelliteRegistry
    {
        private List<SatelliteDefinition>? _definitions;
        private bool _initialised;

        public async Task InitialiseAsync(string definitionsPath)
        {
            var json = await File.ReadAllTextAsync(definitionsPath);
            var definitions = JsonConvert.DeserializeObject<List<SatelliteConfiguration>>(json);

            _definitions = definitions.Select(d => new SatelliteDefinition(
                d.DisplayName!,
                d.FilenamePattern!,
                d.FilenameParserType,
                Angle.FromDegrees(d.Longitude + d.LongitudeAdjustment.GetValueOrDefault()).Radians,
                
                new Range(
                    Angle.FromDegrees(d.VisibleRange.MinLatitude),
                    Angle.FromDegrees(d.VisibleRange.MaxLatitude)),
                
                new Range(
                    Angle.FromDegrees(d.VisibleRange.MinLongitude),
                    Angle.FromDegrees(d.VisibleRange.MaxLongitude)),
                d.Height,
                d.Crop,
                d.Brightness)).ToList();

            _initialised = true;
        }

        public SatelliteDefinition? Locate(string pattern)
        {
            if (!_initialised) throw new InvalidOperationException($"Registry not initialised; call {nameof(InitialiseAsync)} before use");

            return _definitions!.FirstOrDefault(d =>
                Path.GetFileName(pattern).StartsWith(d.FilenamePattern, StringComparison.CurrentCultureIgnoreCase));
        }
    }
}