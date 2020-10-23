using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        public ActionLogFilterAttribute(IServiceProvider provider)
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
            "password",
            "token"
        };

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
        private void Configure(ActionLogFilterConfig config)
        {
            if (config == null)
            {
                return;
            }

            DisableBodyContent = config.DisableBodyContent;

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

        private ActionLogFilterConfig GetControllerActionConfiguration(ActionExecutingContext context)
        {
            // Get attribute for per request configuration
            return context.ActionDescriptor.FilterDescriptors
                .Select(descriptor => descriptor.Filter)
                .OfType<LogFilterAttribute>()
                .Select(attribute => new ActionLogFilterConfig
                {
                    DisableBodyContent = attribute.DisableBodyContent,
                    ExcludeArguments = attribute.ExcludeArguments,
                    IncludeClaims = attribute.IncludeClaims
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

            // For each Action Argument;
            // - Key is the parameter name from the controller action.
            // - Value is the parameter object from the controller action.
            var arguments = new Dictionary<string, object>();
            foreach (var argument in context.ActionArguments)
            {
                IncludeData(argument.Key, argument.Value, arguments);
            }

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

        private void IncludeData(string name, object data, IDictionary<string, object> parent)
        {
            // Skip logging of null data
            // Redact when parameter names contain excluded words
            if (data == null || name.ContainsStringCaseInsensitive(ExcludeArguments))
            {
                return;
            }

            // Filter insecure nested parameters from classes / structs
            if (data.IsClass() || data.IsNonDisplayableStruct())
            {
                IncludeObject(name, data, parent);
                return;
            }

            // Filter insecure keys from dictionaries
            if (data.IsDictionary())
            {
                IncludeDictionary(name, data as IEnumerable, parent);
                return;
            }

            // Include parameter name and value on the parent object's dictionary
            parent.Add(name, data);
        }

        private void IncludeDictionary(string name, IEnumerable data, IDictionary<string, object> parent)
        {
            var objectData = new Dictionary<string, object>();
            foreach (var entry in data)
            {
                var key = entry.GetDictionaryKey();
                var value = entry.GetDictionaryValue();
                IncludeData(key, value, objectData);
            }

            // Append data to the parent object's dictionary
            if (objectData.Count > 0)
            {
                parent.Add(name, objectData);
            }
        }

        private void IncludeObject(string name, object data, IDictionary<string, object> parent)
        {
            // Get the public property names
            var properties = data.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var objectData = new Dictionary<string, object>();

            // Append safe values to objectData
            foreach (var propertyInfo in properties)
            {
                IncludeData(propertyInfo.Name, propertyInfo.GetValue(data), objectData);
            }

            // Append objectData to the parent object's dictionary
            if (objectData.Count > 0)
            {
                parent.Add(name, objectData);
            }
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