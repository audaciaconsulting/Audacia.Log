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
		public static LoggerConfiguration Defaults(this LoggerSinkConfiguration configuration, LogConfig config)
		{
			var loggerConfiguration = configuration
				.EventLog(config.ApplicationName, restrictedToMinimumLevel: LogEventLevel.Warning)
				.WriteTo.Trace(outputTemplate: TraceFormat, restrictedToMinimumLevel: LogEventLevel.Debug);

			if (config.IsDevelopment)
				return loggerConfiguration.WriteTo
					.ApplicationInsightsTraces(Guid.Empty.ToString(), LogEventLevel.Information);

			if (config.SlackUrl != null)
				loggerConfiguration = loggerConfiguration.WriteTo.Slack(config.SlackUrl, restrictedToMinimumLevel: LogEventLevel.Error);

			if (config.ApplicationInsightsKey != null)
				loggerConfiguration = loggerConfiguration.WriteTo.ApplicationInsightsTraces(config.ApplicationInsightsKey, LogEventLevel.Information);

			return loggerConfiguration;

		}
	}
}