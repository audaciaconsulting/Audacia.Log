using Microsoft.AspNetCore.Http;

namespace Audacia.Log.AspNetCore.Extensions;

/// <summary>
/// Extensions for <see cref="HttpContext"/>.
/// </summary>
internal static class HttpContextExtensions
{
    /// <summary>
    /// Returns <see langword="true"/> when <see cref="HttpRequest"/> is a POST or PUT request.
    /// </summary>
    /// <param name="httpContext">Instance of <see cref="HttpRequest"/>.</param>
    /// <returns>Whether the request has form data.</returns>
    public static bool HasFormData(this HttpContext httpContext)
    {
        return httpContext.Request != null &&
               (httpContext.Request.Method == HttpMethods.Post ||
                httpContext.Request.Method == HttpMethods.Put);
    }
}