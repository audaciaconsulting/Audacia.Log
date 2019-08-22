using System;
using System.Diagnostics;
using System.Reflection;
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
			string dataDogKey,
			string slackUrl)
		{
			var loggerConfiguration = configuration
				.EventLog(applicationName, restrictedToMinimumLevel: LogEventLevel.Warning)
				.WriteTo.Trace(outputTemplate: TraceFormat, restrictedToMinimumLevel: LogEventLevel.Debug);

			if (!development)
				return loggerConfiguration
					.WriteTo.Slack(slackUrl, restrictedToMinimumLevel: LogEventLevel.Error)
					.WriteTo.DatadogLogs(dataDogKey, host: Environment.MachineName, service: applicationName, logLevel: LogEventLevel.Debug)
					.WriteTo.ApplicationInsightsTraces(appInsightsKey, LogEventLevel.Information);

			return loggerConfiguration
				.WriteTo.ApplicationInsightsTraces(Guid.Empty.ToString(), LogEventLevel.Information);
		}
	}
}