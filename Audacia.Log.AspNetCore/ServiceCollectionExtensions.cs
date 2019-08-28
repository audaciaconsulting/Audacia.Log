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
			
			return services
				.AddSingleton(logger ?? Serilog.Log.Logger)
				.AddApplicationInsightsTelemetry(config.ApplicationInsightsKey);
		}
	}
}