#if !NETFRAMEWORK || NET40_OR_GREATER
// <copyright file="SyncServiceExtensions.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.IO;

namespace AdvancedSharpAdbClient
{
    /// <summary>
    /// Provides extension methods for the <see cref="ISyncService"/> interface. Provides overloads for commonly used functions.
    /// </summary>
    public static partial class SyncServiceExtensions
    {
        /// <summary>
        /// Pushes (uploads) a file to the remote device.
        /// </summary>
        /// <param name="service">An instance of a class that implements the <see cref="ISyncService"/> interface.</param>
        /// <param name="stream">A <see cref="Stream"/> that contains the contents of the file.</param>
        /// <param name="remotePath">The path, on the device, to which to push the file.</param>
        /// <param name="permissions">The <see cref="UnixFileStatus"/> that contains the permissions of the newly created file on the device.</param>
        /// <param name="timestamp">The time at which the file was last modified.</param>
        /// <param name="progress">An optional parameter which, when specified, returns progress notifications. The progress is reported as <see cref="SyncProgressChangedEventArgs"/>, representing the state of the file which has been transferred.</param>
        /// <param name="isCancelled">A <see cref="bool"/> that can be used to cancel the task.</param>
        public static void Push(this ISyncService service, Stream stream, string remotePath, UnixFileStatus permissions, DateTimeOffset timestamp, IProgress<SyncProgressChangedEventArgs>? progress = null, in bool isCancelled = false) =>
            service.Push(stream, remotePath, permissions, timestamp, progress.AsAction(), isCancelled);

        /// <summary>
        /// Pulls (downloads) a file from the remote device.
        /// </summary>
        /// <param name="service">An instance of a class that implements the <see cref="ISyncService"/> interface.</param>
        /// <param name="remotePath">The path, on the device, of the file to pull.</param>
        /// <param name="stream">A <see cref="Stream"/> that will receive the contents of the file.</param>
        /// <param name="progress">An optional parameter which, when specified, returns progress notifications. The progress is reported as <see cref="SyncProgressChangedEventArgs"/>, representing the state of the file which has been transferred.</param>
        /// <param name="isCancelled">A <see cref="bool"/> that can be used to cancel the task.</param>
        public static void Pull(this ISyncService service, string remotePath, Stream stream, IProgress<SyncProgressChangedEventArgs>? progress = null, in bool isCancelled = false) =>
            service.Pull(remotePath, stream, progress.AsAction(), isCancelled);

#if NET7_0_OR_GREATER
        /// <summary>
        /// Pushes (uploads) a file to the remote device.
        /// </summary>
        /// <param name="service">An instance of a class that implements the <see cref="ISyncService"/> interface.</param>
        /// <param name="stream">A <see cref="Stream"/> that contains the contents of the file.</param>
        /// <param name="remotePath">The path, on the device, to which to push the file.</param>
        /// <param name="permissions">The <see cref="UnixFileMode"/> that contains the permissions of the newly created file on the device.</param>
        /// <param name="timestamp">The time at which the file was last modified.</param>
        /// <param name="callback">An optional parameter which, when specified, returns progress notifications. The progress is reported as <see cref="SyncProgressChangedEventArgs"/>, representing the state of the file which has been transferred.</param>
        /// <param name="isCancelled">A <see cref="bool"/> that can be used to cancel the task.</param>
        public static void Push(this ISyncService service, Stream stream, string remotePath, UnixFileMode permissions, DateTimeOffset timestamp, Action<SyncProgressChangedEventArgs>? callback = null, in bool isCancelled = false) =>
            service.Push(stream, remotePath, (UnixFileStatus)permissions, timestamp, callback, isCancelled);

        /// <summary>
        /// Pushes (uploads) a file to the remote device.
        /// </summary>
        /// <param name="service">An instance of a class that implements the <see cref="ISyncService"/> interface.</param>
        /// <param name="stream">A <see cref="Stream"/> that contains the contents of the file.</param>
        /// <param name="remotePath">The path, on the device, to which to push the file.</param>
        /// <param name="permissions">The <see cref="UnixFileMode"/> that contains the permissions of the newly created file on the device.</param>
        /// <param name="timestamp">The time at which the file was last modified.</param>
        /// <param name="progress">An optional parameter which, when specified, returns progress notifications. The progress is reported as <see cref="SyncProgressChangedEventArgs"/>, representing the state of the file which has been transferred.</param>
        /// <param name="isCancelled">A <see cref="bool"/> that can be used to cancel the task.</param>
        public static void Push(this ISyncService service, Stream stream, string remotePath, UnixFileMode permissions, DateTimeOffset timestamp, IProgress<SyncProgressChangedEventArgs>? progress, in bool isCancelled = false) =>
            service.Push(stream, remotePath, (UnixFileStatus)permissions, timestamp, progress.AsAction(), isCancelled);
#endif
    }
}
#endif