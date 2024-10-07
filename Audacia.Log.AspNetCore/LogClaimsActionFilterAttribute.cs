using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using Audacia.Log.AspNetCore.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace Audacia.Log.AspNetCore;

/// <summary>
/// Attaches request content for each controller action to the <see cref="HttpContext"/> to be used later by Application Insights Telemetry.
/// </summary>
public sealed class LogClaimsActionFilterAttribute : ActionFilterAttribute
{
    /// <summary>Gets the names of claims to include in the logs. If empty, no claims are included.</summary>
    public ICollection<string> IncludeClaims { get; } = new HashSet<string>();

    /// <summary>Gets the names of arguments to exclude from the logs.</summary>
    public ICollection<string> ExcludeArguments { get; } =
        new List<string>()
        {
            "username",
            "password",
            "email",
            "token",
            "bearer"
        };

    /// <summary>
    /// Gets or sets id claim name.
    /// </summary>
    public string IdClaimName { get; set; } = ClaimTypes.Sub;

    /// <summary>
    /// Gets or sets role claim name.
    /// </summary>
    public string RoleClaimName { get; set; } = ClaimTypes.Role;

    /// <summary>
    /// Initializes a new instance of the <see cref="LogClaimsActionFilterAttribute"/> class.
    /// Creates a new instance of <see cref="ActionFilterAttribute"/>.
    /// </summary>
    /// <param name="options">Options for <see cref="LogActionFilterConfig"/>.</param>
    public LogClaimsActionFilterAttribute(IOptions<LogActionFilterConfig> options)
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

            AddUserInfo(httpContext.User, httpContext);

            AddClaims(
                httpContext.User,
                httpContext);
        }

        return base.OnActionExecutionAsync(context, next);
    }

    private void AddUserInfo(
        IPrincipal principal,
        HttpContext httpContext)
    {
        if (principal?.Identity?.IsAuthenticated != true)
        {
            return;
        }

        if (principal is not { Identity: ClaimsIdentity identity })
        {
            return;
        }

        var userId = identity.FindFirst(IdClaimName)?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            httpContext.Items.Add(LogClaimsActionTelemetryInitialiser.ActionUserId, userId);
        }

        var userRoles = identity.FindAll(RoleClaimName).Select(c => c.Value);
        if (userRoles.Any())
        {
            httpContext.Items.Add(LogClaimsActionTelemetryInitialiser.ActionUserRoles, string.Join(", ", userRoles));
        }
    }

    private void AddClaims(
        IPrincipal principal,
        HttpContext httpContext)
    {
        if (principal?.Identity?.IsAuthenticated != true)
        {
            return;
        }

        if (principal is not { Identity: ClaimsIdentity identity })
        {
            return;
        }

        var claims = identity.Claims
            .Where(claim => IncludeClaims.Contains(claim.Subject.Name))
            .Select(claim => $"\"{claim.Subject.Name}\": \"{claim.Value}\"")
            .ToArray();

        if (claims.Any())
        {
            httpContext.Items.Add(
                LogClaimsActionTelemetryInitialiser.ActionClaims,
                $"{{ {string.Join(", ", claims)} }}");
        }
    }

    private void Configure(LogActionFilterConfig? config)
    {
        if (config == null)
        {
            return;
        }

        SetExcludeArguments(config);

        SetIncludeClaims(config);

        IdClaimName = config.IdClaimType;

        RoleClaimName = config.RoleClaimType;
    }

    private void SetExcludeArguments(LogActionFilterConfig config)
    {
        foreach (var item in config.ExcludeArguments ?? Enumerable.Empty<string>())
        {
            if (ExcludeArguments.Contains(item, StringComparer.InvariantCultureIgnoreCase))
            {
                continue;
            }

            ExcludeArguments.Add(item);
        }
    }

    private void SetIncludeClaims(LogActionFilterConfig config)
    {
        foreach (var item in config.IncludeClaims ?? Enumerable.Empty<string>())
        {
            if (IncludeClaims.Contains(item, StringComparer.InvariantCultureIgnoreCase))
            {
                continue;
            }

            IncludeClaims.Add(item);
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
                    ExcludeArguments = attribute.ExcludeArguments.ToList(),
                    IncludeClaims = attribute.IncludeClaims.ToList()
                })
            .FirstOrDefault();
    }
}