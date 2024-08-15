using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Audacia.Log.AspNetCore.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace Audacia.Log.AspNetCore;

/// <summary>
/// Attaches request content for each controller action to the <see cref="HttpContext"/> to be used later by Application Insights Telemetry.
/// </summary>
public sealed class LogRequestBodyActionFilterAttribute : ActionFilterAttribute
{
    /// <summary>
    /// Gets or sets the max depth for deconstructing objects in the request body.
    /// </summary>
    public int MaxDepth { get; set; } = 32;

    /// <summary>
    /// Gets or sets a value indicating whether the logging of all data in the request body is disabled.
    /// </summary>
    public bool DisableBodyContent { get; set; }

    /// <summary>Gets the names of arguments to exclude from the logs.</summary>
    public ICollection<string> ExcludeArguments { get; } =
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
    /// Initializes a new instance of the <see cref="LogRequestBodyActionFilterAttribute"/> class.
    /// Creates a new instance of <see cref="ActionFilterAttribute"/>.
    /// </summary>
    /// <param name="options">Global log filter configuration.</param>
    public LogRequestBodyActionFilterAttribute(IOptions<LogActionFilterConfig> options)
    {
        Configure(options?.Value);
    }

    /// <inheritdoc/>
    public override Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        var httpContext = context?.HttpContext;

        if (httpContext != default)
        {
            Configure(GetControllerActionFilter(context!));
        }

        if (httpContext?.HasFormData() == true)
        {
            AddBodyContent(context!, httpContext);
        }

        return base.OnActionExecutionAsync(context, next);
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
            if (ExcludeArguments.Contains(item, StringComparer.InvariantCultureIgnoreCase))
            {
                continue;
            }

            ExcludeArguments.Add(item);
        }
    }

    private void AddBodyContent(
        ActionExecutingContext context,
        HttpContext httpContext)
    {
        if (DisableBodyContent)
        {
            return;
        }

        // Copy action content and remove PII
        var arguments = new RedactionDictionary(context.ActionArguments, MaxDepth, ExcludeArguments);

        if (arguments.Any())
        {
            httpContext.Items.Add(LogRequestBodyActionTelemetryInitialiser.ActionArguments, arguments);
        }
    }

    private static LogActionFilterConfig? GetControllerActionFilter(ActionExecutingContext context)
    {
        // Get attribute for per request configuration
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