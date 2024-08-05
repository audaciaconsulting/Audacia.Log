using System;
using Audacia.Log.AspNetCore.Extensions;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;

namespace Audacia.Log.AspNetCore;

/// <summary>
/// Attaches request content stored on the <see cref="HttpContext"/> to <see cref="RequestTelemetry"/>.
/// </summary>
public sealed class LogRequestBodyActionTelemetryInitialiser : ITelemetryInitializer
{
    internal const string ActionArguments = "ActionArguments";

    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Creates an instance of RequestDataTelemetryInitialiser.
    /// </summary>
    /// <param name="httpContextAccessor">Http context accessor</param>
    public LogRequestBodyActionTelemetryInitialiser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor ??
            throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    /// <inheritdoc/>
    public void Initialize(ITelemetry telemetry)
    {
        // Only add logs to RequestTelemetry
        if (telemetry is not RequestTelemetry requestTelemetry)
        {
            return;
        }

        if (_httpContextAccessor.HttpContext?.HasFormData() == true && _httpContextAccessor.HttpContext.Items.ContainsKey(ActionArguments))
        {
            var logPropertyData = _httpContextAccessor.HttpContext.Items[ActionArguments].ToString();
            requestTelemetry.Properties.Add("Arguments", logPropertyData);
        }
    }
}