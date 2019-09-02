using System;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace Audacia.Log.AspNetCore
{
	public static class LoggerConfigurationExtensions
	{
		public static LoggerConfiguration ConfigureDefaults(this LoggerConfiguration loggerConfig, string section = "Logging")
		{
			var envName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
		
			var configuration = new ConfigurationBuilder()
				.AddJsonFile("appsettings.json")
				.AddJsonFile($"appsettings.{envName}.json", false)
				.Build();

			var config = configuration.LogConfig(section);
			return loggerConfig.ConfigureDefaults(config);
		}
	}
}	