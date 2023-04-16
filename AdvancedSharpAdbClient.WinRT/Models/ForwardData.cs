// <copyright file="ForwardData.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

namespace AdvancedSharpAdbClient.WinRT
{
    /// <summary>
    /// Contains information about port forwarding configured by the Android Debug Bridge.
    /// </summary>
    public sealed class ForwardData
    {
        internal readonly AdvancedSharpAdbClient.ForwardData forwardData;

        /// <summary>
        /// Gets or sets the serial number of the device for which the port forwarding is configured.
        /// </summary>
        public string SerialNumber
        {
            get => forwardData.SerialNumber;
            set => forwardData.SerialNumber = value;
        }

        /// <summary>
        /// Gets or sets a <see cref="string"/> that represents the local (PC) endpoint.
        /// </summary>
        public string Local
        {
            get => forwardData.Local;
            set => forwardData.Local = value;
        }

        /// <summary>
        /// Gets a <see cref="ForwardSpec"/> that represents the local (PC) endpoint.
        /// </summary>
        public ForwardSpec LocalSpec => ForwardSpec.GetForwardSpec(forwardData.LocalSpec);

        /// <summary>
        /// Gets or sets a <see cref="string"/> that represents the remote (device) endpoint.
        /// </summary>
        public string Remote
        {
            get => forwardData.Remote;
            set => forwardData.Remote = value;
        }

        /// <summary>
        /// Gets a <see cref="ForwardSpec"/> that represents the remote (device) endpoint.
        /// </summary>
        public ForwardSpec RemoteSpec => ForwardSpec.GetForwardSpec(forwardData.RemoteSpec);

        /// <summary>
        /// Creates a new instance of the <seealso cref="ForwardData"/> class by parsing a <see cref="string"/>.
        /// </summary>
        /// <param name="_value">The <see cref="string"/> _value to parse.</param>
        /// <returns>A <see cref="ForwardData"/> object that represents the port forwarding information contained in <paramref name="_value"/>.</returns>
        public static ForwardData FromString(string _value) => GetForwardData(AdvancedSharpAdbClient.ForwardData.FromString(_value));

        /// <summary>
        /// Initializes a new instance of the <see cref="ForwardData"/> class.
        /// </summary>
        public ForwardData() => forwardData = new();

        internal ForwardData(AdvancedSharpAdbClient.ForwardData forwardData) => this.forwardData = forwardData;

        internal static ForwardData GetForwardData(AdvancedSharpAdbClient.ForwardData forwardData) => new(forwardData);

        /// <inheritdoc/>
        public override string ToString() => forwardData.ToString();
    }
}
