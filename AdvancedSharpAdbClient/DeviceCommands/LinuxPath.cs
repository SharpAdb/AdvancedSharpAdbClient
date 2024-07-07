// <copyright file="LinuxPath.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace AdvancedSharpAdbClient.DeviceCommands
{
    /// <summary>
    /// Just like <see cref="Path"/>, except it is geared for Linux.
    /// </summary>
    public static partial class LinuxPath
    {
        /// <summary>
        /// The directory separator character.
        /// </summary>
        public const char DirectorySeparatorChar = '/';

        /// <summary>
        /// Pattern to escape filenames for shell command consumption.
        /// </summary>
        private const string EscapePattern = """([\\()*+?"'#/\s])""";

        /// <summary>
        /// The <see cref="Array"/> of <see cref="char"/>s which are invalid in a path.
        /// </summary>
        public static readonly char[] InvalidCharacters = ['|', '\\', '?', '*', '<', '\"', ':', '>'];

        /// <summary>
        /// Combine the specified paths to form one path.
        /// </summary>
        /// <param name="paths">The paths.</param>
        /// <returns>The combined path.</returns>
        public static string Combine(params string[] paths)
        {
            ExceptionExtensions.ThrowIfNull(paths);

            int capacity = 0;
            int num2 = 0;
            for (int i = 0; i < paths.Length; i++)
            {
                if (paths[i] == null)
                {
                    throw new ArgumentNullException(nameof(paths));
                }

                if (paths[i].Length != 0)
                {
                    CheckInvalidPathChars(paths[i]);
                    if (IsPathRooted(paths[i]))
                    {
                        num2 = i;
                        capacity = paths[i].Length;
                    }
                    else
                    {
                        capacity += paths[i].Length;
                    }
                    char ch = paths[i][^1];
                    if (ch != DirectorySeparatorChar)
                    {
                        capacity++;
                    }
                }
            }

            StringBuilder builder = new(capacity);
            for (int j = num2; j < paths.Length; j++)
            {
                if (paths[j].Length != 0)
                {
                    if (builder.Length == 0)
                    {
                        _ = builder.Append(FixupPath(paths[j]));
                    }
                    else
                    {
                        char ch2 = builder[^1];
                        if (ch2 != DirectorySeparatorChar)
                        {
                            _ = builder.Append(DirectorySeparatorChar);
                        }
                        _ = builder.Append(paths[j]);
                    }
                }
            }

            return builder.ToString();
        }

        /// <summary>
        /// Returns the directory information for the specified path string.
        /// </summary>
        /// <returns>A <see cref="string"></see> containing directory information for path,
        /// or null if path denotes a root directory, is the empty string (""), or is null.
        /// Returns <see cref="string.Empty"></see> if path does not contain directory information.</returns>
        /// <param name="path">The path of a file or directory. </param>
        /// <exception cref="ArgumentException">The path parameter contains invalid characters, is empty,
        /// or contains only white spaces, or contains a wildcard character. </exception>
        /// <exception cref="PathTooLongException">The path parameter is longer
        /// than the system-defined maximum length.</exception>
        /// <filterpriority>1</filterpriority>
        [return: NotNullIfNotNull(nameof(path))]
        public static string? GetDirectoryName(string? path)
        {
            if (path != null)
            {
                CheckInvalidPathChars(path);

                string tpath = path;
                if (tpath.Length > 1)
                {
                    if (tpath.EndsWith(DirectorySeparatorChar))
                    {
                        return tpath;
                    }

                    tpath = tpath[..(tpath.LastIndexOf(DirectorySeparatorChar) + 1)];

                    return FixupPath(tpath);
                }
                else if (tpath.Length == 1)
                {
                    return new string([DirectorySeparatorChar]);
                }
            }

            return null;
        }

        /// <summary>
        /// Returns the file name and extension of the specified path string.
        /// </summary>
        /// <returns>A <see cref="string"/> consisting of the characters after the last directory character in path.
        /// If the last character of path is a directory or volume separator character,
        /// this method returns <see cref="string.Empty"/>. If path is null, this method returns null.</returns>
        /// <param name="path">The path string from which to obtain the file name and extension. </param>
        /// <exception cref="ArgumentException">path contains one or more of the invalid characters
        /// defined in <see cref="InvalidCharacters"/>, or contains a wildcard character. </exception>
        /// <filterpriority>1</filterpriority>
        [return: NotNullIfNotNull(nameof(path))]
        public static string? GetFileName(string path)
        {
            if (path != null)
            {
                CheckInvalidPathChars(path);
                int length = path.Length;
                int num2 = length;
                while (--num2 >= 0)
                {
                    char ch = path[num2];
                    if (ch == DirectorySeparatorChar)
                    {
                        return path.Substring(num2 + 1, length - num2 - 1);
                    }
                }
            }

            return path;
        }

        /// <summary>
        /// Gets a value indicating whether the specified path string contains absolute or relative path information.
        /// </summary>
        /// <returns>true if path contains an absolute path; otherwise, false.</returns>
        /// <param name="path">The path to test.</param>
        /// <exception cref="ArgumentException">path contains one or more of the invalid characters
        /// defined in <see cref="InvalidCharacters"/>, or contains a wildcard character. </exception>
        /// <filterpriority>1</filterpriority>
        public static bool IsPathRooted(string path)
        {
            if (path != null)
            {
                CheckInvalidPathChars(path);
                int length = path.Length;
                if ((length > 0 && (path[0] == DirectorySeparatorChar)) ||
                    length == 1)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns an escaped version of the entry name.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>The entry name.</returns>
        public static string Escape(string path) => EscapeRegex().Replace(path, new MatchEvaluator(m => m.Result(@"\\$1")));

        /// <summary>
        /// Quotes the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>The quoted path.</returns>
        public static string Quote(string path) => path.Contains(' ') ? $"\"{path}\"" : path;

        /// <summary>
        /// Checks the invalid path chars.
        /// </summary>
        /// <param name="path">The path.</param>
        internal static void CheckInvalidPathChars(string path)
        {
            ExceptionExtensions.ThrowIfNull(path);

            if (path.ToCharArray().Any(c => c < 0x20 || InvalidCharacters.Contains(c)))
            {
                throw new ArgumentException("Path contains invalid characters");
            }
        }

        /// <summary>
        /// Fixups the path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>The fixup path</returns>
        private static string FixupPath(string path)
        {
            string sb = path.Replace(Path.DirectorySeparatorChar, DirectorySeparatorChar);

            if (sb != "." && !sb.StartsWith(DirectorySeparatorChar))
            {
                sb = $".{DirectorySeparatorChar}{sb}";
            }

            if (!sb.EndsWith(DirectorySeparatorChar))
            {
                sb = $"{sb}{DirectorySeparatorChar}";
            }

            sb = sb.Replace("//", new string([DirectorySeparatorChar]));

            return sb;
        }

#if NET7_0_OR_GREATER
        [GeneratedRegex(EscapePattern)]
        private static partial Regex EscapeRegex();
#else
        /// <summary>
        /// Gets a <see cref="Regex"/> to escape filenames for shell command consumption.
        /// </summary>
        /// <returns>The <see cref="Regex"/> to escape filenames for shell command consumption.</returns>
        private static Regex EscapeRegex() => new(EscapePattern);
#endif
    }
}
