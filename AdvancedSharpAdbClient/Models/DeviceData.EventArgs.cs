// <copyright file="DeviceData.EventArgs.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Text;

namespace AdvancedSharpAdbClient.Models
{
    /// <summary>
    /// The event arguments that are passed when a device event occurs.
    /// </summary>
    /// <param name="device">The device.</param>
    public abstract class DeviceDataEventArgs(DeviceData device) : EventArgs
    {
        /// <summary>
        /// Gets the device where the change occurred.
        /// </summary>
        /// <value>The device where the change occurred.</value>
        public DeviceData Device { get; } = device;
    }

    /// <summary>
    /// The event arguments that are passed when a device event occurs.
    /// </summary>
    /// <param name="devices">The list of device.</param>
    public sealed class DeviceDataNotifyEventArgs(IEnumerable<DeviceData> devices) : EventArgs
    {
        /// <summary>
        /// Gets the list of device where the change occurred.
        /// </summary>
        /// <value>The list of device where the change occurred.</value>
        public IEnumerable<DeviceData> Devices { get; } = devices;
    }

    /// <summary>
    /// The event arguments that are passed when a device event occurs.
    /// </summary>
    /// <param name="device">The device.</param>
    /// <param name="isConnect">The device after the reported change.</param>
    public sealed class DeviceDataConnectEventArgs(DeviceData device, bool isConnect) : DeviceDataEventArgs(device)
    {
        /// <summary>
        /// Gets the connect state of the device after the reported change.
        /// </summary>
        public bool IsConnect { get; } = isConnect;

        /// <inheritdoc/>
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder("Device ")
                .Append(IsConnect ? "connected" : "disconnected");
            if (Device.IsEmpty)
            {
                return builder.ToString();
            }
            else
            {
                builder.Append(": ").Append(Device.Serial);
                if (!string.IsNullOrEmpty(Device.Name))
                {
                    builder.AppendFormat("({0})", Device.Name);
                }
                return builder.ToString();
            }
        }
    }

    /// <summary>
    /// The event arguments that are passed when a device event occurs.
    /// </summary>
    /// <param name="device">The device.</param>
    /// <param name="newState">The state of the device after the reported change.</param>
    /// <param name="oldState">The state of the device before the reported change.</param>
    public sealed class DeviceDataChangeEventArgs(DeviceData device, DeviceState newState, DeviceState oldState) : DeviceDataEventArgs(device)
    {
        /// <summary>
        /// Gets the state of the device after the reported change.
        /// </summary>
        public DeviceState NewState { get; } = newState;

        /// <summary>
        /// Gets the state of the device before the reported change.
        /// </summary>
        public DeviceState OldState { get; } = oldState;

        /// <inheritdoc/>
        public override string ToString()
        {
            StringBuilder builder = new("Device state changed:");
            if (!Device.IsEmpty)
            {
                builder.Append(' ').Append(Device.Serial);
                if (!string.IsNullOrEmpty(Device.Name))
                {
                    builder.AppendFormat("({0})", Device.Name);
                }
            }
            builder.Append($" {OldState} -> {NewState}");
            return builder.ToString();
        }
    }
}
