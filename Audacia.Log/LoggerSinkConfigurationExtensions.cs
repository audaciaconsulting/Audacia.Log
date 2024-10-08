using System;
using System.Runtime.InteropServices;
using Serilog;
using Serilog.Configuration;
using Serilog.Events;

namespace Audacia.Log
{
    /// <summary>Extension methods for configuring default log sinks.</summary>
    public static class LoggerSinkConfigurationExtensions
    {
        /// <summary>
        /// Message template for trace logs.
        /// </summary>
        private const string TraceFormat = "[{UserName}] :: {Message}{NewLine:l}{Exception:l}";

        /// <summary>
        /// Configure loggers to use the default sinks.
        /// </summary>
        /// <param name="config">Logger configuration.</param>
        /// <param name="audaciaConfig">Audacia Logger configuration.</param>
        /// <exception cref="ArgumentNullException"><paramref name="audaciaConfig"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="config"/> is <see langword="null"/>.</exception>
        /// <returns>Configured Logger Configuration.</returns>
        public static LoggerConfiguration Defaults(
            this LoggerSinkConfiguration config,
            AudaciaLoggerConfiguration audaciaConfig)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            if (audaciaConfig == null)
            {
                throw new ArgumentNullException(nameof(audaciaConfig));
            }

            var loggerConfiguration = config
                .Trace(outputTemplate: TraceFormat, restrictedToMinimumLevel: LogEventLevel.Debug)
                .WriteTo.Console();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                loggerConfiguration = loggerConfiguration.WriteTo
                    .EventLog(audaciaConfig.ApplicationName, restrictedToMinimumLevel: LogEventLevel.Warning);
            }

            if (audaciaConfig.IsApplicationInsightsKeySet())
            {
                loggerConfiguration = loggerConfiguration.WriteTo
                    .ApplicationInsights(TelemetryConverter.Traces);
            }

            return loggerConfiguration;
        }
    }
}