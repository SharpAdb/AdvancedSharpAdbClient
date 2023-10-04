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
        /// Pushes (uploads) a file to the remote device.
        /// </summary>
        /// <param name="stream">A <see cref="Stream"/> that contains the contents of the file.</param>
        /// <param name="remotePath">The path, on the device, to which to push the file.</param>
        /// <param name="permissions">The permission octet that contains the permissions of the newly created file on the device.</param>
        /// <param name="timestamp">The time at which the file was last modified.</param>
        /// <param name="progress">An optional parameter which, when specified, returns progress notifications. The progress is reported as a value between 0 and 100, representing the percentage of the file which has been transferred.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the task.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        Task PushAsync(Stream stream, string remotePath, int permissions, DateTimeOffset timestamp, IProgress<int> progress, CancellationToken cancellationToken);

        /// <summary>
        /// Pulls (downloads) a file from the remote device.
        /// </summary>
        /// <param name="remotePath">The path, on the device, of the file to pull.</param>
        /// <param name="stream">A <see cref="Stream"/> that will receive the contents of the file.</param>
        /// <param name="progress">An optional parameter which, when specified, returns progress notifications. The progress is reported as a value between 0 and 100, representing the percentage of the file which has been transferred.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the task.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        Task PullAsync(string remotePath, Stream stream, IProgress<int> progress, CancellationToken cancellationToken);

        /// <summary>
        /// Returns information about a file on the device.
        /// </summary>
        /// <param name="remotePath">The path of the file on the device.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the task.</param>
        /// <returns>A <see cref="Task"/> which return a <see cref="FileStatistics"/> object that contains information about the file.</returns>
        Task<FileStatistics> StatAsync(string remotePath, CancellationToken cancellationToken);

        /// <summary>
        /// Lists the contents of a directory on the device.
        /// </summary>
        /// <param name="remotePath">The path to the directory on the device.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the task.</param>
        /// <returns>A <see cref="Task"/> which return for each child item of the directory, a <see cref="FileStatistics"/> object with information of the item.</returns>
        Task<List<FileStatistics>> GetDirectoryListingAsync(string remotePath, CancellationToken cancellationToken);

#if NETCOREAPP3_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        /// <summary>
        /// Lists the contents of a directory on the device.
        /// </summary>
        /// <param name="remotePath">The path to the directory on the device.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the task.</param>
        /// <returns>An <see cref="IAsyncEnumerable{FileStatistics}"/> which return for each child item of the directory, a <see cref="FileStatistics"/> object with information of the item.</returns>
        IAsyncEnumerable<FileStatistics> GetDirectoryAsyncListing(string remotePath, CancellationToken cancellationToken);
#endif

        /// <summary>
        /// Opens this connection.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the task.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        Task OpenAsync(CancellationToken cancellationToken);
    }
}
#endif