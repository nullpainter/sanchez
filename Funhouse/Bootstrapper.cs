using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using CommandLine;
using Extend;
using FluentValidation.Results;
using Funhouse.Builders;
using Funhouse.Helpers;
using Funhouse.Models;
using Funhouse.Models.CommandLine;
using Funhouse.Validators;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using SimpleInjector;

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
                RenderOptions renderOptions = null!;

                var parser = Parser.Default
                    .ParseArguments<GeostationaryOptions, EquirectangularOptions>(args)
                    .WithParsed<EquirectangularOptions>(options => renderOptions = ParseReprojectOptions(options))
                    .WithParsed<GeostationaryOptions>(options => renderOptions = ParseGeostationaryOptions(options));

                // Exit if required options not present
                if (parser.Tag == ParserResultType.NotParsed) Environment.Exit(-1);
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
                    .GetInstance<Funhouse>()
                    .ProcessAsync();
            }
            finally
            {
                Console.ResetColor();
            }
        }

        private static RenderOptions ParseGeostationaryOptions(GeostationaryOptions options)
        {
            var validation = new GeostationaryOptionsValidator().Validate(options);
            if (validation.IsValid)
            {
                return OptionsParser.Populate(options);
            }

            ReportErrors(validation);
            Environment.Exit(-1);
            return null;
        }

        private static RenderOptions ParseReprojectOptions(EquirectangularOptions options)
        {
            var validation = new EquirectangularOptionsValidator().Validate(options);
            if (validation.IsValid)
            {
                return OptionsParser.Populate(options);
            }

            ReportErrors(validation);
            Environment.Exit(-1);
            return null;
        }

        private static void ReportErrors(ValidationResult result) => result.Errors.ForEach(error => Console.Error.WriteLine(error.ErrorMessage));

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
            Log.Information("Interpolation type {type}", options.InterpolationType);
            Log.Information("Normalising to {km}km spatial resolution", options.SpatialResolution);
        }
    }
}