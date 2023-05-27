using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdvancedSharpAdbClient.Extensions
{
    /// <summary>
    /// Provides extension methods for the <see cref="String"/> class.
    /// </summary>
    internal static class StringExtensions
    {
        /// <summary>
        /// Checks if a string is not null or empty.
        /// </summary>
        /// <param name="value">The string to check.</param>
        /// <returns>True if the string is not null or empty, false otherwise.</returns>
        public static bool IsNotNullOrEmpty(this string value)
        {
            return !string.IsNullOrEmpty(value);
        }

        /// <summary>
        /// Checks if a string is not null or whitespace.
        /// </summary>
        /// <param name="value">The string to check.</param>
        /// <returns>True if the string is not null or whitespace, false otherwise.</returns>
        public static bool IsNotNullOrWhiteSpace(this string value)
        {
            return !string.IsNullOrWhiteSpace(value);
        }
    }
}
