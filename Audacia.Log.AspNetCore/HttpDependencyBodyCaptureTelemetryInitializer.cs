using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Options;

namespace Audacia.Log.AspNetCore;

/// <summary>
/// Enriches http dependency telemetry with request and response bodies.
/// </summary>
public class HttpDependencyBodyCaptureTelemetryInitializer : ITelemetryInitializer
{
    /// <summary>
    /// Gets the custom property name for the request body.
    /// </summary>
    public const string RequestBodyCustomProperty = "RequestBody";

    /// <summary>
    /// Gets the custom property name for the response body.
    /// </summary>
    public const string ResponseBodyCustomProperty = "ResponseBody";

    /// <summary>
    /// Gets or sets the max depth for deconstructing objects in the request body.
    /// </summary>
    public int MaxDepth { get; set; } = 32;

    /// <summary>
    /// Gets or sets a value indicating whether the logging of all data in the request body is disabled.
    /// </summary>
    public bool DisableBodyContent { get; set; }

    /// <summary>Gets the names of arguments to exclude from the logs.</summary>
    public ICollection<string> ExcludeProperties { get; } =
    [
        "username",
        "password",
        "email",
        "token",
        "bearer",
        "name",
        "firstname",
        "lastname",
        "phonenumber",
        "dateofbirth",
    ];

    /// <summary>
    /// Creates telemetry initialiser for enriching dependency telemetry with request and response bodies.
    /// </summary>
    /// <param name="options"></param>
    public HttpDependencyBodyCaptureTelemetryInitializer(IOptions<LogActionFilterConfig> options)
    {
        Configure(options?.Value);
    }

    /// <summary>
    /// Applies configuration to the log filter if provided.
    /// </summary>
    /// <param name="config">global or action config.</param>
    private void Configure(LogActionFilterConfig? config)
    {
        if (config == null)
        {
            return;
        }

        DisableBodyContent = config.DisableBodyContent;

        if (config.MaxDepth > 0)
        {
            MaxDepth = config.MaxDepth;
        }

        foreach (var item in config.ExcludeArguments ?? Enumerable.Empty<string>())
        {
            if (ExcludeProperties.Contains(item, StringComparer.InvariantCultureIgnoreCase))
            {
                continue;
            }

            ExcludeProperties.Add(item);
        }
    }

    /// <inheritdoc />
    public void Initialize(ITelemetry telemetry)
    {
        if (telemetry is DependencyTelemetry { Type: "Http" } dependencyTelemetry &&
            dependencyTelemetry.TryGetOperationDetail("HttpResponse", out var responseObj) &&
            responseObj is HttpResponseMessage response)
        {
            EnrichResponseBody(response, dependencyTelemetry);

            EnrichWithRequestBody(response, dependencyTelemetry);
        }
    }

    private void EnrichResponseBody(
        HttpResponseMessage response,
        DependencyTelemetry dependencyTelemetry)
    {
        var responseBody = CaptureResponseBodyAsync(response).GetAwaiter().GetResult();
        var redactedResponseBody = RedactBody(responseBody);

        if (redactedResponseBody is not null)
        {
            dependencyTelemetry.Properties[ResponseBodyCustomProperty] =
                JsonSerializer.Serialize(redactedResponseBody);
        }
    }

    private void EnrichWithRequestBody(
        HttpResponseMessage response,
        DependencyTelemetry dependencyTelemetry)
    {
        if (response.RequestMessage != null)
        {
            var requestBody = CaptureRequestBodyAsync(response.RequestMessage).GetAwaiter().GetResult();
            var redactedRequestBody = RedactBody(requestBody);

            if (redactedRequestBody is not null)
            {
                dependencyTelemetry.Properties[RequestBodyCustomProperty] =
                    JsonSerializer.Serialize(redactedRequestBody);
            }
        }
    }

    private RedactionDictionary? RedactBody(string body)
    {
        try
        {
            var bodyObject = JsonSerializer.Deserialize<object>(body);

            var bodyDictionary = new Dictionary<string, object> { { "Value", bodyObject } };

            // Copy action content and remove PII
            var arguments = new RedactionDictionary(bodyDictionary, 5, ExcludeProperties);

            if (arguments.Any())
            {
                return arguments;
            }

            return null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    private async Task<string> CaptureResponseBodyAsync(HttpResponseMessage response)
    {
        if (response.Content == null)
        {
            return string.Empty;
        }

        return await response.Content.ReadAsStringAsync();
    }

    private async Task<string> CaptureRequestBodyAsync(HttpRequestMessage request)
    {
        if (request.Content == null)
        {
            return string.Empty;
        }

        return await request.Content.ReadAsStringAsync();
    }
}