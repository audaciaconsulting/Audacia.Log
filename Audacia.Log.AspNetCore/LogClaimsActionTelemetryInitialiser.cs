using System;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;

namespace Audacia.Log.AspNetCore;

/// <summary>
/// Attaches request content stored on the <see cref="HttpContext"/> to <see cref="RequestTelemetry"/>.
/// </summary>
public sealed class LogClaimsActionTelemetryInitialiser : ITelemetryInitializer
{
    internal const string ActionClaims = "ActionClaims";

    internal const string ActionUserId = "ActionUserId";

    internal const string ActionUserRoles = "ActionUserRoles";

    private readonly IHttpContextAccessor _httpContextAccessor;

    private readonly IAdditionalClaimsTelemetryProvider _additionalClaimsTelemetryProvider;

    /// <summary>
    /// Creates an instance of RequestDataTelemetryInitialiser.
    /// </summary>
    /// <param name="httpContextAccessor">Http context accessor</param>
    /// <param name="additionalClaimsTelemetryProvider">Additional claims provider</param>
    public LogClaimsActionTelemetryInitialiser(
        IHttpContextAccessor httpContextAccessor, 
        IAdditionalClaimsTelemetryProvider additionalClaimsTelemetryProvider)
    {
        _httpContextAccessor = httpContextAccessor ??
            throw new ArgumentNullException(nameof(httpContextAccessor));
        _additionalClaimsTelemetryProvider = additionalClaimsTelemetryProvider;
    }

    /// <inheritdoc/>
    public void Initialize(ITelemetry telemetry)
    {
        // Only add logs to RequestTelemetry
        if (telemetry is not RequestTelemetry requestTelemetry)
        {
            return;
        }

        TryAddProperty(requestTelemetry, "UserId", ActionUserId);
        TryAddProperty(requestTelemetry, "UserRoles", ActionUserRoles);
        TryAddProperty(requestTelemetry, "UserClaims", ActionClaims);

        if (_additionalClaimsTelemetryProvider != null) 
        {
            foreach (var (name, data) in _additionalClaimsTelemetryProvider.GetClaims(_httpContextAccessor)) 
            {
                requestTelemetry.Properties.Add(name, data);
            }
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
