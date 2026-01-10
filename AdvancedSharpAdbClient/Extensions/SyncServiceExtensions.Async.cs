#if HAS_TASK
// <copyright file="SyncServiceExtensions.Async.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace AdvancedSharpAdbClient
{
    public static partial class SyncServiceExtensions
    {
        /// <summary>
        /// Asynchronously returns information about a file on the device.
        /// </summary>
        /// <param name="service">An instance of a class that implements the <see cref="ISyncService"/> interface.</param>
        /// <param name="remotePath">The path of the file on the device.</param>
        /// <param name="useV2"><see langword="true"/> to use <see cref="ISyncService.StatAsync"/>; otherwise, use <see cref="ISyncService.StatExAsync"/>.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the task.</param>
        /// <returns>A <see cref="Task{IFileStatistics}"/> which returns a <see cref="IFileStatistics"/> object that contains information about the file.</returns>
        /// <remarks>V2 need Android 8 or above.</remarks>
        public static Task<IFileStatistics> StatAsync(this ISyncService service, string remotePath, bool useV2 = false, CancellationToken cancellationToken = default) =>
            useV2 ? service.StatExAsync(remotePath, cancellationToken).ContinueWith(x => x.Result as IFileStatistics) : service.StatAsync(remotePath, cancellationToken).ContinueWith(x => x.Result as IFileStatistics);

        /// <summary>
        /// Asynchronously lists the contents of a directory on the device.
        /// </summary>
        /// <param name="service">An instance of a class that implements the <see cref="ISyncService"/> interface.</param>
        /// <param name="remotePath">The path to the directory on the device.</param>
        /// <param name="useV2"><see langword="true"/> to use <see cref="ISyncService.GetDirectoryListingAsync"/>; otherwise, use <see cref="ISyncService.GetDirectoryListingExAsync"/>.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the task.</param>
        /// <returns>A <see cref="Task{IEnumerable}"/> which returns for each child item of the directory, a <see cref="IFileStatistics"/> object with information of the item.</returns>
        public static Task<IEnumerable<IFileStatistics>> GetDirectoryListingAsync(this ISyncService service, string remotePath, bool useV2 = false, CancellationToken cancellationToken = default) =>
#if NETFRAMEWORK && !NET40_OR_GREATER
            useV2 ? service.GetDirectoryListingExAsync(remotePath, cancellationToken).ContinueWith(x => x.Result.OfType<IFileStatistics>()) : service.GetDirectoryListingAsync(remotePath, cancellationToken).ContinueWith(x => x.Result.OfType<IFileStatistics>());
#else
            useV2 ? service.GetDirectoryListingExAsync(remotePath, cancellationToken).ContinueWith(x => x.Result as IEnumerable<IFileStatistics>) : service.GetDirectoryListingAsync(remotePath, cancellationToken).ContinueWith(x => x.Result as IEnumerable<IFileStatistics>);
#endif

#if COMP_NETSTANDARD2_1
        /// <summary>
        /// Asynchronously lists the contents of a directory on the device.
        /// </summary>
        /// <param name="service">An instance of a class that implements the <see cref="ISyncService"/> interface.</param>
        /// <param name="remotePath">The path to the directory on the device.</param>
        /// <param name="useV2"><see langword="true"/> to use <see cref="ISyncService.GetDirectoryListingAsync"/>; otherwise, use <see cref="ISyncService.GetDirectoryListingExAsync"/>.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the task.</param>
        /// <returns>An <see cref="IAsyncEnumerable{IFileStatistics}"/> which returns for each child item of the directory, a <see cref="FileStatistics"/> object with information of the item.</returns>
        public static IAsyncEnumerable<IFileStatistics> GetDirectoryAsyncListing(this ISyncService service, string remotePath, bool useV2 = false, CancellationToken cancellationToken = default) =>
            useV2 ? service.GetDirectoryAsyncListingEx(remotePath, cancellationToken) : service.GetDirectoryAsyncListing(remotePath, cancellationToken);
#endif

#if !NETFRAMEWORK || NET40_OR_GREATER
        /// <summary>
        /// Asynchronously pushes (uploads) a file to the remote device.
        /// </summary>
        /// <param name="service">An instance of a class that implements the <see cref="ISyncService"/> interface.</param>
        /// <param name="stream">A <see cref="Stream"/> that contains the contents of the file.</param>
        /// <param name="remotePath">The path, on the device, to which to push the file.</param>
        /// <param name="permissions">The <see cref="UnixFileStatus"/> that contains the permissions of the newly created file on the device.</param>
        /// <param name="timestamp">The time at which the file was last modified.</param>
        /// <param name="progress">An optional parameter which, when specified, returns progress notifications. The progress is reported as <see cref="SyncProgressChangedEventArgs"/>, representing the state of the file which has been transferred.</param>
        /// <param name="useV2"><see langword="true"/> to use <see cref="SyncCommand.SND2"/>; otherwise, <see langword="false"/> use <see cref="SyncCommand.SEND"/>.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the task.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public static Task PushAsync(this ISyncService service, Stream stream, string remotePath, UnixFileStatus permissions, DateTimeOffset timestamp, IProgress<SyncProgressChangedEventArgs>? progress = null, bool useV2 = false, CancellationToken cancellationToken = default) =>
            service.PushAsync(stream, remotePath, permissions, timestamp, progress.AsAction(), useV2, cancellationToken);

        /// <summary>
        /// Asynchronously pulls (downloads) a file from the remote device.
        /// </summary>
        /// <param name="service">An instance of a class that implements the <see cref="ISyncService"/> interface.</param>
        /// <param name="remotePath">The path, on the device, of the file to pull.</param>
        /// <param name="stream">A <see cref="Stream"/> that will receive the contents of the file.</param>
        /// <param name="progress">An optional parameter which, when specified, returns progress notifications. The progress is reported as <see cref="SyncProgressChangedEventArgs"/>, representing the state of the file which has been transferred.</param>
        /// <param name="useV2"><see langword="true"/> to use <see cref="SyncCommand.RCV2"/> and <see cref="SyncCommand.STA2"/>; otherwise, <see langword="false"/> use <see cref="SyncCommand.RECV"/> and <see cref="SyncCommand.STAT"/>.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the task.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        /// <remarks>V2 need Android 8 or above.</remarks>
        public static Task PullAsync(this ISyncService service, string remotePath, Stream stream, IProgress<SyncProgressChangedEventArgs>? progress = null, bool useV2 = false, CancellationToken cancellationToken = default) =>
            service.PullAsync(remotePath, stream, progress.AsAction(), useV2, cancellationToken);

#if HAS_WINRT
        /// <summary>
        /// Asynchronously pushes (uploads) a file to the remote device.
        /// </summary>
        /// <param name="service">An instance of a class that implements the <see cref="ISyncService.IWinRT"/> interface.</param>
        /// <param name="stream">A <see cref="IInputStream"/> that contains the contents of the file.</param>
        /// <param name="remotePath">The path, on the device, to which to push the file.</param>
        /// <param name="permissions">The <see cref="UnixFileStatus"/> that contains the permissions of the newly created file on the device.</param>
        /// <param name="timestamp">The time at which the file was last modified.</param>
        /// <param name="progress">An optional parameter which, when specified, returns progress notifications. The progress is reported as <see cref="SyncProgressChangedEventArgs"/>, representing the state of the file which has been transferred.</param>
        /// <param name="useV2"><see langword="true"/> to use <see cref="SyncCommand.SND2"/>; otherwise, <see langword="false"/> use <see cref="SyncCommand.SEND"/>.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the task.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public static Task PushAsync(this ISyncService.IWinRT service, IInputStream stream, string remotePath, UnixFileStatus permissions, DateTimeOffset timestamp, IProgress<SyncProgressChangedEventArgs>? progress = null, bool useV2 = false, CancellationToken cancellationToken = default) =>
            service.PushAsync(stream, remotePath, permissions, timestamp, progress.AsAction(), useV2, cancellationToken);

        /// <summary>
        /// Asynchronously pulls (downloads) a file from the remote device.
        /// </summary>
        /// <param name="service">An instance of a class that implements the <see cref="ISyncService.IWinRT"/> interface.</param>
        /// <param name="remotePath">The path, on the device, of the file to pull.</param>
        /// <param name="stream">A <see cref="IOutputStream"/> that will receive the contents of the file.</param>
        /// <param name="progress">An optional parameter which, when specified, returns progress notifications. The progress is reported as <see cref="SyncProgressChangedEventArgs"/>, representing the state of the file which has been transferred.</param>
        /// <param name="useV2"><see langword="true"/> to use <see cref="SyncCommand.RCV2"/> and <see cref="SyncCommand.STA2"/>; otherwise, <see langword="false"/> use <see cref="SyncCommand.RECV"/> and <see cref="SyncCommand.STAT"/>.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the task.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        /// <remarks>V2 need Android 8 or above.</remarks>
        public static Task PullAsync(this ISyncService.IWinRT service, string remotePath, IOutputStream stream, IProgress<SyncProgressChangedEventArgs>? progress = null, bool useV2 = false, CancellationToken cancellationToken = default) =>
            service.PullAsync(remotePath, stream, progress.AsAction(), useV2, cancellationToken);
#endif

#if NET7_0_OR_GREATER
        /// <summary>
        /// Asynchronously pushes (uploads) a file to the remote device.
        /// </summary>
        /// <param name="service">An instance of a class that implements the <see cref="ISyncService"/> interface.</param>
        /// <param name="stream">A <see cref="Stream"/> that contains the contents of the file.</param>
        /// <param name="remotePath">The path, on the device, to which to push the file.</param>
        /// <param name="permissions">The <see cref="UnixFileMode"/> that contains the permissions of the newly created file on the device.</param>
        /// <param name="timestamp">The time at which the file was last modified.</param>
        /// <param name="callback">An optional parameter which, when specified, returns progress notifications. The progress is reported as <see cref="SyncProgressChangedEventArgs"/>, representing the state of the file which has been transferred.</param>
        /// <param name="useV2"><see langword="true"/> to use <see cref="SyncCommand.SND2"/>; otherwise, <see langword="false"/> use <see cref="SyncCommand.SEND"/>.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the task.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public static Task PushAsync(this ISyncService service, Stream stream, string remotePath, UnixFileMode permissions, DateTimeOffset timestamp, Action<SyncProgressChangedEventArgs>? callback = null, bool useV2 = false, CancellationToken cancellationToken = default) =>
            service.PushAsync(stream, remotePath, (UnixFileStatus)permissions, timestamp, callback, useV2, cancellationToken);

        /// <summary>
        /// Asynchronously pushes (uploads) a file to the remote device.
        /// </summary>
        /// <param name="service">An instance of a class that implements the <see cref="ISyncService"/> interface.</param>
        /// <param name="stream">A <see cref="Stream"/> that contains the contents of the file.</param>
        /// <param name="remotePath">The path, on the device, to which to push the file.</param>
        /// <param name="permissions">The <see cref="UnixFileMode"/> that contains the permissions of the newly created file on the device.</param>
        /// <param name="timestamp">The time at which the file was last modified.</param>
        /// <param name="progress">An optional parameter which, when specified, returns progress notifications. The progress is reported as <see cref="SyncProgressChangedEventArgs"/>, representing the state of the file which has been transferred.</param>
        /// <param name="useV2"><see langword="true"/> to use <see cref="SyncCommand.SND2"/>; otherwise, <see langword="false"/> use <see cref="SyncCommand.SEND"/>.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the task.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public static Task PushAsync(this ISyncService service, Stream stream, string remotePath, UnixFileMode permissions, DateTimeOffset timestamp, IProgress<SyncProgressChangedEventArgs>? progress, bool useV2 = false, CancellationToken cancellationToken = default) =>
            service.PushAsync(stream, remotePath, (UnixFileStatus)permissions, timestamp, progress.AsAction(), useV2, cancellationToken);

#if WINDOWS10_0_17763_0_OR_GREATER
        /// <summary>
        /// Asynchronously pushes (uploads) a file to the remote device.
        /// </summary>
        /// <param name="service">An instance of a class that implements the <see cref="ISyncService.IWinRT"/> interface.</param>
        /// <param name="stream">A <see cref="IInputStream"/> that contains the contents of the file.</param>
        /// <param name="remotePath">The path, on the device, to which to push the file.</param>
        /// <param name="permissions">The <see cref="UnixFileMode"/> that contains the permissions of the newly created file on the device.</param>
        /// <param name="timestamp">The time at which the file was last modified.</param>
        /// <param name="callback">An optional parameter which, when specified, returns progress notifications. The progress is reported as <see cref="SyncProgressChangedEventArgs"/>, representing the state of the file which has been transferred.</param>
        /// <param name="useV2"><see langword="true"/> to use <see cref="SyncCommand.SND2"/>; otherwise, <see langword="false"/> use <see cref="SyncCommand.SEND"/>.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the task.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public static Task PushAsync(this ISyncService.IWinRT service, IInputStream stream, string remotePath, UnixFileMode permissions, DateTimeOffset timestamp, Action<SyncProgressChangedEventArgs>? callback = null, bool useV2 = false, CancellationToken cancellationToken = default) =>
            service.PushAsync(stream, remotePath, (UnixFileStatus)permissions, timestamp, callback, useV2, cancellationToken);

        /// <summary>
        /// Asynchronously pushes (uploads) a file to the remote device.
        /// </summary>
        /// <param name="service">An instance of a class that implements the <see cref="ISyncService.IWinRT"/> interface.</param>
        /// <param name="stream">A <see cref="IInputStream"/> that contains the contents of the file.</param>
        /// <param name="remotePath">The path, on the device, to which to push the file.</param>
        /// <param name="permissions">The <see cref="UnixFileMode"/> that contains the permissions of the newly created file on the device.</param>
        /// <param name="timestamp">The time at which the file was last modified.</param>
        /// <param name="progress">An optional parameter which, when specified, returns progress notifications. The progress is reported as <see cref="SyncProgressChangedEventArgs"/>, representing the state of the file which has been transferred.</param>
        /// <param name="useV2"><see langword="true"/> to use <see cref="SyncCommand.SND2"/>; otherwise, <see langword="false"/> use <see cref="SyncCommand.SEND"/>.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the task.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public static Task PushAsync(this ISyncService.IWinRT service, IInputStream stream, string remotePath, UnixFileMode permissions, DateTimeOffset timestamp, IProgress<SyncProgressChangedEventArgs>? progress, bool useV2 = false, CancellationToken cancellationToken = default) =>
            service.PushAsync(stream, remotePath, (UnixFileStatus)permissions, timestamp, progress.AsAction(), useV2, cancellationToken);
#endif
#endif
#endif
    }
}
#endif