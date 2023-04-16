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
        public string Serial
        {
            get => deviceData.Serial;
            set => deviceData.Serial = value;
        }

        /// <summary>
        /// Gets or sets the device state.
        /// </summary>
        public DeviceState State
        {
            get => (DeviceState)deviceData.State;
            set => deviceData.State = (AdvancedSharpAdbClient.DeviceState)value;
        }

        /// <summary>
        /// Gets or sets the device model name.
        /// </summary>
        public string Model
        {
            get => deviceData.Model;
            set => deviceData.Model = value;
        }

        /// <summary>
        /// Gets or sets the device product name.
        /// </summary>
        public string Product
        {
            get => deviceData.Product;
            set => deviceData.Product = value;
        }

        /// <summary>
        /// Gets or sets the device name.
        /// </summary>
        public string Name
        {
            get => deviceData.Name;
            set => deviceData.Name = value;
        }

        /// <summary>
        /// Gets or sets the features available on the device.
        /// </summary>
        public string Features
        {
            get => deviceData.Features;
            set => deviceData.Features = value;
        }

        /// <summary>
        /// Gets or sets the USB port to which this device is connected. Usually available on Linux only.
        /// </summary>
        public string Usb
        {
            get => deviceData.Usb;
            set => deviceData.Usb = value;
        }

        /// <summary>
        /// Gets or sets the transport ID for this device.
        /// </summary>
        public string TransportId
        {
            get => deviceData.TransportId;
            set => deviceData.TransportId = value;
        }

        /// <summary>
        /// Gets or sets the device info message. Currently only seen for NoPermissions state.
        /// </summary>
        public string Message
        {
            get => deviceData.Message;
            set => deviceData.Message = value;
        }

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
