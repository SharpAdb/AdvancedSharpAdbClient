// <copyright file="ForwardData.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

namespace AdvancedSharpAdbClient
{
    /// <summary>
    /// Contains information about port forwarding configured by the Android Debug Bridge.
    /// </summary>
    public class ForwardData
    {
        /// <summary>
        /// Gets or sets the serial number of the device for which the port forwarding is configured.
        /// </summary>
        public string SerialNumber { get; set; }

        /// <summary>
        /// Gets or sets a <see cref="string"/> that represents the local (PC) endpoint.
        /// </summary>
        public string Local { get; set; }

        /// <summary>
        /// Gets a <see cref="ForwardSpec"/> that represents the local (PC) endpoint.
        /// </summary>
        public ForwardSpec LocalSpec => ForwardSpec.Parse(Local);

        /// <summary>
        /// Gets or sets a <see cref="string"/> that represents the remote (device) endpoint.
        /// </summary>
        public string Remote { get; set; }

        /// <summary>
        /// Gets a <see cref="ForwardSpec"/> that represents the remote (device) endpoint.
        /// </summary>
        public ForwardSpec RemoteSpec => ForwardSpec.Parse(Remote);

        /// <summary>
        /// Initializes a new instance of the <see cref="ForwardData"/> struct.
        /// </summary>
        public ForwardData()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ForwardData"/> struct.
        /// </summary>
        /// <param name="serialNumber">The serial number of the device for which the port forwarding is configured.</param>
        /// <param name="local">The <see cref="string"/> that represents the local (PC) endpoint.</param>
        /// <param name="remote">The <see cref="string"/> that represents the remote (device) endpoint.</param>
        public ForwardData(string serialNumber, string local, string remote)
        { 
            SerialNumber = serialNumber;
            Local = local;
            Remote = remote;
        }

        /// <summary>
        /// Creates a new instance of the <seealso cref="ForwardData"/> class by parsing a <see cref="string"/>.
        /// </summary>
        /// <param name="value">The <see cref="string"/> value to parse.</param>
        /// <returns>A <see cref="ForwardData"/> object that represents the port forwarding information contained in <paramref name="value"/>.</returns>
        public static ForwardData FromString(string value)
        {
            if (value == null)
            {
                return null;
            }
            string[] parts = value.Split(' ');
            return new ForwardData(parts[0], parts[1], parts[2]);
        }

        /// <inheritdoc/>
        public override string ToString() => $"{SerialNumber} {Local} {Remote}";
    }
}
