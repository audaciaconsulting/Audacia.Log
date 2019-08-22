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
			string dataDogKey,
			string slackUrl)
		{
			return configuration
				.MinimumLevel.Verbose()
				.Enrich.WithDefaults(environmentName)
				.WriteTo.Defaults(applicationName, isDevelopment, appInsightsKey, dataDogKey, slackUrl); 
		}
	}
}