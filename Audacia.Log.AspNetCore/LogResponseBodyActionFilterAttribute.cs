using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using JsonSerializer = System.Text.Json.JsonSerializer;

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
    public ICollection<string> ExcludedProperties { get; } = new List<string>
    {
        "username",
        "password",
        "email",
        "token",
        "bearer",
        "name",
        "firstname",
        "lastname",
        "phonenumber",
        "dateofbirth"
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="LogResponseBodyActionFilterAttribute"/> class.
    /// Creates a new instance of <see cref="ActionFilterAttribute"/>.
    /// </summary>
    /// <param name="options">Global log filter configuration.</param>
    public LogResponseBodyActionFilterAttribute(IOptions<LogActionFilterConfig> options)
    {
        Configure(options?.Value);
    }

    /// <inheritdoc />
    public override Task OnResultExecutionAsync(
        ResultExecutingContext context,
        ResultExecutionDelegate next)
    {
        var httpContext = context.HttpContext;

        if (httpContext != default)
        {
            Configure(GetControllerActionFilter(context!));
        }

        if (DisableBodyContent)
        {
            return Task.CompletedTask;
        }

        if (context is { Result: ObjectResult objectResult } && httpContext is not null)
        {
            ProcessResponse(objectResult, httpContext);
        }

        return Task.CompletedTask;
    }

    private void ProcessResponse(
        ObjectResult objectResult,
        HttpContext httpContext)
    {
        var responseDic = new Dictionary<string, object> { { "Value", objectResult.Value } };

        // Copy action content and remove PII
        var arguments = new RedactionDictionary(responseDic, MaxDepth, ExcludedProperties);

        if (arguments.Any())
        {
            httpContext.Items.Add(LogResponseBodyActionTelemetryInitialiser.ActionResponseBody, arguments);
        }
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

    private static LogActionFilterConfig? GetControllerActionFilter(ResultExecutingContext context)
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