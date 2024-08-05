using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using JsonException = System.Text.Json.JsonException;

namespace Audacia.Log.AspNetCore;

/// <summary>
/// Attaches response content for each controller action to the <see cref="HttpContext"/> to be used later by Application Insights Telemetry.
/// </summary>
public sealed class LogResponseBodyActionFilterAttribute : ActionFilterAttribute
{
    /// <summary>
    /// Gets or sets the max depth for deconstructing objects in the response body.
    /// </summary>
    public int MaxDepth { get; set; } = 32;

    /// <summary>
    /// Gets or sets a value indicating whether the logging of all data in the response body is disabled.
    /// </summary>
    public bool DisableBodyContent { get; set; }

    /// <summary>
    /// Gets the names of properties to exclude from the logs.
    /// </summary>
    public ICollection<string> ExcludedProperties { get; } =
    [
        "username",
        "password",
        "email",
        "token",
        "bearer"
    ];

    /// <summary>
    /// Initializes a new instance of the <see cref="LogResponseBodyActionFilterAttribute"/> class.
    /// Creates a new instance of <see cref="ActionFilterAttribute"/>.
    /// </summary>
    /// <param name="options">Global log filter configuration.</param>
    [SuppressMessage(
        "Design",
        "CA1019:Define accessors for attribute arguments",
        Justification = "Options does not need to a corresponding property.")]
    public LogResponseBodyActionFilterAttribute(IOptions<LogActionFilterConfig> options)
    {
        Configure(options?.Value);
    }

    /// <inheritdoc/>
#pragma warning disable ACL1002
    public override async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
#pragma warning restore ACL1002
    {
        var httpContext = context?.HttpContext;

        if (httpContext != default)
        {
            Configure(GetControllerActionFilter(context!));
        }

        if (DisableBodyContent)
        {
            return;
        }

        var originalBodyStream = httpContext!.Response.Body;

        await using var responseBodyStream = new MemoryStream();
        httpContext.Response.Body = responseBodyStream;

        var executedContext = await next();

        httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        using var sr = new StreamReader(httpContext.Response.Body);
        var responseBodyText = await sr.ReadToEndAsync();
        httpContext.Response.Body.Seek(0, SeekOrigin.Begin);

        try
        {
            var responseBodyJson = JsonConvert.DeserializeObject<JObject>(responseBodyText);
            if (responseBodyJson != null)
            {
                // Remove the excluded properties
#pragma warning disable ACL1011
                foreach (var property in ExcludedProperties)
                {
                    responseBodyJson.Remove(property);
                }
#pragma warning restore ACL1011
            }

            httpContext!.Items.Add(LogResponseBodyActionTelemetryInitialiser.ActionResponseBody, responseBodyJson);
        }
        catch (JsonException)
        {
            // Handle JSON parsing errors if necessary
            httpContext!.Items.Add(
                LogResponseBodyActionTelemetryInitialiser.ActionResponseBody,
                "Failure to deserialize response body");
        }

        await responseBodyStream.CopyToAsync(originalBodyStream);
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
            if (ExcludedProperties.Contains(item, StringComparer.InvariantCultureIgnoreCase))
            {
                continue;
            }

            ExcludedProperties.Add(item);
        }
    }

    private static LogActionFilterConfig? GetControllerActionFilter(ActionExecutingContext context)
    {
        // Get attribute for per response configuration
        return context.ActionDescriptor.FilterDescriptors
            .Select(descriptor => descriptor.Filter)
            .OfType<LogFilterAttribute>()
            .Select(
                attribute => new LogActionFilterConfig
                {
                    ExcludeArguments = attribute.ExcludeArguments,
                    MaxDepth = attribute.MaxDepth,
                    DisableBodyContent = attribute.DisableBodyContent
                })
            .FirstOrDefault();
    }
}