using System;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Extensibility.PerfCounterCollector.QuickPulse;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Audacia.Log.AspNetCore;

/// <summary>Extension methods for configuring logging services for an ASP.NET Core application.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Configures default application insights configuration and logging of request data.
    /// </summary>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="configuration"/> is <see langword="null"/>.</exception>
    public static IServiceCollection ConfigureApplicationInsights(this IServiceCollection services, IConfiguration configuration)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        if (configuration == null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        // Configure Application Insights, configuration will be pulled from the appsettings file
        // see: https://docs.microsoft.com/en-us/azure/azure-monitor/app/asp-net-core
        services.AddApplicationInsightsTelemetry();

        // Live metrics stream is enabled by default, configure a secure control channel
        // see: https://docs.microsoft.com/en-us/azure/azure-monitor/app/live-stream#secure-the-control-channel
        var quickPulseKey = configuration.GetValue<string>("APPINSIGHTS_QUICKPULSEAUTHAPIKEY") ??
                            configuration.GetValue<string>("ApplicationInsights:QuickPulseApiKey");

        if (!string.IsNullOrWhiteSpace(quickPulseKey))
        {
            services.ConfigureTelemetryModule<QuickPulseTelemetryModule>((module, _) => module.AuthenticationApiKey = quickPulseKey);
        }

        return services;
    }

    /// <summary>
    /// Configures logging of the request body for all actions.
    /// </summary>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="configuration"/> is <see langword="null"/>.</exception>
    public static IServiceCollection ConfigureActionContentLogging(this IServiceCollection services, IConfiguration configuration)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        if (configuration == null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        // Required for attaching the request telemetry to the HttpContext
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

        // Configure global configuration for the LogActionFilter
        services.Configure<LogActionFilterConfig>(configuration.GetSection("LogActionFilter"));

        // Add telemetry initialiser to attach request data captured by LogActionFilter
        services.AddSingleton<ITelemetryInitializer, LogActionTelemetryInitialiser>();

        return services;
    }
}