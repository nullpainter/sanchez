using System;
using System.IO;
using System.Runtime.CompilerServices;
using CommandLine;
using Sanchez.Builders;
using Sanchez.Models;
using Sanchez.Services;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using SimpleInjector;

[assembly: InternalsVisibleTo("Sanchez.Test")]
namespace Sanchez
{
    internal static class Sanchez
    {
        /// <summary>
        ///     Main entry point to application, parsing command-line arguments and creating composite image.
        /// </summary>
        internal static void Main(params string[] args)
        {
            var container = new Container().AddAllService();
            container.Verify();

            Parser.Default.ParseArguments<CommandLineOptions>(args).WithParsed(options =>
            {
                ConfigureLogging();

                // Disable stdout if required
                if (options.Quiet) Console.SetOut(TextWriter.Null);

                // Perform additional validation on input options
                var validator = container.GetInstance<IOptionValidator>();
                if (!validator.Validate(options)) return;

                // Composite images
                var compositor = container.GetInstance<ICompositor>();
                compositor.Create(options);
            });
        }

        /// <summary>
        ///     Configures logging output.
        /// </summary>
        private static void ConfigureLogging()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.RollingFile(Path.Combine("logs", "sanchez-{Date}.log"), LogEventLevel.Information, fileSizeLimitBytes: 5 * 1024 * 1024)
                .Enrich.FromLogContext()
                .Enrich.WithExceptionDetails()
                .CreateLogger();
        }
    }
}