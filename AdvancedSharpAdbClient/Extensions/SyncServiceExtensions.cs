// <copyright file="SyncServiceExtensions.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdvancedSharpAdbClient
{
    /// <summary>
    /// Provides extension methods for the <see cref="ISyncService"/> interface. Provides overloads for commonly used functions.
    /// </summary>
    public static partial class SyncServiceExtensions
    {
        /// <summary>
        /// Returns information about a file on the device.
        /// </summary>
        /// <param name="service">An instance of a class that implements the <see cref="ISyncService"/> interface.</param>
        /// <param name="remotePath">The path of the file on the device.</param>
        /// <param name="useV2"><see langword="true"/> to use <see cref="ISyncService.Stat"/>; otherwise, use <see cref="ISyncService.StatEx"/>.</param>
        /// <returns>A <see cref="IFileStatistics"/> object that contains information about the file.</returns>
        /// <remarks>V2 need Android 8 or above.</remarks>
        public static IFileStatistics Stat(this ISyncService service, string remotePath, bool useV2 = false) =>
            useV2 ? service.StatEx(remotePath) : service.Stat(remotePath);

        /// <summary>
        /// Lists the contents of a directory on the device.
        /// </summary>
        /// <param name="service">An instance of a class that implements the <see cref="ISyncService"/> interface.</param>
        /// <param name="remotePath">The path to the directory on the device.</param>
        /// <param name="useV2"><see langword="true"/> to use <see cref="ISyncService.GetDirectoryListing"/>; otherwise, <see langword="false"/> use <see cref="ISyncService.GetDirectoryListingEx"/>.</param>
        /// <returns>For each child item of the directory, a <see cref="IFileStatistics"/> object with information of the item.</returns>
        /// <remarks>V2 need Android 11 or above.</remarks>
        public static IEnumerable<IFileStatistics> GetDirectoryListing(this ISyncService service, string remotePath, bool useV2 = false) =>
#if NETFRAMEWORK && !NET40_OR_GREATER
            useV2 ? service.GetDirectoryListingEx(remotePath).OfType<IFileStatistics>() : service.GetDirectoryListing(remotePath).OfType<IFileStatistics>();
#else
            useV2 ? service.GetDirectoryListingEx(remotePath) : service.GetDirectoryListing(remotePath);
#endif

#if !NETFRAMEWORK || NET40_OR_GREATER
        /// <summary>
        /// Pushes (uploads) a file to the remote device.
        /// </summary>
        /// <param name="service">An instance of a class that implements the <see cref="ISyncService"/> interface.</param>
        /// <param name="stream">A <see cref="Stream"/> that contains the contents of the file.</param>
        /// <param name="remotePath">The path, on the device, to which to push the file.</param>
        /// <param name="permissions">The <see cref="UnixFileStatus"/> that contains the permissions of the newly created file on the device.</param>
        /// <param name="timestamp">The time at which the file was last modified.</param>
        /// <param name="progress">An optional parameter which, when specified, returns progress notifications. The progress is reported as <see cref="SyncProgressChangedEventArgs"/>, representing the state of the file which has been transferred.</param>
        /// <param name="useV2"><see langword="true"/> to use <see cref="SyncCommand.SND2"/>; otherwise, <see langword="false"/> use <see cref="SyncCommand.SEND"/>.</param>
        /// <param name="isCancelled">A <see cref="bool"/> that can be used to cancel the task.</param>
        /// <remarks>V2 need Android 11 or above.</remarks>
        public static void Push(this ISyncService service, Stream stream, string remotePath, UnixFileStatus permissions, DateTimeOffset timestamp, IProgress<SyncProgressChangedEventArgs>? progress = null, bool useV2 = false, in bool isCancelled = false) =>
            service.Push(stream, remotePath, permissions, timestamp, progress.AsAction(), useV2, isCancelled);

        /// <summary>
        /// Pulls (downloads) a file from the remote device.
        /// </summary>
        /// <param name="service">An instance of a class that implements the <see cref="ISyncService"/> interface.</param>
        /// <param name="remotePath">The path, on the device, of the file to pull.</param>
        /// <param name="stream">A <see cref="Stream"/> that will receive the contents of the file.</param>
        /// <param name="progress">An optional parameter which, when specified, returns progress notifications. The progress is reported as <see cref="SyncProgressChangedEventArgs"/>, representing the state of the file which has been transferred.</param>
        /// <param name="useV2"><see langword="true"/> to use <see cref="SyncCommand.RCV2"/> and <see cref="SyncCommand.STA2"/>; otherwise, <see langword="false"/> use <see cref="SyncCommand.RECV"/> and <see cref="SyncCommand.STAT"/>.</param>
        /// <param name="isCancelled">A <see cref="bool"/> that can be used to cancel the task.</param>
        /// <remarks>V2 need Android 8 or above.</remarks>
        public static void Pull(this ISyncService service, string remotePath, Stream stream, IProgress<SyncProgressChangedEventArgs>? progress = null, bool useV2 = false, in bool isCancelled = false) =>
            service.Pull(remotePath, stream, progress.AsAction(), useV2, isCancelled);

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
        /// <param name="useV2"><see langword="true"/> to use <see cref="SyncCommand.SND2"/>; otherwise, <see langword="false"/> use <see cref="SyncCommand.SEND"/>.</param>
        /// <param name="isCancelled">A <see cref="bool"/> that can be used to cancel the task.</param>
        /// <remarks>V2 need Android 11 or above.</remarks>
        public static void Push(this ISyncService service, Stream stream, string remotePath, UnixFileMode permissions, DateTimeOffset timestamp, Action<SyncProgressChangedEventArgs>? callback = null, bool useV2 = false, in bool isCancelled = false) =>
            service.Push(stream, remotePath, (UnixFileStatus)permissions, timestamp, callback, useV2, isCancelled);

        /// <summary>
        /// Pushes (uploads) a file to the remote device.
        /// </summary>
        /// <param name="service">An instance of a class that implements the <see cref="ISyncService"/> interface.</param>
        /// <param name="stream">A <see cref="Stream"/> that contains the contents of the file.</param>
        /// <param name="remotePath">The path, on the device, to which to push the file.</param>
        /// <param name="permissions">The <see cref="UnixFileMode"/> that contains the permissions of the newly created file on the device.</param>
        /// <param name="timestamp">The time at which the file was last modified.</param>
        /// <param name="progress">An optional parameter which, when specified, returns progress notifications. The progress is reported as <see cref="SyncProgressChangedEventArgs"/>, representing the state of the file which has been transferred.</param>
        /// <param name="useV2"><see langword="true"/> to use <see cref="SyncCommand.SND2"/>; otherwise, <see langword="false"/> use <see cref="SyncCommand.SEND"/>.</param>
        /// <param name="isCancelled">A <see cref="bool"/> that can be used to cancel the task.</param>
        /// <remarks>V2 need Android 11 or above.</remarks>
        public static void Push(this ISyncService service, Stream stream, string remotePath, UnixFileMode permissions, DateTimeOffset timestamp, IProgress<SyncProgressChangedEventArgs>? progress, bool useV2 = false, in bool isCancelled = false) =>
            service.Push(stream, remotePath, (UnixFileStatus)permissions, timestamp, progress.AsAction(), useV2, isCancelled);
#endif
#endif
    }
}