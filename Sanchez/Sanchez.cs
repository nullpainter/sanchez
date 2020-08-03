using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
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
            var cancellationToken = new CancellationTokenSource();
            
            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                cancellationToken.Cancel();
            };
            
            var container = new Container().AddAllService();
            container.Verify();

            try
            {
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
                    compositor.Compose(options, cancellationToken);
                });
            }
            finally
            {
                Console.ResetColor();
            }
        }

        /// <summary>
        ///     Configures logging output.
        /// </summary>
        private static void ConfigureLogging()
        {
            var applicationPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
            
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.RollingFile(Path.Combine(applicationPath, "logs", "sanchez-{Date}.log"), LogEventLevel.Information, fileSizeLimitBytes: 5 * 1024 * 1024)
                .Enrich.FromLogContext()
                .Enrich.WithExceptionDetails()
                .CreateLogger();
        }
    }
}