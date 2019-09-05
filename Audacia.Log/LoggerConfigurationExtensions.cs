using Microsoft.ApplicationInsights.Extensibility;
using Serilog;

namespace Audacia.Log
{
	public static class LoggerConfigurationExtensions
	{
		/// <summary>Creates a default logger config with enrichers and sinks.</summary>
		public static LoggerConfiguration ConfigureDefaults(this LoggerConfiguration config, AudaciaLoggerConfiguration audaciaConfig)
		{
			TelemetryConfiguration.Active.InstrumentationKey = audaciaConfig.ApplicationInsightsKey;
			
			return config
				.MinimumLevel.Defaults()
				.Enrich.WithDefaults(audaciaConfig)
				.WriteTo.Defaults(audaciaConfig); 
		}
	}
}