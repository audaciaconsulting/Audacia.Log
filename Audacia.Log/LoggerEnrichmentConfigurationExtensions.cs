using Serilog;
using Serilog.Configuration;

namespace Audacia.Log
{
	public static class LoggerEnrichmentConfigurationExtensions
	{
		/// <summary>Configure loggers to use the default enrichers.</summary>
		public static LoggerConfiguration WithDefaults(
			this LoggerEnrichmentConfiguration configuration,
			string environmentName)
		{
			return configuration.FromLogContext()
				.Enrich.WithAssemblyName()
				.Enrich.WithAssemblyVersion()
				.Enrich.WithMachineName()
				.Enrich.WithEnvironmentUserName()
				.Enrich.WithProperty("Environment", environmentName)
				.Enrich.WithThreadId();
		}
	}
}