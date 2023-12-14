// <copyright file="IDeviceMonitor.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.Collections.ObjectModel;

namespace AdvancedSharpAdbClient
{
    /// <summary>
    /// Provides a common interface for any class that allows you to monitor the list of devices that are currently connected to the adb server.
    /// </summary>
    public partial interface IDeviceMonitor : IDisposable
#if HAS_TASK
        , IDisposableWithTask
#endif
    {
        /// <summary>
        /// Occurs when the status of one of the connected devices has changed.
        /// </summary>
        event EventHandler<DeviceDataChangeEventArgs>? DeviceChanged;

        /// <summary>
        /// Occurs when received a list of device from the Android Debug Bridge.
        /// </summary>
        event EventHandler<DeviceDataNotifyEventArgs>? DeviceNotified;

        /// <summary>
        /// Occurs when a device has connected to the Android Debug Bridge.
        /// </summary>
        event EventHandler<DeviceDataConnectEventArgs>? DeviceConnected;

        /// <summary>
        /// Occurs when the list of the connected devices has changed.
        /// </summary>
        event EventHandler<DeviceDataNotifyEventArgs>? DeviceListChanged;

        /// <summary>
        /// Occurs when a device has disconnected from the Android Debug Bridge.
        /// </summary>
        event EventHandler<DeviceDataConnectEventArgs>? DeviceDisconnected;

        /// <summary>
        /// Gets the devices that are currently connected to the Android Debug Bridge.
        /// </summary>
        ReadOnlyCollection<DeviceData> Devices { get; }

        /// <summary>
        /// Starts the monitoring.
        /// </summary>
        void Start();
    }
}
