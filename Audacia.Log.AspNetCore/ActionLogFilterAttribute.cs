using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Serilog;

namespace Audacia.Log.AspNetCore
{
    /// <summary>Logs requests and responses for each Controller Action.</summary>
    public sealed class ActionLogFilterAttribute : ActionFilterAttribute
    {
        /// <summary>Gets the logger used by this filter for writing logs.</summary>
        public ILogger Logger { get; }

        /// <summary>Initializes a new instance of the <see cref="ActionLogFilterAttribute"/> class.Creates a new instance of <see cref="ActionFilterAttribute"/>.</summary>
        public ActionLogFilterAttribute(ILogger logger)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            Logger = logger.ForContext<ActionLogFilterAttribute>();
        }

        /// <summary>Gets the names of claims to include in the logs. If empty, no claims are included.</summary>
        public ICollection<string> IncludeClaims { get; } = new HashSet<string>();

        /// <summary>Gets the names of arguments to exclude from the logs.</summary>
        public ICollection<string> ExcludeArguments { get; } = new HashSet<string>();

        /// <inheritdoc />
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            // For each Action Argument;
            // - Key is the parameter name from the controller action.
            // - Value is the parameter object from the controller action.
            // We need to recursively search the Value for parameter names that should be redacted.
            var arguments = new Dictionary<string, object>();
            foreach (var argument in context.ActionArguments)
            {
                IncludeData(argument.Key, argument.Value, arguments);
            }

            var log = Logger.ForContext("Arguments", arguments, true);

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

            // kinda smells but is there a better way? I think not.
            var result = context.Result?.GetType().GetProperty("Value")?.GetValue(context.Result);
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

#pragma warning disable ACL1002 // Function too long - really though?
        private void IncludeData(string name, object data, IDictionary<string, object> parent)
#pragma warning restore ACL1002
        {
            // Skip logging of null data
            // Redact arguments with keys containing excluded words
            if (data == null || name.ContainsStringCaseInsensitive(ExcludeArguments))
            {
                return;
            }

            // Iterate through classes and structs to remove parameters with redacted words in the name
            var dataType = data.GetType();
            var isClass = dataType.IsClass && !(data is string);
            var isStruct = dataType.IsValueType && !dataType.IsPrimitive && !dataType.IsEnum;
            if (isClass || isStruct)
            {
                // Get the public property names
                var properties = dataType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                var objectData = new Dictionary<string, object>();

                // Append safe values to classData
                foreach (var propertyInfo in properties)
                {
                    IncludeData(propertyInfo.Name, propertyInfo.GetValue(data), objectData);
                }

                // Append classData to the parent object's dictionary
                if (objectData.Count > 0)
                {
                    parent.Add(name, objectData);
                }

                return;
            }

            // Include parameter name and value on the parent object's dictionary
            parent.Add(name, data);
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