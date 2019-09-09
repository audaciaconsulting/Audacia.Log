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
				ApplicationInsightsKey = ConfigurationManager.AppSettings[$"{prefix}:ApplicationInsightsKey"]
			};
			
			return config.ConfigureDefaults(audaciaConfig)
				.Enrich.WithRequest()
				.Enrich.WithResponse();
		}
	}
}