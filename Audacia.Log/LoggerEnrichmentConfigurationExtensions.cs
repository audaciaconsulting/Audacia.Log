using Serilog;
using Serilog.Configuration;

namespace Audacia.Log
{
	public static class LoggerEnrichmentConfigurationExtensions
	{
		/// <summary>Configure loggers to use the default enrichers.</summary>
		public static LoggerConfiguration WithDefaults(this LoggerEnrichmentConfiguration config, AudaciaLoggerConfiguration audaciaConfig)
		{
			return config.FromLogContext()
				.Enrich.WithAssemblyName()
				.Enrich.WithAssemblyVersion()
				.Enrich.WithMachineName()
				.Enrich.WithEnvironmentUserName()
				.Enrich.WithProperty("Environment", audaciaConfig.EnvironmentName)
				.Enrich.WithThreadId();
		}
	}
}


