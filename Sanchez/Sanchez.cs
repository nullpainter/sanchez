using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Sanchez.Compositors;
using Sanchez.Exceptions;
using Sanchez.Helpers;
using Sanchez.Models;
using Sanchez.Services;
using Sanchez.Services.Underlay;
using Newtonsoft.Json;
using Serilog;
using ShellProgressBar;

namespace Sanchez
{
    internal class Sanchez
    {
        private readonly RenderOptions _renderOptions;
        private readonly IProgressBar _progressBar;
        private readonly IConsoleLogger _consoleLogger;
        private readonly ISatelliteRegistry _satelliteRegistry;
        private readonly ICompositor _compositor;
        private readonly IUnderlayCacheRepository _underlayCacheRepository;

        public Sanchez(
            RenderOptions renderOptions,
            IProgressBar progressBar,
            IConsoleLogger consoleLogger,
            ISatelliteRegistry satelliteRegistry,
            ICompositor compositor,
            IUnderlayCacheRepository underlayCacheRepository)
        {
            _renderOptions = renderOptions;
            _progressBar = progressBar;
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
            var numRendered = await _compositor.ComposeAsync(cancellationToken);
            
            _progressBar.Dispose();
            
            if (numRendered > 0) Console.WriteLine($"Output saved to {Path.GetFullPath(_renderOptions.OutputPath)}");

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