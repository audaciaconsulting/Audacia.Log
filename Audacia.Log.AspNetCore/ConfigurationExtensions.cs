using System;
using Microsoft.Extensions.Configuration;

namespace Audacia.Log.AspNetCore
{
	/// <summary>Extension methods for configuring logging against an ASP.NET 4 application  via settings stored in web.config.</summary>
	public static class ConfigurationExtensions
	{
		/// <summary>Reads the log configuration from the application's appsettings.json.</summary>
		public static AudaciaLoggerConfiguration LogConfig(this IConfiguration config, string section = "Logging")
		{
			if (config == null) throw new ArgumentNullException(nameof(config));
			if (section == null) throw new ArgumentNullException(nameof(section));

			return new AudaciaLoggerConfiguration
			{
				ApplicationName = config[$"{section}:ApplicationName"],
				EnvironmentName = config[$"{section}:EnvironmentName"],
				ApplicationInsightsKey = config[$"{section}:ApplicationInsightsKey"]
			};
		}
	}
}