using System;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;

namespace Audacia.Log.AspNetCore
{
    /// <summary>
    /// An application insights telemetery initialiser that attaches request telemetry to the http context,
    /// to be used later by <see cref="LogActionFilterAttribute"/>.
    /// </summary>
    public sealed class LogActionTelemetryInitialiser : ITelemetryInitializer
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// Creates an instance of RequestDataTelemetryInitialiser.
        /// </summary>
        /// <param name="httpContextAccessor">Http context accessor</param>
        public LogActionTelemetryInitialiser(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor ??
                throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        /// <inheritdoc/>
        public void Initialize(ITelemetry telemetry)
        {
            // Add RequestTelemetry to the HttpContext so that the request body can be appended
            if (telemetry is RequestTelemetry && _httpContextAccessor.HttpContext != null)
            {
                _httpContextAccessor.HttpContext.Items.Add("Telemetry", telemetry);
            }
        }
    }
}
