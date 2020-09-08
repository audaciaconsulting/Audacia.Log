using System;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace Audacia.Log.AspNetCore
{
    /// <summary>Extension methods for configuring default logging for an ASP.NET Core application.</summary>
    public static class LoggerConfigurationExtensions
    {
        /// <summary>Configure default logging for an ASP.NET Core application.</summary>
        public static LoggerConfiguration ConfigureDefaults(this LoggerConfiguration loggerConfig, string section = "Logging")
        {
            var envName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{envName}.json", true)
                .Build();

            var config = configuration.LogConfig(section);
            return loggerConfig.ConfigureDefaults(config);
        }
    }
}