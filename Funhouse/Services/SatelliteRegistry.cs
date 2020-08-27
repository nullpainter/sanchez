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
                d.FilePrefix!,
                d.DisplayName!,
                Angle.FromDegrees(d.Longitude),
                
                new Range(
                    Angle.FromDegrees(d.VisibleRange.MinLatitude),
                    Angle.FromDegrees(d.VisibleRange.MaxLatitude)),
                
                new Range(
                    Angle.FromDegrees(d.VisibleRange.MinLongitude),
                    Angle.FromDegrees(d.VisibleRange.MaxLongitude)),
                new ImageOffset(Angle.FromRadians(d.ImageOffset.X), Angle.FromRadians(d.ImageOffset.Y), d.ImageOffset.ScaleFactor),
                d.Height)).ToList();

            _initialised = true;
        }

        public SatelliteDefinition? Locate(string pattern)
        {
            if (!_initialised) throw new InvalidOperationException($"Registry not initialised; call {nameof(InitialiseAsync)} before use");

            return _definitions!.FirstOrDefault(d =>
                Path.GetFileName(pattern).StartsWith(d.FilePrefix, StringComparison.CurrentCultureIgnoreCase));
        }
    }
}