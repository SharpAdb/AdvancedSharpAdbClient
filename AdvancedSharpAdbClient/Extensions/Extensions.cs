// <copyright file="Extensions.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace AdvancedSharpAdbClient
{
    internal static class Extensions
    {
        public static char[] NewLineSeparator { get; } = ['\r', '\n'];

        /// <summary>
        /// Adds the elements of the specified collection to the end of the <see cref="ICollection{TSource}"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">The <see cref="ICollection{TSource}"/> to be added.</param>
        /// <param name="collection">The collection whose elements should be added to the end of the <see cref="ICollection{TSource}"/>.
        /// The collection itself cannot be <see langword="null"/>, but it can contain elements that are
        /// <see langword="null"/>, if type <typeparamref name="TSource"/> is a reference type.</param>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="collection"/> is null.</exception>
        public static void AddRange<TSource>(this ICollection<TSource> source, IEnumerable<TSource> collection)
        {
            ExceptionExtensions.ThrowIfNull(source);
            ExceptionExtensions.ThrowIfNull(collection);

            if (source is List<TSource> list)
            {
                list.AddRange(collection);
            }
#if !NETFRAMEWORK || NET40_OR_GREATER
            else if (source is ISet<TSource> set)
            {
                set.UnionWith(collection);
            }
#endif
            else
            {
                foreach (TSource item in collection)
                {
                    source.Add(item);
                }
            }
        }

        /// <summary>
        /// Converts the string representation of the name or numeric value of one or more
        /// enumerated constants to an equivalent enumerated object. A parameter specifies
        /// whether the operation is case-sensitive. The return value indicates whether the
        /// conversion succeeded.
        /// </summary>
        /// <typeparam name="TEnum">The enumeration type to which to convert <paramref name="value"/>.</typeparam>
        /// <param name="value">The string representation of the enumeration name or underlying value to convert.</param>
        /// <param name="ignoreCase"><see langword="true"/> to ignore case; <see langword="false"/> to consider case.</param>
        /// <param name="result">When this method returns, contains an object of type <typeparamref name="TEnum"/> whose
        /// value is represented by <paramref name="value"/> if the parse operation succeeds. If the parse operation fails,
        /// contains the default value of the underlying type of <typeparamref name="TEnum"/>. This parameter is passed uninitialized.</param>
        /// <returns><see langword="true"/> if the value parameter was converted successfully; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentException"><typeparamref name="TEnum"/> is not an enumeration type.</exception>
        public static bool TryParse<TEnum>(string value, bool ignoreCase, out TEnum result) where TEnum : struct
        {
#if NETFRAMEWORK && !NET40_OR_GREATER
            string strTypeFixed = value.Replace(' ', '_');
            if (Enum.IsDefined(typeof(TEnum), strTypeFixed))
            {
                result = (TEnum)Enum.Parse(typeof(TEnum), strTypeFixed, ignoreCase);
                return true;
            }
            else
            {
                foreach (string str in Enum.GetNames(typeof(TEnum)))
                {
                    if (str.Equals(strTypeFixed, StringComparison.OrdinalIgnoreCase))
                    {
                        result = (TEnum)Enum.Parse(typeof(TEnum), str, ignoreCase);
                        return true;
                    }
                }
                result = default;
                return false;
            }
#else
            return Enum.TryParse(value, ignoreCase, out result);
#endif
        }

#if NETFRAMEWORK && !NET40_OR_GREATER
        /// <summary>
        /// Removes all characters from the current <see cref="StringBuilder"/> instance.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder"/> to removes all characters.</param>
        /// <returns>An object whose <see cref="StringBuilder.Length"/> is 0 (zero).</returns>
        public static StringBuilder Clear(this StringBuilder builder)
        {
            builder.Length = 0;
            return builder;
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="WaitHandle"/> class.
        /// </summary>
        /// <param name="waitHandle">The <see cref="WaitHandle"/> to release.</param>
        public static void Dispose(this WaitHandle waitHandle)
        {
            waitHandle.Close();
            GC.SuppressFinalize(waitHandle);
        }
#endif

#if HAS_TASK
        /// <summary>
        /// Creates an array from a <see cref="IEnumerable{T}"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">An <see cref="IEnumerable{T}"/> to create an array from.</param>
        /// <returns>An array that contains the elements from the input sequence.</returns>
        public static Task<TSource[]> ToArrayAsync<TSource>(this Task<IEnumerable<TSource>> source) =>
            source.ContinueWith(x => x.Result.ToArray());

        /// <summary>
        /// Creates an array from a <see cref="IEnumerable{T}"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">An <see cref="IEnumerable{T}"/> to create an array from.</param>
        /// <returns>An array that contains the elements from the input sequence.</returns>
        public static Task<TSource[]> ToArrayAsync<TSource>(this IEnumerable<Task<TSource>> source) => WhenAll(source);

        /// <summary>
        /// Creates a task that completes after a specified number of milliseconds.
        /// </summary>
        /// <param name="dueTime">The number of milliseconds to wait before completing the returned task, or -1 to wait indefinitely.</param>
        /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the time delay.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The <paramref name="dueTime"/> argument is less than -1.</exception>
        public static Task Delay(int dueTime, CancellationToken cancellationToken = default) =>
#if NETFRAMEWORK && !NET45_OR_GREATER
            TaskEx
#else
            Task
#endif
            .Delay(dueTime, cancellationToken);

        /// <summary>
        /// Queues the specified work to run on the thread pool and returns a proxy for the task returned by <paramref name="function"/>.
        /// </summary>
        /// <param name="function">The work to execute asynchronously.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the work if it has not yet started.</param>
        /// <returns>A task that represents a proxy for the task returned by <paramref name="function"/>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="function"/> parameter was <see langword="null"/>.</exception>
        /// <remarks>For information on handling exceptions thrown by task operations, see <see href="https://learn.microsoft.com/en-us/dotnet/standard/parallel-programming/exception-handling-task-parallel-library">Exception Handling</see>.</remarks>
        public static Task Run(Action function, CancellationToken cancellationToken = default) =>
#if NETFRAMEWORK && !NET45_OR_GREATER
            TaskEx
#else
            Task
#endif
            .Run(function, cancellationToken);

        /// <summary>
        /// Queues the specified work to run on the thread pool and returns a proxy for the <see cref="Task{TResult}"/>
        /// returned by function. A cancellation token allows the work to be cancelled if it has not yet started.
        /// </summary>
        /// <typeparam name="TResult">The type of the result returned by the proxy task.</typeparam>
        /// <param name="function">The work to execute asynchronously.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the work if it has not yet started.</param>
        /// <returns>A <see cref="Task{TResult}"/> that represents a proxy for the
        /// <see cref="Task{TResult}"/> returned by <paramref name="function"/>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="function"/> parameter was <see langword="null"/>.</exception>
        public static Task<TResult> Run<TResult>(Func<TResult> function, CancellationToken cancellationToken = default) =>
#if NETFRAMEWORK && !NET45_OR_GREATER
            TaskEx
#else
            Task
#endif
            .Run(function, cancellationToken);

        /// <summary>
        /// Creates a task that will complete when all of the <see cref="Task"/> objects in an enumerable collection have completed.
        /// </summary>
        /// <param name="tasks">The tasks to wait on for completion.</param>
        /// <returns>A task that represents the completion of all of the supplied tasks.</returns>
        public static Task WhenAll(IEnumerable<Task> tasks) =>
#if NETFRAMEWORK && !NET45_OR_GREATER
            TaskEx
#else
            Task
#endif
            .WhenAll(tasks);

        /// <summary>
        /// Creates a task that will complete when all of the <see cref="Task{TResult}"/> objects in an enumerable collection have completed.
        /// </summary>
        /// <typeparam name="TResult">The type of the completed task.</typeparam>
        /// <param name="tasks">The tasks to wait on for completion.</param>
        /// <returns>A task that represents the completion of all of the supplied tasks.</returns>
        public static Task<TResult[]> WhenAll<TResult>(IEnumerable<Task<TResult>> tasks) =>
#if NETFRAMEWORK && !NET45_OR_GREATER
            TaskEx
#else
            Task
#endif
            .WhenAll(tasks);

#if !NET7_0_OR_GREATER
        /// <summary>
        /// Reads a line of characters asynchronously and returns the data as a string.
        /// </summary>
        /// <param name="reader">The <see cref="TextReader"/> to read a line.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A value task that represents the asynchronous read operation. The value of the
        /// TResult parameter contains the next line from the text reader, or is null if
        /// all of the characters have been read.</returns>
        public static Task<string> ReadLineAsync(this TextReader reader, CancellationToken cancellationToken) =>
#if !NET35
            reader.ReadLineAsync();
#else
            Run(reader.ReadLine, cancellationToken);
#endif

        /// <summary>
        /// Reads all characters from the current position to the end of the stream asynchronously and returns them as one string.
        /// </summary>
        /// <param name="reader">The <see cref="TextReader"/> to read all characters.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous read operation. The value of the TResult
        /// parameter contains a string with the characters from the current position to
        /// the end of the stream.</returns>
        public static Task<string> ReadToEndAsync(this TextReader reader, CancellationToken cancellationToken) =>
#if !NET35
            reader.ReadToEndAsync();
#else
            Run(reader.ReadToEnd, cancellationToken);
#endif
#endif
#endif

        public static bool IsWindowsPlatform() =>
#if HAS_RUNTIMEINFORMATION
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
#elif NETCORE
            true;
#else
            Environment.OSVersion.Platform
                is PlatformID.Win32S
                or PlatformID.Win32Windows
                or PlatformID.Win32NT
                or PlatformID.WinCE
                or PlatformID.Xbox;
#endif

        public static bool IsUnixPlatform() =>
#if HAS_RUNTIMEINFORMATION
            RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
            || RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
#if NETCOREAPP3_0_OR_GREATER
            || RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD)
#endif
            ;
#elif NETCORE
            false;
#else
            Environment.OSVersion.Platform
                is PlatformID.Unix
                or PlatformID.MacOSX;
#endif
    }
}
