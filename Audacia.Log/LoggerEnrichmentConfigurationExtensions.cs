using Serilog;
using Serilog.Configuration;

namespace Audacia.Log
{
	public static class LoggerEnrichmentConfigurationExtensions
	{
		/// <summary>Configure loggers to use the default enrichers.</summary>
		public static LoggerConfiguration WithDefaults(this LoggerEnrichmentConfiguration configuration, LogConfig config)
		{
			return configuration.FromLogContext()
				.Enrich.WithAssemblyName()
				.Enrich.WithAssemblyVersion()
				.Enrich.WithMachineName()
				.Enrich.WithEnvironmentUserName()
				.Enrich.WithProperty("Environment", config.EnvironmentName)
				.Enrich.WithThreadId();
		}
	}
}


