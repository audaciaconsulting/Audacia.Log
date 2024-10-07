using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Audacia.Log.AspNetCore.Extensions;

/// <summary>
/// Extensions for <see cref="Type"/>.
/// </summary>
internal static class TypeExtensions
{
    /// <summary>
    /// Returns true if object is a class.
    /// </summary>
    /// <param name="type">The type to check if a class.</param>
    /// <returns>Whether the instance is of type class.</returns>
    public static bool IsClassObject(this Type type)
    {
        return type.IsClass &&
            !type.IsString() &&
            !type.IsList() &&
            !type.IsDictionary();
    }

    /// <summary>
    /// Returns true if object is a dictionary.
    /// </summary>
    /// <param name="type">The type to check if a dictionary.</param>
    /// <returns>Whether the instance is of type dictionary.</returns>
    public static bool IsDictionary(this Type type)
    {
        var dictionaryInterfaces = new[]
        {
            typeof(IDictionary<,>),
            typeof(IDictionary),
            typeof(IReadOnlyDictionary<,>)
        };
        return type.IsGenericType &&
               dictionaryInterfaces.Any(dictionaryType => dictionaryType
                   .IsAssignableFrom(type.GetGenericTypeDefinition()));
    }

    /// <summary>
    /// Returns true if the object is a KeyValuePair.
    /// </summary>
    /// <param name="type">The type to check if a key value pair.</param>
    /// <returns>Whether the instance is of type key value pair.</returns>
    public static bool IsKeyValuePair(this Type type)
    {
        return type.IsGenericType &&
            type.GetGenericTypeDefinition()
                .IsAssignableFrom(typeof(KeyValuePair<,>));
    }

    /// <summary>
    /// Returns true if the object implements <see cref="IEnumerable"/>.
    /// </summary>
    /// <param name="type">The type to check if a list.</param>
    /// <returns>Whether the instance is of type list.</returns>
    public static bool IsList(this Type type)
    {
        return typeof(IEnumerable).IsAssignableFrom(type) &&
            !type.IsString();
    }

    /// <summary>
    /// Returns true if object is a struct that doesn't implement ToString().
    /// </summary>
    /// <param name="type">The type to check if a non displayable struct.</param>
    /// <param name="data">Object data.</param>
    /// <returns>Whether the instance is of type a non displayable struct.</returns>
    public static bool IsNonDisplayableStruct(this Type type, object data)
    {
        // ref: https://docs.microsoft.com/en-us/dotnet/api/system.valuetype?view=netcore-3.1
        // Things like Guids and Datetimes are structs which don't need to be redacted.
        // This is to cover redaction of structs created by Audacia developers.
        var isValueType = type is { IsValueType: true, IsPrimitive: false, IsEnum: false };
        var cannotBeDisplayedAsString = data.ToString() == type.FullName;
        var hasPublicProperties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Any();
        var hasPublicFields = type.GetFields(BindingFlags.Public | BindingFlags.Instance).Any();

        return isValueType && cannotBeDisplayedAsString && hasPublicProperties && hasPublicFields;
    }

    /// <summary>
    /// Returns true if the object is a string.
    /// </summary>
    /// <param name="type">the string to test.</param>
    /// <returns>Whether the instance is of type string.</returns>
    public static bool IsString(this Type type)
    {
        return typeof(string) == type;
    }
}