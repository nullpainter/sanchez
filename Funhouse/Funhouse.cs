using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Funhouse.Models;
using Funhouse.Services;
using Newtonsoft.Json;

namespace Funhouse
{
    internal class Funhouse
    {
        private readonly ISatelliteRegistry _satelliteRegistry;
        private readonly ICompositor _compositor;

        public Funhouse(ISatelliteRegistry satelliteRegistry, ICompositor compositor)
        {
            _satelliteRegistry = satelliteRegistry;
            _compositor = compositor;
        }
        
        public async Task ProcessAsync()
        {
            await InitialiseSatelliteRegistryAsync();
            await _compositor.ComposeAsync(); 
        }
        
        /// <summary>
        ///     Registers all known satellites.
        /// </summary>
        private async Task InitialiseSatelliteRegistryAsync()
        {
            const string definitionsPath = Constants.DefinitionsPath;

            // Verify that satellite definitions file is present
            if (!File.Exists(definitionsPath))
            {
                await Console.Error.WriteLineAsync($"Unable to find satellite definition file: {definitionsPath}");
                Environment.Exit(-1);
                return;
            }

            try
            {
                // Initialise satellite registry
                await _satelliteRegistry.InitialiseAsync(Constants.DefinitionsPath);
            }
            catch (JsonSerializationException e)
            {
                await Console.Error.WriteLineAsync($"Unable to parse satellite definition file: {e.Message}");
                Environment.Exit(-1);
            }
        }
    }
}