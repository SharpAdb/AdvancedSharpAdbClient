#if HAS_TASK
// <copyright file="DeviceMonitor.Async.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;

namespace AdvancedSharpAdbClient
{
    public partial class DeviceMonitor
    {
        /// <summary>
        /// When the <see cref="StartAsync(CancellationToken)"/> method is called, this <see cref="ManualResetEvent"/>
        /// is used to block the <see cref="StartAsync(CancellationToken)"/> method until the <see cref="DeviceMonitorLoopAsync"/>
        /// has processed the first list of devices.
        /// </summary>
        protected TaskCompletionSource<object?>? FirstDeviceListParsed;

        /// <summary>
        /// A <see cref="CancellationToken"/> that can be used to cancel the <see cref="MonitorTask"/>.
        /// </summary>
        protected readonly CancellationTokenSource MonitorTaskCancellationTokenSource = new();

        /// <summary>
        /// The <see cref="Task"/> that monitors the <see cref="Socket"/> and waits for device notifications.
        /// </summary>
        protected Task? MonitorTask;

        /// <inheritdoc/>
        [MemberNotNull(nameof(MonitorTask))]
        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            if (MonitorTask == null)
            {
                try
                {
                    FirstDeviceListParsed = new TaskCompletionSource<object?>();
                    using CancellationTokenRegistration cancellationTokenRegistration = cancellationToken.Register(() => FirstDeviceListParsed.SetCanceled());
                    MonitorTask = DeviceMonitorLoopAsync(MonitorTaskCancellationTokenSource.Token);
                    // Wait for the worker thread to have read the first list of devices.
                    _ = await FirstDeviceListParsed.Task.ConfigureAwait(false);
                }
                finally
                {
                    FirstDeviceListParsed = null;
                }
            }
        }

        /// <summary>
        /// Asynchronously stops the monitoring
        /// </summary>
        protected virtual async Task DisposeAsyncCore()
        {
            if (disposed) { return; }

            // First kill the monitor task, which has a dependency on the socket,
            // then close the socket.
            if (MonitorTask != null)
            {
                IsRunning = false;

                // Stop the thread. The tread will keep waiting for updated information from adb
                // eternally, so we need to forcefully abort it here.
#if NET8_0_OR_GREATER
                await MonitorTaskCancellationTokenSource.CancelAsync();
#else
                MonitorTaskCancellationTokenSource.Cancel();
#endif
                await MonitorTask.ConfigureAwait(false);
#if HAS_PROCESS
                MonitorTask.Dispose();
#endif
                MonitorTask = null;
            }

            // Close the connection to adb. To be done after the monitor task exited.
            if (Socket != null)
            {
                Socket.Dispose();
                Socket = null!;
            }

            MonitorTaskCancellationTokenSource.Dispose();

            disposed = true;
        }

#if COMP_NETSTANDARD2_1
        /// <inheritdoc/>
        async ValueTask IAsyncDisposable.DisposeAsync()
        {
            await DisposeAsyncCore().ConfigureAwait(false);
            Dispose(disposing: false);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        public Task DisposeAsync() => ((IAsyncDisposable)this).DisposeAsync().AsTask();
#else
        /// <inheritdoc/>
        public async Task DisposeAsync()
        {
            await DisposeAsyncCore().ConfigureAwait(false);
            Dispose();
        }
#endif

        /// <summary>
        /// Asynchronously monitors the devices. This connects to the Debug Bridge
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        protected virtual async Task DeviceMonitorLoopAsync(CancellationToken cancellationToken = default)
        {
            IsRunning = true;
            await this; // Switch to the background thread, so that the loop can continue to run.

            // Set up the connection to track the list of devices.
            await InitializeSocketAsync(cancellationToken).ConfigureAwait(false);

            do
            {
                try
                {
                    string value = await Socket.ReadStringAsync(cancellationToken).ConfigureAwait(false);
                    ProcessIncomingDeviceData(value);
                    if (FirstDeviceListParsed != null)
                    {
                        // Switch to the background thread to avoid blocking the caller.
                        _ = Task.Factory.StartNew(() => FirstDeviceListParsed?.TrySetResult(null), default, TaskCreationOptions.None, TaskScheduler.Default);
                    }
                }
                catch (TaskCanceledException ex)
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
                        logger.LogError(ex, ex.Message);
                        throw;
                    }
                }
                catch (ObjectDisposedException ex)
                {
                    // ... but an ObjectDisposedException on .NET Core App on Linux and macOS.
                    if (cancellationToken.IsCancellationRequested)
                    {
                        // The DeviceMonitor is shutting down (disposing) and Dispose()
                        // has called cancellationToken.Cancel(). This exception is expected,
                        // so we can safely swallow it.
                    }
                    else
                    {
                        // The exception was unexpected, so log it & rethrow.
                        logger.LogError(ex, ex.Message);
                        throw;
                    }
                }
                catch (OperationCanceledException ex)
                {
                    // ... and an OperationCanceledException on .NET Core App 2.1 or greater.
                    if (cancellationToken.IsCancellationRequested)
                    {
                        // The DeviceMonitor is shutting down (disposing) and Dispose()
                        // has called cancellationToken.Cancel(). This exception is expected,
                        // so we can safely swallow it.
                    }
                    else
                    {
                        // The exception was unexpected, so log it & rethrow.
                        logger.LogError(ex, ex.Message);
                        throw;
                    }
                }
                catch (AdbException adbException)
                {
                    if (adbException.InnerException is SocketException ex)
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            // The DeviceMonitor is shutting down (disposing) and Dispose()
                            // has called Socket.Close(). This exception is expected,
                            // so we can safely swallow it.
                        }
                        else if (adbException.ConnectionReset)
                        {
                            // The adb server was killed, for whatever reason. Try to restart it and recover from this.
                            await AdbServer.Instance.RestartServerAsync(cancellationToken).ConfigureAwait(false);
                            Socket.Reconnect(false);
                            await InitializeSocketAsync(cancellationToken).ConfigureAwait(false);
                        }
                        else
                        {
                            // The exception was unexpected, so log it & rethrow.
                            logger.LogError(ex, ex.Message);
                            throw ex;
                        }
                    }
                    else
                    {
                        // The exception was unexpected, so log it & rethrow.
                        logger.LogError(adbException, adbException.Message);
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    // The exception was unexpected, so log it & rethrow.
                    logger.LogError(ex, ex.Message);
                    throw;
                }
            }
            while (!cancellationToken.IsCancellationRequested);
        }

        /// <summary>
        /// Initializes the <see cref="Socket"/> and sends the <c>host:track-devices</c> command to the adb server.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        private async Task InitializeSocketAsync(CancellationToken cancellationToken)
        {
            // Set up the connection to track the list of devices.
            await Socket.SendAdbRequestAsync("host:track-devices", cancellationToken).ConfigureAwait(false);
            _ = await Socket.ReadAdbResponseAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets an awaiter used to switch to background thread.
        /// </summary>
        /// <returns>An awaiter instance.</returns>
        private ThreadSwitcher GetAwaiter() => new();

        /// <summary>
        /// A helper type for switch thread by <see cref="Task"/>. This type is not intended to be used directly from your code.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        private readonly struct ThreadSwitcher : INotifyCompletion
        {
            /// <summary>
            /// Gets a value that indicates whether the asynchronous operation has completed.
            /// </summary>
            public bool IsCompleted => false;

            /// <summary>
            /// Ends the await on the completed task.
            /// </summary>
            public void GetResult() { }

            /// <inheritdoc/>
            public void OnCompleted(Action continuation) => _ = Task.Factory.StartNew(continuation, default, TaskCreationOptions.None, TaskScheduler.Default);
        }
    }
}
#endif