using System;
using Sanchez.Models;
using Serilog;
using Serilog.Events;

namespace Sanchez.Helpers
{
    // TODO remove this and verify that exceptions and info are logged sensibly at end (so, summary)
    public interface IConsoleLogger
    {
        /// <summary>
        ///     Write a log entry at <see cref="LogEventLevel.Warning"/> level and to stdout.
        /// </summary>
        void Warning(string message);

        /// <summary>
        ///     Write a log entry at <see cref="LogEventLevel"/> level and to stderr1G.
        /// </summary>
        void Error(string message);
    }

    /// <summary>
    ///     Methods which write to Serilog and the console.
    /// </summary>
    public class ConsoleLogger : IConsoleLogger
    {
        private readonly RenderOptions _options;

        public ConsoleLogger(RenderOptions options) => _options = options;

        /// <summary>
        ///     Write a log entry at <see cref="LogEventLevel.Warning"/> level and to stdout.
        /// </summary>
        public void Warning(string message)
        {
            Log.Warning(message);
            if (!_options.Verbose) Console.WriteLine(message);
        }

        /// <summary>
        ///     Write a log entry at <see cref="LogEventLevel"/> level and to stderr1G.
        /// </summary>
        public void Error(string message)
        {
            Log.Error(message);
            if (!_options.Verbose) Console.Error.WriteLine(message);
        }
    }
}