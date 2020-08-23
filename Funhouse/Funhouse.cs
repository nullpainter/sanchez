using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Funhouse.Builders;
using Funhouse.Models;
using Funhouse.Models.Projections;
using Funhouse.Projections;
using Funhouse.Services;
using Newtonsoft.Json;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using SimpleInjector;

[assembly: InternalsVisibleTo("Funhouse.Test")]

namespace Funhouse
{
    internal static class Funhouse
    {
        internal static async Task Main(params string[] args)
        {
            var cancellationToken = new CancellationTokenSource();

            try
            {
                await Parser.Default.ParseArguments<CommandLineOptions>(args).WithParsedAsync(async options =>
                {
                    var container = new Container().AddAllService(options);
                    container.Verify();

                    ConfigureLogging(options.Verbose);
                    Log.Information("Sanchez starting");

                    // Initialise the satellite registry
                    var satelliteRegistry = container.GetInstance<ISatelliteRegistry>();
                    await InitialiseSatelliteRegistryAsync(satelliteRegistry);

                    var projectionRegistry = container.GetInstance<IProjectionRegistry>();
                    InitialiseProjectionRegistry(projectionRegistry);

                    // Disable stdout if required
                    if (options.Quiet) Console.SetOut(TextWriter.Null);

                    // Composite images
                    var compositor = container.GetInstance<ICompositor>();
                    await compositor.ComposeAsync(cancellationToken);
                });
            }
            finally
            {
                Console.ResetColor();
            }
        }

        private static void InitialiseProjectionRegistry(IProjectionRegistry registry)
        {
            registry.Register(ProjectionType.Mercator, new MercatorProjection());
            registry.Register(ProjectionType.PseudoMercator, new PseudoMercatorProjection());
        }

        private static async Task InitialiseSatelliteRegistryAsync(ISatelliteRegistry registry)
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
                await registry.InitialiseAsync(Constants.DefinitionsPath);
            }
            catch (JsonSerializationException e)
            {
                await Console.Error.WriteLineAsync($"Unable to parse satellite definition file: {e.Message}");
                Environment.Exit(-1);
            }
        }

        /// <summary>
        ///     Configures logging output.
        /// </summary>
        private static void ConfigureLogging(bool consoleLogging)
        {
            var processFilename = Process.GetCurrentProcess().MainModule.FileName;

            // Determine correct location of logs relative to application, depending on whether we are running from a published
            // executable or via dotnet.
            var applicationPath = (Path.GetFileNameWithoutExtension(processFilename) == "dotnet"
                ? Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
                : Path.GetDirectoryName(processFilename))!;

            var builder = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.RollingFile(Path.Combine(applicationPath, "logs", "sanchez-{Date}.log"), LogEventLevel.Information, fileSizeLimitBytes: 5 * 1024 * 1024)
                .Enrich.FromLogContext()
                .Enrich.WithExceptionDetails();

            if (consoleLogging) builder.WriteTo.Console();

            Log.Logger = builder.CreateLogger();
        }
    }
}