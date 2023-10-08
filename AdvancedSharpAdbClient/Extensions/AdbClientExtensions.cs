// <copyright file="AdbClientExtensions.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using AdvancedSharpAdbClient.Exceptions;
using System;
using System.Linq;
using System.Net;

namespace AdvancedSharpAdbClient
{
    /// <summary>
    /// Provides extension methods for the <see cref="IAdbClient"/> interface. Provides overloads for commonly used functions.
    /// </summary>
    public static partial class AdbClientExtensions
    {
        /// <summary>
        /// Asks the ADB server to forward local connections from <paramref name="local"/>
        /// to the <paramref name="remote"/> address on the <paramref name="device"/>.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="device">The device on which to forward the connections.</param>
        /// <param name="local">The local address to forward.</param>
        /// <param name="remote">The remote address to forward.</param>
        /// <param name="allowRebind">If set to <see langword="true"/>, the request will fail if there is already a forward
        /// connection from <paramref name="local"/>.</param>
        /// <returns>If your requested to start forwarding to local port TCP:0, the port number of the TCP port
        /// which has been opened. In all other cases, <c>0</c>.</returns>
        public static int CreateForward(this IAdbClient client, DeviceData device, ForwardSpec local, ForwardSpec remote, bool allowRebind) =>
            client.CreateForward(device, local?.ToString(), remote?.ToString(), allowRebind);

        /// <summary>
        /// Asks the ADB server to reverse forward local connections from <paramref name="remote"/>
        /// to the <paramref name="local"/> address on the <paramref name="device"/>.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="device">The device on which to reverse forward the connections.</param>
        /// <param name="remote">The remote address to reverse forward.</param>
        /// <param name="local">The local address to reverse forward.</param>
        /// <param name="allowRebind">If set to <see langword="true"/>, the request will fail if if the specified socket
        /// is already bound through a previous reverse command.</param>
        /// <returns>If your requested to start reverse to remote port TCP:0, the port number of the TCP port
        /// which has been opened. In all other cases, <c>0</c>.</returns>
        public static int CreateReverseForward(this IAdbClient client, DeviceData device, ForwardSpec remote, ForwardSpec local, bool allowRebind) =>
            client.CreateReverseForward(device, remote?.ToString(), local?.ToString(), allowRebind);

        /// <summary>
        /// Remove a reverse port forwarding between a remote and a local port.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="device">The device on which to remove the reverse port forwarding</param>
        /// <param name="remote">Specification of the remote that was forwarded</param>
        public static void RemoveReverseForward(this IAdbClient client, DeviceData device, string remote) =>
            client.RemoveReverseForward(device, remote);

        /// <summary>
        /// Executes a command on the adb server.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="target">The target of command, such as <c>shell</c>, <c>remount</c>, <c>dev</c>, <c>tcp</c>, <c>local</c>,
        /// <c>localreserved</c>, <c>localabstract</c>, <c>jdwp</c>, <c>track-jdwp</c>, <c>sync</c>, <c>reverse</c> and so on.</param>
        /// <param name="command">The command to execute.</param>
        public static void ExecuteServerCommand(this IAdbClient client, string target, string command) =>
            client.ExecuteServerCommand(target, command, AdbClient.Encoding);

        /// <summary>
        /// Executes a command on the adb server.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="target">The target of command, such as <c>shell</c>, <c>remount</c>, <c>dev</c>, <c>tcp</c>, <c>local</c>,
        /// <c>localreserved</c>, <c>localabstract</c>, <c>jdwp</c>, <c>track-jdwp</c>, <c>sync</c>, <c>reverse</c> and so on.</param>
        /// <param name="command">The command to execute.</param>
        /// <param name="socket">The <see cref="IAdbSocket"/> to send command.</param>
        public static void ExecuteServerCommand(this IAdbClient client, string target, string command, IAdbSocket socket) =>
            client.ExecuteServerCommand(target, command, socket, AdbClient.Encoding);

        /// <summary>
        /// Executes a shell command on the device.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="command">The command to execute.</param>
        /// <param name="device">The device on which to run the command.</param>
        public static void ExecuteRemoteCommand(this IAdbClient client, string command, DeviceData device) =>
            client.ExecuteRemoteCommand(command, device, AdbClient.Encoding);

        /// <summary>
        /// Executes a command on the adb server.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="target">The target of command, such as <c>shell</c>, <c>remount</c>, <c>dev</c>, <c>tcp</c>, <c>local</c>,
        /// <c>localreserved</c>, <c>localabstract</c>, <c>jdwp</c>, <c>track-jdwp</c>, <c>sync</c>, <c>reverse</c> and so on.</param>
        /// <param name="command">The command to execute.</param>
        /// <param name="receiver">Optionally, a <see cref="IShellOutputReceiver"/> that processes the command output.</param>
        public static void ExecuteServerCommand(this IAdbClient client, string target, string command, IShellOutputReceiver receiver) =>
            client.ExecuteServerCommand(target, command, receiver, AdbClient.Encoding);

        /// <summary>
        /// Executes a command on the adb server.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="target">The target of command, such as <c>shell</c>, <c>remount</c>, <c>dev</c>, <c>tcp</c>, <c>local</c>,
        /// <c>localreserved</c>, <c>localabstract</c>, <c>jdwp</c>, <c>track-jdwp</c>, <c>sync</c>, <c>reverse</c> and so on.</param>
        /// <param name="command">The command to execute.</param>
        /// <param name="socket">The <see cref="IAdbSocket"/> to send command.</param>
        /// <param name="receiver">Optionally, a <see cref="IShellOutputReceiver"/> that processes the command output.</param>
        public static void ExecuteServerCommand(this IAdbClient client, string target, string command, IAdbSocket socket, IShellOutputReceiver receiver) =>
            client.ExecuteServerCommand(target, command, socket, receiver, AdbClient.Encoding);

        /// <summary>
        /// Executes a shell command on the device.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="command">The command to execute.</param>
        /// <param name="device">The device on which to run the command.</param>
        /// <param name="receiver">Optionally, a <see cref="IShellOutputReceiver"/> that processes the command output.</param>
        public static void ExecuteRemoteCommand(this IAdbClient client, string command, DeviceData device, IShellOutputReceiver receiver) =>
            client.ExecuteRemoteCommand(command, device, receiver, AdbClient.Encoding);

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

        /// <summary>
        /// Clear the input text. The input should be in focus. Use <see cref="Element.ClearInput(int)"/> if the element isn't focused.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="device">The device on which to clear the input text.</param>
        /// <param name="charCount">The length of text to clear.</param>
        public static void ClearInput(this IAdbClient client, DeviceData device, int charCount)
        {
            client.SendKeyEvent(device, "KEYCODE_MOVE_END");
            client.SendKeyEvent(device, StringExtensions.Join(" ", Enumerable.Repeat("KEYCODE_DEL", charCount)));
        }

        /// <summary>
        /// Click BACK button.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="device">The device on which to click BACK button.</param>
        public static void ClickBackButton(this IAdbClient client, DeviceData device) => client.SendKeyEvent(device, "KEYCODE_BACK");

        /// <summary>
        /// Click HOME button.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="device">The device on which to click HOME button.</param>
        public static void ClickHomeButton(this IAdbClient client, DeviceData device) => client.SendKeyEvent(device, "KEYCODE_HOME");
    }
}
