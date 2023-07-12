using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using Audacia.Log.AspNetCore.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Options;

namespace Audacia.Log.AspNetCore;

/// <summary>
/// Attaches request content for each controller action to the <see cref="HttpContext"/> to be used later by Application Insights Telemetry.
/// </summary>
public sealed class LogActionFilterAttribute : ActionFilterAttribute
{
    /// <summary>Gets the names of claims to include in the logs. If empty, no claims are included.</summary>
    public ICollection<string> IncludeClaims { get; } = new HashSet<string>();

    /// <summary>Gets the names of arguments to exclude from the logs.</summary>
    public ICollection<string> ExcludeArguments { get; } = new HashSet<string>
    {
        "username",
        "password",
        "email",
        "token",
        "bearer"
    };

    /// <summary>
    /// Gets or sets the max depth for deconstructing objects in the request body.
    /// </summary>
    public int MaxDepth { get; set; } = 32;

    /// <summary>
    /// Gets or sets a value indicating whether the logging of all data in the request body is disabled.
    /// </summary>
    public bool DisableBodyContent { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LogActionFilterAttribute"/> class.
    /// Creates a new instance of <see cref="ActionFilterAttribute"/>.
    /// </summary>
    /// <param name="options">Global log filter configuration.</param>
#pragma warning disable CA1019 // Define accessors for attribute arguments
    public LogActionFilterAttribute(IOptions<LogActionFilterConfig> options)
#pragma warning restore CA1019 // Define accessors for attribute arguments
    {
        // Apply global log filters
        Configure(options?.Value);
    }

    /// <inheritdoc/>
    public override Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var httpContext = context?.HttpContext;

        if (httpContext != default)
        {
            Configure(GetControllerActionFilter(context!));

            AddUserInfo(httpContext.User, httpContext);

            AddClaims(httpContext.User, httpContext);
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
#pragma warning disable ACL1002 // Member or local function contains too many statements
    private void Configure(LogActionFilterConfig? config)
#pragma warning restore ACL1002 // Member or local function contains too many statements
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

        foreach (var item in config.IncludeClaims ?? Enumerable.Empty<string>())
        {
            if (IncludeClaims.Contains(item, StringComparer.InvariantCultureIgnoreCase))
            {
                continue;
            }

            IncludeClaims.Add(item);
        }
    }

    private void AddBodyContent(ActionExecutingContext context, HttpContext httpContext)
    {
        if (DisableBodyContent) { return; }

        // Copy action content and remove PII
        var arguments = new ActionArgumentDictionary(context.ActionArguments, MaxDepth, ExcludeArguments);

        if (arguments.Any())
        {
            httpContext.Items.Add(LogActionTelemetryInitialiser.ActionArguments, arguments);
        }
    }

    private void AddClaims(IPrincipal principal, HttpContext httpContext)
    {
        if (principal?.Identity?.IsAuthenticated != true) { return; }

        if (!(principal.Identity is ClaimsIdentity identity)) { return; }

        var claims = identity.Claims
            .Where(claim => IncludeClaims.Contains(claim.Subject.Name))
            .Select(claim => $"\"{claim.Subject.Name}\": \"{claim.Value}\"")
            .ToArray();

        if (claims.Any())
        {
            httpContext.Items.Add(LogActionTelemetryInitialiser.ActionClaims, $"{{ {string.Join(", ", claims)} }}");
        }
    }

    private static void AddUserInfo(IPrincipal principal, HttpContext httpContext)
    {
        if (principal?.Identity?.IsAuthenticated != true) { return; }

        if (!(principal.Identity is ClaimsIdentity identity)) { return; }

        var userId = identity.FindFirst("sub")?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            httpContext.Items.Add(LogActionTelemetryInitialiser.ActionUserId, userId);
        }

        var userRoles = identity.FindAll("role").Select(c => c.Value);
        if (userRoles.Any()) 
        {
            httpContext.Items.Add(LogActionTelemetryInitialiser.ActionUserRoles, string.Join(", ", userRoles));
        }
    }

    private static LogActionFilterConfig GetControllerActionFilter(ActionExecutingContext context)
    {
        // Get attribute for per request configuration
        return context.ActionDescriptor.FilterDescriptors
            .Select(descriptor => descriptor.Filter)
            .OfType<LogFilterAttribute>()
            .Select(attribute => new LogActionFilterConfig
            {
                DisableBodyContent = attribute.DisableBodyContent,
                ExcludeArguments = attribute.ExcludeArguments,
                IncludeClaims = attribute.IncludeClaims,
                MaxDepth = attribute.MaxDepth
            })
            .FirstOrDefault();
    }
}
