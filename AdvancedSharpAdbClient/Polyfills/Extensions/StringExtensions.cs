// <copyright file="StringExtensions.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace AdvancedSharpAdbClient.Polyfills
{
    /// <summary>
    /// Provides extension methods for the <see cref="string"/> class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal static class StringExtensions
    {
        /// <summary>
        /// Indicates whether a specified string is <see langword="null"/>, empty, or consists only of white-space characters.
        /// </summary>
        /// <param name="value">The string to test.</param>
        /// <returns><see langword="true"/> if the <paramref name="value"/> parameter is <see langword="null"/> or
        /// <see cref="string.Empty"/>, or if <paramref name="value"/> consists exclusively of white-space characters.</returns>
        public static bool IsNullOrWhiteSpace(string? value)
        {
#if NETFRAMEWORK && !NET40_OR_GREATER
            if (value == null)
            {
                return true;
            }

            for (int i = 0; i < value.Length; i++)
            {
                if (!char.IsWhiteSpace(value[i]))
                {
                    return false;
                }
            }

            return true;
#else
            return string.IsNullOrWhiteSpace(value);
#endif
        }

#if !HAS_FULLSTRING
        /// <summary>
        /// Returns a value indicating whether a specified string occurs within this string, using the specified comparison rules.
        /// </summary>
        /// <param name="text">A sequence in which to locate a value.</param>
        /// <param name="value">The string to seek.</param>
        /// <param name="comparisonType">One of the enumeration values that specifies the rules to use in the comparison.</param>
        /// <returns><see langword="true"/> if the <paramref name="value"/> parameter occurs within this string,
        /// or if <paramref name="value"/> is the empty string (""); otherwise, <see langword="false"/>.</returns>
        public static bool Contains(this string text, string value, StringComparison comparisonType) =>
            text.IndexOf(value, comparisonType) != -1;

        /// <summary>
        /// Splits a string into substrings based on a specified delimiting character and, optionally, options.
        /// </summary>
        /// <param name="text">The string to split.</param>
        /// <param name="separator">A character that delimits the substrings in this string.</param>
        /// <param name="options">A bitwise combination of the enumeration values that specifies whether to trim substrings and include empty substrings.</param>
        /// <returns>An array whose elements contain the substrings from this instance that are delimited by <paramref name="separator"/>.</returns>
        public static string[] Split(this string text, char separator, StringSplitOptions options = StringSplitOptions.None) =>
            text.Split(new[] { separator }, options);

        /// <summary>
        /// Splits a string into a maximum number of substrings based on a specified delimiting
        /// character and, optionally, options. Splits a string into a maximum number of
        /// substrings based on the provided character separator, optionally omitting empty
        /// substrings from the result.
        /// </summary>
        /// <param name="text">The string to split.</param>
        /// <param name="separator">A character that delimits the substrings in this string.</param>
        /// <param name="count">The maximum number of elements expected in the array.</param>
        /// <param name="options">A bitwise combination of the enumeration values that specifies whether to trim substrings and include empty substrings.</param>
        /// <returns>An array that contains at most count substrings from this instance that are delimited by <paramref name="separator"/>.</returns>
        public static string[] Split(this string text, char separator, int count, StringSplitOptions options = StringSplitOptions.None) =>
            text.Split(new[] { separator }, count, options);

        /// <summary>
        /// Determines whether this string instance starts with the specified character.
        /// </summary>
        /// <param name="text">A sequence in which to locate a value.</param>
        /// <param name="value">The character to compare.</param>
        /// <returns><see langword="true"/> if <paramref name="value"/> matches the beginning of this string; otherwise, <see langword="false"/>.</returns>
        public static bool StartsWith(this string text, char value) => text.StartsWith(new string([value]));

        /// <summary>
        /// Determines whether the end of this string instance matches the specified character.
        /// </summary>
        /// <param name="text">A sequence in which to locate a value.</param>
        /// <param name="value">The character to compare to the character at the end of this instance.</param>
        /// <returns><see langword="true"/> if <paramref name="value"/> matches the end of this instance; otherwise, <see langword="false"/>.</returns>
        public static bool EndsWith(this string text, char value) => text.EndsWith(new string([value]));
#endif

        /// <summary>
        /// Concatenates the string representations of an array of objects, using the specified separator between each member.
        /// </summary>
        /// <param name="separator">The string to use as a separator. <paramref name="separator"/> is included
        /// in the returned string only if <paramref name="values"/> has more than one element.</param>
        /// <param name="values">An array of objects whose string representations will be concatenated.</param>
        /// <returns>A string that consists of the elements of <paramref name="values"/> delimited by the
        /// <paramref name="separator"/> string.<para>-or-</para><see cref="string.Empty"/> if values has zero elements.</returns>
        public static string Join(char separator, params object?[] values) =>
#if HAS_FULLSTRING
            string.Join(separator, values);
#else
            Join(new string([separator]), values);
#endif

        /// <summary>
        /// Concatenates an array of strings, using the specified separator between each member.
        /// </summary>
        /// <param name="separator">The string to use as a separator. <paramref name="separator"/> is included
        /// in the returned string only if <paramref name="value"/> has more than one element.</param>
        /// <param name="value">An array of strings to concatenate.</param>
        /// <returns>A string that consists of the elements of <paramref name="value"/> delimited by the
        /// <paramref name="separator"/> string.<para>-or-</para><see cref="string.Empty"/> if values has zero elements.</returns>
        public static string Join(char separator, params string?[] value) =>
#if HAS_FULLSTRING
            string.Join(separator, value);
#else
            string.Join(new string([separator]), value);
#endif

        /// <summary>
        /// Concatenates the elements of an object array, using the specified separator between each element.
        /// </summary>
        /// <param name="separator">The string to use as a separator. <paramref name="separator"/> is included
        /// in the returned string only if <paramref name="values"/> has more than one element.</param>
        /// <param name="values">An array that contains the elements to concatenate.</param>
        /// <returns>A string that consists of the elements of <paramref name="values"/> delimited by the
        /// <paramref name="separator"/> string.<para>-or-</para><see cref="string.Empty"/> if values has zero elements.</returns>
        public static string Join(string? separator, params object?[] values)
        {
#if NETFRAMEWORK && !NET40_OR_GREATER
            ExceptionExtensions.ThrowIfNull(values);

            if (values.Length == 0 || values[0] == null)
            {
                return string.Empty;
            }

            separator ??= string.Empty;

            StringBuilder stringBuilder = new();
            string? text = values[0]?.ToString();
            if (text != null)
            {
                _ = stringBuilder.Append(text);
            }

            for (int i = 1; i < values.Length; i++)
            {
                _ = stringBuilder.Append(separator);
                if (values[i] != null)
                {
                    text = values[i]?.ToString();
                    if (text != null)
                    {
                        _ = stringBuilder.Append(text);
                    }
                }
            }

            return stringBuilder.ToString();
#else
            return string.Join(separator, values);
#endif
        }

        /// <summary>
        /// Concatenates the members of a collection, using the specified separator between each member.
        /// </summary>
        /// <typeparam name="T">The type of the members of values.</typeparam>
        /// <param name="separator">The string to use as a separator. <paramref name="separator"/> is included
        /// in the returned string only if <paramref name="values"/> has more than one element.</param>
        /// <param name="values">A collection that contains the objects to concatenate.</param>
        /// <returns>A string that consists of the elements of <paramref name="values"/> delimited by the
        /// <paramref name="separator"/> string.<para>-or-</para><see cref="string.Empty"/> if values has zero elements.</returns>
        public static string Join<T>(string? separator, IEnumerable<T> values)
        {
#if NETFRAMEWORK && !NET40_OR_GREATER
            ExceptionExtensions.ThrowIfNull(values);

            separator ??= string.Empty;

            using IEnumerator<T> enumerator = values.GetEnumerator();

            if (!enumerator.MoveNext())
            {
                return string.Empty;
            }

            StringBuilder stringBuilder = new();
            if (enumerator.Current != null)
            {
                string? text = enumerator.Current.ToString();
                if (text != null)
                {
                    stringBuilder.Append(text);
                }

            }

            while (enumerator.MoveNext())
            {
                stringBuilder.Append(separator);
                if (enumerator.Current != null)
                {
                    string? text = enumerator.Current.ToString();
                    if (text != null)
                    {
                        stringBuilder.Append(text);
                    }
                }
            }

            return stringBuilder.ToString();
#else
            return string.Join(separator, values);
#endif
        }

        /// <summary>
        /// Concatenates the members of a constructed <see cref="IEnumerable{T}"/> collection of type <see cref="string"/>,
        /// using the specified separator between each member.
        /// </summary>
        /// <param name="separator">The string to use as a separator. <paramref name="separator"/> is included
        /// in the returned string only if <paramref name="values"/> has more than one element.</param>
        /// <param name="values">A collection that contains the strings to concatenate.</param>
        /// <returns>A string that consists of the elements of <paramref name="values"/> delimited by the
        /// <paramref name="separator"/> string.<para>-or-</para><see cref="string.Empty"/> if values has zero elements.</returns>
        public static string Join(string? separator, IEnumerable<string?> values)
        {
#if NETFRAMEWORK && !NET40_OR_GREATER
            ExceptionExtensions.ThrowIfNull(values);

            separator ??= string.Empty;

            using IEnumerator<string?> enumerator = values.GetEnumerator();
            if (!enumerator.MoveNext())
            {
                return string.Empty;
            }

            StringBuilder result = new();
            if (enumerator.Current != null)
            {
                _ = result.Append(enumerator.Current);
            }

            while (enumerator.MoveNext())
            {
                _ = result.Append(separator);
                if (enumerator.Current != null)
                {
                    _ = result.Append(enumerator.Current);
                }
            }
            return result.ToString();
#else
            return string.Join(separator, values);
#endif
        }
    }
}
