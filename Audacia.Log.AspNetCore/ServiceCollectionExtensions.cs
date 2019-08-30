using System;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Audacia.Log.AspNetCore
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection ConfigureLogging(this IServiceCollection services, LogConfig config, ILogger logger = null)
		{
			TelemetryConfiguration.Active.InstrumentationKey = config.ApplicationInsightsKey;
			
			return services
				.AddSingleton(logger ?? Serilog.Log.Logger)
				.AddLogging(l => l.AddSerilog())
				.AddApplicationInsightsTelemetry(config.ApplicationInsightsKey);
		}
		
		public static IServiceCollection ConfigureLogging(this IServiceCollection services, string section = "Logging", ILogger logger = null)
		{
			var envName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
			
			var configuration = new ConfigurationBuilder()
				.AddJsonFile("appsettings.json")
				.AddJsonFile($"appsettings.{envName}.json")
				.Build();
			
			var config = configuration.LogConfig(section);
			
			TelemetryConfiguration.Active.InstrumentationKey = config.ApplicationInsightsKey;
			
			return services
				.AddSingleton(logger ?? Serilog.Log.Logger)
				.AddLogging(l => l.AddSerilog())
				.AddApplicationInsightsTelemetry(config.ApplicationInsightsKey);
		}
	}
}