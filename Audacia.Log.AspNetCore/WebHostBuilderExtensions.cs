using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace Audacia.Log.AspNetCore
{
	/// <summary>Extension methods for configuring logging for an ASP.NET Core application against the <see cref="IWebHostBuilder"/>.</summary>
	public static class WebHostBuilderExtensions
	{
		/// <summary>Reads the log configuration from the application's appsettings.json.</summary>
		public static IWebHostBuilder ConfigureLogging(this IWebHostBuilder builder, AudaciaLoggerConfiguration configuration)
		{
			if (builder == null) throw new ArgumentNullException(nameof(builder));
			if (configuration == null) throw new ArgumentNullException(nameof(configuration));

			return builder.UseSerilog()
				.UseApplicationInsights(configuration.ApplicationInsightsKey);
		}

		/// <summary>Reads the log configuration from the application's appsettings.json.</summary>
		public static IWebHostBuilder ConfigureLogging(this IWebHostBuilder builder, string section = "Logging")
		{
			var envName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
			var configuration = new ConfigurationBuilder()
				.AddJsonFile("appsettings.json")
				.AddJsonFile($"appsettings.{envName}.json", true)
				.Build();

			var config = configuration.LogConfig(section);

			return builder.UseSerilog()
				.UseApplicationInsights(config.ApplicationInsightsKey);
		}
	}
}