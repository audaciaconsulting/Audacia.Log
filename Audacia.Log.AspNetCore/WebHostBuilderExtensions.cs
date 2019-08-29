using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace Audacia.Log.AspNetCore
{
	public static class WebHostBuilderExtensions
	{
		/// <summary>Reads the log configuration from the application's appsettings.json</summary>
		public static IWebHostBuilder ConfigureLogging(this IWebHostBuilder builder, LogConfig config)
		{
			return builder.UseSerilog()
				.UseApplicationInsights(config.ApplicationInsightsKey);
		}
		/// <summary>Reads the log configuration from the application's appsettings.json</summary>
		public static IWebHostBuilder ConfigureLogging(this IWebHostBuilder builder, string section = "LogConfig")
		{
			var configuration = new ConfigurationBuilder()
				.AddJsonFile("appsettings.json")
				.Build();
			
			var config = configuration.LogConfig(section);
			
			return builder.UseSerilog()
				.UseApplicationInsights(config.ApplicationInsightsKey);
		}
	}
}