using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Audacia.Log.AspNetCore
{
    /// <summary>Logs requests and responses for each Controller Action.</summary>
    public sealed class ActionLogFilterAttribute : ActionFilterAttribute
    {
        private readonly IActionLogFilterConfigAccessor _configAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionLogFilterAttribute"/> class.
        /// Creates a new instance of <see cref="ActionFilterAttribute"/>.
        /// </summary>
        /// <param name="configAccessor">Action log filter configuration accessor</param>
#pragma warning disable CA1019 // Define accessors for attribute arguments
        public ActionLogFilterAttribute(IActionLogFilterConfigAccessor configAccessor)
#pragma warning restore CA1019 // Define accessors for attribute arguments
        {
            _configAccessor = configAccessor ??
                throw new ArgumentNullException(nameof(configAccessor));
        }

        /// <inheritdoc />
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            _configAccessor.ActionConfiguration = GetControllerActionConfiguration(context);

            base.OnActionExecuting(context);
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
    }
}