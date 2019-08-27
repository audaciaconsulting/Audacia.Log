using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Serilog;

namespace Audacia.Log.AspNetCore
{
	public class ActionLogFilter : ActionFilterAttribute
	{
		private readonly ILogger _logger;

		public ActionLogFilter(ILogger logger) => _logger = logger.ForContext<ActionLogFilter>();

		public ICollection<string> IncludeClaims { get; } = new HashSet<string>();
		
		public override void OnActionExecuting(ActionExecutingContext context)
		{
			var log = _logger.ForContext("Arguments", context.ActionArguments, true);

			if (context.Controller is Controller controller && IncludeClaims.Any())
			{
				var claims = controller.User?.Claims?.Where(c => IncludeClaims.Contains(c.Subject.Name)).Select(c => c.Subject.Name + ": " + c.Value);
				
				if (claims != null && claims.Any())
					log = log.ForContext("Claims", claims, true);
			}
			
			log.Information("Action Executing: {Action}.", context.ActionDescriptor.DisplayName);
			base.OnActionExecuting(context);
		}

		public override void OnActionExecuted(ActionExecutedContext context)
		{
			// kinda smells but is there a better way? I think not.
			var result = context.Result?.GetType().GetProperty("Value")?.GetValue(context.Result);
			var resultType = result?.GetType().Name;
			var actionName = context.ActionDescriptor.DisplayName;

			var log = context.Exception == null ? _logger : _logger.ForContext("Exception", context.Exception, true);
			
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