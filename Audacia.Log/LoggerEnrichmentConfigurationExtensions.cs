using System;
using Serilog;
using Serilog.Configuration;

namespace Audacia.Log
{
    /// <summary>
    /// Extension methods for configuring default log enrichers.
    /// </summary>
    public static class LoggerEnrichmentConfigurationExtensions
    {
        /// <summary>
        /// Configure loggers to use the default enrichers.
        /// </summary>
        /// <param name="config">Logger configuration.</param>
        /// <param name="audaciaConfig">Audacia Logger configuration.</param>
        /// <exception cref="ArgumentNullException"><paramref name="config"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="audaciaConfig"/> is <see langword="null"/>.</exception>
        /// <returns>Configured Logger Configuration.</returns>
        public static LoggerConfiguration WithDefaults(this LoggerEnrichmentConfiguration config, AudaciaLoggerConfiguration audaciaConfig)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            if (audaciaConfig == null)
            {
                throw new ArgumentNullException(nameof(audaciaConfig));
            }

            return config.FromLogContext()
                .Enrich.WithAssemblyName()
                .Enrich.WithAssemblyVersion()
                .Enrich.WithMachineName()
                .Enrich.WithEnvironmentUserName()
                .Enrich.WithProperty("Environment", audaciaConfig.EnvironmentName)
                .Enrich.WithThreadId();
        }
    }
}