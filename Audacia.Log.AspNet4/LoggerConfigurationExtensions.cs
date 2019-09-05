using System.Configuration;
using Serilog;

namespace Audacia.Log.AspNet4
{
	public static class LoggerConfigurationExtensions
	{
		public static LoggerConfiguration ConfigureDefaults(this LoggerConfiguration config, string prefix = "logging")
		{
			var audaciaConfig = new AudaciaLoggerConfiguration
			{
				ApplicationName = ConfigurationManager.AppSettings[$"{prefix}:EnvironmentName"],
				EnvironmentName = ConfigurationManager.AppSettings[$"{prefix}:EnvironmentName"],
				IsDevelopment = Bool(ConfigurationManager.AppSettings[$"{prefix}:IsDevelopment"]),
				SlackUrl = ConfigurationManager.AppSettings[$"{prefix}:SlackUrl"],
				ApplicationInsightsKey = ConfigurationManager.AppSettings[$"{prefix}:ApplicationInsightsKey"]
			};
			
			return config.ConfigureDefaults(audaciaConfig)
				.Enrich.WithRequest()
				.Enrich.WithResponse();
		}

		private static bool Bool(string value) => bool.TryParse(value, out var result) && result;
	}
}