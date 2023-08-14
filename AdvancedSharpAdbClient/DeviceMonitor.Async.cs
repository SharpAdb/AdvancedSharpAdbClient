#if HAS_TASK
// <copyright file="DeviceMonitor.Async.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using AdvancedSharpAdbClient.Exceptions;
using System;
using System.Threading;

namespace AdvancedSharpAdbClient
{
    public partial class DeviceMonitor
    {
        /// <summary>
        /// When the <see cref="Start"/> method is called, this <see cref="ManualResetEvent"/>
        /// is used to block the <see cref="Start"/> method until the <see cref="DeviceMonitorLoopAsync"/>
        /// has processed the first list of devices.
        /// </summary>
        private readonly ManualResetEvent firstDeviceListParsed = new(false);

        /// <summary>
        /// A <see cref="CancellationToken"/> that can be used to cancel the <see cref="monitorTask"/>.
        /// </summary>
        private readonly CancellationTokenSource monitorTaskCancellationTokenSource = new();

        /// <summary>
        /// The <see cref="Task"/> that monitors the <see cref="Socket"/> and waits for device notifications.
        /// </summary>
        private Task monitorTask;

        /// <inheritdoc/>
        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            if (monitorTask == null)
            {
                _ = firstDeviceListParsed.Reset();

                monitorTask = Utilities.Run(() => DeviceMonitorLoopAsync(monitorTaskCancellationTokenSource.Token), cancellationToken);

                // Wait for the worker thread to have read the first list of devices.
                _ = await Utilities.Run(firstDeviceListParsed.WaitOne, cancellationToken);
            }
        }

        /// <summary>
        /// Stops the monitoring
        /// </summary>
        protected virtual async
#if NETCOREAPP2_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER
            ValueTask
#else
            Task
#endif
            DisposeAsyncCore()
        {
            if (disposed) { return; }

            // First kill the monitor task, which has a dependency on the socket,
            // then close the socket.
            if (monitorTask != null)
            {
                IsRunning = false;

                // Stop the thread. The tread will keep waiting for updated information from adb
                // eternally, so we need to forcefully abort it here.
                monitorTaskCancellationTokenSource.Cancel();
                await monitorTask;
#if HAS_PROCESS
                monitorTask.Dispose();
#endif
                monitorTask = null;
            }

            // Close the connection to adb. To be done after the monitor task exited.
            if (Socket != null)
            {
                Socket.Dispose();
                Socket = null;
            }

            firstDeviceListParsed.Dispose();
            monitorTaskCancellationTokenSource.Dispose();

            disposed = true;
        }

        /// <inheritdoc/>
        public async
#if NETCOREAPP2_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER
            ValueTask
#else
            Task
#endif
            DisposeAsync()
        {
            await DisposeAsyncCore().ConfigureAwait(false);
#if NETCOREAPP3_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
            GC.SuppressFinalize(this);
#else
            Dispose();
#endif
        }

        /// <summary>
        /// Monitors the devices. This connects to the Debug Bridge
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        private async Task DeviceMonitorLoopAsync(CancellationToken cancellationToken = default)
        {
            IsRunning = true;

            // Set up the connection to track the list of devices.
            await InitializeSocketAsync(cancellationToken);

            do
            {
                try
                {
                    string value = await Socket.ReadStringAsync(cancellationToken).ConfigureAwait(false);
                    ProcessIncomingDeviceData(value);

                    firstDeviceListParsed.Set();
                }
#if HAS_LOGGER
                catch (TaskCanceledException ex)
#else
                catch (TaskCanceledException)
#endif
                {
                    // We get a TaskCanceledException on Windows
                    if (cancellationToken.IsCancellationRequested)
                    {
                        // The DeviceMonitor is shutting down (disposing) and Dispose()
                        // has called cancellationToken.Cancel(). This exception is expected,
                        // so we can safely swallow it.
                    }
                    else
                    {
                        // The exception was unexpected, so log it & rethrow.
#if HAS_LOGGER
                        logger.LogError(ex, ex.Message);
#endif
                        throw;
                    }
                }
#if HAS_LOGGER
                catch (ObjectDisposedException ex)
#else
                catch (ObjectDisposedException)
#endif
                {
                    // ... but an ObjectDisposedException on .NET Core on Linux and macOS.
                    if (cancellationToken.IsCancellationRequested)
                    {
                        // The DeviceMonitor is shutting down (disposing) and Dispose()
                        // has called cancellationToken.Cancel(). This exception is expected,
                        // so we can safely swallow it.
                    }
                    else
                    {
                        // The exception was unexpected, so log it & rethrow.
#if HAS_LOGGER
                        logger.LogError(ex, ex.Message);
#endif
                        throw;
                    }
                }
                catch (AdbException adbException)
                {
                    if (adbException.ConnectionReset)
                    {
                        // The adb server was killed, for whatever reason. Try to restart it and recover from this.
                        await AdbServer.Instance.RestartServerAsync(cancellationToken);
                        Socket.Reconnect();
                        await InitializeSocketAsync(cancellationToken);
                    }
                    else
                    {
                        throw;
                    }
                }
#if HAS_LOGGER
                catch (Exception ex)
                {
                    // The exception was unexpected, so log it & rethrow.
                    logger.LogError(ex, ex.Message);
#else
                catch (Exception)
                {
#endif
                    throw;
                }
            }
            while (!cancellationToken.IsCancellationRequested);
        }

        private async Task InitializeSocketAsync(CancellationToken cancellationToken)
        {
            // Set up the connection to track the list of devices.
            await Socket.SendAdbRequestAsync("host:track-devices", cancellationToken);
            _ = await Socket.ReadAdbResponseAsync(cancellationToken);
        }
    }
}
#endif