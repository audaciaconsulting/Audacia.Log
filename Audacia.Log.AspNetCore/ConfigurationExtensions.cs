using Microsoft.Extensions.Configuration;

namespace Audacia.Log.AspNetCore
{
	/// <summary>Extension methods for configuring logging against an ASP.NET 4 application  via settings stored in web.config.</summary>
	public static class ConfigurationExtensions
	{
		/// <summary>Reads the log configuration from the application's appsettings.json</summary>
		public static AudaciaLoggerConfiguration LogConfig(this IConfiguration config, string section = "Logging")
		{
			return new AudaciaLoggerConfiguration
			{
				ApplicationName = config[$"{section}:ApplicationName"],
				EnvironmentName = config[$"{section}:EnvironmentName"],
				IsDevelopment = bool.Parse(config[$"{section}:IsDevelopment"]),
				ApplicationInsightsKey = config[$"{section}:ApplicationInsightsKey"]
			};
		}
	}
}