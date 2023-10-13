#if HAS_TASK
// <copyright file="AdbServer.Async.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using AdvancedSharpAdbClient.Exceptions;
using System;
using System.Net.Sockets;
using System.Threading;

namespace AdvancedSharpAdbClient
{
    public partial class AdbServer
    {
        /// <inheritdoc/>
        public virtual async Task<StartServerResult> StartServerAsync(string adbPath, bool restartServerIfNewer = false, CancellationToken cancellationToken = default)
        {
            AdbServerStatus serverStatus = await GetStatusAsync(cancellationToken).ConfigureAwait(false);
            Version commandLineVersion = null;

            IAdbCommandLineClient commandLineClient = adbCommandLineClientFactory(adbPath);
            CheckFileExists = commandLineClient.CheckFileExists;

            if (commandLineClient.CheckFileExists(adbPath))
            {
                CachedAdbPath = adbPath;
                commandLineVersion = await commandLineClient.GetVersionAsync(cancellationToken).ConfigureAwait(false);
            }

            // If the server is running, and no adb path is provided, check if we have the minimum version
            if (adbPath == null)
            {
                return !serverStatus.IsRunning
                    ? throw new AdbException("The adb server is not running, but no valid path to the adb.exe executable was provided. The adb server cannot be started.")
                    : serverStatus.Version >= RequiredAdbVersion
                    ? StartServerResult.AlreadyRunning
                    : throw new AdbException($"The adb daemon is running an outdated version ${commandLineVersion}, but not valid path to the adb.exe executable was provided. A more recent version of the adb server cannot be started.");
            }

            if (serverStatus.IsRunning)
            {
                if (serverStatus.Version < RequiredAdbVersion
                    || (serverStatus.Version < commandLineVersion && restartServerIfNewer))
                {
                    ExceptionExtensions.ThrowIfNull(adbPath);

                    await adbClient.KillAdbAsync(cancellationToken);
                    await commandLineClient.StartServerAsync(cancellationToken);
                    return StartServerResult.RestartedOutdatedDaemon;
                }
                else
                {
                    return StartServerResult.AlreadyRunning;
                }
            }
            else
            {
                ExceptionExtensions.ThrowIfNull(adbPath);

                await commandLineClient.StartServerAsync(cancellationToken);
                return StartServerResult.Started;
            }
        }

        /// <inheritdoc/>
        public Task<StartServerResult> RestartServerAsync(CancellationToken cancellationToken = default) => RestartServerAsync(null, cancellationToken);

        /// <inheritdoc/>
        public virtual async Task<StartServerResult> RestartServerAsync(string adbPath, CancellationToken cancellationToken = default)
        {
            adbPath ??= CachedAdbPath;

            if (!CheckFileExists(adbPath))
            {
                throw new InvalidOperationException($"The adb server was not started via {nameof(AdbServer)}.{nameof(this.StartServer)} or no path to adb was specified. The adb server cannot be restarted.");
            }

            using ManualResetEvent manualResetEvent =
                await Extensions.Run(() =>
                {
                    lock (RestartLock)
                    {
                        return new ManualResetEvent(false);
                    }
                }, cancellationToken).ConfigureAwait(false);

            _ = Extensions.Run(() =>
            {
                lock (RestartLock)
                {
                    manualResetEvent.WaitOne();
                }
            }, cancellationToken);

            StartServerResult result = await StartServerAsync(adbPath, false, cancellationToken).ConfigureAwait(false);
            manualResetEvent.Set();
            return result;
        }

        /// <inheritdoc/>
        public virtual async Task<AdbServerStatus> GetStatusAsync(CancellationToken cancellationToken = default)
        {
            // Try to connect to a running instance of the adb server
            try
            {
                int versionCode = await adbClient.GetAdbVersionAsync(cancellationToken).ConfigureAwait(false);
                return new AdbServerStatus(true, new Version(1, 0, versionCode));
            }
            catch (AggregateException ex)
            {
                if (ex.InnerException is SocketException exception)
                {
                    if (exception.SocketErrorCode == SocketError.ConnectionRefused)
                    {
                        return new AdbServerStatus(false, null);
                    }
                    else
                    {
                        // An unexpected exception occurred; re-throw the exception
                        throw exception;
                    }
                }
                else
                {
                    throw;
                }
            }
            catch (SocketException ex)
            {
                if (ex.SocketErrorCode == SocketError.ConnectionRefused)
                {
                    return new AdbServerStatus(false, null);
                }
                else
                {
                    // An unexpected exception occurred; re-throw the exception
                    throw;
                }
            }
        }
    }
}
#endif