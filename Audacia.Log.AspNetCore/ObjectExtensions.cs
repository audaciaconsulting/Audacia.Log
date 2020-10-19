using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Audacia.Log.AspNetCore
{
    internal static class ObjectExtensions
    {
        /// <summary>
        /// Returns true if object is a class.
        /// </summary>
        public static bool IsClass(this object data)
        {
            return data.GetType().IsClass && !(data is string);
        }

        /// <summary>
        /// Returns true if object is a dictionary.
        /// </summary>
        public static bool IsDictionary(this object data)
        {
            var dictionaryInterfaces = new[]
            {
                typeof(IDictionary<,>),
                typeof(IDictionary),
                typeof(IReadOnlyDictionary<,>),
            };
            var dataType = data.GetType();
            return dataType.IsGenericType &&
                   dictionaryInterfaces.Any(dictionaryType => dictionaryType
                       .IsAssignableFrom(dataType.GetGenericTypeDefinition()));
        }

        /// <summary>
        /// Returns true if object is a struct.
        /// </summary>
        public static bool IsStruct(this object data)
        {
            var type = data.GetType();
            return type.IsValueType && !type.IsPrimitive && !type.IsEnum;
        }

        public static string GetDictionaryKey(this object item)
        {
            var method = typeof(KeyValuePair<,>).GetProperty("Key").GetGetMethod();
            return method.Invoke(item, null).ToString();
        }

        public static object GetDictionaryValue(this object item)
        {
            var method = typeof(KeyValuePair<,>).GetProperty("Value").GetGetMethod();
            return method.Invoke(item, null);
        }
    }
}