using System;
using Serilog;
using Serilog.Configuration;
using Serilog.Events;
using Serilog.Sinks.Slack;

namespace Audacia.Log
{
	public static class LoggerSinkConfigurationExtensions
	{
		internal const string TraceFormat = "[{UserName}] :: {Message}{NewLine:l}{Exception:l}";

		/// <summary>Configure loggers to use the default sinks.</summary>
		public static LoggerConfiguration Defaults(
			this LoggerSinkConfiguration configuration,
			string applicationName,
			bool development,
			string appInsightsKey,
			string slackUrl)
		{
			var loggerConfiguration = configuration
				.EventLog(applicationName, restrictedToMinimumLevel: LogEventLevel.Warning)
				.WriteTo.Trace(outputTemplate: TraceFormat, restrictedToMinimumLevel: LogEventLevel.Debug);

			if (development)
				return loggerConfiguration.WriteTo
					.ApplicationInsightsTraces(Guid.Empty.ToString(), LogEventLevel.Information);

			if (slackUrl != null)
				loggerConfiguration = loggerConfiguration.WriteTo.Slack(slackUrl, restrictedToMinimumLevel: LogEventLevel.Error);

			if (appInsightsKey != null)
				loggerConfiguration = loggerConfiguration.WriteTo.ApplicationInsightsTraces(appInsightsKey, LogEventLevel.Information);

			return loggerConfiguration;

		}
	}
}