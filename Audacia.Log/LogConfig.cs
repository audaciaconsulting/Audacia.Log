namespace Audacia.Log
{
	public class LogConfig
	{
		public string ApplicationName { get; set; }

		public string EnvironmentName { get; set; }

		public bool IsDevelopment { get; set; }

		public string ApplicationInsightsKey { get; set; }

		public string SlackUrl { get; set; }
	}
}