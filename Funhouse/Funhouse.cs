using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Funhouse.Compositors;
using Funhouse.Exceptions;
using Funhouse.Helpers;
using Funhouse.Services;
using Funhouse.Services.Underlay;
using Newtonsoft.Json;
using Serilog;

namespace Funhouse
{
    internal class Funhouse
    {
        private readonly IConsoleLogger _consoleLogger;
        private readonly ISatelliteRegistry _satelliteRegistry;
        private readonly ICompositor _compositor;
        private readonly IUnderlayCacheRepository _underlayCacheRepository;

        public Funhouse(
            IConsoleLogger consoleLogger,
            ISatelliteRegistry satelliteRegistry, 
            ICompositor compositor, 
            IUnderlayCacheRepository underlayCacheRepository)
        {
            _consoleLogger = consoleLogger;
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

            Console.WriteLine();

            Log.Information("Elapsed time: {elapsed}", stopwatch.Elapsed);
        }

        /// <summary>
        ///     Registers all known satellites.
        /// </summary>
        private async Task InitialiseSatelliteRegistryAsync()
        {
            try
            {
                // Initialise satellite registry
                await _satelliteRegistry.InitialiseAsync();
            }
            catch (JsonSerializationException e)
            {
                _consoleLogger.Error($"Unable to parse satellite definition file: {e.Message}");
                throw new ValidationException();
            }
        }
    }
}