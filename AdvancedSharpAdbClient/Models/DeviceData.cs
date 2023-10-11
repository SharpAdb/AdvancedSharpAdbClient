// <copyright file="DeviceData.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.Text.RegularExpressions;

namespace AdvancedSharpAdbClient
{
    /// <summary>
    /// Represents a device that is connected to the Android Debug Bridge.
    /// </summary>
    public partial class DeviceData : IEquatable<DeviceData>
    {
        /// <summary>
        /// A regular expression that can be used to parse the device information that is returned by the Android Debut Bridge.
        /// </summary>
        private const string DeviceDataRegexString = @"^(?<serial>[a-zA-Z0-9_-]+(?:\s?[\.a-zA-Z0-9_-]+)?(?:\:\d{1,})?)\s+(?<state>device|connecting|offline|unknown|bootloader|recovery|download|authorizing|unauthorized|host|no permissions)(?<message>.*?)(\s+usb:(?<usb>[^:]+))?(?:\s+product:(?<product>[^:]+))?(\s+model\:(?<model>[\S]+))?(\s+device\:(?<device>[\S]+))?(\s+features:(?<features>[^:]+))?(\s+transport_id:(?<transport_id>[^:]+))?$";

        /// <summary>
        /// A regular expression that can be used to parse the device information that is returned by the Android Debut Bridge.
        /// </summary>
        private static readonly Regex Regex = DeviceDataRegex();

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceData"/> class.
        /// </summary>
        public DeviceData() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceData"/> class based on
        /// data retrieved from the Android Debug Bridge.
        /// </summary>
        /// <param name="data">The data retrieved from the Android Debug Bridge that represents a device.</param>
        public DeviceData(string data)
        {
            Match match = Regex.Match(data);
            if (match.Success)
            {
                Serial = match.Groups["serial"].Value;
                State = GetStateFromString(match.Groups["state"].Value);
                Model = match.Groups["model"].Value;
                Product = match.Groups["product"].Value;
                Name = match.Groups["device"].Value;
                Features = match.Groups["features"].Value;
                Usb = match.Groups["usb"].Value;
                TransportId = match.Groups["transport_id"].Value;
                Message = match.Groups["message"].Value;
            }
            else
            {
                throw new ArgumentException($"Invalid device list data '{data}'");
            }
        }

        /// <summary>
        /// Gets or sets the device serial number.
        /// </summary>
        public string Serial { get; init; }

        /// <summary>
        /// Gets or sets the device state.
        /// </summary>
        public DeviceState State { get; init; }

        /// <summary>
        /// Gets or sets the device model name.
        /// </summary>
        public string Model { get; init; }

        /// <summary>
        /// Gets or sets the device product name.
        /// </summary>
        public string Product { get; init; }

        /// <summary>
        /// Gets or sets the device name.
        /// </summary>
        public string Name { get; init; }

        /// <summary>
        /// Gets or sets the features available on the device.
        /// </summary>
        public string Features { get; init; }

        /// <summary>
        /// Gets or sets the USB port to which this device is connected. Usually available on Linux only.
        /// </summary>
        public string Usb { get; init; }

        /// <summary>
        /// Gets or sets the transport ID for this device.
        /// </summary>
        public string TransportId { get; init; }

        /// <summary>
        /// Gets or sets the device info message. Currently only seen for NoPermissions state.
        /// </summary>
        public string Message { get; init; }

        /// <summary>
        /// Creates a new instance of the <see cref="DeviceData"/> class based on
        /// data retrieved from the Android Debug Bridge.
        /// </summary>
        /// <param name="data">The data retrieved from the Android Debug Bridge that represents a device.</param>
        /// <returns>A <see cref="DeviceData"/> object that represents the device.</returns>
        public static DeviceData CreateFromAdbData(string data) => new(data);

        /// <inheritdoc/>
        public override bool Equals(object obj) => Equals(obj as DeviceData);

        /// <inheritdoc/>
        public bool Equals(DeviceData other) =>
            other is not null
                && Serial == other.Serial
                && State == other.State
                && Model == other.Model
                && Product == other.Product
                && Name == other.Name
                && Features == other.Features
                && Usb == other.Usb
                && TransportId == other.TransportId
                && Message == other.Message;

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            HashCode hash = new();
            hash.Add(Serial);
            hash.Add(State);
            hash.Add(Model);
            hash.Add(Product);
            hash.Add(Name);
            hash.Add(Features);
            hash.Add(Usb);
            hash.Add(TransportId);
            hash.Add(Message);
            return hash.ToHashCode();
        }

        /// <inheritdoc/>
        public override string ToString() => Serial;

        /// <summary>
        /// Get the device state from the string value.
        /// </summary>
        /// <param name="state">The device state string.</param>
        /// <returns>The device state.</returns>
        internal static DeviceState GetStateFromString(string state)
        {
            // Default to the unknown state
            DeviceState value;

            if (string.Equals(state, "device", StringComparison.OrdinalIgnoreCase))
            {
                // As a special case, the "device" state in ADB is translated to the
                // "Online" state in Managed.Adb
                value = DeviceState.Online;
            }
            else if (string.Equals(state, "no permissions", StringComparison.OrdinalIgnoreCase))
            {
                value = DeviceState.NoPermissions;
            }
            else
            {
                // Else, we try to match a value of the DeviceState enumeration.
                if (!Extensions.TryParse(state, true, out value))
                {
                    value = DeviceState.Unknown;
                }
            }

            return value;
        }

#if NET7_0_OR_GREATER
        [GeneratedRegex(DeviceDataRegexString, RegexOptions.IgnoreCase)]
        private static partial Regex DeviceDataRegex();
#else
        private static Regex DeviceDataRegex() => new(DeviceDataRegexString, RegexOptions.IgnoreCase);
#endif
    }
}
