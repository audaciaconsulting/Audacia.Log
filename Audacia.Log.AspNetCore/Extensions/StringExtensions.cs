using System;
using System.Collections.Generic;
using System.Linq;

namespace Audacia.Log.AspNetCore.Extensions
{
    internal static class StringExtensions
    {
        /// <summary>
        /// Applies a case insensitive check for a partial match against a single string.
        /// </summary>
        /// <param name="source">the string to test.</param>
        /// <param name="expected">the string to expect.</param>
        /// <returns>true if the string partially matches.</returns>
        private static bool ContainsStringCaseInsensitive(this string source, string expected)
        {
            return source.IndexOf(expected, StringComparison.InvariantCultureIgnoreCase) >= 0;
        }

        /// <summary>
        /// Applies a case insensitive check for a partial match against multiple strings.
        /// </summary>
        /// <param name="source">the string to test.</param>
        /// <param name="expectedCollection">the strings to expect.</param>
        /// <returns>true if any string partially matches.</returns>
        public static bool ContainsStringCaseInsensitive(this string source, ICollection<string> expectedCollection)
        {
            return expectedCollection.Any(expectedItem => ContainsStringCaseInsensitive(source, expectedItem));
        }
    }
}