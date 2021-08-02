using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;

namespace Audacia.Log.AspNetCore
{
    /// <summary>
    /// An application insights telemetery initialiser that attaches claims and form data to the request.
    /// </summary>
    public sealed class RequestDataTelemetryInitialiser : ITelemetryInitializer
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
        /// Creates an instance of RequestDataTelemetryInitialiser.
        /// </summary>
        /// <param name="httpContextAccessor">Http context accessor</param>
        /// <param name="config">Action log filter configuration</param>
        public RequestDataTelemetryInitialiser(IHttpContextAccessor httpContextAccessor, ActionLogFilterConfig config)
        {
            _httpContextAccessor = httpContextAccessor ??
                throw new ArgumentNullException(nameof(httpContextAccessor));

            Configure(config);
        }

        /// <summary>
        /// Attaches claims and form data to the request telemetry.
        /// </summary>
        /// <param name="telemetry"></param>
        public void Initialize(ITelemetry telemetry)
        {
            // Ensure current telemetry is a request log
            if (!(telemetry is RequestTelemetry requestTelemetry)) { return; }

            var httpContext = _httpContextAccessor.HttpContext;

            LogUserInfo(httpContext.User, requestTelemetry);

            LogClaims(httpContext.User, requestTelemetry);

            LogBodyContent(httpContext.Request, requestTelemetry);
        }

        /// <summary>
        /// Applies configuration to the log filter if provided.
        /// </summary>
        /// <param name="config">global or action config.</param>
#pragma warning disable ACL1002 // Member or local function contains too many statements
        private void Configure(ActionLogFilterConfig config)
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

            if (config.ExcludeArguments?.Length > 0)
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

            if (config.IncludeClaims?.Length > 0)
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

        private void LogBodyContent(HttpRequest request, RequestTelemetry requestTelemetry)
        {
            if (DisableBodyContent) { return; }

            if (request?.Form.Any() != true) { return; }

            var arguments = new ActionArgumentDictionary(request.Form, MaxDepth, ExcludeArguments);

            requestTelemetry.Properties.Add("Arguments", arguments.ToString());
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
    }
}
