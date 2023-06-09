// <copyright file="Utilities.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using AdvancedSharpAdbClient.Exceptions;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace AdvancedSharpAdbClient
{
    internal static class Utilities
    {
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

        /// <summary>
        /// Indicates whether a specified string is <see langword="null"/>, empty, or consists only of white-space characters.
        /// </summary>
        /// <param name="value">The string to test.</param>
        /// <returns><see langword="true"/> if the <paramref name="value"/> parameter is <see langword="null"/> or
        /// <see cref="string.Empty"/>, or if <paramref name="value"/> consists exclusively of white-space characters.</returns>
        public static bool IsNullOrWhiteSpace(this string value)
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

        /// <summary>
        /// Concatenates the members of a constructed <see cref="IEnumerable{T}"/> collection of type <see cref="string"/>,
        /// using the specified separator between each member.
        /// </summary>
        /// <param name="separator">The string to use as a separator. <paramref name="separator"/> is included
        /// in the returned string only if <paramref name="values"/> has more than one element.</param>
        /// <param name="values">A collection that contains the strings to concatenate.</param>
        /// <returns>A string that consists of the elements of <paramref name="values"/> delimited by the
        /// <paramref name="separator"/> string.<para>-or-</para><see cref="string.Empty"/> if values has zero elements.</returns>
        public static string Join(string separator, IEnumerable<string> values)
        {
#if NETFRAMEWORK && !NET40_OR_GREATER
            ExceptionExtensions.ThrowIfNull(values);

            separator ??= string.Empty;

            using IEnumerator<string> en = values.GetEnumerator();
            if (!en.MoveNext())
            {
                return string.Empty;
            }

            StringBuilder result = new();
            if (en.Current != null)
            {
                _ = result.Append(en.Current);
            }

            while (en.MoveNext())
            {
                _ = result.Append(separator);
                if (en.Current != null)
                {
                    _ = result.Append(en.Current);
                }
            }
            return result.ToString();
#else
            return string.Join(separator, values);
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
#endif

#if HAS_TASK
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
#endif

#if NET20
#pragma warning disable CS1574 // XML 注释中有无法解析的 cref 特性
#endif
        /// <summary>
        /// Converts a Unix time expressed as the number of seconds that have elapsed
        /// since 1970-01-01T00:00:00Z to a <see cref="DateTimeOffset"/> value.
        /// </summary>
        /// <param name="seconds">A Unix time, expressed as the number of seconds that have elapsed
        /// since 1970-01-01T00:00:00Z (January 1, 1970, at 12:00 AM UTC). For Unix times before this date,
        /// its value is negative.</param>
        /// <returns>A date and time value that represents the same moment in time as the Unix time.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="seconds"/> is less than -62,135,596,800.
        /// <para>-or-</para><paramref name="seconds"/> is greater than 253,402,300,799.</exception>
        /// <remarks>The Offset property value of the returned <see cref="DateTimeOffset"/> instance is
        /// <see cref="TimeSpan.Zero"/>, which represents Coordinated Universal Time. You can convert it to the time in
        /// a specific time zone by calling the <see cref="TimeZoneInfo.ConvertTime(DateTimeOffset, TimeZoneInfo)"/> method.</remarks>
        public static DateTimeOffset FromUnixTimeSeconds(long seconds) =>
#if NETFRAMEWORK && !NET46_OR_GREATER
            new(seconds.ToDateTime());
#else
            DateTimeOffset.FromUnixTimeSeconds(seconds);
#endif
#if NET20
#pragma warning restore CS1574 // XML 注释中有无法解析的 cref 特性
#endif

        /// <summary>
        /// Returns the number of seconds that have elapsed since 1970-01-01T00:00:00Z.
        /// </summary>
        /// <param name="dateTimeOffset">The DateTimeOffset</param>
        /// <returns>The number of seconds that have elapsed since 1970-01-01T00:00:00Z.</returns>
        public static long ToUnixTimeSeconds(this DateTimeOffset dateTimeOffset) =>
#if NETFRAMEWORK && !NET46_OR_GREATER
            (int)dateTimeOffset.DateTime.ToUnixEpoch();
#else
            (int)dateTimeOffset.ToUnixTimeSeconds();
#endif

        public static bool IsWindowsPlatform() =>
#if HAS_RUNTIMEINFORMATION
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
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
#else
            Environment.OSVersion.Platform
                is PlatformID.Unix
                or PlatformID.MacOSX;
#endif

#if !HAS_PROCESS
        /// <summary>
        /// Begins to asynchronously receive data from a connected <see cref="System.Net.Sockets.Socket"/>.
        /// </summary>
        /// <param name="socket">The Socket.</param>
        /// <param name="buffer">An array of type <see cref="byte"/> that is the storage location for the received data.</param>
        /// <param name="offset">The zero-based position in the <paramref name="buffer"/> parameter at which to store the received data.</param>
        /// <param name="size">The number of bytes to receive.</param>
        /// <param name="socketFlags">A bitwise combination of the <see cref="System.Net.Sockets.SocketFlags"/> values.</param>
        /// <param name="callback">An <see cref="AsyncCallback"/> delegate that references the method to invoke when the operation is complete.</param>
        /// <param name="state">A user-defined object that contains information about the receive operation. This object is
        /// passed to the <see cref="EndReceive(System.Net.Sockets.Socket, IAsyncResult)"/> delegate when the operation is complete.</param>
        /// <returns>An <see cref="IAsyncResult"/> that references the asynchronous read.</returns>
        public static IAsyncResult BeginReceive(this System.Net.Sockets.Socket socket, byte[] buffer, int offset, int size, System.Net.Sockets.SocketFlags socketFlags, AsyncCallback callback, object state) =>
            TaskToApm.Begin(socket.ReceiveAsync(buffer, offset, size, socketFlags, default), callback, state);
#endif

#if !HAS_PROCESS
        /// <summary>
        /// Ends a pending asynchronous read.
        /// </summary>
        /// <param name="_">The Socket.</param>
        /// <param name="asyncResult">An <see cref="IAsyncResult"/> that stores state information and any user defined data for this asynchronous operation.</param>
        /// <returns>The number of bytes received.</returns>
        public static int EndReceive(this System.Net.Sockets.Socket _, IAsyncResult asyncResult) =>
            TaskToApm.End<int>(asyncResult);
#endif
    }
}
