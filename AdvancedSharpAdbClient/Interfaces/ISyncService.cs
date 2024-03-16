// <copyright file="ISyncService.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;

namespace AdvancedSharpAdbClient
{
    /// <summary>
    /// Interface containing methods for file synchronization.
    /// </summary>
    public partial interface ISyncService : IDisposable
    {
        /// <summary>
        /// Gets a value indicating whether this instance is open.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if this instance is open; otherwise, <see langword="false"/>.
        /// </value>
        bool IsOpen { get; }

        /// <summary>
        /// Pushes (uploads) a file to the remote device.
        /// </summary>
        /// <param name="stream">A <see cref="Stream"/> that contains the contents of the file.</param>
        /// <param name="remotePath">The path, on the device, to which to push the file.</param>
        /// <param name="permissions">The <see cref="UnixFileStatus"/> that contains the permissions of the newly created file on the device.</param>
        /// <param name="timestamp">The time at which the file was last modified.</param>
        /// <param name="callback">An optional parameter which, when specified, returns progress notifications. The progress is reported as <see cref="SyncProgressChangedEventArgs"/>, representing the state of the file which has been transferred.</param>
        /// <param name="isCancelled">A <see cref="bool"/> that can be used to cancel the task.</param>
        void Push(Stream stream, string remotePath, UnixFileStatus permissions, DateTimeOffset timestamp, Action<SyncProgressChangedEventArgs>? callback, in bool isCancelled);

        /// <summary>
        /// Pulls (downloads) a file from the remote device.
        /// </summary>
        /// <param name="remotePath">The path, on the device, of the file to pull.</param>
        /// <param name="stream">A <see cref="Stream"/> that will receive the contents of the file.</param>
        /// <param name="callback">An optional parameter which, when specified, returns progress notifications. The progress is reported as <see cref="SyncProgressChangedEventArgs"/>, representing the state of the file which has been transferred.</param>
        /// <param name="isCancelled">A <see cref="bool"/> that can be used to cancel the task.</param>
        void Pull(string remotePath, Stream stream, Action<SyncProgressChangedEventArgs>? callback, in bool isCancelled);

        /// <summary>
        /// Returns information about a file on the device.
        /// </summary>
        /// <param name="remotePath">The path of the file on the device.</param>
        /// <returns>A <see cref="FileStatistics"/> object that contains information about the file.</returns>
        FileStatistics Stat(string remotePath);

        /// <summary>
        /// Lists the contents of a directory on the device.
        /// </summary>
        /// <param name="remotePath">The path to the directory on the device.</param>
        /// <returns>For each child item of the directory, a <see cref="FileStatistics"/> object with information of the item.</returns>
        IEnumerable<FileStatistics> GetDirectoryListing(string remotePath);

        /// <summary>
        /// Opens this connection.
        /// </summary>
        void Open();

        /// <summary>
        /// Reopen this connection. Use this when the socket was disconnected by adb and you have restarted adb.
        /// </summary>
        void Reopen();
    }
}
