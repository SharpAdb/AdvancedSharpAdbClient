// <copyright file="DeviceMonitor.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using AdvancedSharpAdbClient.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

#if HAS_LOGGER
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
#endif

namespace AdvancedSharpAdbClient
{
    /// <summary>
    /// <para>
    /// A Device monitor. This connects to the Android Debug Bridge and get device and
    /// debuggable process information from it.
    /// </para>
    /// </summary>
    /// <example>
    /// <para>
    /// To receive notifications when devices connect to or disconnect from your PC, you can use the following code:
    /// </para>
    /// <code>
    /// void Test()
    /// {
    ///     var monitor = new DeviceMonitor(new AdbSocket());
    ///     monitor.DeviceConnected += this.OnDeviceConnected;
    ///     monitor.Start();
    /// }
    ///
    /// void OnDeviceConnected(object sender, DeviceDataEventArgs e)
    /// {
    ///     Console.WriteLine($"The device {e.Device.Name} has connected to this PC");
    /// }
    /// </code>
    /// </example>
    public class DeviceMonitor : IDeviceMonitor, IDisposable
    {
#if HAS_LOGGER
        /// <summary>
        /// The logger to use when logging messages.
        /// </summary>
        private readonly ILogger<DeviceMonitor> logger;
#endif

        /// <summary>
        /// The list of devices currently connected to the Android Debug Bridge.
        /// </summary>
        private readonly List<DeviceData> devices;

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

#if !HAS_LOGGER
#pragma warning disable CS1572 // XML 注释中有 param 标记，但是没有该名称的参数
#endif
        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceMonitor"/> class.
        /// </summary>
        /// <param name="socket">The <see cref="IAdbSocket"/> that manages the connection with the adb server.</param>
        /// <param name="logger">The logger to use when logging.</param>
        public DeviceMonitor(IAdbSocket socket
#if HAS_LOGGER
            , ILogger<DeviceMonitor> logger = null
#endif
            )
        {
            Socket = socket ?? throw new ArgumentNullException(nameof(socket));
            devices = new List<DeviceData>();
            Devices = devices.AsReadOnly();
#if HAS_LOGGER
            this.logger = logger ?? NullLogger<DeviceMonitor>.Instance;
#endif
        }
#if !HAS_LOGGER
#pragma warning restore CS1572 // XML 注释中有 param 标记，但是没有该名称的参数
#endif

        /// <inheritdoc/>
        public event EventHandler<DeviceDataEventArgs> DeviceChanged;

        /// <inheritdoc/>
        public event EventHandler<DeviceDataEventArgs> DeviceConnected;

        /// <inheritdoc/>
        public event EventHandler<DeviceDataEventArgs> DeviceDisconnected;

        /// <inheritdoc/>
        public
#if !NET35 && !NET40
            IReadOnlyCollection
#else
            IEnumerable
#endif
            <DeviceData> Devices
        { get; private set; }

        /// <summary>
        /// Gets the <see cref="IAdbSocket"/> that represents the connection to the
        /// Android Debug Bridge.
        /// </summary>
        public IAdbSocket Socket { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance is running.
        /// </summary>
        /// <value><see langword="true"/> if this instance is running; otherwise, <see langword="false"/>.</value>
        public bool IsRunning { get; private set; }

        /// <inheritdoc/>
        public void Start()
        {
            if (monitorTask == null)
            {
                _ = firstDeviceListParsed.Reset();

                monitorTask = Utilities.Run(() => DeviceMonitorLoopAsync(monitorTaskCancellationTokenSource.Token));

                // Wait for the worker thread to have read the first list
                // of devices.
                _ = firstDeviceListParsed.WaitOne();
            }
        }

        /// <summary>
        /// Stops the monitoring
        /// </summary>
        public void Dispose()
        {
            // First kill the monitor task, which has a dependency on the socket,
            // then close the socket.
            if (monitorTask != null)
            {
                IsRunning = false;

                // Stop the thread. The tread will keep waiting for updated information from adb
                // eternally, so we need to forcefully abort it here.
                monitorTaskCancellationTokenSource.Cancel();
                monitorTask.Wait();

#if HAS_Process
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

#if !NET35
            firstDeviceListParsed.Dispose();
#else
            firstDeviceListParsed.Close();
#endif
            monitorTaskCancellationTokenSource.Dispose();
        }

        /// <summary>
        /// Raises the <see cref="DeviceChanged"/> event.
        /// </summary>
        /// <param name="e">The <see cref="DeviceDataEventArgs"/> instance containing the event data.</param>
        protected void OnDeviceChanged(DeviceDataEventArgs e) => DeviceChanged?.Invoke(this, e);

        /// <summary>
        /// Raises the <see cref="DeviceConnected"/> event.
        /// </summary>
        /// <param name="e">The <see cref="DeviceDataEventArgs"/> instance containing the event data.</param>
        protected void OnDeviceConnected(DeviceDataEventArgs e) => DeviceConnected?.Invoke(this, e);

        /// <summary>
        /// Raises the <see cref="DeviceDisconnected"/> event.
        /// </summary>
        /// <param name="e">The <see cref="DeviceDataEventArgs"/> instance containing the event data.</param>
        protected void OnDeviceDisconnected(DeviceDataEventArgs e) => DeviceDisconnected?.Invoke(this, e);

        /// <summary>
        /// Monitors the devices. This connects to the Debug Bridge
        /// </summary>
        private async Task DeviceMonitorLoopAsync(CancellationToken cancellationToken = default)
        {
            IsRunning = true;

            // Set up the connection to track the list of devices.
            InitializeSocket();

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
                        AdbServer.Instance.RestartServer();
                        Socket.Reconnect();
                        InitializeSocket();
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

        private void InitializeSocket()
        {
            // Set up the connection to track the list of devices.
            Socket.SendAdbRequest("host:track-devices");
            _ = Socket.ReadAdbResponse();
        }

        /// <summary>
        /// Processes the incoming device data.
        /// </summary>
        private void ProcessIncomingDeviceData(string result)
        {
            List<DeviceData> list = new();

            string[] deviceValues = result.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            List<DeviceData> currentDevices = deviceValues.Select(DeviceData.CreateFromAdbData).ToList();
            UpdateDevices(currentDevices);
        }

        private void UpdateDevices(List<DeviceData> devices)
        {
            lock (this.devices)
            {
                // For each device in the current list, we look for a matching the new list.
                // * if we find it, we update the current object with whatever new information
                //   there is
                //   (mostly state change, if the device becomes ready, we query for build info).
                //   We also remove the device from the new list to mark it as "processed"
                // * if we do not find it, we remove it from the current list.
                // Once this is done, the new list contains device we aren't monitoring yet, so we
                // add them to the list, and start monitoring them.

                // Add or update existing devices
                foreach (DeviceData device in devices)
                {
                    DeviceData existingDevice = Devices.SingleOrDefault(d => d.Serial == device.Serial);

                    if (existingDevice == null)
                    {
                        this.devices.Add(device);
                        OnDeviceConnected(new DeviceDataEventArgs(device));
                    }
                    else if (existingDevice.State != device.State)
                    {
                        existingDevice.State = device.State;
                        OnDeviceChanged(new DeviceDataEventArgs(existingDevice));
                    }
                }

                // Remove devices
                foreach (DeviceData device in Devices.Where(d => !devices.Any(e => e.Serial == d.Serial)).ToArray())
                {
                    this.devices.Remove(device);
                    OnDeviceDisconnected(new DeviceDataEventArgs(device));
                }
            }
        }
    }
}
