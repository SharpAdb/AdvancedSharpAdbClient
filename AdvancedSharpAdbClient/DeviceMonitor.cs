// <copyright file="DeviceMonitor.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace AdvancedSharpAdbClient
{
    /// <summary>
    /// A Device monitor. This connects to the Android Debug Bridge and get device and
    /// debuggable process information from it.
    /// </summary>
    /// <example>
    /// <para>To receive notifications when devices connect to or disconnect from your PC, you can use the following code:</para>
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
    [DebuggerDisplay($"{nameof(DeviceMonitor)} \\{{ {nameof(IsRunning)} = {{{nameof(IsRunning)}}}, {nameof(Devices)} = {{{nameof(Devices)}}}, {nameof(Socket)} = {{{nameof(Socket)}}} }}")]
    public partial class DeviceMonitor : IDeviceMonitor, ICloneable<DeviceMonitor>, ICloneable
#if COMP_NETSTANDARD2_1
        , IAsyncDisposable
#endif
    {
        /// <summary>
        /// The <see cref="Array"/> of <see cref="char"/>s that represent a new line.
        /// </summary>
        private static readonly char[] separator = Extensions.NewLineSeparator;

        /// <summary>
        /// The <see cref="bool"/> that indicates whether this instance has been disposed.
        /// </summary>
        private bool disposed = false;

        /// <summary>
        /// The logger to use when logging messages.
        /// </summary>
        private readonly ILogger<DeviceMonitor> logger;

        /// <summary>
        /// The list of devices currently connected to the Android Debug Bridge.
        /// </summary>
        protected readonly List<DeviceData> devices = [];

#if !HAS_TASK
        /// <summary>
        /// When the <see cref="Start"/> method is called, this <see cref="ManualResetEvent"/>
        /// is used to block the <see cref="Start"/> method until the <see cref="DeviceMonitorLoop"/>
        /// has processed the first list of devices.
        /// </summary>
        protected readonly ManualResetEvent FirstDeviceListParsed = new(false);

        /// <summary>
        /// When the <see cref="DeviceMonitorLoop"/> method is called, this <see cref="ManualResetEvent"/>
        /// is used to block until the <see cref="DeviceMonitorLoop"/> is finished.
        /// </summary>
        protected readonly ManualResetEvent MonitorLoopFinished = new(false);

        /// <summary>
        /// A <see cref="bool"/> that can be used to cancel the <see cref="MonitorThread"/>.
        /// </summary>
        protected bool IsMonitorThreadCancel = false;

        /// <summary>
        /// The <see cref="Thread"/> that monitors the <see cref="Socket"/> and waits for device notifications.
        /// </summary>
        protected Thread? MonitorThread;
#endif

        /// <inheritdoc/>
        public event EventHandler<DeviceDataChangeEventArgs>? DeviceChanged;

        /// <inheritdoc/>
        public event EventHandler<DeviceDataNotifyEventArgs>? DeviceNotified;

        /// <inheritdoc/>
        public event EventHandler<DeviceDataConnectEventArgs>? DeviceConnected;

        /// <inheritdoc/>
        public event EventHandler<DeviceDataNotifyEventArgs>? DeviceListChanged;

        /// <inheritdoc/>
        public event EventHandler<DeviceDataConnectEventArgs>? DeviceDisconnected;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceMonitor"/> class.
        /// </summary>
        /// <param name="logger">The logger to use when logging.</param>
        public DeviceMonitor(ILogger<DeviceMonitor>? logger = null)
            : this(Factories.AdbSocketFactory(AdbClient.AdbServerEndPoint), logger)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceMonitor"/> class.
        /// </summary>
        /// <param name="endPoint">The <see cref="EndPoint"/> at which the adb server is listening.</param>
        /// <param name="logger">The logger to use when logging.</param>
        public DeviceMonitor(EndPoint endPoint, ILogger<DeviceMonitor>? logger = null)
            : this(Factories.AdbSocketFactory(endPoint), logger)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceMonitor"/> class.
        /// </summary>
        /// <param name="socket">The <see cref="IAdbSocket"/> that manages the connection with the adb server.</param>
        /// <param name="logger">The logger to use when logging.</param>
        public DeviceMonitor(IAdbSocket socket, ILogger<DeviceMonitor>? logger = null)
        {
            Socket = socket ?? throw new ArgumentNullException(nameof(socket));
            Devices = devices.AsReadOnly();
            this.logger = logger ?? LoggerProvider.CreateLogger<DeviceMonitor>();
        }

        /// <inheritdoc/>
        public ReadOnlyCollection<DeviceData> Devices { get; init; }

        /// <summary>
        /// Gets the <see cref="IAdbSocket"/> that represents the connection to the
        /// Android Debug Bridge.
        /// </summary>
        public IAdbSocket Socket { get; protected set; }

        /// <summary>
        /// Gets a value indicating whether this instance is running.
        /// </summary>
        /// <value><see langword="true"/> if this instance is running; otherwise, <see langword="false"/>.</value>
        public bool IsRunning { get; protected set; }

        /// <inheritdoc/>
        [MemberNotNull(
#if HAS_TASK
            nameof(MonitorTask)
#else
            nameof(MonitorThread)
#endif
            )]
        public void Start()
        {
#if HAS_TASK
            if (MonitorTask == null)
            {
                try
                {
                    FirstDeviceListParsed = new TaskCompletionSource<object?>();
                    MonitorTask = DeviceMonitorLoopAsync(MonitorTaskCancellationTokenSource.Token);
                    // Wait for the worker thread to have read the first list of devices.
                    _ = FirstDeviceListParsed.Task.AwaitByTaskCompleteSource();
                }
                finally
                {
                    FirstDeviceListParsed = null;
                }
            }
#else
            if (MonitorThread == null)
            {
                _ = FirstDeviceListParsed.Reset();

                MonitorThread = new Thread(DeviceMonitorLoop)
                {
                    Name = nameof(DeviceMonitorLoop),
                    IsBackground = true
                };
                MonitorThread.Start();

                // Wait for the worker thread to have read the first list of devices.
                _ = FirstDeviceListParsed.WaitOne();
            }
#endif
        }

        /// <summary>
        /// Stops the monitoring
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposing || disposed) { return; }
#if HAS_TASK
            // First kill the monitor task, which has a dependency on the socket,
            // then close the socket.
            if (MonitorTask != null)
            {
                IsRunning = false;

                // Stop the thread. The tread will keep waiting for updated information from adb
                // eternally, so we need to forcefully abort it here.
                MonitorTaskCancellationTokenSource.Cancel();
                MonitorTask.AwaitByTaskCompleteSource();
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
#else
            // First kill the monitor task, which has a dependency on the socket,
            // then close the socket.
            if (MonitorThread != null)
            {
                IsRunning = false;

                // Stop the thread. The tread will keep waiting for updated information from adb
                // eternally, so we need to forcefully abort it here.
                IsMonitorThreadCancel = true;
                Socket?.Close();
                _ = MonitorLoopFinished.WaitOne();

                MonitorThread = null;
            }

            // Close the connection to adb. To be done after the monitor task exited.
            if (Socket != null)
            {
                Socket.Dispose();
                Socket = null!;
            }

            FirstDeviceListParsed.Close();
#endif
            disposed = true;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

#if !HAS_TASK
        /// <summary>
        /// Monitors the devices. This connects to the Debug Bridge
        /// </summary>
        protected virtual void DeviceMonitorLoop()
        {
            IsRunning = true;
            MonitorLoopFinished.Reset();

            try
            {
                // Set up the connection to track the list of devices.
                InitializeSocket();

                do
                {
                    try
                    {
                        string value = Socket.ReadString();
                        ProcessIncomingDeviceData(value);

                        FirstDeviceListParsed.Set();
                    }
                    catch (AdbException adbException)
                    {
                        if (adbException.InnerException is SocketException ex)
                        {
                            if (IsMonitorThreadCancel)
                            {
                                // The DeviceMonitor is shutting down (disposing) and Dispose()
                                // has called Socket.Close(). This exception is expected,
                                // so we can safely swallow it.
                            }
                            else if (adbException.ConnectionReset)
                            {
                                // The adb server was killed, for whatever reason. Try to restart it and recover from this.
                                AdbServer.Instance.RestartServer();
                                Socket.Reconnect(false);
                                InitializeSocket();
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
                }
                while (!IsMonitorThreadCancel);
                IsMonitorThreadCancel = false;
            }
            finally
            {
                MonitorLoopFinished.Set();
            }
        }

        /// <summary>
        /// Initializes the <see cref="Socket"/> and sends the <c>host:track-devices</c> command to the adb server.
        /// </summary>
        private void InitializeSocket()
        {
            // Set up the connection to track the list of devices.
            Socket.SendAdbRequest("host:track-devices");
            _ = Socket.ReadAdbResponse();
        }
#endif

        /// <summary>
        /// Processes the incoming device data.
        /// </summary>
        private void ProcessIncomingDeviceData(string result)
        {
            string[] deviceValues = result.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            IEnumerable<DeviceData> currentDevices = deviceValues.Select(x => new DeviceData(x));
            UpdateDevices(currentDevices);
            DeviceNotified?.Invoke(this, new DeviceDataNotifyEventArgs(currentDevices));
        }

        /// <summary>
        /// Processes the incoming <see cref="DeviceData"/>.
        /// </summary>
        protected virtual void UpdateDevices(IEnumerable<DeviceData> collection)
        {
            lock (devices)
            {
                // For each device in the current list, we look for a matching the new list.
                // * if we find it, we update the current object with whatever new information
                //   there is
                //   (mostly state change, if the device becomes ready, we query for build info).
                //   We also remove the device from the new list to mark it as "processed"
                // * if we do not find it, we remove it from the current list.
                // Once this is done, the new list contains device we aren't monitoring yet, so we
                // add them to the list, and start monitoring them.

                bool isChanged = false;
                List<DeviceData> devices = collection.ToList();
                for (int i = this.devices.Count; --i >= 0;)
                {
                    DeviceData currentDevice = this.devices[i];
                    int index = devices.FindIndex(d => d.Serial == currentDevice.Serial);
                    if (index == -1)
                    {
                        // Remove disconnected devices
                        this.devices.RemoveAt(i);
                        DeviceDisconnected?.Invoke(this, new DeviceDataConnectEventArgs(currentDevice, false));
                        isChanged = true;
                    }
                    else
                    {
                        DeviceData device = devices[index];
                        if (currentDevice.State != device.State)
                        {
                            // Change device state
                            this.devices[i] = device;
                            DeviceChanged?.Invoke(this, new DeviceDataChangeEventArgs(device, device.State, currentDevice.State));
                            isChanged = true;
                        }
                        devices.RemoveAt(index);
                    }
                }

                if (devices.Count > 0)
                {
                    // Add connected devices
                    foreach (DeviceData device in devices)
                    {
                        this.devices.Add(device);
                        DeviceConnected?.Invoke(this, new DeviceDataConnectEventArgs(device, false));
                    }
                    isChanged = true;
                }

                if (isChanged)
                {
                    DeviceListChanged?.Invoke(this, new DeviceDataNotifyEventArgs(devices));
                }
            }
        }

        /// <inheritdoc/>
        public override string ToString() =>
            new StringBuilder(nameof(SyncService))
                .Append(" { ")
                .Append(nameof(Socket))
                .Append(" = ")
                .Append(Socket)
                .Append(", ")
                .Append(nameof(IsRunning))
                .Append(" = ")
                .Append(IsRunning)
                .Append(" }")
                .ToString();

        /// <inheritdoc/>
        public virtual DeviceMonitor Clone() =>
            Socket is ICloneable<IAdbSocket> cloneable
                ? new DeviceMonitor(cloneable.Clone(), logger)
                : throw new NotSupportedException($"{Socket.GetType()} does not support cloning.");

        /// <inheritdoc/>
        object ICloneable.Clone() => Clone();
    }
}
