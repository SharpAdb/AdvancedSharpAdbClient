// <copyright file="LinuxPath.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion. All rights reserved.
// </copyright>

using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace AdvancedSharpAdbClient.DeviceCommands
{
    /// <summary>
    /// Just like System.IO.Path, except it is geared for Linux
    /// </summary>
    public static class LinuxPath
    {
        /// <summary>
        /// The directory separator character.
        /// </summary>
        public const char DirectorySeparatorChar = '/';

        /// <summary>
        /// Pattern to escape filenames for shell command consumption.
        /// </summary>
        private const string EscapePattern = "([\\\\()*+?\"'#/\\s])";

        private static readonly char[] InvalidCharacters = new char[]
        {
            '|', '\\', '?', '*', '<', '\"', ':', '>', '+', '[', ']'
        };

        /// <summary>
        /// Returns the path of the current user's temporary folder.
        /// </summary>
        /// <returns>The path to the temporary folder, ending with a <see cref="DirectorySeparatorChar"/>.</returns>
        public static string GetTempPath() => "/data/local/tmp/";

        internal static bool IsDirectorySeparator(char c) => c == DirectorySeparatorChar;

        /// <summary>
        /// Combine the specified paths to form one path
        /// </summary>
        /// <param name="paths">The paths.</param>
        /// <returns>The combined path.</returns>
        public static string Combine(params string[] paths)
        {
            if (paths == null)
            {
                throw new ArgumentNullException(nameof(paths));
            }

            int maxSize = 0;
            int firstComponent = 0;

            // We have two passes, the first calculates how large a buffer to allocate and does some precondition
            // checks on the paths passed in.  The second actually does the combination.

            for (int i = 0; i < paths.Length; i++)
            {
                if (paths[i] == null)
                {
                    throw new ArgumentNullException(nameof(paths));
                }

                if (paths[i].Length == 0)
                {
                    continue;
                }

                CheckInvalidPathChars(paths[i]);
                if (IsPathRooted(paths[i]))
                {
                    firstComponent = i;
                    maxSize = paths[i].Length;
                }
                else
                {
                    maxSize += paths[i].Length;
                }

                char ch = paths[i][paths[i].Length - 1];
                if (!IsDirectorySeparator(ch))
                {
                    maxSize++;
                }
            }

            StringBuilder builder = new StringBuilder(maxSize);
            for (int i = firstComponent; i < paths.Length; i++)
            {
                if (paths[i].Length == 0)
                {
                    continue;
                }

                if (builder.Length == 0)
                {
                    builder.Append(FixupPath(paths[i]));
                }
                else
                {
                    char ch = builder[builder.Length - 1];
                    if (!IsDirectorySeparator(ch))
                    {
                        builder.Append(DirectorySeparatorChar);
                    }

                    builder.Append(paths[i]);
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
        /// <exception cref="System.IO.PathTooLongException">The path parameter is longer
        /// than the system-defined maximum length.</exception>
        /// <filterpriority>1</filterpriority>
        public static string GetDirectoryName(string path)
        {
            if (path != null)
            {
                CheckInvalidPathChars(path);

                string tpath = path;
                if (tpath.Length > 1)
                {
                    if (tpath.EndsWith(new string(new char[] { DirectorySeparatorChar })))
                    {
                        return tpath.Substring(0, tpath.Length);
                    }

                    tpath = tpath.Substring(0, tpath.LastIndexOf(DirectorySeparatorChar) + 1);

                    return FixupPath(tpath);
                }
                else if (tpath.Length == 1)
                {
                    return new string(new char[] { DirectorySeparatorChar });
                }
            }

            return null;
        }

        /// <summary>Returns the file name and extension of the specified path string.</summary>
        /// <returns>A <see cref="string"/> consisting of the characters after the last directory character in path.
        /// If the last character of path is a directory or volume separator character,
        /// this method returns <see cref="string.Empty"/>. If path is null, this method returns null.</returns>
        /// <param name="path">The path string from which to obtain the file name and extension. </param>
        /// <exception cref="ArgumentException">path contains one or more of the invalid characters
        /// defined in <see cref="InvalidCharacters"/>, or contains a wildcard character. </exception>
        /// <filterpriority>1</filterpriority>
        public static string GetFileName(string path)
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
                if ((length >= 1 && (path[0] == DirectorySeparatorChar)) ||
                    (length == 1))
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
        public static string Escape(string path)
        {
            return new Regex(EscapePattern).Replace(path, new MatchEvaluator(m => m.Result("\\\\$1")));
        }

        /// <summary>
        /// Quotes the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>The quoted path.</returns>
        public static string Quote(string path)
        {
            return path.Contains(" ") ? string.Format("\"{0}\"", path) : path;
        }

        /// <summary>
        /// Checks the invalid path chars.
        /// </summary>
        /// <param name="path">The path.</param>
        internal static void CheckInvalidPathChars(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (path.ToCharArray().Any(c => c < 0x20 || InvalidCharacters.Contains(c)))
            {
                throw new ArgumentException("Path contains invalid characters");
            }
        }

        /// <summary>
        /// Fixups the path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>The fixuped path</returns>
        private static string FixupPath(string path)
        {
            string sb = path;
            sb = sb.Replace(System.IO.Path.DirectorySeparatorChar, DirectorySeparatorChar);

            if (sb != "." && !sb.StartsWith(new string(new char[] { DirectorySeparatorChar })))
            {
                sb = string.Format(".{0}{1}", DirectorySeparatorChar, sb);
            }

            if (!sb.EndsWith(new string(new char[] { DirectorySeparatorChar })))
            {
                sb = string.Format("{0}{1}", sb, DirectorySeparatorChar);
            }

            sb = sb.Replace("//", new string(new char[] { DirectorySeparatorChar }));

            return sb;
        }

        /// <summary>
        /// Returns the absolute path for the specified path string.
        /// </summary>
        /// <param name="path">The file or directory for which to obtain absolute path information.</param>
        /// <returns>The fully qualified location of <paramref name="path"/>, such as "/MyFile.txt".</returns>
        // Expands the given path to a fully qualified path.
        public static string GetFullPath(string path)
        {
            if (path != null)
            {
                CheckInvalidPathChars(path);
                
                // Expand with current directory if necessary
                if (!IsPathRooted(path))
                {
                    path = Combine("/", path);
                }
            }

            return path;
        }

        /// <summary>
        /// Returns an absolute path from a relative path and a fully qualified base path.
        /// </summary>
        /// <param name="path">A relative path to concatenate to <paramref name="basePath"/>.</param>
        /// <param name="basePath">The beginning of a fully qualified path.</param>
        /// <returns>The absolute path.</returns>
        public static string GetFullPath(string path, string basePath)
        {
            if (path != null)
            {
                CheckInvalidPathChars(path);

                // Expand with current directory if necessary
                if (!IsPathRooted(path))
                {
                    path = GetFullPath(Combine(basePath, path));
                }
            }

            return path;
        }
    }
}
