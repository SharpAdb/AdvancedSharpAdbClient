// <copyright file="DeviceDataEventArgs.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;

namespace AdvancedSharpAdbClient
{
    /// <summary>
    /// The event arguments that are passed when a device event occurs.
    /// </summary>
    public class DeviceDataEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceDataEventArgs"/> class.
        /// </summary>
        /// <param name="device">The device.</param>
        public DeviceDataEventArgs(DeviceData device) => Device = device;

        /// <summary>
        /// Gets the device where the change occurred.
        /// </summary>
        /// <value>The device where the change occurred.</value>
        public DeviceData Device { get; }
    }

    /// <summary>
    /// The event arguments that are passed when a device event occurs.
    /// </summary>
    public class DeviceDataNotifyEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceDataNotifyEventArgs"/> class.
        /// </summary>
        /// <param name="devices">The list of device.</param>
        public DeviceDataNotifyEventArgs(IEnumerable<DeviceData> devices) => Devices = devices;

        /// <summary>
        /// Gets the list of device where the change occurred.
        /// </summary>
        /// <value>The list of device where the change occurred.</value>
        public IEnumerable<DeviceData> Devices { get; }
    }

    /// <summary>
    /// The event arguments that are passed when a device event occurs.
    /// </summary>
    public class DeviceDataConnectEventArgs : DeviceDataEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceDataConnectEventArgs"/> class.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="isConnect">The device after the reported change.</param>
        public DeviceDataConnectEventArgs(DeviceData device, bool isConnect) : base(device) => IsConnect = isConnect;

        /// <summary>
        /// Gets the connect state of the device after the reported change.
        /// </summary>
        public bool IsConnect { get; }
    }

    /// <summary>
    /// The event arguments that are passed when a device event occurs.
    /// </summary>
    public class DeviceDataChangeEventArgs : DeviceDataEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceDataChangeEventArgs"/> class.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="newState">The state of the device after the reported change.</param>
        /// <param name="oldState">The state of the device before the reported change.</param>
        public DeviceDataChangeEventArgs(DeviceData device, DeviceState newState, DeviceState oldState) : base(device)
        {
            NewState = newState;
            OldState = oldState;
        }

        /// <summary>
        /// Gets the state of the device after the reported change.
        /// </summary>
        public DeviceState NewState { get; }

        /// <summary>
        /// Gets the state of the device before the reported change.
        /// </summary>
        public DeviceState OldState { get; }
    }
}
