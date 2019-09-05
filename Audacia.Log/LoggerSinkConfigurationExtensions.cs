using System;
using Serilog;
using Serilog.Configuration;
using Serilog.Events;
using Serilog.Sinks.Slack;

namespace Audacia.Log
{
	/// <summary>Extension methods for configuring default log sinks.</summary>
	public static class LoggerSinkConfigurationExtensions
	{
		internal const string TraceFormat = "[{UserName}] :: {Message}{NewLine:l}{Exception:l}";

		/// <summary>Configure loggers to use the default sinks.</summary>
		public static LoggerConfiguration Defaults(this LoggerSinkConfiguration config, AudaciaLoggerConfiguration audaciaConfig)
		{
			var loggerConfiguration = config
				.EventLog(audaciaConfig.ApplicationName, restrictedToMinimumLevel: LogEventLevel.Warning)
				.WriteTo.Trace(outputTemplate: TraceFormat, restrictedToMinimumLevel: LogEventLevel.Debug);

			if (audaciaConfig.IsDevelopment)
				return loggerConfiguration.WriteTo
					.ApplicationInsightsTraces(Guid.Empty.ToString(), LogEventLevel.Information);

			if (audaciaConfig.SlackUrl != null)
				loggerConfiguration = loggerConfiguration.WriteTo.Slack(audaciaConfig.SlackUrl, restrictedToMinimumLevel: LogEventLevel.Error);

			if (audaciaConfig.ApplicationInsightsKey != null)
				loggerConfiguration = loggerConfiguration.WriteTo.ApplicationInsightsTraces(audaciaConfig.ApplicationInsightsKey, LogEventLevel.Information);

			return loggerConfiguration;

		}
	}
}