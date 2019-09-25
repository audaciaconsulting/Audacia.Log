using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Serilog;

namespace Audacia.Log.AspNetCore
{
	/// <summary>Logs requests and responses for each Controller Action.</summary>
	public sealed class ActionLogFilterAttribute : ActionFilterAttribute
	{
		/// <summary>The logger used by this filter for writing logs.</summary>
		public ILogger Logger { get; }

        /// <summary>Initializes a new instance of the <see cref="ActionLogFilterAttribute"/> class.Creates a new instance of <see cref="ActionFilterAttribute"/>.</summary>
		public ActionLogFilterAttribute(ILogger logger)
        {
	        if (logger == null) throw new ArgumentNullException(nameof(logger));

	        Logger = logger.ForContext<ActionLogFilterAttribute>();
        }

        /// <summary>The names of claims to include in the logs. If empty, no claims are included.</summary>
		public ICollection<string> IncludeClaims { get; } = new HashSet<string>();

		/// <summary>The names of arguments to exclude from the logs.</summary>
		public ICollection<string> ExcludeArguments { get; } = new HashSet<string>();

		/// <inheritdoc />
		public override void OnActionExecuting(ActionExecutingContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			var arguments = context.ActionArguments?.Where(a => !ExcludeArguments.Contains(a.Key, StringComparer.InvariantCultureIgnoreCase));
			var log = Logger.ForContext("Arguments", arguments, true);

			if (context.Controller is Controller controller && IncludeClaims.Any())
			{
				var claims = controller.User?.Claims?.Where(c => IncludeClaims.Contains(c.Subject.Name)).Select(c => c.Subject.Name + ": " + c.Value);

				if (claims != null && claims.Any())
					log = log.ForContext("Claims", claims, true);
			}

			log.Information("Action Executing: {Action}.", context.ActionDescriptor.DisplayName);
			base.OnActionExecuting(context);
		}

		/// <inheritdoc />
		public override void OnActionExecuted(ActionExecutedContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			// kinda smells but is there a better way? I think not.
			var result = context.Result?.GetType().GetProperty("Value")?.GetValue(context.Result);
			var resultType = result?.GetType().Name;
			var actionName = context.ActionDescriptor.DisplayName;

			var log = context.Exception == null ? Logger : Logger.ForContext("Exception", context.Exception, true);

			if (context.Controller is Controller controller && IncludeClaims.Any())
			{
				var claims = controller.User?.Claims?.Where(c => IncludeClaims.Contains(c.Subject.Name)).Select(c => c.Subject.Name + ": " + c.Value);

				if (claims != null && claims.Any())
					log = log.ForContext("Claims", claims, true);
			}

			if (result == null) log.Information("Action Executed: {Action}.", actionName);
			else log.Information("Action Executed: {ModelType} returned from {Action}.", resultType, actionName);

			base.OnActionExecuted(context);
		}
    }
}