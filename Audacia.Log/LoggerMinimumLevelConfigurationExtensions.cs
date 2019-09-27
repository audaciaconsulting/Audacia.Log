using System;
using Serilog;
using Serilog.Configuration;
using Serilog.Events;

namespace Audacia.Log
{
	/// <summary>Extension methods for configuring default minimal level filters..</summary>
	public static class LoggerMinimumLevelConfigurationExtensions
	{
		/// <summary>Creates a default logger config with enrichers and sinks.</summary>
		public static LoggerConfiguration Defaults(this LoggerMinimumLevelConfiguration configuration)
		{
			if (configuration == null) throw new ArgumentNullException(nameof(configuration));

			return configuration
				.Verbose()
				.MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
				.MinimumLevel.Override("IdentityServer4", LogEventLevel.Warning)
				.MinimumLevel.Override("IdentityServer4.Validation.TokenValidator", LogEventLevel.Fatal);
		}
	}
}