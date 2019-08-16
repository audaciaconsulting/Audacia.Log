using Serilog;

namespace Audacia.Log
{
	public static class LoggerConfigurationExtensions
	{
		/// <summary>Creates a default logger config with enrichers and sinks.</summary>
		public static LoggerConfiguration ConfigureDefaults(
			this LoggerConfiguration configuration,
			bool isDevelopment,
			string environmentName,
			string appInsightsKey,
			string slackUrl)
		{
			return configuration
				.MinimumLevel.Verbose()
				.Enrich.WithDefaults(environmentName)
				.WriteTo.Defaults(isDevelopment, appInsightsKey, slackUrl);
		}
	}
}