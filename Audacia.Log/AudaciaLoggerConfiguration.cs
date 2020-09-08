using System;

namespace Audacia.Log
{
	/// <summary>Specifies logging configuration for an instance of an application.</summary>
	public class AudaciaLoggerConfiguration
	{
		/// <summary>Gets or sets the name of the application.</summary>
		/// <example>Example App, ECar, Monetize. </example>
		public string ApplicationName { get; set; }

		/// <summary>Gets or sets the name of the environment the application is running on.</summary>
		/// <example>Development, Quality Assurance, User Acceptance.</example>
		public string EnvironmentName { get; set; }

		/// <summary>Gets or sets the telemetry key for the application insights instance for this environment.</summary>
		public string ApplicationInsightsKey { get; set; }

		/// <summary>Gets or sets a value indicating whether to enable adaptive sampling for application insights telemetry. By default this is turned off.</summary>
		public bool EnableSampling { get; set; }

		/// <summary>
		/// Checks whether the application insights key is set for the current object.
		/// </summary>
		public bool IsApplicationInsightsKeySet()
        {
			return Guid.TryParse(ApplicationInsightsKey, out var parsedKey) &&
				   parsedKey != Guid.Empty;
        }
	}
}