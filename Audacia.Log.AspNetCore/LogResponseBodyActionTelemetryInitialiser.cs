using System;
using System.IO;
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
    internal const string ActionResponseBody = "ActionResponseBody";

    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Creates an instance of RequestDataTelemetryInitialiser.
    /// </summary>
    /// <param name="httpContextAccessor">Http context accessor</param>
    public LogResponseBodyActionTelemetryInitialiser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor ??
                               throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    /// <inheritdoc/>
    public void Initialize(ITelemetry telemetry)
    {
        // Only add logs to RequestTelemetry
        if (telemetry is RequestTelemetry requestTelemetry)
        {
            var context = _httpContextAccessor.HttpContext;

            if (context is { Request: not null, Response: not null })
            {
                // Add request body
                context.Request.EnableBuffering();
                // using (var reader = new StreamReader(context.Request.Body))
                // {
                //     var requestBody = reader.ReadToEnd();
                //     requestTelemetry.Properties["RequestBody"] = requestBody;
                //     context.Request.Body.Position = 0;
                // }

                // Add response body
                context.Response.OnCompleted(
                    async () =>
                    {
                        context.Response.Body.Seek(0, SeekOrigin.Begin);
                        using var reader = new StreamReader(context.Response.Body);
                        var responseBody = await reader.ReadToEndAsync();
                        requestTelemetry.Properties["ResponseBody"] = responseBody;
                        context.Response.Body.Seek(0, SeekOrigin.Begin);
                    });
            }
        }
    }
}