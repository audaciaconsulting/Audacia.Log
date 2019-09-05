using System.Configuration;
using Serilog;

namespace Audacia.Log.AspNet4
{
	/// <summary>Extension methods for configuring default logging for a Legacy Audacia ASP.NET Core application.</summary>
	public static class LoggerConfigurationExtensions
	{
		/// <summary>Configure the default log sinks against the specified <see cref="LoggerConfiguration"/></summary>
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