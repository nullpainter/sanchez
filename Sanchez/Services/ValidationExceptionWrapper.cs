using FluentValidation;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;

namespace Sanchez.Services;

/// <summary>
///     Wraps log entries resulting from <see cref="ValidationException"/>, reporting
///     as warning rather than error. This ensure that spurious Sentry.io reports aren't
///     made for user error.
/// </summary>
internal class ValidationExceptionWrapper : ILogEventSink, IDisposable
{
    readonly ILogEventSink _wrappedSink;

    public ValidationExceptionWrapper(ILogEventSink wrappedSink) => _wrappedSink = wrappedSink;

    public void Emit(LogEvent logEvent)
    {
        // Report validation exceptions are warnings
        if (logEvent.Exception is ValidationException)
        {
            var boosted = new LogEvent(
                logEvent.Timestamp,
                LogEventLevel.Warning,
                logEvent.Exception,
                logEvent.MessageTemplate,
                logEvent.Properties
                    .Select(kvp => new LogEventProperty(kvp.Key, kvp.Value)));

            _wrappedSink.Emit(boosted);
        }
        else
        {
            _wrappedSink.Emit(logEvent);
        }
    }

    public void Dispose()
    {
        (_wrappedSink as IDisposable)?.Dispose();
    }
}

static class LoggerSinkConfigurationExtensions
{
    /// <summary>
    ///     Wraps log entries resulting from <see cref="ValidationException"/>, reporting
    ///     as warning rather than error. This ensure that spurious Sentry.io reports aren't
    ///     made for user error.
    /// </summary>
    internal static LoggerConfiguration ValidationWrapper(
        this LoggerSinkConfiguration lsc,
        Action<LoggerSinkConfiguration> writeTo,
        LogEventLevel minimumLevel,
        LoggingLevelSwitch levelSwitch)
    {
        return LoggerSinkConfiguration.Wrap(
            lsc,
            wrapped => new ValidationExceptionWrapper(wrapped),
            writeTo,
            minimumLevel,
            levelSwitch);
    }
}