using System;
using System.Reflection;

namespace Audacia.Log.AspNetCore.Extensions;

/// <summary>
/// Extensions for <see cref="object"/>.
/// </summary>
internal static class ObjectExtensions
{
    /// <summary>
    /// Gets the Key from any generic dictionary item.
    /// </summary>
    /// <param name="item">Item value within a dictionary.</param>
    /// <exception cref="InvalidCastException">Thrown when unable to cast type of item to a key value pair.</exception>
    /// <returns>Key value of object within the dictionary.</returns>
    public static string GetDictionaryKey(this object item)
    {
        var type = item.GetType();
        if (!type.IsKeyValuePair())
        {
            throw new InvalidCastException($"{type.FullName} is not a KeyValuePair.");
        }

        var method = type
#pragma warning disable REFL009 // The referenced member is not known to exist.
            .GetProperty("Key", BindingFlags.Public | BindingFlags.Instance)
#pragma warning restore REFL009 // The referenced member is not known to exist.
            .GetGetMethod();
        return method.Invoke(item, null).ToString();
    }

    /// <summary>
    /// Gets the Value from any generic dictionary item.
    /// </summary>
    /// <param name="item">Item key within a dictionary.</param>
    /// <exception cref="InvalidCastException">Thrown when unable to cast type of item to a key value pair.</exception>
    /// <returns>Get object value from Dictionary based on item representing the key.</returns>
    public static object GetDictionaryValue(this object item)
    {
        var type = item.GetType();
        if (!type.IsKeyValuePair())
        {
            throw new InvalidCastException($"{type.FullName} is not a KeyValuePair.");
        }

        var method = type
#pragma warning disable REFL009 // The referenced member is not known to exist.
            .GetProperty("Value", BindingFlags.Public | BindingFlags.Instance)
#pragma warning restore REFL009 // The referenced member is not known to exist.
            .GetGetMethod();
        return method.Invoke(item, null);
    }
}