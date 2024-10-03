using System;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;

namespace Audacia.Log.AspNetCore;

/// <summary>
/// Attaches response content stored on the <see cref="HttpContext"/> to <see cref="RequestTelemetry"/>.
/// </summary>
public sealed class LogResponseBodyActionTelemetryInitialiser : ITelemetryInitializer
{
    /// <summary>
    /// Custom property name of action response body.
    /// </summary>
    internal const string ActionResponseBody = "ActionResponseBody";

    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Creates an instance of LogResponseBodyActionTelemetryInitialiser.
    /// </summary>
    /// <param name="httpContextAccessor">Http context accessor.</param>
    public LogResponseBodyActionTelemetryInitialiser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor ??
                               throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    /// <inheritdoc/>
    public void Initialize(ITelemetry telemetry)
    {
        // Only add logs to RequestTelemetry
        if (telemetry is RequestTelemetry requestTelemetry &&
            _httpContextAccessor.HttpContext.Items.TryGetValue(ActionResponseBody, value: out var response))
        {
            var logPropertyData = response?.ToString();
            requestTelemetry.Properties.Add("ResponseBody", logPropertyData);
        }
    }
}