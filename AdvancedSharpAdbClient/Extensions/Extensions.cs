// <copyright file="Extensions.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace AdvancedSharpAdbClient
{
    /// <summary>
    /// Extension methods for the <see cref="AdvancedSharpAdbClient"/> namespace.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal static class Extensions
    {
        /// <summary>
        /// The <see cref="Array"/> of <see cref="char"/>s that represent a new line.
        /// </summary>
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
            return await Task.Factory.StartNew(() => reader.ReadLine(), CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
#else
            return await reader.ReadLineAsync().ConfigureAwait(false);
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
            return await Task.Factory.StartNew(() => reader.ReadToEnd(), CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default).ConfigureAwait(false);
#else
            return await reader.ReadToEndAsync().ConfigureAwait(false);
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
