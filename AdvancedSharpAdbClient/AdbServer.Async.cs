#if HAS_TASK
// <copyright file="AdbServer.Async.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.Globalization;
using System.Net.Sockets;
using System.Threading;

namespace AdvancedSharpAdbClient
{
    public partial class AdbServer
    {
        /// <inheritdoc/>
        public async Task<StartServerResult> StartServerAsync(string adbPath, bool restartServerIfNewer = false, CancellationToken cancellationToken = default)
        {
            if (IsStarting) { return StartServerResult.Starting; }
            try
            {
                AdbServerStatus serverStatus = await GetStatusAsync(cancellationToken).ConfigureAwait(false);
                Version? commandLineVersion = null;

                IAdbCommandLineClient commandLineClient = AdbCommandLineClientFactory(adbPath);

                if (await commandLineClient.CheckAdbFileExistsAsync(adbPath, cancellationToken).ConfigureAwait(false))
                {
                    CachedAdbPath = adbPath;
                    commandLineVersion = await commandLineClient.GetVersionAsync(cancellationToken).ContinueWith(x => x.Result.AdbVersion).ConfigureAwait(false);
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
                        || (restartServerIfNewer && serverStatus.Version < commandLineVersion))
                    {
                        await StopServerAsync(cancellationToken);
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
                    await commandLineClient.StartServerAsync(cancellationToken);
                    return StartServerResult.Started;
                }
            }
            finally
            {
                IsStarting = false;
            }
        }

        /// <inheritdoc/>
        public Task<StartServerResult> RestartServerAsync(CancellationToken cancellationToken = default) =>
            StartServerAsync(CachedAdbPath!, true, cancellationToken);

        /// <inheritdoc/>
        public Task<StartServerResult> RestartServerAsync(string adbPath, CancellationToken cancellationToken = default) =>
            StringExtensions.IsNullOrWhiteSpace(adbPath) ? RestartServerAsync(cancellationToken) : StartServerAsync(adbPath, true, cancellationToken);

        /// <inheritdoc/>
        public async Task StopServerAsync(CancellationToken cancellationToken = default)
        {
            using IAdbSocket socket = CreateAdbSocket();
            await socket.SendAdbRequestAsync("host:kill", cancellationToken).ConfigureAwait(false);

            // The host will immediately close the connection after the kill
            // command has been sent; no need to read the response.
        }

        /// <inheritdoc/>
        public async Task<AdbServerStatus> GetStatusAsync(CancellationToken cancellationToken = default)
        {
            // Try to connect to a running instance of the adb server
            try
            {
                using IAdbSocket socket = CreateAdbSocket();
                await socket.SendAdbRequestAsync("host:version", cancellationToken).ConfigureAwait(false);
                AdbResponse response = await socket.ReadAdbResponseAsync(cancellationToken).ConfigureAwait(false);
                string version = await socket.ReadStringAsync(cancellationToken).ConfigureAwait(false);

                int versionCode = int.Parse(version, NumberStyles.HexNumber);
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