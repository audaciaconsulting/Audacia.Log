using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Audacia.Log.AspNetCore
{
    /// <summary>Logs requests and responses for each Controller Action.</summary>
    public sealed class ActionLogFilterAttribute : ActionFilterAttribute
    {
        /// <summary>Gets the logger used by this filter for writing logs.</summary>
        public ILogger Logger { get; }

        /// <summary>Initializes a new instance of the <see cref="ActionLogFilterAttribute"/> class.Creates a new instance of <see cref="ActionFilterAttribute"/>.</summary>
#pragma warning disable CA1019 // Define accessors for attribute arguments
        public ActionLogFilterAttribute(IServiceProvider provider)
#pragma warning restore CA1019 // Define accessors for attribute arguments
        {
            if (provider == null)
            {
                throw new ArgumentNullException(nameof(provider));
            }

            var logger = provider.GetRequiredService<ILogger>();
            var globalConfig = provider.GetService<ActionLogFilterConfig>();

            Configure(globalConfig);
            Logger = logger.ForContext<ActionLogFilterAttribute>();
        }

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

        /// <inheritdoc />
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var actionConfig = GetControllerActionConfiguration(context);
            Configure(actionConfig);

            var log = LogArguments(Logger, context);

            if (context.Controller is Controller controller && IncludeClaims.Any())
            {
                log = LogClaims(log, controller);
            }

            log.Information("Action Executing: {Action}.", context.ActionDescriptor.DisplayName);
            base.OnActionExecuting(context);
        }

        /// <inheritdoc />
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var result = context.Result?.GetValue();
            var resultType = result?.GetType().Name;
            var actionName = context.ActionDescriptor.DisplayName;

            var log = context.Exception == null ? Logger : Logger.ForContext("Exception", context.Exception, true);

            if (context.Controller is Controller controller && IncludeClaims.Any())
            {
                log = LogClaims(log, controller);
            }

            LogFinish(result, resultType, actionName, log);

            base.OnActionExecuted(context);
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

        private static ActionLogFilterConfig GetControllerActionConfiguration(ActionExecutingContext context)
        {
            // Get attribute for per request configuration
            return context.ActionDescriptor.FilterDescriptors
                .Select(descriptor => descriptor.Filter)
                .OfType<LogFilterAttribute>()
                .Select(attribute => new ActionLogFilterConfig
                {
                    DisableBodyContent = attribute.DisableBodyContent,
                    ExcludeArguments = attribute.ExcludeArguments,
                    IncludeClaims = attribute.IncludeClaims,
                    MaxDepth = attribute.MaxDepth
                })
                .FirstOrDefault();
        }

        /// <summary>
        /// Recursively search the Value for parameter names that should be redacted.
        /// </summary>
        private ILogger LogArguments(ILogger log, ActionExecutingContext context)
        {
            if (DisableBodyContent)
            {
                return log;
            }

            // Redact PII
            var arguments = new ActionArgumentDictionary(context.ActionArguments, MaxDepth, ExcludeArguments);

            return log.ForContext("Arguments", arguments, true);
        }

        private ILogger LogClaims(ILogger log, Controller controller)
        {
            var claims = controller.User?.Claims?
                .Where(claim => IncludeClaims.Contains(claim.Subject.Name))
                .Select(claim => claim.Subject.Name + ": " + claim.Value);

            var returnLog = log;
            if (claims?.Any() == true)
            {
                returnLog = log.ForContext("Claims", claims, true);
            }

            return returnLog;
        }

        private static void LogFinish(object result, string resultType, string actionName, ILogger log)
        {
            if (result == null)
            {
                log.Information("Action Executed: {Action}.", actionName);
            }
            else
            {
                log.Information("Action Executed: {ModelType} returned from {Action}.", resultType, actionName);
            }
        }
    }
}