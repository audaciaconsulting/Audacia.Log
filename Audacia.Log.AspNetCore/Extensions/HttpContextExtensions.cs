using Microsoft.AspNetCore.Http;

namespace Audacia.Log.AspNetCore.Extensions
{
    internal static class HttpContextExtensions
    {
        public static bool HasFormData(this HttpContext httpContext)
        {
            return httpContext.Request != null &&
                (httpContext.Request.Method == HttpMethods.Post ||
                 httpContext.Request.Method == HttpMethods.Put);
        }
    }
}
