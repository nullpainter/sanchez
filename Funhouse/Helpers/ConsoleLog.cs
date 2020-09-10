using System;
using Serilog;

namespace Funhouse.Helpers
{
    /// <summary>
    ///     Methods which write to Serilog and the console.
    /// </summary>
    public static class ConsoleLog
    {
        public static void Information(string message)
        {
            Log.Information(message);
            Console.WriteLine(message);
        }

        public static void Warning(string message)
        {
            Log.Warning(message);
            Console.WriteLine(message);
        }

        public static void Error(string message)
        {
            Log.Error(message);
            Console.Error.WriteLine(message);
        }
    }
}