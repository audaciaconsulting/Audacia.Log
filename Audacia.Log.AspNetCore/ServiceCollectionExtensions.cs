using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Audacia.Log.AspNetCore
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection ConfigureLogging(this IServiceCollection services, LogConfig config,
			ILogger logger = null)
		{
			TelemetryConfiguration.Active.InstrumentationKey = config.ApplicationInsightsKey;

			Serilog.Log.Logger = new LoggerConfiguration()
				.ConfigureDefaults(
					config.ApplicationName,
					config.EnvironmentName,
					config.IsDevelopment,
					config.ApplicationInsightsKey,
					config.SlackUrl
				)
				.CreateLogger();

			return services
				.AddSingleton(logger ?? Serilog.Log.Logger)
				.AddApplicationInsightsTelemetry("00000000-0000-0000-0000-000000000000");
		}
	}
}