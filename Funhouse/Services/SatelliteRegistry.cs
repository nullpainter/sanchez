using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Funhouse.Models.Configuration;
using Funhouse.Models.Configuration.Definitions;
using MathNet.Spatial.Units;
using Newtonsoft.Json;
using Range = Funhouse.Models.Angles.Range;

namespace Funhouse.Services
{
    // Interim code-based registry
    public class SatelliteRegistry
    {
        private readonly string _definitionsPath;
        private List<SatelliteDefinition>? _definitions;
        private bool _initialised;

        public SatelliteRegistry(string definitionsPath) => _definitionsPath = definitionsPath;

        public async Task InitialiseAsync()
        {
            var json = await File.ReadAllTextAsync(_definitionsPath);
            var definitions = JsonConvert.DeserializeObject<List<SatelliteConfiguration>>(json);

            _definitions = definitions.Select(d => new SatelliteDefinition(
                    d.FilePrefix!,
                    d.DisplayName!,
                    Angle.FromDegrees(d.Longitude),
                    new Range(
                        Angle.FromDegrees(d.VisibleRange.MinLongitude),
                        Angle.FromDegrees(d.VisibleRange.MaxLongitude)),
                    new ImageOffset(Angle.FromRadians(d.ImageOffset.X), Angle.FromRadians(d.ImageOffset.Y), d.ImageOffset.ScaleFactor),
                    d.Height)).ToList()
                ;

            _initialised = true;
        }

        public SatelliteDefinition? Locate(string filePath) => LocateByPrefix(Path.GetFileName(filePath));

        public SatelliteDefinition? LocateByPrefix(string prefix)
        {
            if (!_initialised) throw new InvalidOperationException($"Registry not initialised; call {nameof(InitialiseAsync)} before use");

            return _definitions!.FirstOrDefault(d =>
                    prefix.StartsWith(d.FilePrefix, StringComparison.CurrentCultureIgnoreCase)); 
        }
    }
}