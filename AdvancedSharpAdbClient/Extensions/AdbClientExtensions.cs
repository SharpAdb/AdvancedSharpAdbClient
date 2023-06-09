// <copyright file="AdbClientExtensions.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using AdvancedSharpAdbClient.Exceptions;
using System;
using System.Net;

namespace AdvancedSharpAdbClient
{
    /// <summary>
    /// Provides extension methods for the <see cref="IAdbClient"/> interface. Provides overloads for commonly used functions.
    /// </summary>
    public static partial class AdbClientExtensions
    {
        /// <summary>
        /// Creates a port forwarding between a local and a remote port.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="device">The device to which to forward the connections.</param>
        /// <param name="localPort">The local port to forward.</param>
        /// <param name="remotePort">The remote port to forward to</param>
        /// <returns>If your requested to start forwarding to local port TCP:0, the port number of the TCP port
        /// which has been opened. In all other cases, <c>0</c>.</returns>
        /// <exception cref="AdbException">Failed to submit the forward command. Or Device rejected command:  + resp.Message.</exception>
        public static int CreateForward(this IAdbClient client, DeviceData device, int localPort, int remotePort) =>
            client.CreateForward(device, $"tcp:{localPort}", $"tcp:{remotePort}", true);

        /// <summary>
        /// Forwards a remote Unix socket to a local TCP socket.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="device">The device to which to forward the connections.</param>
        /// <param name="localPort">The local port to forward.</param>
        /// <param name="remoteSocket">The remote Unix socket.</param>
        /// <returns>If your requested to start forwarding to local port TCP:0, the port number of the TCP port
        /// which has been opened. In all other cases, <c>0</c>.</returns>
        /// <exception cref="AdbException">The client failed to submit the forward command.</exception>
        /// <exception cref="AdbException">The device rejected command. The error message will include the error message provided by the device.</exception>
        public static int CreateForward(this IAdbClient client, DeviceData device, int localPort, string remoteSocket) =>
            client.CreateForward(device, $"tcp:{localPort}", $"local:{remoteSocket}", true);

        /// <summary>
        /// Reboots the specified adb socket address.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="device">The device.</param>
        public static void Reboot(this IAdbClient client, DeviceData device) => client.Reboot(string.Empty, device);

        /// <summary>
        /// Pair with a device for secure TCP/IP communication.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="address">The IP address of the remote device.</param>
        /// <param name="code">The pairing code.</param>
        /// <returns>The results from adb.</returns>
        public static string Pair(this IAdbClient client, IPAddress address, string code) =>
            address == null
                ? throw new ArgumentNullException(nameof(address))
                : client.Pair(new IPEndPoint(address, AdbClient.DefaultPort), code);

        /// <summary>
        /// Pair with a device for secure TCP/IP communication.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="endpoint">The DNS endpoint at which the <c>adb</c> server on the device is running.</param>
        /// <param name="code">The pairing code.</param>
        /// <returns>The results from adb.</returns>
        public static string Pair(this IAdbClient client, IPEndPoint endpoint, string code) =>
            endpoint == null
                ? throw new ArgumentNullException(nameof(endpoint))
                : client.Pair(new DnsEndPoint(endpoint.Address.ToString(), endpoint.Port), code);

        /// <summary>
        /// Pair with a device for secure TCP/IP communication.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="host">The host address of the remote device.</param>
        /// <param name="code">The pairing code.</param>
        /// <returns>The results from adb.</returns>
        public static string Pair(this IAdbClient client, string host, string code)
        {
            if (string.IsNullOrEmpty(host))
            {
                throw new ArgumentNullException(nameof(host));
            }

            string[] values = host.Split(':');

            return values.Length <= 0
                ? throw new ArgumentNullException(nameof(host))
                : client.Pair(new DnsEndPoint(values[0], values.Length > 1 && int.TryParse(values[1], out int port) ? port : AdbClient.DefaultPort), code);
        }

        /// <summary>
        /// Pair with a device for secure TCP/IP communication.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="host">The host address of the remote device.</param>
        /// <param name="port">The port of the remote device.</param>
        /// <param name="code">The pairing code.</param>
        /// <returns>The results from adb.</returns>
        public static string Pair(this IAdbClient client, string host, int port, string code)
        {
            if (string.IsNullOrEmpty(host))
            {
                throw new ArgumentNullException(nameof(host));
            }

            string[] values = host.Split(':');

            return values.Length <= 0
                ? throw new ArgumentNullException(nameof(host))
                : client.Pair(new DnsEndPoint(values[0], values.Length > 1 && int.TryParse(values[1], out int _port) ? _port : port), code);
        }

        /// <summary>
        /// Connect to a device via TCP/IP.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="address">The IP address of the remote device.</param>
        /// <returns>The results from adb.</returns>
        public static string Connect(this IAdbClient client, IPAddress address) =>
            address == null
                ? throw new ArgumentNullException(nameof(address))
                : client.Connect(new IPEndPoint(address, AdbClient.DefaultPort));

        /// <summary>
        /// Connect to a device via TCP/IP.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="endpoint">The IP endpoint at which the <c>adb</c> server on the device is running.</param>
        /// <returns>The results from adb.</returns>
        public static string Connect(this IAdbClient client, IPEndPoint endpoint) =>
            endpoint == null
                ? throw new ArgumentNullException(nameof(endpoint))
                : client.Connect(new DnsEndPoint(endpoint.Address.ToString(), endpoint.Port));

        /// <summary>
        /// Connect to a device via TCP/IP.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="host">The host address of the remote device.</param>
        /// <param name="port">The port of the remote device.</param>
        /// <returns>The results from adb.</returns>
        public static string Connect(this IAdbClient client, string host, int port = AdbClient.DefaultPort)
        {
            if (string.IsNullOrEmpty(host))
            {
                throw new ArgumentNullException(nameof(host));
            }

            string[] values = host.Split(':');

            return values.Length <= 0
                ? throw new ArgumentNullException(nameof(host))
                : client.Connect(new DnsEndPoint(values[0], values.Length > 1 && int.TryParse(values[1], out int _port) ? _port : port));
        }
    }
}
