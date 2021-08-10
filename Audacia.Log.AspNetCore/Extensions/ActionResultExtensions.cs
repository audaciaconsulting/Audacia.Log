using Microsoft.AspNetCore.Mvc;

namespace Audacia.Log.AspNetCore.Extensions
{
    internal static class ActionResultExtensions
    {
        /// <summary>
        /// Returns the content of the <see cref="IActionResult"/>.
        /// </summary>
        public static object GetValue(this IActionResult result)
        {
            return result?.GetType().GetProperty("Value")?.GetValue(result);
        }
    }
}