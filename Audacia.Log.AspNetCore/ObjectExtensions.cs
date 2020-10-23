using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Audacia.Log.AspNetCore
{
    internal static class ObjectExtensions
    {
        /// <summary>
        /// Gets the Key from any generic dictionary item.
        /// </summary>
        public static string GetDictionaryKey(this object item)
        {
            var method = typeof(KeyValuePair<,>)
#pragma warning disable REFL016 // Use nameof.
                .GetProperty("Key", BindingFlags.Public | BindingFlags.Instance)
#pragma warning restore REFL016 // Use nameof.
                .GetGetMethod();
            return method.Invoke(item, null).ToString();
        }

        /// <summary>
        /// Gets the Value from any generic dictionary item.
        /// </summary>
        public static object GetDictionaryValue(this object item)
        {
            var method = typeof(KeyValuePair<,>)
#pragma warning disable REFL016 // Use nameof.
                .GetProperty("Value", BindingFlags.Public | BindingFlags.Instance)
#pragma warning restore REFL016 // Use nameof.
                .GetGetMethod();
            return method.Invoke(item, null);
        }
    }
}