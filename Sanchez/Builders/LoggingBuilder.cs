using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Sanchez.Processing.Helpers;
using Sanchez.Services;
using Sentry;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Exceptions;

namespace Sanchez.Builders
{
    internal static class LoggingBuilder
    {
        /// <summary>
        ///     Configures logging output.
        /// </summary>
        public static IServiceCollection ConfigureLogging(this IServiceCollection services, bool consoleLogging)
        {
            var builder = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("WorkflowCore", LogEventLevel.Information)
                .WriteTo.RollingFile(Path.Combine(PathHelper.LogPath(), "sanchez-{Date}.log"), LogEventLevel.Information, fileSizeLimitBytes: 5 * 1024 * 1024)
                .WriteTo.ValidationWrapper(c => c.Sentry(o =>
                {
                    o.MinimumEventLevel = LogEventLevel.Error;
                    o.Dsn = new Dsn("https://2d7d5615b9f249f7890e275774e9eaf6@o456714.ingest.sentry.io/5450049");
                }), LogEventLevel.Debug, new LoggingLevelSwitch())
                .Enrich.FromLogContext()
                .Enrich.WithExceptionDetails();

            if (consoleLogging) builder.WriteTo.Console();

            Log.Logger = builder.CreateLogger();

            services.AddLogging(serviceBuilder => serviceBuilder.AddSerilog(dispose: true));

            return services;
        }
    }
}