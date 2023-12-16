#if HAS_TASK
// <copyright file="IAdbServer.Async.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System.Threading;

namespace AdvancedSharpAdbClient
{
    public partial interface IAdbServer
    {
        /// <summary>
        /// Asynchronously starts the adb server if it was not previously running.
        /// </summary>
        /// <param name="adbPath">The path to the <c>adb.exe</c> executable that can be used to start the adb server.
        /// If this path is not provided, this method will throw an exception if the server
        /// is not running or is not up to date.</param>
        /// <param name="restartServerIfNewer"><see langword="true"/> to restart the adb server if the version of the <c>adb.exe</c>
        /// executable at <paramref name="adbPath"/> is newer than the version that is currently
        /// running; <see langword="false"/> to keep a previous version of the server running.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>
        /// A <see cref="Task{StartServerResult}"/> which return
        /// <list type="ordered">
        /// <item>
        ///   <see cref="StartServerResult.AlreadyRunning"/> if the adb server was already
        ///   running and the version of the adb server was at least <see cref="AdbServer.RequiredAdbVersion"/>.
        /// </item>
        /// <item>
        ///   <see cref="StartServerResult.RestartedOutdatedDaemon"/> if the adb server
        ///   was already running, but the version was less than <see cref="AdbServer.RequiredAdbVersion"/>
        ///   or less than the version of the adb client at <paramref name="adbPath"/> and the
        ///   <paramref name="restartServerIfNewer"/> flag was set.
        /// </item>
        /// <item>
        ///   <see cref="StartServerResult.Started"/> if the adb server was not running,
        ///   and the server was started.
        /// </item>
        /// <item>
        ///   <see cref="StartServerResult.Starting"/> if a <see cref="StartServerAsync(string, bool, CancellationToken)"/>
        ///   operation is already in progress.
        /// </item>
        /// </list>
        /// </returns>
        /// <exception cref="AdbException">The server was not running, or an outdated version of the server was running,
        /// and the <paramref name="adbPath"/> parameter was not specified.</exception>
        Task<StartServerResult> StartServerAsync(string adbPath, bool restartServerIfNewer, CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously restarts the adb server if it suddenly became unavailable. Call this class if, for example,
        /// you receive an <see cref="AdbException"/> with the <see cref="AdbException.ConnectionReset"/> flag
        /// set to <see langword="true"/> - a clear indicating the ADB server died.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        /// <remarks>You can only call this method if you have previously started the adb server via
        /// <see cref="StartServerAsync(string, bool, CancellationToken)"/> and passed the full path to the adb server.</remarks>
        Task<StartServerResult> RestartServerAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously restarts the adb server with new adb path if it suddenly became unavailable. Call this class if, for example,
        /// you receive an <see cref="AdbException"/> with the <see cref="AdbException.ConnectionReset"/> flag
        /// set to <see langword="true"/> - a clear indicating the ADB server died.
        /// </summary>
        /// <param name="adbPath">The path to the <c>adb.exe</c> executable that can be used to start the adb server.
        /// If this path is not provided, this method will use the path that was cached by
        /// <see cref="StartServerAsync(string, bool, CancellationToken)"/></param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        /// <remarks>You can only call this method if you have previously started the adb server via
        /// <see cref="StartServerAsync(string, bool, CancellationToken)"/> and passed the full path to the adb server.</remarks>
        Task<StartServerResult> RestartServerAsync(string adbPath, CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously stop the adb server asynchronously.
        /// </summary>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        Task StopServerAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously gets the status of the adb server asynchronously.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task{AdbServerStatus}"/> which returns a <see cref="AdbServerStatus"/> object that describes the status of the adb server.</returns>
        Task<AdbServerStatus> GetStatusAsync(CancellationToken cancellationToken);
    }
}
#endif