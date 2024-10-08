using System;
using Serilog;

namespace Audacia.Log
{
    /// <summary>
    /// Extension methods for configuring default logging.
    /// </summary>
    public static class LoggerConfigurationExtensions
    {
        /// <summary>
        /// Creates a default logger config with enrichers and sinks.
        /// </summary>
        /// <param name="config">Logger configuration.</param>
        /// <param name="audaciaConfig">Audacia Logger configuration.</param>
        /// <exception cref="ArgumentNullException"><paramref name="config"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="audaciaConfig"/> is <see langword="null"/>.</exception>
        /// <returns>Configured Logger configuration.</returns>
        public static LoggerConfiguration ConfigureDefaults(this LoggerConfiguration config, AudaciaLoggerConfiguration audaciaConfig)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            if (audaciaConfig == null)
            {
                throw new ArgumentNullException(nameof(audaciaConfig));
            }

            return config
                .MinimumLevel.Defaults()
                .Enrich.WithDefaults(audaciaConfig)
                .WriteTo.Defaults(audaciaConfig);
        }
    }
}