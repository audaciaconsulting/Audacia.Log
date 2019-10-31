using System;
using System.Web.Mvc;
using Microsoft.ApplicationInsights;

namespace Audacia.Log.AspNet4
{
	/// <summary>The default error handling filter that application insights uses for ASP.NET 4 projects.</summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
	public sealed class AiHandleErrorAttribute : HandleErrorAttribute
	{
		/// <inheritdoc />
		public override void OnException(ExceptionContext filterContext)
		{
			if (filterContext?.HttpContext != null && filterContext.Exception != null)
			{
				// If customError is Off, then AI HTTPModule will report the exception
				if (filterContext.HttpContext.IsCustomErrorEnabled)
				{
					var ai = new TelemetryClient();
					ai.TrackException(filterContext.Exception);
				}
			}

			base.OnException(filterContext);
		}
	}
}