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
        /// Returns true if object is a struct that doesn't implement ToString().
        /// </summary>
        public static bool IsNonDisplayableStruct(this object data)
        {
            // ref: https://docs.microsoft.com/en-us/dotnet/api/system.valuetype?view=netcore-3.1
            // Things like Guids and Datetimes are structs which don't need to be redacted.
            // This is to cover redaction of structs created by Audacia developers.
            var type = data.GetType();
            var isValueType = type.IsValueType && !type.IsPrimitive && !type.IsEnum;
            var cannotBeDisplayedAsString = data.ToString() == type.FullName;
            var hasPublicProperties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Any();
            var hasPublicFields = type.GetFields(BindingFlags.Public | BindingFlags.Instance).Any();

            return isValueType && cannotBeDisplayedAsString && hasPublicProperties && hasPublicFields;
        }

        /// <summary>
        /// Gets the Key from any generic dictionary item.
        /// </summary>
        public static string GetDictionaryKey(this object item)
        {
            var method = typeof(KeyValuePair<,>).GetProperty("Key").GetGetMethod();
            return method.Invoke(item, null).ToString();
        }

        /// <summary>
        /// Gets the Value from any generic dictionary item.
        /// </summary>
        public static object GetDictionaryValue(this object item)
        {
            var method = typeof(KeyValuePair<,>).GetProperty("Value").GetGetMethod();
            return method.Invoke(item, null);
        }
    }
}