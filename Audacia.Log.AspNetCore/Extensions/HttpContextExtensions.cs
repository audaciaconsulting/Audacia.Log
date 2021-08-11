using Microsoft.AspNetCore.Http;

namespace Audacia.Log.AspNetCore.Extensions
{
    internal static class HttpContextExtensions
    {
        /// <summary>
        /// Returns <see langword="true"/> when <see cref="HttpRequest"/> is a POST or PUT request.
        /// </summary>
        public static bool HasFormData(this HttpContext httpContext)
        {
            return httpContext.Request != null &&
                (httpContext.Request.Method == HttpMethods.Post ||
                 httpContext.Request.Method == HttpMethods.Put);
        }
    }
}
