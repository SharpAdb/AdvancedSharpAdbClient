// <copyright file="Extensions.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

#if NET40
using Microsoft.Runtime.CompilerServices;
#endif

namespace AdvancedSharpAdbClient
{
    internal static class Extensions
    {
        public static char[] NewLineSeparator { get; } = ['\r', '\n'];

        /// <summary>
        /// Creates a <see cref="DnsEndPoint"/> from the specified host and port information.
        /// </summary>
        /// <param name="host">The host address.</param>
        /// <param name="port">The port.</param>
        /// <returns>The <see cref="DnsEndPoint"/> created from the specified host and port information.</returns>
        public static DnsEndPoint CreateDnsEndPoint(string host, int port)
        {
            if (string.IsNullOrEmpty(host))
            {
                throw new ArgumentNullException(nameof(host));
            }

            string[] values = host.Split(':');

            return values.Length <= 0
                ? throw new ArgumentNullException(nameof(host))
                : new DnsEndPoint(values[0], values.Length > 1 && int.TryParse(values[1], out int _port) ? _port : port);
        }

#if HAS_TASK
#if NETFRAMEWORK && !NET46_OR_GREATER
        /// <summary>
        /// Singleton cached task that's been completed successfully.
        /// </summary>
        internal static readonly Task s_cachedCompleted =
#if NET45_OR_GREATER
            Task.
#else
            TaskEx.
#endif
            FromResult<object?>(null);

        /// <summary>
        /// Gets a task that's already been completed successfully.
        /// </summary>
        public static Task CompletedTask => s_cachedCompleted;
#else
        public static Task CompletedTask => Task.CompletedTask;
#endif

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

        /// <summary>
        /// Creates an awaitable task that asynchronously yields back to the current context when awaited.
        /// </summary>
        /// <returns>A context that, when awaited, will asynchronously transition back into the current context at the time of the await.
        /// If the current <see cref="SynchronizationContext"/> is non-null, it is treated as the current context. Otherwise, the task scheduler
        /// that is associated with the currently executing task is treated as the current context.</returns>
        public static YieldAwaitable Yield() =>
#if NETFRAMEWORK && !NET45_OR_GREATER
            TaskEx
#else
            Task
#endif
            .Yield();

#if !NET7_0_OR_GREATER
        /// <summary>
        /// Reads a line of characters asynchronously and returns the data as a string.
        /// </summary>
        /// <param name="reader">The <see cref="TextReader"/> to read a line.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A value task that represents the asynchronous read operation. The value of the
        /// TResult parameter contains the next line from the text reader, or is null if
        /// all of the characters have been read.</returns>
        public static async Task<string?> ReadLineAsync(this TextReader reader, CancellationToken cancellationToken)
        {
            using CancellationTokenRegistration cancellationTokenRegistration = cancellationToken.Register(reader.Close);
#if NET35
            await Yield();
            return reader.ReadLine();
#else
            return await reader.ReadLineAsync();
#endif
        }

        /// <summary>
        /// Reads all characters from the current position to the end of the stream asynchronously and returns them as one string.
        /// </summary>
        /// <param name="reader">The <see cref="TextReader"/> to read all characters.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous read operation. The value of the TResult
        /// parameter contains a string with the characters from the current position to
        /// the end of the stream.</returns>
        public static async Task<string> ReadToEndAsync(this TextReader reader, CancellationToken cancellationToken)
        {
            using CancellationTokenRegistration cancellationTokenRegistration = cancellationToken.Register(reader.Close);
#if NET35
            await Yield();
            return reader.ReadToEnd();
#else
            return await reader.ReadToEndAsync();
#endif
        }
#endif
#endif

#if !HAS_PROCESS
        /// <summary>
        /// Closes the <see cref="Socket"/> connection and releases all associated resources.
        /// </summary>
        /// <param name="socket">The <see cref="Socket"/> to release.</param>
        public static void Close(this Socket socket) => socket.Dispose();

        /// <summary>
        /// Closes the <see cref="TextReader"/> and releases any system resources associated with the <see cref="TextReader"/>.
        /// </summary>
        /// <param name="reader">The <see cref="TextReader"/> to release.</param>
        public static void Close(this TextReader reader) => reader.Dispose();
#endif

#if NETFRAMEWORK && !NET40_OR_GREATER
        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="Socket"/> class.
        /// </summary>
        /// <param name="socket">The <see cref="Socket"/> to release.</param>
        public static void Dispose(this Socket socket)
        {
            socket.Close();
            GC.SuppressFinalize(socket);
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
