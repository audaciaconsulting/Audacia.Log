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
			string environmentName,
			string appInsightsKey,
			string slackUrl)
		{
			var entryAssembly = Assembly.GetEntryAssembly()?.GetName().Name;
			var callingAssembly = Assembly.GetCallingAssembly().GetName().Name;

			var loggerConfiguration = configuration
				.EventLog(entryAssembly ?? callingAssembly, restrictedToMinimumLevel: LogEventLevel.Warning)
				.WriteTo.Trace(outputTemplate: TraceFormat, restrictedToMinimumLevel: LogEventLevel.Debug);

			if (!Debugger.IsAttached && !environmentName.Equals("local", StringComparison.OrdinalIgnoreCase))
				return loggerConfiguration
					.WriteTo.Slack(slackUrl, restrictedToMinimumLevel: LogEventLevel.Error)
					.WriteTo.ApplicationInsightsTraces(appInsightsKey, LogEventLevel.Information);

			return loggerConfiguration;
		}
	}
}