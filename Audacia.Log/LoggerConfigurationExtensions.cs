using Microsoft.ApplicationInsights.Extensibility;
using Serilog;

namespace Audacia.Log
{
	public static class LoggerConfigurationExtensions
	{
		/// <summary>Creates a default logger config with enrichers and sinks.</summary>
		public static LoggerConfiguration ConfigureDefaults(this LoggerConfiguration configuration, LogConfig config)
		{
			TelemetryConfiguration.Active.InstrumentationKey = config.ApplicationInsightsKey;
			
			return configuration
				.MinimumLevel.Defaults()
				.Enrich.WithDefaults(config)
				.WriteTo.Defaults(config); 
		}
	}
}