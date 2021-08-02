﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Http;

namespace Audacia.Log.AspNetCore
{
    /// <summary>
    /// Creates a dictionary of form values on the action, excluding parameters marked as personal information.
    /// </summary>
    public sealed class ActionArgumentDictionary : Dictionary<string, object>
    {
        private ICollection<string> ExcludedArguments { get; }

        private int MaxDepth { get; }

        /// <summary>
        /// Creates a dictionary of form values on the action, excluding parameters marked as personal information.
        /// </summary>
        /// <param name="actionArgumentContext">Dictionary of all arguments on the action context</param>
        /// <param name="maxDepth">Max object depth to inspect before stopping</param>
        /// <param name="excludedArguments">Parameter names where content should be redacted</param>
        public ActionArgumentDictionary(IDictionary<string, object> actionArgumentContext, int maxDepth, ICollection<string> excludedArguments)
        {
            if (actionArgumentContext == null)
            {
                throw new ArgumentNullException(nameof(actionArgumentContext));
            }

            if (maxDepth < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxDepth), maxDepth, "MaxDepth must be zero or above");
            }

            if (excludedArguments == null)
            {
                throw new ArgumentNullException(nameof(excludedArguments));
            }

            MaxDepth = maxDepth;
            ExcludedArguments = excludedArguments;

            foreach (var argument in actionArgumentContext)
            {
                IncludeData(argument.Key, argument.Value, 0, this);
            }
        }

        /// <summary>
        /// Creates a dictionary of form values on the action, excluding parameters marked as personal information.
        /// </summary>
        /// <param name="formCollection">The form collection on the request</param>
        /// <param name="maxDepth">Max object depth to inspect before stopping</param>
        /// <param name="excludedArguments">Parameter names where content should be redacted</param>
        public ActionArgumentDictionary(IFormCollection formCollection, int maxDepth, ICollection<string> excludedArguments)
        {
            if (formCollection == null)
            {
                throw new ArgumentNullException(nameof(formCollection));
            }

            if (maxDepth < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxDepth), maxDepth, "MaxDepth must be zero or above");
            }

            if (excludedArguments == null)
            {
                throw new ArgumentNullException(nameof(excludedArguments));
            }

            MaxDepth = maxDepth;
            ExcludedArguments = excludedArguments;

            foreach (var argument in formCollection)
            {
                IncludeData(argument.Key, argument.Value, 0, this);
            }
        }

        /// <summary>
        /// Returns dictionary content as a json object string.
        /// </summary>
        public override string ToString() => DictionaryToString(this);

        private string DictionaryToString(IDictionary<string, object> dictionary)
        {
            var outputData = dictionary.Select(kv =>
            {
                if (kv.Value is IDictionary<string, object> data)
                {
                    return DictionaryToString(data);
                }

                return $"\"{kv.Key}\": \"{kv.Value}\"";
            }).ToArray();

            return $"{{ {string.Join(", ", outputData)} }}";
        }

#pragma warning disable ACL1002 // Member or local function contains too many statements
        private void IncludeData(string name, object data, int depth, IDictionary<string, object> parent)
#pragma warning restore ACL1002 // Member or local function contains too many statements
        {
            // Skip logging of null data
            // Redact when parameter names contain excluded words
            if (depth >= MaxDepth ||
                data == null ||
                name.ContainsStringCaseInsensitive(ExcludedArguments))
            {
                return;
            }

            var type = data.GetType();

            // Filter insecure keys from dictionaries
            if (type.IsDictionary())
            {
                IncludeDictionary(name, data as IEnumerable, depth, parent);
                return;
            }

            // Filter insecure objects from lists
            if (type.IsList())
            {
                IncludeList(name, data as IEnumerable, depth, parent);
                return;
            }

            // Filter insecure nested parameters from classes / structs
            if (type.IsClassObject() || type.IsNonDisplayableStruct(data))
            {
                IncludeObject(name, data, type, depth, parent);
                return;
            }

            // Include parameter name and value on the parent object's dictionary
            parent.Add(name, data);
        }

        private void IncludeDictionary(string name, IEnumerable data, int depth, IDictionary<string, object> parent)
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

        private void IncludeList(string name, IEnumerable data, int depth, IDictionary<string, object> parent)
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

#pragma warning disable ACL1003 // Signature contains too many parameters
        private void IncludeObject(string name, object data, Type type, int depth, IDictionary<string, object> parent)
#pragma warning restore ACL1003 // Signature contains too many parameters
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
    }
}
