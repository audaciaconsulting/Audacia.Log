using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using Audacia.Log.AspNetCore.Internal;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace Audacia.Log.AspNetCore
{
    /// <summary>Logs requests and responses for each Controller Action.</summary>
    public sealed class LogActionFilterAttribute : ActionFilterAttribute
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

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
        /// Gets or sets the max depth for desconstructing objects in the request body.
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
        /// <param name="options">Global log filter configuration</param>
        /// <param name="httpContextAccessor">HTTP context accessor</param>
#pragma warning disable CA1019 // Define accessors for attribute arguments
        public LogActionFilterAttribute(IOptions<LogActionFilterConfig> options, IHttpContextAccessor httpContextAccessor)
#pragma warning restore CA1019 // Define accessors for attribute arguments
        {
            _httpContextAccessor = httpContextAccessor;

            // Apply global log filters
            Configure(options?.Value);
        }

        /// <inheritdoc/>
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var request = context?.HttpContext?.Request;
            if (request != null && (request.Method == HttpMethods.Post || request.Method == HttpMethods.Put))
            {
                // Apply action specific log filters
                Configure(GetControllerActionFilter(context));

                // Add request info to telemetry
                var telemetry = _httpContextAccessor.HttpContext.Items["Telemetry"] as RequestTelemetry;
                if (telemetry != null)
                {
                    LogUserInfo(_httpContextAccessor.HttpContext.User, telemetry);

                    LogClaims(_httpContextAccessor.HttpContext.User, telemetry);

                    LogBodyContent(context, telemetry);
                }
            }
        }

        /// <summary>
        /// Applies configuration to the log filter if provided.
        /// </summary>
        /// <param name="config">global or action config.</param>
#pragma warning disable ACL1002 // Member or local function contains too many statements
        private void Configure(LogActionFilterConfig config)
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

            if (config.ExcludeArguments?.Any() == true)
            {
                foreach (var item in config.ExcludeArguments)
                {
                    if (ExcludeArguments.Contains(item, StringComparer.InvariantCultureIgnoreCase))
                    {
                        continue;
                    }

                    ExcludeArguments.Add(item);
                }
            }

            if (config.IncludeClaims?.Any() == true)
            {
                foreach (var item in config.IncludeClaims)
                {
                    if (IncludeClaims.Contains(item, StringComparer.InvariantCultureIgnoreCase))
                    {
                        continue;
                    }

                    IncludeClaims.Add(item);
                }
            }
        }

        private void LogBodyContent(ActionExecutingContext context, RequestTelemetry requestTelemetry)
        {
            if (DisableBodyContent) { return; }

            // Copy action content and remove PII
            var arguments = new ActionArgumentDictionary(context.ActionArguments, MaxDepth, ExcludeArguments);

            if (arguments.Any())
            {
                requestTelemetry.Properties.Add("Arguments", arguments.ToString());
            }
        }

        private void LogClaims(IPrincipal principal, RequestTelemetry requestTelemetry)
        {
            if (principal?.Identity?.IsAuthenticated != true) { return; }

            if (!(principal.Identity is ClaimsIdentity identity)) { return; }

            var claims = identity.Claims
                .Where(claim => IncludeClaims.Contains(claim.Subject.Name))
                .Select(claim => $"\"{claim.Subject.Name}\": \"{claim.Value}\"")
                .ToArray();

            if (claims.Any())
            {
                requestTelemetry.Properties.Add("Claims", $"{{ {string.Join(", ", claims)} }}");
            }
        }

        private static void LogUserInfo(IPrincipal principal, RequestTelemetry requestTelemetry)
        {
            if (principal?.Identity?.IsAuthenticated != true) { return; }

            if (!(principal.Identity is ClaimsIdentity identity)) { return; }

            var userId = identity.FindFirst("sub")?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                requestTelemetry.Properties.Add("UserId", userId);
            }

            var userRoles = identity.FindAll("role").Select(c => c.Value);
            if (userRoles.Any()) 
            {
                requestTelemetry.Properties.Add("UserRoles", string.Join(", ", userRoles));
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
}