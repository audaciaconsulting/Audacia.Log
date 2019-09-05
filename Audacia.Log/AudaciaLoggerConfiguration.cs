namespace Audacia.Log
{
	public class AudaciaLoggerConfiguration
	{
		public string ApplicationName { get; set; }

		public string EnvironmentName { get; set; }

		public bool IsDevelopment { get; set; }

		public string ApplicationInsightsKey { get; set; }

		public string SlackUrl { get; set; }
	}
}