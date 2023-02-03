// <copyright file="Utilities.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

#if NETSTANDARD1_3
using System.Net.Sockets;
#endif

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
        public static bool TryParse<TEnum>(string value, bool ignoreCase, out TEnum result) where TEnum : struct =>
#if NET35
            EnumEx
#else
            Enum
#endif
            .TryParse(value, ignoreCase, out result);

        /// <summary>
        /// Indicates whether a specified string is <see langword="null"/>, empty, or consists only of white-space characters.
        /// </summary>
        /// <param name="value">The string to test.</param>
        /// <returns><see langword="true"/> if the <paramref name="value"/> parameter is <see langword="null"/> or
        /// <see cref="string.Empty"/>, or if <paramref name="value"/> consists exclusively of white-space characters.</returns>
        public static bool IsNullOrWhiteSpace(this string value) =>
#if NET35
            StringEx
#else
            string
#endif
            .IsNullOrWhiteSpace(value);

        /// <summary>
        /// Concatenates the members of a constructed <see cref="IEnumerable{T}"/> collection of type <see cref="string"/>,
        /// using the specified separator between each member.
        /// </summary>
        /// <param name="separator">The string to use as a separator. <paramref name="separator"/> is included
        /// in the returned string only if <paramref name="values"/> has more than one element.</param>
        /// <param name="values">A collection that contains the strings to concatenate.</param>
        /// <returns>A string that consists of the elements of <paramref name="values"/> delimited by the
        /// <paramref name="separator"/> string.<para>-or-</para><see cref="string.Empty"/> if values has zero elements.</returns>
        public static string Join(string separator, IEnumerable<string> values) =>
#if NET35
            StringEx
#else
            string
#endif
            .Join(separator, values);

        /// <summary>
        /// Creates a task that completes after a specified number of milliseconds.
        /// </summary>
        /// <param name="dueTime">The number of milliseconds to wait before completing the returned task, or -1 to wait indefinitely.</param>
        /// <returns>A task that represents the time delay.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The <paramref name="dueTime"/> argument is less than -1.</exception>
        public static Task Delay(int dueTime) =>
#if NET35 || NET40
            TaskEx
#else
            Task
#endif
            .Delay(dueTime);

        /// <summary>
        /// Queues the specified work to run on the thread pool and returns a proxy for the task returned by <paramref name="function"/>.
        /// </summary>
        /// <param name="function">The work to execute asynchronously.</param>
        /// <returns>A task that represents a proxy for the task returned by <paramref name="function"/>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="function"/> parameter was <see langword="null"/>.</exception>
        /// <remarks>For information on handling exceptions thrown by task operations, see <see href="https://learn.microsoft.com/en-us/dotnet/standard/parallel-programming/exception-handling-task-parallel-library">Exception Handling</see>.</remarks>
        public static Task Run(Func<Task> function) =>
#if NET35 || NET40
            TaskEx
#else
            Task
#endif
            .Run(function);

        /// <summary>
        /// Queues the specified work to run on the thread pool and returns a proxy for the <see cref="Task{TResult}"/>
        /// returned by function. A cancellation token allows the work to be cancelled if it has not yet started.
        /// </summary>
        /// <typeparam name="TResult">The type of the result returned by the proxy task.</typeparam>
        /// <param name="function">The work to execute asynchronously.</param>
        /// <returns>A <see cref="Task{TResult}"/> that represents a proxy for the
        /// <see cref="Task{TResult}"/> returned by <paramref name="function"/>.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="function"/> parameter was <see langword="null"/>.</exception>
        public static Task<TResult> Run<TResult>(Func<TResult> function) =>
#if NET35 || NET40
            TaskEx
#else
            Task
#endif
            .Run(function);

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
#if NET35 || NET40 || NET452
            new(seconds.ToDateTime());
#else
            DateTimeOffset.FromUnixTimeSeconds(seconds);
#endif

        /// <summary>
        /// Returns the number of seconds that have elapsed since 1970-01-01T00:00:00Z.
        /// </summary>
        /// <param name="dateTimeOffset">The DateTimeOffset</param>
        /// <returns>The number of seconds that have elapsed since 1970-01-01T00:00:00Z.</returns>
        public static long ToUnixTimeSeconds(this DateTimeOffset dateTimeOffset) =>
#if NET35 || NET40 || NET452
            (int)dateTimeOffset.DateTime.ToUnixEpoch();
#else
            (int)dateTimeOffset.ToUnixTimeSeconds();
#endif

#if NETSTANDARD1_3
        /// <summary>
        /// Begins to asynchronously receive data from a connected <see cref="Socket"/>.
        /// </summary>
        /// <param name="socket">The Socket.</param>
        /// <param name="buffer">An array of type <see cref="Byte"/> that is the storage location for the received data.</param>
        /// <param name="offset">The zero-based position in the <paramref name="buffer"/> parameter at which to store the received data.</param>
        /// <param name="size">The number of bytes to receive.</param>
        /// <param name="socketFlags">A bitwise combination of the <see cref="SocketFlags"/> values.</param>
        /// <param name="callback">An <see cref="AsyncCallback"/> delegate that references the method to invoke when the operation is complete.</param>
        /// <param name="state">A user-defined object that contains information about the receive operation. This object is
        /// passed to the <see cref="EndReceive(Socket, IAsyncResult)"/> delegate when the operation is complete.</param>
        /// <returns>An <see cref="IAsyncResult"/> that references the asynchronous read.</returns>
        public static IAsyncResult BeginReceive(this Socket socket, byte[] buffer, int offset, int size, SocketFlags socketFlags, AsyncCallback callback, object state) =>
            TaskToApm.Begin(socket.ReceiveAsync(buffer, offset, size, socketFlags, default), callback, state);
#endif

#if NETSTANDARD1_3
        /// <summary>
        /// Ends a pending asynchronous read.
        /// </summary>
        /// <param name="_">The Socket.</param>
        /// <param name="asyncResult">An <see cref="IAsyncResult"/> that stores state information and any user defined data for this asynchronous operation.</param>
        /// <returns>The number of bytes received.</returns>
        public static int EndReceive(this Socket _, IAsyncResult asyncResult) =>
            TaskToApm.End<int>(asyncResult);
#endif
    }
}
