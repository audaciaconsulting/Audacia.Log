using Microsoft.Extensions.Configuration;

namespace Audacia.Log.AspNetCore
{
	public static class ConfigurationExtensions
	{
		/// <summary>Reads the log configuration from the application's appsettings.json</summary>
		public static LogConfig LogConfig(this IConfiguration config, string section = "Logging")
		{
			return new LogConfig
			{
				ApplicationName = config[$"{section}:ApplicationName"],
				EnvironmentName = config[$"{section}:EnvironmentName"],
				IsDevelopment = bool.Parse(config[$"{section}:IsDevelopment"]),
				SlackUrl = config[$"{section}:SlackUrl"],
				ApplicationInsightsKey = config[$"{section}:ApplicationInsightsKey"]
			};
		}
	}
}