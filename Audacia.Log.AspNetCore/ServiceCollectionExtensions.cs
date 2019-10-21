using System;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Audacia.Log.AspNetCore
{
	/// <summary>Extension methods for configuring logging services for an ASP.NET Core application.</summary>
	public static class ServiceCollectionExtensions
	{
		/// <summary>Configures logging for an ASP.NET Core application using the specified <see cref="AudaciaLoggerConfiguration"/>.</summary>
		public static IServiceCollection ConfigureLogging(this IServiceCollection services, AudaciaLoggerConfiguration configuration, ILogger logger = null)
		{
			if (services == null) throw new ArgumentNullException(nameof(services));
			if (configuration == null) throw new ArgumentNullException(nameof(configuration));

			TelemetryConfiguration.Active.InstrumentationKey = configuration.ApplicationInsightsKey;
			var options = new Microsoft.ApplicationInsights.AspNetCore.Extensions.ApplicationInsightsServiceOptions
			{
				EnableAdaptiveSampling = false,
				InstrumentationKey = configuration.ApplicationInsightsKey,
			};

			return services
				.AddSingleton(logger ?? Serilog.Log.Logger)
				.AddLogging(l => l.AddSerilog())
				.AddApplicationInsightsTelemetry(options);
		}

		/// <summary>Configures logging for an ASP.NET Core application using settings specified in appSettings.json file.</summary>
		public static IServiceCollection ConfigureLogging(this IServiceCollection services, string section = "Logging", ILogger logger = null)
		{
			var envName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

			var webConfig = new ConfigurationBuilder()
				.AddJsonFile("appsettings.json")
				.AddJsonFile($"appsettings.{envName}.json", true)
				.Build();

			var logConfig = webConfig.LogConfig(section);

			TelemetryConfiguration.Active.InstrumentationKey = logConfig.ApplicationInsightsKey;
			var options = new Microsoft.ApplicationInsights.AspNetCore.Extensions.ApplicationInsightsServiceOptions
			{
				EnableAdaptiveSampling = false,
				InstrumentationKey = logConfig.ApplicationInsightsKey,
			};

			return services
				.AddSingleton(logger ?? Serilog.Log.Logger)
				.AddLogging(l => l.AddSerilog())
				.AddApplicationInsightsTelemetry(options);
		}
	}
}