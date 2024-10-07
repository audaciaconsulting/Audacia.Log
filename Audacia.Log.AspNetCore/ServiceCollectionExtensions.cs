using System;
using System.Collections.Generic;
using Audacia.Log.AspNetCore.Configuration;
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
    /// <param name="services">Service collection.</param>
    /// <param name="configuration">Configuration.</param>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="configuration"/> is <see langword="null"/>.</exception>
    /// <returns>Service collection with add claims telemetry.</returns>
    public static IServiceCollection ConfigureApplicationInsights(
        this IServiceCollection services,
        IConfiguration configuration)
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
            services.ConfigureTelemetryModule<QuickPulseTelemetryModule>(
                (
                    module,
                    _) => module.AuthenticationApiKey = quickPulseKey);
        }

        return services;
    }

    /// <summary>
    /// Configures logging of the request and responses body for all actions.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="configuration">Configuration.</param>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="configuration"/> is <see langword="null"/>.</exception>
    /// <returns>Service collection with add claims telemetry.</returns>
    public static IServiceCollection ConfigureActionContentLogging(
        this IServiceCollection services,
        IConfiguration configuration)
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
        services.Configure<LogActionFilterConfig>(configuration.GetSection(LogActionFilterConfig.Location));

        return services;
    }

    /// <summary>
    /// This will inject <see cref="LogRequestBodyActionTelemetryInitialiser" />.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is <see langword="null"/>.</exception>
    /// <returns>Service collection with action request body telemetry.</returns>
    public static IServiceCollection AddActionRequestBodyTelemetry(this IServiceCollection services)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        // Add telemetry initialiser to attach request data captured by LogRequestBodyActionFilter
        services.AddSingleton<ITelemetryInitializer, LogRequestBodyActionTelemetryInitialiser>();

        return services;
    }

    /// <summary>
    /// This will inject <see cref="LogResponseBodyActionTelemetryInitialiser" />.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is <see langword="null"/>.</exception>
    /// <returns>Service collection with add action response body telemetry.</returns>
    public static IServiceCollection AddActionResponseBodyTelemetry(this IServiceCollection services)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        // Add telemetry initialiser to attach request data captured by LogRequestBodyActionFilter
        services.AddSingleton<ITelemetryInitializer, LogRequestBodyActionTelemetryInitialiser>();

        return services;
    }

    /// <summary>
    /// Configures logging of the bodies of HTTP requests for all Dependency of type HTTP requests.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="configuration">Configuration.</param>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="configuration"/> is <see langword="null"/>.</exception>
    /// <returns>Service collection with configured dependency body content.</returns>
    public static IServiceCollection ConfigureDependencyBodyContentLogging(
        this IServiceCollection services,
        IConfiguration configuration)
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
        services.Configure<LogDependencyFilterConfig>(configuration.GetSection(LogDependencyFilterConfig.Location));

        return services;
    }

    /// <summary>
    /// This will inject <see cref="HttpDependencyBodyCaptureTelemetryInitializer" />.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is <see langword="null"/>.</exception>
    /// <returns>Service collection with add dependency body telemetry.</returns>
    public static IServiceCollection AddDependencyBodyTelemetry(this IServiceCollection services)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        // Add telemetry initialiser to attach dependency http data captured by HttpDependencyBodyCaptureTelemetryInitializer
        services.AddSingleton<ITelemetryInitializer, HttpDependencyBodyCaptureTelemetryInitializer>();

        return services;
    }

    /// <summary>
    /// This will inject <see cref="IAdditionalClaimsTelemetryProvider" /> and <see cref="LogClaimsActionTelemetryInitialiser" />.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is <see langword="null"/>.</exception>
    /// <returns>Service collection with add claims telemetry.</returns>
    public static IServiceCollection AddClaimsTelemetry(this IServiceCollection services)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        return AddClaimsTelemetry(
            services,
            new CustomAdditionalClaimsTelemetryProvider(_ => new List<ClaimsData>()));
    }

    /// <summary>
    /// This will inject <see cref="IAdditionalClaimsTelemetryProvider" />.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="additionalClaimsTelemetryProvider">Additional claims telemetry provider.</param>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is <see langword="null"/>.</exception>
    /// <returns>Service collection with add claims telemetry with additional claims telemetry.</returns>
    public static IServiceCollection AddClaimsTelemetry(
        this IServiceCollection services,
        IAdditionalClaimsTelemetryProvider additionalClaimsTelemetryProvider)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        // Add telemetry initialiser to attach request data captured by LogRequestBodyActionFilter
        services.AddSingleton<ITelemetryInitializer, LogClaimsActionTelemetryInitialiser>();

        services.AddSingleton<IAdditionalClaimsTelemetryProvider>(additionalClaimsTelemetryProvider);

        return services;
    }
}