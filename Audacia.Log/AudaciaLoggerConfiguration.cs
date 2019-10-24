namespace Audacia.Log
{
	/// <summary>Specifies logging configuration for an instance of an application.</summary>
	public class AudaciaLoggerConfiguration
	{
		/// <summary>The name of the application.</summary>
		/// <example>Example App, ECar, Monetize. </example>
		public string ApplicationName { get; set; }

		/// <summary>The name of the environment the application is running on.</summary>
		/// <example>Development, Quality Assurance, User Acceptance.</example>
		public string EnvironmentName { get; set; }

		/// <summary>The telemetry key for the application insights instance for this environment.</summary>
		public string ApplicationInsightsKey { get; set; }

		/// <summary>Enable adaptive sampling for application insights telemetry. By default this is turned off.</summary>
		public bool EnableSampling { get; set; }
	}
}