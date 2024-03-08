#if (HAS_TASK && !NETFRAMEWORK) || NET40_OR_GREATER
// <copyright file="SyncServiceExtensions.Async.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.IO;
using System.Threading;

namespace AdvancedSharpAdbClient
{
    public static partial class SyncServiceExtensions
    {
        /// <summary>
        /// Asynchronously pushes (uploads) a file to the remote device.
        /// </summary>
        /// <param name="service">An instance of a class that implements the <see cref="ISyncService"/> interface.</param>
        /// <param name="stream">A <see cref="Stream"/> that contains the contents of the file.</param>
        /// <param name="remotePath">The path, on the device, to which to push the file.</param>
        /// <param name="permissions">The permission octet that contains the permissions of the newly created file on the device.</param>
        /// <param name="timestamp">The time at which the file was last modified.</param>
        /// <param name="progress">An optional parameter which, when specified, returns progress notifications. The progress is reported as <see cref="SyncProgressChangedEventArgs"/>, representing the state of the file which has been transferred.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the task.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        /// <remarks>The <paramref name="permissions"/> should coverts to a decimal number. For example, <c>644</c> should be <c>420</c> in decimal, <c>&amp;O644</c> in VB.NET and <c>0o644</c> in F# and Python.</remarks>
        public static Task PushAsync(this ISyncService service, Stream stream, string remotePath, int permissions, DateTimeOffset timestamp, IProgress<SyncProgressChangedEventArgs>? progress = null, CancellationToken cancellationToken = default) =>
            service.PushAsync(stream, remotePath, permissions, timestamp, progress.AsAction(), cancellationToken);

        /// <summary>
        /// Asynchronously pulls (downloads) a file from the remote device.
        /// </summary>
        /// <param name="service">An instance of a class that implements the <see cref="ISyncService"/> interface.</param>
        /// <param name="remotePath">The path, on the device, of the file to pull.</param>
        /// <param name="stream">A <see cref="Stream"/> that will receive the contents of the file.</param>
        /// <param name="progress">An optional parameter which, when specified, returns progress notifications. The progress is reported as <see cref="SyncProgressChangedEventArgs"/>, representing the state of the file which has been transferred.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the task.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public static Task PullAsync(this ISyncService service, string remotePath, Stream stream, IProgress<SyncProgressChangedEventArgs>? progress = null, CancellationToken cancellationToken = default) =>
            service.PullAsync(remotePath, stream, progress.AsAction(), cancellationToken);

#if WINDOWS_UWP || WINDOWS10_0_17763_0_OR_GREATER
        /// <summary>
        /// Asynchronously pushes (uploads) a file to the remote device.
        /// </summary>
        /// <param name="service">An instance of a class that implements the <see cref="ISyncService.IWinRT"/> interface.</param>
        /// <param name="stream">A <see cref="IInputStream"/> that contains the contents of the file.</param>
        /// <param name="remotePath">The path, on the device, to which to push the file.</param>
        /// <param name="permissions">The permission octet that contains the permissions of the newly created file on the device.</param>
        /// <param name="timestamp">The time at which the file was last modified.</param>
        /// <param name="progress">An optional parameter which, when specified, returns progress notifications. The progress is reported as <see cref="SyncProgressChangedEventArgs"/>, representing the state of the file which has been transferred.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the task.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        /// <remarks>The <paramref name="permissions"/> should coverts to a decimal number. For example, <c>644</c> should be <c>420</c> in decimal, <c>&amp;O644</c> in VB.NET and <c>0o644</c> in F# and Python.</remarks>
        public static Task PushAsync(this ISyncService.IWinRT service, IInputStream stream, string remotePath, int permissions, DateTimeOffset timestamp, IProgress<SyncProgressChangedEventArgs>? progress = null, CancellationToken cancellationToken = default) =>
            service.PushAsync(stream, remotePath, permissions, timestamp, progress.AsAction(), cancellationToken);

        /// <summary>
        /// Asynchronously pulls (downloads) a file from the remote device.
        /// </summary>
        /// <param name="service">An instance of a class that implements the <see cref="ISyncService.IWinRT"/> interface.</param>
        /// <param name="remotePath">The path, on the device, of the file to pull.</param>
        /// <param name="stream">A <see cref="IOutputStream"/> that will receive the contents of the file.</param>
        /// <param name="progress">An optional parameter which, when specified, returns progress notifications. The progress is reported as <see cref="SyncProgressChangedEventArgs"/>, representing the state of the file which has been transferred.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the task.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public static Task PullAsync(this ISyncService.IWinRT service, string remotePath, IOutputStream stream, IProgress<SyncProgressChangedEventArgs>? progress = null, CancellationToken cancellationToken = default) =>
            service.PullAsync(remotePath, stream, progress.AsAction(), cancellationToken);
#endif

#if NET7_0_OR_GREATER
        /// <summary>
        /// Asynchronously pushes (uploads) a file to the remote device.
        /// </summary>
        /// <param name="service">An instance of a class that implements the <see cref="ISyncService"/> interface.</param>
        /// <param name="stream">A <see cref="Stream"/> that contains the contents of the file.</param>
        /// <param name="remotePath">The path, on the device, to which to push the file.</param>
        /// <param name="permission">The <see cref="UnixFileMode"/> that contains the permissions of the newly created file on the device.</param>
        /// <param name="timestamp">The time at which the file was last modified.</param>
        /// <param name="callback">An optional parameter which, when specified, returns progress notifications. The progress is reported as <see cref="SyncProgressChangedEventArgs"/>, representing the state of the file which has been transferred.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the task.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public static Task PushAsync(this ISyncService service, Stream stream, string remotePath, UnixFileMode permission, DateTimeOffset timestamp, Action<SyncProgressChangedEventArgs>? callback = null, CancellationToken cancellationToken = default) =>
            service.PushAsync(stream, remotePath, (int)permission, timestamp, callback, cancellationToken);

        /// <summary>
        /// Asynchronously pushes (uploads) a file to the remote device.
        /// </summary>
        /// <param name="service">An instance of a class that implements the <see cref="ISyncService"/> interface.</param>
        /// <param name="stream">A <see cref="Stream"/> that contains the contents of the file.</param>
        /// <param name="remotePath">The path, on the device, to which to push the file.</param>
        /// <param name="permission">The <see cref="UnixFileMode"/> that contains the permissions of the newly created file on the device.</param>
        /// <param name="timestamp">The time at which the file was last modified.</param>
        /// <param name="progress">An optional parameter which, when specified, returns progress notifications. The progress is reported as <see cref="SyncProgressChangedEventArgs"/>, representing the state of the file which has been transferred.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the task.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public static Task PushAsync(this ISyncService service, Stream stream, string remotePath, UnixFileMode permission, DateTimeOffset timestamp, IProgress<SyncProgressChangedEventArgs>? progress, CancellationToken cancellationToken = default) =>
            service.PushAsync(stream, remotePath, (int)permission, timestamp, progress.AsAction(), cancellationToken);

#if WINDOWS10_0_17763_0_OR_GREATER
        /// <summary>
        /// Asynchronously pushes (uploads) a file to the remote device.
        /// </summary>
        /// <param name="service">An instance of a class that implements the <see cref="ISyncService.IWinRT"/> interface.</param>
        /// <param name="stream">A <see cref="IInputStream"/> that contains the contents of the file.</param>
        /// <param name="remotePath">The path, on the device, to which to push the file.</param>
        /// <param name="permission">The <see cref="UnixFileMode"/> that contains the permissions of the newly created file on the device.</param>
        /// <param name="timestamp">The time at which the file was last modified.</param>
        /// <param name="callback">An optional parameter which, when specified, returns progress notifications. The progress is reported as <see cref="SyncProgressChangedEventArgs"/>, representing the state of the file which has been transferred.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the task.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public static Task PushAsync(this ISyncService.IWinRT service, IInputStream stream, string remotePath, UnixFileMode permission, DateTimeOffset timestamp, Action<SyncProgressChangedEventArgs>? callback = null, CancellationToken cancellationToken = default) =>
            service.PushAsync(stream, remotePath, (int)permission, timestamp, callback, cancellationToken);

        /// <summary>
        /// Asynchronously pushes (uploads) a file to the remote device.
        /// </summary>
        /// <param name="service">An instance of a class that implements the <see cref="ISyncService.IWinRT"/> interface.</param>
        /// <param name="stream">A <see cref="IInputStream"/> that contains the contents of the file.</param>
        /// <param name="remotePath">The path, on the device, to which to push the file.</param>
        /// <param name="permission">The <see cref="UnixFileMode"/> that contains the permissions of the newly created file on the device.</param>
        /// <param name="timestamp">The time at which the file was last modified.</param>
        /// <param name="progress">An optional parameter which, when specified, returns progress notifications. The progress is reported as <see cref="SyncProgressChangedEventArgs"/>, representing the state of the file which has been transferred.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the task.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public static Task PushAsync(this ISyncService.IWinRT service, IInputStream stream, string remotePath, UnixFileMode permission, DateTimeOffset timestamp, IProgress<SyncProgressChangedEventArgs>? progress, CancellationToken cancellationToken = default) =>
            service.PushAsync(stream, remotePath, (int)permission, timestamp, progress.AsAction(), cancellationToken);
#endif
#endif
    }
}
#endif