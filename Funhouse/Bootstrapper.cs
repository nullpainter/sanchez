using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CommandLine;
using Funhouse.Builders;
using Funhouse.Factories;
using Funhouse.Models;
using Funhouse.Services;
using Newtonsoft.Json;
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
            try
            {
                await Parser.Default.ParseArguments<CommandLineOptions>(args).WithParsedAsync(async options =>
                {
                    // Disable stdout if required
                    if (options.Quiet) Console.SetOut(TextWriter.Null);

                    var renderOptions = RenderOptionFactory.ToRenderOptions(options);
                    await ValidateOptionsAsync(renderOptions);

                    // Build DI container
                    var container = new Container().AddAllService(options, renderOptions);
                    container.Verify();

                    ConfigureLogging(options.Verbose);

                    Log.Information("Sanchez starting");

                    LogOptions(options);

                    // Peform image processing
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

        private static async Task ValidateOptionsAsync(RenderOptions renderOptions)
        {
            // Verify selected tint
            if (renderOptions.Tint == null)
            {
                await Console.Error.WriteLineAsync("Unable to parse tint as a hex tuple. Expected format is 5ebfff");
                Environment.Exit(-1);
            }
        }

        private static void LogOptions(CommandLineOptions options)
        {
            if (options.AutoCrop) Log.Information("Autocrop enabled");
            if (options.Stitch) Log.Information("Stitching enabled");
            if (options.BlurEdges) Log.Information("Edge blurring enabled");
            
            Log.Information("Interpolation type {type}", options.InterpolationType);
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