using System;
using Audacia.Log.AspNetCore.Extensions;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;

namespace Audacia.Log.AspNetCore.Internal
{
    /// <summary>
    /// Attaches request content stored on the <see cref="HttpContext"/> to <see cref="RequestTelemetry"/>.
    /// </summary>
    internal sealed class LogActionTelemetryInitialiser : ITelemetryInitializer
    {
        public const string ActionArguments = "ActionArguments";
   
        public const string ActionClaims = "ActionClaims";

        public const string ActionUserId = "ActionUserId";

        public const string ActionUserRoles = "ActionUserRoles";

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
            if ((telemetry is RequestTelemetry requestTelemetry) &&
                _httpContextAccessor.HttpContext?.HasFormData() == true)
            {
                TryAddProperty(requestTelemetry, "UserId", ActionUserId);
                TryAddProperty(requestTelemetry, "UserRoles", ActionUserRoles);
                TryAddProperty(requestTelemetry, "UserClaims", ActionClaims);
                TryAddProperty(requestTelemetry, "Arguments", ActionArguments);
            }
        }

        private void TryAddProperty(RequestTelemetry telemetry, string propertyName, string httpContextItemKey)
        {
            if (_httpContextAccessor.HttpContext.Items.ContainsKey(httpContextItemKey))
            {
                var logPropertyData = _httpContextAccessor.HttpContext.Items[httpContextItemKey].ToString();
                telemetry.Properties.Add(propertyName, logPropertyData);
            }
        }
    }
}
