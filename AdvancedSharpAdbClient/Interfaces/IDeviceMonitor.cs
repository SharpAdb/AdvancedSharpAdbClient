﻿// <copyright file="IDeviceMonitor.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;

namespace AdvancedSharpAdbClient
{
    /// <summary>
    /// Provides a common interface for any class that allows you to monitor the list of devices that are currently connected to the adb server.
    /// </summary>
    public interface IDeviceMonitor : IDisposable
    {
        /// <summary>
        /// Occurs when the status of one of the connected devices has changed.
        /// </summary>
        event EventHandler<DeviceDataEventArgs> DeviceChanged;

        /// <summary>
        /// Occurs when a device has connected to the Android Debug Bridge.
        /// </summary>
        event EventHandler<DeviceDataEventArgs> DeviceConnected;

        /// <summary>
        /// Occurs when a device has disconnected from the Android Debug Bridge.
        /// </summary>
        event EventHandler<DeviceDataEventArgs> DeviceDisconnected;

        /// <summary>
        /// Gets the devices that are currently connected to the Android Debug Bridge.
        /// </summary>
#if !NET35 && !NET40
        IReadOnlyCollection
#else
        IEnumerable
#endif
            <DeviceData> Devices
        { get; }

        /// <summary>
        /// Starts the monitoring.
        /// </summary>
        void Start();
    }
}
