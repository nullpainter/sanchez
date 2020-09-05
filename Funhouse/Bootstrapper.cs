using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CommandLine;
using Extend;
using Funhouse.Builders;
using Funhouse.Extensions;
using Funhouse.Factories;
using Funhouse.Helpers;
using Funhouse.Models;
using Funhouse.Models.CommandLine;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using SimpleInjector;
using static Funhouse.Models.Constants.Satellite;
using ProjectionOptions = Funhouse.Models.CommandLine.ProjectionOptions;

[assembly: InternalsVisibleTo("Funhouse.Test")]
namespace Funhouse
{
    internal static class Bootstrapper
    {
        internal static async Task Main(params string[] args)
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Converters = new Collection<JsonConverter> { new StringEnumConverter() }
            };

            try
            {
                await Parser.Default.ParseArguments<CommandLineOptions>(args).WithParsedAsync(async options =>
                {
                    // Disable stdout if required
                    if (options.Quiet) Console.SetOut(TextWriter.Null);

                    await ValidateOptionsAsync(options);
                    var renderOptions = RenderOptionFactory.ToRenderOptions(options);

                    // Build DI container
                    var container = new Container().AddAllService(options, renderOptions);
                    container.Verify();

                    ConfigureLogging(options.Verbose);

                    Log.Information("Sanchez starting");

                    LogOptions(options, renderOptions);

                    // Perform image processing
                    await container
                        .GetInstance<Funhouse>()
                        .ProcessAsync();
                });
            }
            finally
            {
                Console.ResetColor();
            }
        }

        private static async Task ValidateOptionsAsync(CommandLineOptions options)
        {
            // Verify selected tint
            if (options.Tint.FromHexString() == null)
            {
                await Console.Error.WriteLineAsync("Unable to parse tint as a hex tuple. Expected format is 5ebfff");
                Environment.Exit(-1);
            }

            if (options.HazeAmount < 0 || options.HazeAmount > 1)
            {
                await Console.Error.WriteLineAsync("Invalid haze amount; valid values are between 0.0 and 1.0");
                Environment.Exit(-1);
            }

            // Verify underlay image exists
            if (!File.Exists(options.UnderlayPath ?? Constants.DefaultUnderlayPath))
            {
                await Console.Error.WriteLineAsync($"Underlay path {options.UnderlayPath} isn't valid");
                Environment.Exit(-1);
            }

            // Verify argument compatibility

            if (options.ProjectionType == ProjectionOptions.G)
            {
                if (options.AutoCrop)
                {
                    await Console.Error.WriteLineAsync($"Autocrop only available with projection type {ProjectionType.Geostationary}");
                    Environment.Exit(-1);
                }
            }

            // Verify spatial resolution
            if (!options.SpatialResolution.IsIn(SpatialResolution.TwoKm, SpatialResolution.FourKm))
            {
                await Console.Error.WriteLineAsync( $"Unsupported output spatial resolution. Valid values are: {SpatialResolution.TwoKm}, {SpatialResolution.FourKm}");
                Environment.Exit(-1); 
            }
        }

        private static void LogOptions(CommandLineOptions commandLineOptions, RenderOptions renderOptions)
        {
            if (commandLineOptions.AutoCrop) Log.Information("Autocrop enabled");
            if (commandLineOptions.Stitch) Log.Information("Stitching enabled");
            
            Log.Information("Render mode {renderMode}", renderOptions.ProjectionType);
            Log.Information("Interpolation type {type}", renderOptions.InterpolationType);
            Log.Information("{km}km spatial resolution", commandLineOptions.SpatialResolution);
        }

        /// <summary>
        ///     Configures logging output.
        /// </summary>
        private static void ConfigureLogging(bool consoleLogging)
        {
            var builder = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.RollingFile(Path.Combine(PathHelper.LogPath(), "sanchez-{Date}.log"), LogEventLevel.Information, fileSizeLimitBytes: 5 * 1024 * 1024)
                .Enrich.FromLogContext()
                .Enrich.WithExceptionDetails();

            if (consoleLogging) builder.WriteTo.Console();

            Log.Logger = builder.CreateLogger();
        }
    }
}