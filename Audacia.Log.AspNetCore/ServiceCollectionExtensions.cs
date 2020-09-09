using System;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Audacia.Log.AspNetCore
{
    /// <summary>Extension methods for configuring logging services for an ASP.NET Core application.</summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>Configures logging for an ASP.NET Core application using the specified <see cref="AudaciaLoggerConfiguration"/>.</summary>
        /// <exception cref="ArgumentNullException"><paramref name="services"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="configuration"/> is <see langword="null"/>.</exception>
        public static IServiceCollection ConfigureLogging(this IServiceCollection services, AudaciaLoggerConfiguration configuration, ILogger logger = null)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            services = services
                .AddSingleton(logger ?? Serilog.Log.Logger)
                .AddLogging(l => l.AddSerilog());

            if (string.IsNullOrWhiteSpace(configuration.ApplicationInsightsKey))
            {
                configuration.ApplicationInsightsKey = Guid.Empty.ToString();
            }

            var options = new ApplicationInsightsServiceOptions
            {
                EnableAdaptiveSampling = configuration.EnableSampling,
                InstrumentationKey = configuration.ApplicationInsightsKey
            };

            return services.AddApplicationInsightsTelemetry(options);
        }

        /// <summary>Configures logging for an ASP.NET Core application using settings specified in appSettings.json file.</summary>
#pragma warning disable CA1801 // Review unused parameters - publicly shipped API
        public static IServiceCollection ConfigureLogging(this IServiceCollection services, string section = "Logging", ILogger logger = null)
#pragma warning restore CA1801 // Review unused parameters
        {
            var envName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            var webConfig = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{envName}.json", true)
                .Build();

            var logConfig = webConfig.LogConfig(section);

            return services.ConfigureLogging(logConfig);
        }
    }
}