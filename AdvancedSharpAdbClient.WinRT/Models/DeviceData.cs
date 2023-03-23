// <copyright file="DeviceData.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

namespace AdvancedSharpAdbClient.WinRT
{
    /// <summary>
    /// Represents a device that is connected to the Android Debug Bridge.
    /// </summary>
    public sealed class DeviceData
    {
        internal readonly AdvancedSharpAdbClient.DeviceData deviceData;

        /// <summary>
        /// Gets or sets the device serial number.
        /// </summary>
        public string Serial => deviceData.Serial;

        /// <summary>
        /// Gets or sets the device state.
        /// </summary>
        public DeviceState State => (DeviceState)deviceData.State;

        /// <summary>
        /// Gets or sets the device model name.
        /// </summary>
        public string Model => deviceData.Model;

        /// <summary>
        /// Gets or sets the device product name.
        /// </summary>
        public string Product => deviceData.Product;

        /// <summary>
        /// Gets or sets the device name.
        /// </summary>
        public string Name => deviceData.Name;

        /// <summary>
        /// Gets or sets the features available on the device.
        /// </summary>
        public string Features => deviceData.Features;

        /// <summary>
        /// Gets or sets the USB port to which this device is connected. Usually available on Linux only.
        /// </summary>
        public string Usb => deviceData.Usb;

        /// <summary>
        /// Gets or sets the transport ID for this device.
        /// </summary>
        public string TransportId => deviceData.TransportId;

        /// <summary>
        /// Gets or sets the device info message. Currently only seen for NoPermissions state.
        /// </summary>
        public string Message => deviceData.Message;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdbResponse"/> class.
        /// </summary>
        public DeviceData() => deviceData = new();

        internal DeviceData(AdvancedSharpAdbClient.DeviceData deviceData) => this.deviceData = deviceData;

        internal static DeviceData GetDeviceData(AdvancedSharpAdbClient.DeviceData deviceData) => new(deviceData);

        /// <inheritdoc/>
        public override string ToString() => deviceData.Serial;
    }
}
