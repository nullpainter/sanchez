using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Funhouse.Exceptions;
using Funhouse.Models;
using Funhouse.Services;
using Funhouse.Services.Underlay;
using Newtonsoft.Json;
using Serilog;

namespace Funhouse
{
    internal class Funhouse
    {
        private readonly ISatelliteRegistry _satelliteRegistry;
        private readonly ICompositor _compositor;
        private readonly IUnderlayCacheRepository _underlayCacheRepository;

        public Funhouse(ISatelliteRegistry satelliteRegistry, ICompositor compositor, IUnderlayCacheRepository underlayCacheRepository)
        {
            _satelliteRegistry = satelliteRegistry;
            _compositor = compositor;
            _underlayCacheRepository = underlayCacheRepository;
        }

        public async Task ProcessAsync(CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();

            _underlayCacheRepository.Initialise();
            await InitialiseSatelliteRegistryAsync();
            await _compositor.ComposeAsync(cancellationToken);

            Log.Information("Elapsed time: {elapsed}", stopwatch.Elapsed);
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
                throw new ValidationException();
            }

            try
            {
                // Initialise satellite registry
                await _satelliteRegistry.InitialiseAsync(Constants.DefinitionsPath);
            }
            catch (JsonSerializationException e)
            {
                await Console.Error.WriteLineAsync($"Unable to parse satellite definition file: {e.Message}");
                throw new ValidationException();
            }
        }
    }
}