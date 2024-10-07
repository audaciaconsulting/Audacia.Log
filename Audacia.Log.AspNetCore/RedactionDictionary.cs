using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using Audacia.CodeAnalysis.Analyzers.Helpers.MethodLength;
using Audacia.CodeAnalysis.Analyzers.Helpers.ParameterCount;
using Audacia.Log.AspNetCore.Extensions;

namespace Audacia.Log.AspNetCore;

/// <summary>
/// Creates a dictionary of key value pairs, excluding parameters marked as personal information.
/// </summary>
public sealed class RedactionDictionary : Dictionary<string, object>
{
    private ICollection<string> ExcludedProperties { get; }

    private int MaxDepth { get; }

    /// <summary>
    /// Creates a dictionary of key value pairs, excluding parameters marked as personal information.
    /// </summary>
    /// <param name="context">Dictionary of all arguments on the action context.</param>
    /// <param name="maxDepth">Max object depth to inspect before stopping.</param>
    /// <param name="excludedProperties">Parameter names where content should be redacted.</param>
    /// <exception cref="ArgumentNullException">Thrown when excluded properties has a value of null.</exception>
    public RedactionDictionary(
        IDictionary<string, object> context,
        int maxDepth,
        ICollection<string> excludedProperties)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (maxDepth < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxDepth), maxDepth, "MaxDepth must be zero or above");
        }

        MaxDepth = maxDepth;
        ExcludedProperties = excludedProperties ?? throw new ArgumentNullException(nameof(excludedProperties));

        foreach (var argument in context)
        {
            IncludeData(argument.Key, argument.Value, 0, this);
        }
    }

    /// <summary>
    /// Converts dictionary content as a json object string.
    /// </summary>
    /// <returns>Returns dictionary content as a json object string.</returns>
    public override string ToString()
    {
        return AppendDictionary(new StringBuilder(), this).ToString();
    }

    [MaxMethodLength(13, Justification = "Method is concise and splitting out would not improve readability")]
    private void IncludeData(
        string name,
        object data,
        int depth,
        IDictionary<string, object> parent)
    {
        // Skip logging of null data
        // Redact when parameter names contain excluded words
        if (depth >= MaxDepth || data == null || name.ContainsStringCaseInsensitive(ExcludedProperties))
        {
            return;
        }

        var type = data.GetType();

        // Filter insecure keys from dictionaries
        if (type.IsDictionary())
        {
            IncludeDictionary(name, (data as IEnumerable)!, depth, parent);
            return;
        }

        // Filter insecure objects from lists
        if (type.IsList())
        {
            IncludeList(name, (data as IEnumerable)!, depth, parent);
            return;
        }

        // Filter insecure nested parameters from classes / structs
        if (type.IsClassObject() || type.IsNonDisplayableStruct(data))
        {
            IncludeObject(
                name,
                data,
                type,
                depth,
                parent);
            return;
        }

        // Include parameter name and value on the parent object's dictionary
        parent.Add(name, data);
    }

    private void IncludeDictionary(
        string name,
        IEnumerable data,
        int depth,
        IDictionary<string, object> parent)
    {
        var objectData = new Dictionary<string, object>();
        foreach (var entry in data)
        {
            var key = entry.GetDictionaryKey();
            var value = entry.GetDictionaryValue();
            IncludeData(key, value, depth + 1, objectData);
        }

        // Append data to the parent object's dictionary
        if (objectData.Count > 0)
        {
            parent.Add(name, objectData);
        }
    }

    private void IncludeList(
        string name,
        IEnumerable data,
        int depth,
        IDictionary<string, object> parent)
    {
        var objectData = new Dictionary<string, object>();

        var index = 0;
        var enumerator = data.GetEnumerator();
        while (enumerator.MoveNext())
        {
            IncludeData($"{index}", enumerator.Current, depth + 1, objectData);
            index++;
        }

        // Append data to the parent object's dictionary
        if (objectData.Count > 0)
        {
            parent.Add(name, objectData.Values);
        }
    }

    [MaxParameterCount(5)]
    private void IncludeObject(
        string name,
        object data,
        Type type,
        int depth,
        IDictionary<string, object> parent)
    {
        var objectData = new Dictionary<string, object>();

        // Append safe fields to objectData
        foreach (var fieldInfo in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
        {
            IncludeData(fieldInfo.Name, fieldInfo.GetValue(data), depth + 1, objectData);
        }

        // Append safe properties to objectData
        foreach (var propertyInfo in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            IncludeData(propertyInfo.Name, propertyInfo.GetValue(data), depth + 1, objectData);
        }

        // Append objectData to the parent object's dictionary
        if (objectData.Count > 0)
        {
            parent.Add(name, objectData);
        }
    }

    [MaxMethodLength(15)]
    private static StringBuilder AppendDictionary(
        StringBuilder builder,
        IDictionary<string, object> dictionary)
    {
        builder.Append("{");

        var lastIndex = dictionary.Count - 1;
        for (var index = 0; index < dictionary.Count; index++)
        {
            var entry = dictionary.ElementAt(index);

            builder.AppendFormat(CultureInfo.InvariantCulture, " \"{0}\": ", entry.Key);

            if (entry.Value is Dictionary<string, object> nestedObject)
            {
                AppendDictionary(builder, nestedObject);
            }
            else if (entry.Value is ValueCollection list)
            {
                AppendValueCollection(builder, list);
            }
            else
            {
                AppendValue(builder, entry.Value.ToString());
            }

            if (index < lastIndex)
            {
                builder.Append(",");
            }
            else
            {
                builder.Append(" ");
            }
        }

        builder.Append("}");

        return builder;
    }

    private static void AppendValue(
        StringBuilder builder,
        string value)
    {
        // Escape double quotes before writing to value
        builder.AppendFormat(CultureInfo.InvariantCulture, "\"{0}\"", value.Replace("\"", "\\\""));
    }

    [MaxMethodLength(14)]
    private static void AppendValueCollection(
        StringBuilder builder,
        ValueCollection valueCollection)
    {
        builder.Append("[");

        var lastIndex = valueCollection.Count - 1;
        for (var index = 0; index < valueCollection.Count; index++)
        {
            builder.Append(" ");

            var value = valueCollection.ElementAt(index);

            if (value is Dictionary<string, object> dictionary)
            {
                AppendDictionary(builder, dictionary);
            }
            else if (value is ValueCollection list)
            {
                AppendValueCollection(builder, list);
            }
            else
            {
                AppendValue(builder, value.ToString());
            }

            if (index < lastIndex)
            {
                builder.Append(",");
            }
            else
            {
                builder.Append(" ");
            }
        }

        builder.Append("]");
    }
}