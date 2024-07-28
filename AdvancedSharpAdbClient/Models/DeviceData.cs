// <copyright file="DeviceData.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace AdvancedSharpAdbClient.Models
{
    /// <summary>
    /// Represents a device that is connected to the Android Debug Bridge.
    /// </summary>
    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    public readonly partial struct DeviceData : IEquatable<DeviceData>
    {
        /// <summary>
        /// A regular expression that can be used to parse the device information that is returned by the Android Debut Bridge.
        /// </summary>
        private const string DeviceDataRegexString = @"^(?<serial>[a-zA-Z0-9_-]+(?:\s?[\.a-zA-Z0-9_-]+)?(?:\:\d{1,})?)\s+(?<state>device|connecting|offline|unknown|bootloader|recovery|sideload|download|authorizing|unauthorized|host|no permissions)(?<message>.*?)(\s+usb:(?<usb>[^:]+))?(?:\s+product:(?<product>[^:]+))?(\s+model\:(?<model>[\S]+))?(\s+device\:(?<device>[\S]+))?(\s+features:(?<features>[^:]+))?(\s+transport_id:(?<transport_id>[^:]+))?$";

        /// <summary>
        /// A regular expression that can be used to parse the device information that is returned by the Android Debut Bridge.
        /// </summary>
        private static readonly Regex Regex = DeviceDataRegex();

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceData"/> struct based on
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
                Features = match.Groups["features"].Value.Split(',', StringSplitOptions.RemoveEmptyEntries);
                Usb = match.Groups["usb"].Value;
                TransportId = match.Groups["transport_id"].Value;
                Message = match.Groups["message"].Value.TrimStart();
            }
            else
            {
                throw new ArgumentException($"Invalid device list data '{data}'");
            }
        }

        /// <summary>
        /// Gets or sets the device serial number.
        /// </summary>
        public string Serial { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the device state.
        /// </summary>
        public DeviceState State { get; init; } = DeviceState.Unknown;

        /// <summary>
        /// Gets or sets the device model name.
        /// </summary>
        public string Model { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the device product name.
        /// </summary>
        public string Product { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the device name.
        /// </summary>
        public string Name { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the features available on the device.
        /// </summary>
        public string[] Features { get; init; } = [];

        /// <summary>
        /// Gets or sets the USB port to which this device is connected. Usually available on Linux only.
        /// </summary>
        public string Usb { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the transport ID for this device.
        /// </summary>
        public string TransportId { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the device info message. Currently only seen for NoPermissions state.
        /// </summary>
        public string Message { get; init; } = string.Empty;

        /// <summary>
        /// <see langword="false"/> if <see cref="DeviceData"/> does not have a valid serial number; otherwise, <see langword="true"/>.
        /// </summary>
        public bool IsEmpty => !uint.TryParse(TransportId, out _) && string.IsNullOrEmpty(Serial);

        /// <summary>
        /// Creates a new instance of the <see cref="DeviceData"/> class based on
        /// data retrieved from the Android Debug Bridge.
        /// </summary>
        /// <param name="data">The data retrieved from the Android Debug Bridge that represents a device.</param>
        /// <returns>A <see cref="DeviceData"/> object that represents the device.</returns>
        public static DeviceData CreateFromAdbData(string data) => new(data);

        /// <summary>
        /// Creates a new instance of the <see cref="SyncService"/> class, which provides access to the sync service running on the Android device.
        /// </summary>
        /// <param name="endPoint">The <see cref="EndPoint"/> at which the adb server is listening.</param>
        /// <returns>A new instance of the <see cref="SyncService"/> class.</returns>
        public SyncService CreateSyncService(EndPoint endPoint) => new(endPoint, this);

        /// <summary>
        /// Creates a new instance of the <see cref="SyncService"/> class, which provides access to the sync service running on the Android device.
        /// </summary>
        /// <returns>A new instance of the <see cref="SyncService"/> class.</returns>
        public SyncService CreateSyncService() => new(this);

        /// <summary>
        /// Creates a new instance of the <see cref="DeviceClient"/> class, which can be used to interact with this device.
        /// </summary>
        /// <param name="client">The <see cref="IAdbClient"/> to use to communicate with the Android Debug Bridge.</param>
        /// <returns>A new instance of the <see cref="DeviceClient"/> class.</returns>
        public DeviceClient CreateDeviceClient(IAdbClient client) => new(client, this);

        /// <summary>
        /// Creates a new instance of the <see cref="DeviceClient"/> class, which can be used to interact with this device.
        /// </summary>
        /// <returns>A new instance of the <see cref="DeviceClient"/> class.</returns>
        public DeviceClient CreateDeviceClient() => new(AdbClient.Instance, this);

        /// <summary>
        /// Creates a new instance of the <see cref="DeviceClient"/> class, which can be used to get information about packages that are installed on a device.
        /// </summary>
        /// <param name="client">The <see cref="IAdbClient"/> to use to communicate with the Android Debug Bridge.</param>
        /// <param name="arguments">The arguments to pass to <c>pm list packages</c>.</param>
        /// <returns>A new instance of the <see cref="PackageManager"/> class.</returns>
        public PackageManager CreatePackageManager(IAdbClient client, params string[] arguments) => new(client, this, arguments);

        /// <summary>
        /// Creates a new instance of the <see cref="DeviceClient"/> class, which can be used to get information about packages that are installed on a device.
        /// </summary>
        /// <param name="arguments">The arguments to pass to <c>pm list packages</c>.</param>
        /// <returns>A new instance of the <see cref="PackageManager"/> class.</returns>
        public PackageManager CreatePackageManager(params string[] arguments) => new(AdbClient.Instance, this, arguments);

        /// <inheritdoc/>
        public override bool Equals([NotNullWhen(true)] object? obj) => obj is DeviceData other && Equals(other);

        /// <inheritdoc/>
        public bool Equals(DeviceData other) =>
            Serial == other.Serial
                && State == other.State
                && Model == other.Model
                && Product == other.Product
                && Name == other.Name
                && Features == other.Features
                && Usb == other.Usb
                && TransportId == other.TransportId
                && Message == other.Message;

        /// <summary>
        /// Tests whether two <see cref='DeviceData'/> objects are equally.
        /// </summary>
        /// <param name="left">The <see cref='DeviceData'/> structure that is to the left of the equality operator.</param>
        /// <param name="right">The <see cref='DeviceData'/> structure that is to the right of the equality operator.</param>
        /// <returns>This operator returns <see langword="true"/> if the two <see cref="DeviceData"/> structures are equally; otherwise <see langword="false"/>.</returns>
        public static bool operator ==(DeviceData? left, DeviceData? right) => left.Equals(right);

        /// <summary>
        /// Tests whether two <see cref='DeviceData'/> objects are different.
        /// </summary>
        /// <param name="left">The <see cref='DeviceData'/> structure that is to the left of the inequality operator.</param>
        /// <param name="right">The <see cref='DeviceData'/> structure that is to the right of the inequality operator.</param>
        /// <returns>This operator returns <see langword="true"/> if the two <see cref="DeviceData"/> structures are unequally; otherwise <see langword="false"/>.</returns>
        public static bool operator !=(DeviceData? left, DeviceData? right) => !left.Equals(right);

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
        public override string ToString()
        {
            if (IsEmpty)
            {
                return $"An empty {GetType()} without {nameof(TransportId)} and {nameof(Serial)}";
            }

            StringBuilder builder =
                new StringBuilder(Serial)
                    .Append('\t');

            _ = State switch
            {
                DeviceState.Online => builder.Append("device"),
                DeviceState.NoPermissions => builder.Append("no permissions"),
                DeviceState.Connecting
                or DeviceState.Offline
                or DeviceState.BootLoader
                or DeviceState.Host
                or DeviceState.Recovery
                or DeviceState.Download
                or DeviceState.Sideload
                or DeviceState.Unauthorized
                or DeviceState.Authorizing
                or DeviceState.Unknown => builder.Append(State.ToString().ToLowerInvariant()),
                _ => builder.AppendFormat("unknown({0:X})", (int)State),
            };

            if (!string.IsNullOrEmpty(Message))
            {
                _ = builder.Append(' ').Append(Message);
            }

            if (!string.IsNullOrEmpty(Usb))
            {
                _ = builder.Append(" usb:").Append(Usb);
            }

            if (!string.IsNullOrEmpty(Product))
            {
                _ = builder.Append(" product:").Append(Product);
            }

            if (!string.IsNullOrEmpty(Model))
            {
                _ = builder.Append(" model:").Append(Model);
            }

            if (!string.IsNullOrEmpty(Name))
            {
                _ = builder.Append(" device:").Append(Name);
            }

            if (Features?.Length > 0)
            {
                _ = builder.Append(" features:").Append(StringExtensions.Join(',', Features));
            }

            if (!string.IsNullOrEmpty(TransportId))
            {
                _ = builder.Append(" transport_id:").Append(TransportId);
            }

            return builder.ToString();
        }

        /// <summary>
        /// Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="device"/> does not have a valid serial number.
        /// </summary>
        /// <param name="device">A <see cref="DeviceData"/> object to validate.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="device"/> corresponds.</param>
        /// <returns>The <paramref name="device"/> parameter, if it is valid.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The <paramref name="device"/> does not have a valid serial number.</exception>
        public static ref DeviceData EnsureDevice(ref DeviceData device, [CallerArgumentExpression(nameof(device))] string? paramName = "device")
        {
            if (device.IsEmpty)
            {
                throw new ArgumentOutOfRangeException(nameof(device), "You must specific a transport ID or serial number for the device");
            }
            return ref device;
        }

        /// <summary>
        /// Gets the device state from the string value.
        /// </summary>
        /// <param name="state">The device state string.</param>
        /// <returns>The device state.</returns>
        public static DeviceState GetStateFromString(string state)
        {
            if (string.Equals(state, "device", StringComparison.OrdinalIgnoreCase))
            {
                // As a special case, the "device" state in ADB is translated to the
                // "Online" state in Managed.Adb
                return DeviceState.Online;
            }
            else if (string.Equals(state, "no permissions", StringComparison.OrdinalIgnoreCase))
            {
                return DeviceState.NoPermissions;
            }
            // Else, we try to match a value of the DeviceState enumeration.
            else if (EnumExtensions.TryParse(state, true, out DeviceState value))
            {
                return value;
            }

            // Default to the unknown state
            return DeviceState.Unknown;
        }

        /// <summary>
        /// Get the value of the <see cref="DebuggerDisplayAttribute"/> for this instance.
        /// </summary>
        /// <returns>The value of the <see cref="DebuggerDisplayAttribute"/> for this instance.</returns>
        private string GetDebuggerDisplay()
        {
            StringBuilder builder =
                new StringBuilder(nameof(DeviceData))
                    .Append(" { ");

            if (!string.IsNullOrEmpty(Serial))
            {
                _ = builder
                    .Append(nameof(Serial))
                    .Append(" = ")
                    .Append(Serial)
                    .Append(", ");
            }

            _ = builder
                .Append(nameof(State))
                .Append(" = ")
                .Append(State);

            if (!string.IsNullOrEmpty(Message))
            {
                _ = builder
                    .Append(nameof(Message))
                    .Append(" = ")
                    .Append(Message);
            }

            if (!string.IsNullOrEmpty(Usb))
            {
                _ = builder
                    .Append(nameof(Usb))
                    .Append(" = ")
                    .Append(Usb);
            }

            if (!string.IsNullOrEmpty(Product))
            {
                _ = builder
                    .Append(nameof(Product))
                    .Append(" = ")
                    .Append(Product);
            }

            if (!string.IsNullOrEmpty(Model))
            {
                _ = builder
                    .Append(nameof(Model))
                    .Append(" = ")
                    .Append(Model);
            }

            if (!string.IsNullOrEmpty(Name))
            {
                _ = builder
                    .Append(nameof(Name))
                    .Append(" = ")
                    .Append(Name);
            }

            if (Features?.Length > 0)
            {
                _ = builder
                    .Append(nameof(Features))
                    .Append(" = ")
                    .Append(Features);
            }

            if (!string.IsNullOrEmpty(TransportId))
            {
                _ = builder
                    .Append(nameof(TransportId))
                    .Append(" = ")
                    .Append(TransportId);
            }

            return builder.Append(" }").ToString();
        }

#if NET7_0_OR_GREATER
        [GeneratedRegex(DeviceDataRegexString, RegexOptions.IgnoreCase)]
        private static partial Regex DeviceDataRegex();
#else
        /// <summary>
        /// Gets a <see cref="System.Text.RegularExpressions.Regex"/> that can be used to parse the device information that is returned by the Android Debut Bridge.
        /// </summary>
        /// <returns>The <see cref="System.Text.RegularExpressions.Regex"/> that can be used to parse the device information that is returned by the Android Debut Bridge.</returns>
        private static Regex DeviceDataRegex() => new(DeviceDataRegexString, RegexOptions.IgnoreCase);
#endif
    }
}
