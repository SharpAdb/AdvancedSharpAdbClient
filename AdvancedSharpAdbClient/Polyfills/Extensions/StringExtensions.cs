#if !HAS_FULLSTRING
// <copyright file="StringExtensions.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
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
        /// The extension for the <see cref="string"/> class.
        /// </summary>
        /// <param name="text">The <see cref="string"/> to extend.</param>
        extension(string text)
        {
#if NETFRAMEWORK && !NET40_OR_GREATER
            /// <summary>
            /// Indicates whether a specified string is <see langword="null"/>, empty, or consists only of white-space characters.
            /// </summary>
            /// <param name="value">The string to test.</param>
            /// <returns><see langword="true"/> if the <paramref name="value"/> parameter is <see langword="null"/> or
            /// <see cref="string.Empty"/>, or if <paramref name="value"/> consists exclusively of white-space characters.</returns>
            public static bool IsNullOrWhiteSpace([NotNullWhen(false)] string? value)
            {
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
            }
#endif

            /// <summary>
            /// Returns a value indicating whether a specified string occurs within this string, using the specified comparison rules.
            /// </summary>
            /// <param name="value">The string to seek.</param>
            /// <param name="comparisonType">One of the enumeration values that specifies the rules to use in the comparison.</param>
            /// <returns><see langword="true"/> if the <paramref name="value"/> parameter occurs within this string,
            /// or if <paramref name="value"/> is the empty string (""); otherwise, <see langword="false"/>.</returns>
            public bool Contains(string value, StringComparison comparisonType) => text.IndexOf(value, comparisonType) >= 0;

            /// <summary>
            /// Splits a string into substrings based on a specified delimiting character and, optionally, options.
            /// </summary>
            /// <param name="separator">A character that delimits the substrings in this string.</param>
            /// <param name="options">A bitwise combination of the enumeration values that specifies whether to trim substrings and include empty substrings.</param>
            /// <returns>An array whose elements contain the substrings from this instance that are delimited by <paramref name="separator"/>.</returns>
            public string[] Split(char separator, StringSplitOptions options = StringSplitOptions.None) => text.Split([separator], options);

            /// <summary>
            /// Splits a string into a maximum number of substrings based on a specified delimiting
            /// character and, optionally, options. Splits a string into a maximum number of
            /// substrings based on the provided character separator, optionally omitting empty
            /// substrings from the result.
            /// </summary>
            /// <param name="separator">A character that delimits the substrings in this string.</param>
            /// <param name="count">The maximum number of elements expected in the array.</param>
            /// <param name="options">A bitwise combination of the enumeration values that specifies whether to trim substrings and include empty substrings.</param>
            /// <returns>An array that contains at most count substrings from this instance that are delimited by <paramref name="separator"/>.</returns>
            public string[] Split(char separator, int count, StringSplitOptions options = StringSplitOptions.None) => text.Split([separator], count, options);

            /// <summary>
            /// Determines whether this string instance starts with the specified character.
            /// </summary>
            /// <param name="value">The character to compare.</param>
            /// <returns><see langword="true"/> if <paramref name="value"/> matches the beginning of this string; otherwise, <see langword="false"/>.</returns>
            public bool StartsWith(char value) => text.StartsWith(new string([value]));

            /// <summary>
            /// Determines whether the end of this string instance matches the specified character.
            /// </summary>
            /// <param name="value">The character to compare to the character at the end of this instance.</param>
            /// <returns><see langword="true"/> if <paramref name="value"/> matches the end of this instance; otherwise, <see langword="false"/>.</returns>
            public bool EndsWith(char value) => text.EndsWith(new string([value]));
            
            /// <summary>
            /// Concatenates the string representations of an array of objects, using the specified separator between each member.
            /// </summary>
            /// <param name="separator">The character to use as a separator. <paramref name="separator"/> is included
            /// in the returned string only if <paramref name="values"/> has more than one element.</param>
            /// <param name="values">An array of objects whose string representations will be concatenated.</param>
            /// <returns>A string that consists of the elements of <paramref name="values"/> delimited by the <paramref name="separator"/> character.
            /// <para>-or-</para> <see cref="string.Empty"/> if <paramref name="values"/> has zero elements.</returns>
            public static string Join(char separator, params object?[] values)
            {
                ArgumentNullException.ThrowIfNull(values);

                if (values.Length <= 0)
                {
                    return string.Empty;
                }

                string? firstString = values[0]?.ToString();

                if (values.Length == 1)
                {
                    return firstString ?? string.Empty;
                }

                StringBuilder result = new();

                result.Append(firstString);

                for (int i = 1; i < values.Length; i++)
                {
                    result.Append(separator);
                    object? value = values[i];
                    if (value != null)
                    {
                        result.Append(value.ToString());
                    }
                }

                return result.ToString();
            }
            
            /// <summary>
            /// Concatenates an array of strings, using the specified separator between each member.
            /// </summary>
            /// <param name="separator">The character to use as a separator. <paramref name="separator"/> is included
            /// in the returned string only if <paramref name="value"/> has more than one element.</param>
            /// <param name="value">An array of strings to concatenate.</param>
            /// <returns>A string that consists of the elements of <paramref name="value"/> delimited by the <paramref name="separator"/> character.
            /// <para>-or-</para> <see cref="string.Empty"/> if <paramref name="value"/> has zero elements.</returns>
            public static string Join(char separator, params string?[] value)
            {
                ArgumentNullException.ThrowIfNull(value);

                if (value.Length <= 1)
                {
                    return value.Length <= 0 ?
                        string.Empty :
                        value[0] ?? string.Empty;
                }

                return string.Join(new string([separator]), value);
            }

#if NETFRAMEWORK && !NET40_OR_GREATER
            /// <summary>
            /// Concatenates the elements of an object array, using the specified separator between each element.
            /// </summary>
            /// <param name="separator">The string to use as a separator. <paramref name="separator"/> is included in
            /// the returned string only if <paramref name="values"/> has more than one element.</param>
            /// <param name="values">An array that contains the elements to concatenate.</param>
            /// <returns>A string that consists of the elements of <paramref name="values"/> delimited by the <paramref name="separator"/> string.
            /// <para>-or-</para><see cref="string.Empty"/> if <paramref name="values"/> has zero elements.<para>-or-</para>
            /// .NET Framework only: <see cref="string.Empty"/> if the first element of <paramref name="values"/> is <see langword="null"/>.</returns>
            /// <exception cref="ArgumentNullException"><paramref name="values"/> is <see langword="null"/>.</exception>
            public static string Join(string? separator, params object?[] values)
            {
                ArgumentNullException.ThrowIfNull(values);

                if (values.Length <= 0)
                {
                    return string.Empty;
                }

                string? firstString = values[0]?.ToString();

                if (values.Length == 1)
                {
                    return firstString ?? string.Empty;
                }

                StringBuilder result = new();

                result.Append(firstString);

                for (int i = 1; i < values.Length; i++)
                {
                    result.Append(separator);
                    object? value = values[i];
                    if (value != null)
                    {
                        result.Append(value.ToString());
                    }
                }

                return result.ToString();
            }

            /// <summary>
            /// Concatenates the members of a collection, using the specified separator between each member.
            /// </summary>
            /// <typeparam name="T">The type of the members of <paramref name="values"/>.</typeparam>
            /// <param name="separator">The string to use as a separator. <paramref name="separator"/> is included in
            /// the returned string only if <paramref name="values"/> has more than one element.</param>
            /// <param name="values">A collection that contains the objects to concatenate.</param>
            /// <returns>A string that consists of the elements of <paramref name="values"/> delimited by the <paramref name="separator"/> string.
            /// <para>-or-</para><see cref="string.Empty"/> if <paramref name="values"/> has zero elements.</returns>
            /// <exception cref="ArgumentNullException"><paramref name="values"/> is <see langword="null"/>.</exception>
            public static string Join<T>(string? separator, IEnumerable<T> values)
            {
                ArgumentNullException.ThrowIfNull(values);

                if (typeof(T) == typeof(string))
                {
                    if (values is string?[] valuesArray)
                    {
                        return string.Join(separator, valuesArray);
                    }
                }

                using IEnumerator<T> e = values.GetEnumerator();
                if (!e.MoveNext())
                {
                    // If the enumerator is empty, just return an empty string.
                    return string.Empty;
                }

                if (e is IEnumerator<char> en)
                {
                    // Special-case T==char, as we can handle that case much more efficiently,
                    // and string.Concat(IEnumerable<char>) can be used as an efficient
                    // enumerable-based equivalent of new string(char[]).

                    char c = en.Current; // save the first value
                    if (!en.MoveNext())
                    {
                        // There was only one char.  Return a string from it directly.
                        return new string([c]);
                    }

                    // Create the builder, add the char we already enumerated,
                    // add the rest, and then get the resulting string.
                    StringBuilder result = new();
                    result.Append(c); // first value
                    do
                    {
                        result.Append(separator);
                        c = en.Current;
                        result.Append(c);
                    }
                    while (en.MoveNext());
                    return result.ToString();
                }
                else
                {
                    // For all other Ts, fall back to calling ToString on each and appending the resulting
                    // string to a builder.

                    string? firstString = e.Current?.ToString();  // save the first value
                    if (!e.MoveNext())
                    {
                        return firstString ?? string.Empty;
                    }

                    StringBuilder result = new();

                    result.Append(firstString);
                    do
                    {
                        result.Append(separator);
                        result.Append(e.Current?.ToString());
                    }
                    while (e.MoveNext());

                    return result.ToString();
                }
            }

            /// <summary>
            /// Concatenates the members of a constructed <see cref="IEnumerable{T}"/> collection of type <see cref="string"/>,
            /// using the specified separator between each member.
            /// </summary>
            /// <param name="separator">The string to use as a separator. <paramref name="separator"/> is included in
            /// the returned string only if values has more than one element.</param>
            /// <param name="values">A collection that contains the strings to concatenate.</param>
            /// <returns>A string that consists of the elements of <paramref name="values"/> delimited by the <paramref name="separator"/> string.
            /// <para>-or-</para><see cref="string.Empty"/> if <paramref name="values"/> has zero elements.</returns>
            /// <exception cref="ArgumentNullException"><paramref name="values"/> is <see langword="null"/>.</exception>
            public static string Join(string? separator, IEnumerable<string?> values)
            {
                ArgumentNullException.ThrowIfNull(values);

                using IEnumerator<string?> en = values.GetEnumerator();
                if (!en.MoveNext())
                {
                    return string.Empty;
                }

                string? firstValue = en.Current;

                if (!en.MoveNext())
                {
                    // Only one value available
                    return firstValue ?? string.Empty;
                }

                // Null separator and values are handled by the StringBuilder
                StringBuilder result = new();

                result.Append(firstValue);

                do
                {
                    result.Append(separator);
                    result.Append(en.Current);
                }
                while (en.MoveNext());

                return result.ToString();
            }
#endif
        }
    }
}
#endif