using System;
using Microsoft.ApplicationInsights.Extensibility;
using Serilog;

namespace Audacia.Log
{
	/// <summary>Extension methods for configuring default logging.</summary>
	public static class LoggerConfigurationExtensions
	{
		/// <summary>Creates a default logger config with enrichers and sinks.</summary>
		public static LoggerConfiguration ConfigureDefaults(this LoggerConfiguration config, AudaciaLoggerConfiguration audaciaConfig)
		{
			if (config == null) throw new ArgumentNullException(nameof(config));
			if (audaciaConfig == null) throw new ArgumentNullException(nameof(audaciaConfig));

			TelemetryConfiguration.Active.InstrumentationKey = audaciaConfig.ApplicationInsightsKey;

			return config
				.MinimumLevel.Defaults()
				.Enrich.WithDefaults(audaciaConfig)
				.WriteTo.Defaults(audaciaConfig);
		}
	}
}