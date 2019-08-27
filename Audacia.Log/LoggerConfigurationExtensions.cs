using Microsoft.ApplicationInsights.Extensibility;
using Serilog;

namespace Audacia.Log
{
	public static class LoggerConfigurationExtensions
	{
		/// <summary>Creates a default logger config with enrichers and sinks.</summary>
		public static LoggerConfiguration ConfigureDefaults(
			this LoggerConfiguration configuration,
			string applicationName,
			string environmentName,
			bool isDevelopment,
			string appInsightsKey,
			string slackUrl)
		{
			TelemetryConfiguration.Active.InstrumentationKey = appInsightsKey;
			
			return configuration
				.MinimumLevel.Defaults()
				.Enrich.WithDefaults(environmentName)
				.WriteTo.Defaults(applicationName, isDevelopment, appInsightsKey, slackUrl); 
		}
	}
}