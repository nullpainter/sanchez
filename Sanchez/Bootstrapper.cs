using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using CommandLine;
using Extend;
using Sanchez.Builders;
using Sanchez.Exceptions;
using Sanchez.Helpers;
using Sanchez.Models;
using Sanchez.Models.CommandLine;
using Sanchez.Services;
using Sanchez.Validators;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using SimpleInjector;

[assembly: InternalsVisibleTo("Sanchez.Test")]
namespace Sanchez
{
    internal static class Bootstrapper
    {
        internal static async Task Main(params string[] args)
        {
            try
            {
                var cancellationToken = new CancellationTokenSource();

                // Explicitly handle ctrl+c to avoid writing corrupted files
                Console.CancelKeyPress += (sender, e) =>
                {
                    e.Cancel = true;
                    cancellationToken.Cancel();
                };

                JsonConvert.DefaultSettings = () => new JsonSerializerSettings
                {
                    Converters = new Collection<JsonConverter> { new StringEnumConverter() }
                };

                RenderOptions renderOptions = null!;

                var parser = new Parser(with =>
                    {
                        with.CaseInsensitiveEnumValues = true;
                        with.HelpWriter = Console.Error;
                    }).
                    ParseArguments<GeostationaryOptions, EquirectangularOptions>(args)
                    .WithParsed<EquirectangularOptions>(options => renderOptions = ParseReprojectOptions(options)) 
                    .WithParsed<GeostationaryOptions>(options => renderOptions = ParseGeostationaryOptions(options));

                // Exit if required options not present
                if (parser.Tag == ParserResultType.NotParsed) throw new ValidationException();
                Guard.Against.Null(renderOptions, nameof(renderOptions));

                // Disable stdout if required
                if (renderOptions.Quiet) Console.SetOut(TextWriter.Null);

                // Build DI container
                var container = new Container().AddAllService(renderOptions);
                container.Verify();

                ConfigureLogging(renderOptions.Verbose);

                Log.Information("Sanchez starting");
                LogOptions(renderOptions);

                // Perform image processing
                await container
                    .GetInstance<Sanchez>()
                    .ProcessAsync(cancellationToken.Token);
            }
            catch (ValidationException e)
            {
                Log.Warning(e, "No image processing possible");
                
                if (!string.IsNullOrEmpty(e.Message)) await Console.Error.WriteLineAsync(e.Message);
                else e.Result?.Errors.ForEach(error => Console.Error.WriteLine(error.ErrorMessage));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine("Unhandled failure; check logs for details.");
                Log.Error(e, "Unhandled failure");
            }
            finally
            {
                Console.ResetColor();
            }
        }

        private static RenderOptions ParseGeostationaryOptions(GeostationaryOptions options)
        {
            var validation = new GeostationaryOptionsValidator().Validate(options);
            if (validation.IsValid) return OptionsParser.Populate(options);

            throw new ValidationException(validation);
        }

        private static RenderOptions ParseReprojectOptions(EquirectangularOptions options)
        {
            var validation = new EquirectangularOptionsValidator().Validate(options);
            if (validation.IsValid) return OptionsParser.Populate(options);

            throw new ValidationException(validation);
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

        private static void LogOptions(RenderOptions options)
        {
            if (options.EquirectangularRender?.AutoCrop == true) Log.Information("Autocrop enabled");
            Log.Information("Using {type} interpolation", options.InterpolationType);
            Log.Information("Normalising images to {km} km spatial resolution", options.SpatialResolution);
            Log.Information("Using underlay path {path}", options.UnderlayPath);
            Log.Information("Using satellite definitions {path}", options.DefinitionsPath);
            Log.Information("Processing {numParallel} images in parallel", options.NumImagesParallel);

            if (options.GeostationaryRender != null)
            {
                Log.Information("Apply {haze} haze", options.GeostationaryRender.HazeAmount);
            }
        }
    }
}