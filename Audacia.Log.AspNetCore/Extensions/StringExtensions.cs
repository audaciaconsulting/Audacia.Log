using System;
using System.Collections.Generic;
using System.Linq;

namespace Audacia.Log.AspNetCore.Extensions;

/// <summary>
/// Extensions for <see cref="string"/>.
/// </summary>
internal static class StringExtensions
{
    /// <summary>
    /// Applies a case insensitive check for a partial match against a single string.
    /// </summary>
    /// <param name="source">String to see if has expected.</param>
    /// <param name="expected">String to check within source.</param>
    /// <returns>true if the string partially matches.</returns>
    private static bool ContainsStringCaseInsensitive(this string source, string expected)
    {
        return source.IndexOf(expected, StringComparison.InvariantCultureIgnoreCase) >= 0;
    }

    /// <summary>
    /// Applies a case insensitive check for a partial match against multiple strings.
    /// </summary>
    /// <param name="source">String to see if has expected.</param>
    /// <param name="expectedCollection">Collection of strings to check within source.</param>
    /// <returns>true if any string partially matches.</returns>
    public static bool ContainsStringCaseInsensitive(this string source, ICollection<string> expectedCollection)
    {
        return expectedCollection.Any(expectedItem => ContainsStringCaseInsensitive(source, expectedItem));
    }
}