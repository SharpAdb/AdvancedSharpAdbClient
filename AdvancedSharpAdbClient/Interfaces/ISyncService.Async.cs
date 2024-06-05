#if HAS_TASK
// <copyright file="ISyncService.Async.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace AdvancedSharpAdbClient
{
    public partial interface ISyncService
    {
        /// <summary>
        /// Asynchronously pushes (uploads) a file to the remote device.
        /// </summary>
        /// <param name="stream">A <see cref="Stream"/> that contains the contents of the file.</param>
        /// <param name="remotePath">The path, on the device, to which to push the file.</param>
        /// <param name="permissions">The <see cref="UnixFileStatus"/> that contains the permissions of the newly created file on the device.</param>
        /// <param name="timestamp">The time at which the file was last modified.</param>
        /// <param name="callback">An optional parameter which, when specified, returns progress notifications. The progress is reported as <see cref="SyncProgressChangedEventArgs"/>, representing the state of the file which has been transferred.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the task.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        Task PushAsync(Stream stream, string remotePath, UnixFileStatus permissions, DateTimeOffset timestamp, Action<SyncProgressChangedEventArgs>? callback, CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously pulls (downloads) a file from the remote device.
        /// </summary>
        /// <param name="remotePath">The path, on the device, of the file to pull.</param>
        /// <param name="stream">A <see cref="Stream"/> that will receive the contents of the file.</param>
        /// <param name="callback">An optional parameter which, when specified, returns progress notifications. The progress is reported as <see cref="SyncProgressChangedEventArgs"/>, representing the state of the file which has been transferred.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the task.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        Task PullAsync(string remotePath, Stream stream, Action<SyncProgressChangedEventArgs>? callback, CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously returns information about a file on the device.
        /// </summary>
        /// <param name="remotePath">The path of the file on the device.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the task.</param>
        /// <returns>A <see cref="Task{FileStatistics}"/> which returns a <see cref="FileStatistics"/> object that contains information about the file.</returns>
        Task<FileStatistics> StatAsync(string remotePath, CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously lists the contents of a directory on the device.
        /// </summary>
        /// <param name="remotePath">The path to the directory on the device.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the task.</param>
        /// <returns>A <see cref="Task{List}"/> which returns for each child item of the directory, a <see cref="FileStatistics"/> object with information of the item.</returns>
        Task<List<FileStatistics>> GetDirectoryListingAsync(string remotePath, CancellationToken cancellationToken);

#if COMP_NETSTANDARD2_1
        /// <summary>
        /// Asynchronously lists the contents of a directory on the device.
        /// </summary>
        /// <param name="remotePath">The path to the directory on the device.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the task.</param>
        /// <returns>An <see cref="IAsyncEnumerable{FileStatistics}"/> which returns for each child item of the directory, a <see cref="FileStatistics"/> object with information of the item.</returns>
        IAsyncEnumerable<FileStatistics> GetDirectoryAsyncListing(string remotePath, CancellationToken cancellationToken) =>
            GetDirectoryListingAsync(remotePath, cancellationToken).ContinueWith(x => x.Result as IEnumerable<FileStatistics>).AsEnumerableAsync(cancellationToken);
#endif

        /// <summary>
        /// Asynchronously opens this connection.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the task.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        Task OpenAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously reopen this connection. Use this when the socket was disconnected by adb and you have restarted adb.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        Task ReopenAsync(CancellationToken cancellationToken);

#if HAS_WINRT
        /// <summary>
        /// Provides access to the WinRT specific methods of the <see cref="ISyncService"/> interface.
        /// </summary>
        public interface IWinRT
        {
            /// <summary>
            /// Asynchronously pushes (uploads) a file to the remote device.
            /// </summary>
            /// <param name="stream">A <see cref="IInputStream"/> that contains the contents of the file.</param>
            /// <param name="remotePath">The path, on the device, to which to push the file.</param>
            /// <param name="permissions">The <see cref="UnixFileStatus"/> that contains the permissions of the newly created file on the device.</param>
            /// <param name="timestamp">The time at which the file was last modified.</param>
            /// <param name="callback">An optional parameter which, when specified, returns progress notifications. The progress is reported as <see cref="SyncProgressChangedEventArgs"/>, representing the state of the file which has been transferred.</param>
            /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the task.</param>
            /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
            [ContractVersion(typeof(UniversalApiContract), 65536u)]
            Task PushAsync(IInputStream stream, string remotePath, UnixFileStatus permissions, DateTimeOffset timestamp, Action<SyncProgressChangedEventArgs>? callback, CancellationToken cancellationToken);

            /// <summary>
            /// Asynchronously pulls (downloads) a file from the remote device.
            /// </summary>
            /// <param name="remotePath">The path, on the device, of the file to pull.</param>
            /// <param name="stream">A <see cref="IOutputStream"/> that will receive the contents of the file.</param>
            /// <param name="callback">An optional parameter which, when specified, returns progress notifications. The progress is reported as <see cref="SyncProgressChangedEventArgs"/>, representing the state of the file which has been transferred.</param>
            /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the task.</param>
            /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
            [ContractVersion(typeof(UniversalApiContract), 65536u)]
            Task PullAsync(string remotePath, IOutputStream stream, Action<SyncProgressChangedEventArgs>? callback, CancellationToken cancellationToken);
        }
#endif
    }
}
#endif