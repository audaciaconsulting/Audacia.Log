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
		/// <exception cref="ArgumentNullException"><paramref name="builder"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="configuration"/> is <see langword="null"/>.</exception>
		public static IWebHostBuilder ConfigureLogging(this IWebHostBuilder builder, AudaciaLoggerConfiguration configuration)
		{
			if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

			if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

			return builder.UseSerilog();
		}

        /// <summary>Reads the log configuration from the application's appsettings.json.</summary>
#pragma warning disable CA1801 // Review unused parameters - publicly shipped API
		public static IWebHostBuilder ConfigureLogging(this IWebHostBuilder builder, string section = "Logging")
#pragma warning restore CA1801 // Review unused parameters
        {
			return builder.UseSerilog();
		}
	}
}