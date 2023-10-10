#if HAS_TASK
// <copyright file="AdbClientExtensions.Async.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using AdvancedSharpAdbClient.Exceptions;
using AdvancedSharpAdbClient.Logs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;

namespace AdvancedSharpAdbClient
{
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
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.
        /// If your requested to start forwarding to local port TCP:0, the port number of the TCP port
        /// which has been opened. In all other cases, <c>0</c>.</returns>
        public static Task<int> CreateForwardAsync(this IAdbClient client, DeviceData device, ForwardSpec local, ForwardSpec remote, bool allowRebind, CancellationToken cancellationToken = default) =>
            client.CreateForwardAsync(device, local?.ToString(), remote?.ToString(), allowRebind, cancellationToken);

        /// <summary>
        /// Creates a port forwarding between a local and a remote port.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="device">The device to which to forward the connections.</param>
        /// <param name="localPort">The local port to forward.</param>
        /// <param name="remotePort">The remote port to forward to</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.
        /// If your requested to start forwarding to local port TCP:0, the port number of the TCP port
        /// which has been opened. In all other cases, <c>0</c>.</returns>
        /// <exception cref="AdbException">Failed to submit the forward command. Or Device rejected command:  + resp.Message.</exception>
        public static Task<int> CreateForwardAsync(this IAdbClient client, DeviceData device, int localPort, int remotePort, CancellationToken cancellationToken = default) =>
            client.CreateForwardAsync(device, $"tcp:{localPort}", $"tcp:{remotePort}", true, cancellationToken);

        /// <summary>
        /// Forwards a remote Unix socket to a local TCP socket.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="device">The device to which to forward the connections.</param>
        /// <param name="localPort">The local port to forward.</param>
        /// <param name="remoteSocket">The remote Unix socket.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.
        /// If your requested to start forwarding to local port TCP:0, the port number of the TCP port
        /// which has been opened. In all other cases, <c>0</c>.</returns>
        /// <exception cref="AdbException">The client failed to submit the forward command.</exception>
        /// <exception cref="AdbException">The device rejected command. The error message will include the error message provided by the device.</exception>
        public static Task<int> CreateForwardAsync(this IAdbClient client, DeviceData device, int localPort, string remoteSocket, CancellationToken cancellationToken = default) =>
            client.CreateForwardAsync(device, $"tcp:{localPort}", $"local:{remoteSocket}", true, cancellationToken);

        /// <summary>
        /// Asks the ADB server to reverse forward local connections from <paramref name="remote"/>
        /// to the <paramref name="local"/> address on the <paramref name="device"/>.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="device">The device on which to reverse forward the connections.</param>
        /// <param name="remote">The remote address to reverse forward.</param>
        /// <param name="local">The local address to reverse forward.</param>
        /// <param name="allowRebind">If set to <see langword="true"/>, the request will fail if if the specified
        /// socket is already bound through a previous reverse command.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.
        /// If your requested to start reverse to remote port TCP:0, the port number of the TCP port
        /// which has been opened. In all other cases, <c>0</c>.</returns>
        public static Task<int> CreateReverseForwardAsync(this IAdbClient client, DeviceData device, ForwardSpec remote, ForwardSpec local, bool allowRebind, CancellationToken cancellationToken = default) =>
            client.CreateReverseForwardAsync(device, remote?.ToString(), local?.ToString(), allowRebind, cancellationToken);

        /// <summary>
        /// Remove a reverse port forwarding between a remote and a local port.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="device">The device on which to remove the reverse port forwarding</param>
        /// <param name="remote">Specification of the remote that was forwarded</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public static Task RemoveReverseForwardAsync(this IAdbClient client, DeviceData device, ForwardSpec remote, CancellationToken cancellationToken = default) =>
            client.RemoveReverseForwardAsync(device, remote?.ToString(), cancellationToken);

        /// <summary>
        /// Executes a command on the adb server.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="target">The target of command, such as <c>shell</c>, <c>remount</c>, <c>dev</c>, <c>tcp</c>, <c>local</c>,
        /// <c>localreserved</c>, <c>localabstract</c>, <c>jdwp</c>, <c>track-jdwp</c>, <c>sync</c>, <c>reverse</c> and so on.</param>
        /// <param name="command">The command to execute.</param>
        /// <param name="receiver">Optionally, a <see cref="IShellOutputReceiver"/> that processes the command output.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public static Task ExecuteServerCommandAsync(this IAdbClient client, string target, string command, IShellOutputReceiver receiver, CancellationToken cancellationToken = default) =>
            client.ExecuteServerCommandAsync(target, command, receiver, AdbClient.Encoding, cancellationToken);

        /// <summary>
        /// Executes a command on the adb server.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="target">The target of command, such as <c>shell</c>, <c>remount</c>, <c>dev</c>, <c>tcp</c>, <c>local</c>,
        /// <c>localreserved</c>, <c>localabstract</c>, <c>jdwp</c>, <c>track-jdwp</c>, <c>sync</c>, <c>reverse</c> and so on.</param>
        /// <param name="command">The command to execute.</param>
        /// <param name="socket">The <see cref="IAdbSocket"/> to send command.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public static Task ExecuteServerCommandAsync(this IAdbClient client, string target, string command, IAdbSocket socket, CancellationToken cancellationToken = default) =>
            client.ExecuteServerCommandAsync(target, command, socket, AdbClient.Encoding, cancellationToken);

        /// <summary>
        /// Executes a command on the device.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="command">The command to execute.</param>
        /// <param name="device">The device on which to run the command.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public static Task ExecuteRemoteCommandAsync(this IAdbClient client, string command, DeviceData device, CancellationToken cancellationToken = default) =>
            client.ExecuteRemoteCommandAsync(command, device, AdbClient.Encoding, cancellationToken);

        /// <summary>
        /// Executes a command on the adb server.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="target">The target of command, such as <c>shell</c>, <c>remount</c>, <c>dev</c>, <c>tcp</c>, <c>local</c>,
        /// <c>localreserved</c>, <c>localabstract</c>, <c>jdwp</c>, <c>track-jdwp</c>, <c>sync</c>, <c>reverse</c> and so on.</param>
        /// <param name="command">The command to execute.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public static Task ExecuteServerCommandAsync(this IAdbClient client, string target, string command, CancellationToken cancellationToken = default) =>
            client.ExecuteServerCommandAsync(target, command, AdbClient.Encoding, cancellationToken);

        /// <summary>
        /// Executes a command on the adb server.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="target">The target of command, such as <c>shell</c>, <c>remount</c>, <c>dev</c>, <c>tcp</c>, <c>local</c>,
        /// <c>localreserved</c>, <c>localabstract</c>, <c>jdwp</c>, <c>track-jdwp</c>, <c>sync</c>, <c>reverse</c> and so on.</param>
        /// <param name="command">The command to execute.</param>
        /// <param name="socket">The <see cref="IAdbSocket"/> to send command.</param>
        /// <param name="receiver">Optionally, a <see cref="IShellOutputReceiver"/> that processes the command output.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public static Task ExecuteServerCommandAsync(this IAdbClient client, string target, string command, IAdbSocket socket, IShellOutputReceiver receiver, CancellationToken cancellationToken = default) =>
            client.ExecuteServerCommandAsync(target, command, socket, receiver, AdbClient.Encoding, cancellationToken);

        /// <summary>
        /// Executes a command on the device.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="command">The command to execute.</param>
        /// <param name="device">The device on which to run the command.</param>
        /// <param name="receiver">Optionally, a <see cref="IShellOutputReceiver"/> that processes the command output.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public static Task ExecuteRemoteCommandAsync(this IAdbClient client, string command, DeviceData device, IShellOutputReceiver receiver, CancellationToken cancellationToken = default) =>
            client.ExecuteRemoteCommandAsync(command, device, receiver, AdbClient.Encoding, cancellationToken);

        /// <summary>
        /// Asynchronously runs the event log service on a device.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="device">The device on which to run the event log service.</param>
        /// <param name="messageSink">A callback which will receive the event log messages as they are received.</param>
        /// <param name="logNames">Optionally, the names of the logs to receive.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public static Task RunLogServiceAsync(this IAdbClient client, DeviceData device, Action<LogEntry> messageSink, params LogId[] logNames) =>
            client.RunLogServiceAsync(device, messageSink, default, logNames);

        /// <summary>
        /// Reboots the specified adb socket address.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="device">The device.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public static Task RebootAsync(this IAdbClient client, DeviceData device, CancellationToken cancellationToken = default) => client.RebootAsync(string.Empty, device, cancellationToken);

        /// <summary>
        /// Pair with a device for secure TCP/IP communication.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="address">The IP address of the remote device.</param>
        /// <param name="code">The pairing code.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>The results from adb.</returns>
        public static Task<string> PairAsync(this IAdbClient client, IPAddress address, string code, CancellationToken cancellationToken = default) =>
            address == null
                ? throw new ArgumentNullException(nameof(address))
                : client.PairAsync(new IPEndPoint(address, AdbClient.DefaultPort), code, cancellationToken);

        /// <summary>
        /// Pair with a device for secure TCP/IP communication.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="endpoint">The DNS endpoint at which the <c>adb</c> server on the device is running.</param>
        /// <param name="code">The pairing code.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>The results from adb.</returns>
        public static Task<string> PairAsync(this IAdbClient client, IPEndPoint endpoint, string code, CancellationToken cancellationToken = default) =>
            endpoint == null
                ? throw new ArgumentNullException(nameof(endpoint))
                : client.PairAsync(new DnsEndPoint(endpoint.Address.ToString(), endpoint.Port), code, cancellationToken);

        /// <summary>
        /// Pair with a device for secure TCP/IP communication.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="host">The host address of the remote device.</param>
        /// <param name="code">The pairing code.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>The results from adb.</returns>
        public static Task<string> PairAsync(this IAdbClient client, string host, string code, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(host))
            {
                throw new ArgumentNullException(nameof(host));
            }

            string[] values = host.Split(':');

            return values.Length <= 0
                ? throw new ArgumentNullException(nameof(host))
                : client.PairAsync(new DnsEndPoint(values[0], values.Length > 1 && int.TryParse(values[1], out int port) ? port : AdbClient.DefaultPort), code, cancellationToken);
        }

        /// <summary>
        /// Pair with a device for secure TCP/IP communication.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="host">The host address of the remote device.</param>
        /// <param name="port">The port of the remote device.</param>
        /// <param name="code">The pairing code.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>The results from adb.</returns>
        public static Task<string> PairAsync(this IAdbClient client, string host, int port, string code, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(host))
            {
                throw new ArgumentNullException(nameof(host));
            }

            string[] values = host.Split(':');

            return values.Length <= 0
                ? throw new ArgumentNullException(nameof(host))
                : client.PairAsync(new DnsEndPoint(values[0], values.Length > 1 && int.TryParse(values[1], out int _port) ? _port : port), code, cancellationToken);
        }

        /// <summary>
        /// Connect to a device via TCP/IP.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="address">The IP address of the remote device.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which return the results from adb.</returns>
        public static Task<string> ConnectAsync(this IAdbClient client, IPAddress address, CancellationToken cancellationToken = default) =>
            address == null
                ? throw new ArgumentNullException(nameof(address))
                : client.ConnectAsync(new IPEndPoint(address, AdbClient.DefaultPort), cancellationToken);

        /// <summary>
        /// Connect to a device via TCP/IP.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="endpoint">The IP endpoint at which the <c>adb</c> server on the device is running.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which return the results from adb.</returns>
        public static Task<string> ConnectAsync(this IAdbClient client, IPEndPoint endpoint, CancellationToken cancellationToken = default) =>
            endpoint == null
                ? throw new ArgumentNullException(nameof(endpoint))
                : client.ConnectAsync(new DnsEndPoint(endpoint.Address.ToString(), endpoint.Port), cancellationToken);

        /// <summary>
        /// Connect to a device via TCP/IP.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="host">The host address of the remote device.</param>
        /// <param name="port">The port of the remote device.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which return the results from adb.</returns>
        public static Task<string> ConnectAsync(this IAdbClient client, string host, int port = AdbClient.DefaultPort, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(host))
            {
                throw new ArgumentNullException(nameof(host));
            }

            string[] values = host.Split(':');

            return values.Length <= 0
                ? throw new ArgumentNullException(nameof(host))
                : client.ConnectAsync(new DnsEndPoint(values[0], values.Length > 1 && int.TryParse(values[1], out int _port) ? _port : port), cancellationToken);
        }

        /// <summary>
        /// Asynchronously installs an Android application on an device.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="device">The device on which to install the application.</param>
        /// <param name="apk">A <see cref="Stream"/> which represents the application to install.</param>
        /// <param name="arguments">The arguments to pass to <c>adb install</c>.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public static Task InstallAsync(this IAdbClient client, DeviceData device, Stream apk, params string[] arguments) => client.InstallAsync(device, apk, default, arguments);

        /// <summary>
        /// Asynchronously push multiple APKs to the device and install them.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="device">The device on which to install the application.</param>
        /// <param name="splitAPKs"><see cref="Stream"/>s which represents the split APKs to install.</param>
        /// <param name="packageName">The package name of the base APK to install.</param>
        /// <param name="arguments">The arguments to pass to <c>adb install-create</c>.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public static Task InstallMultipleAsync(this IAdbClient client, DeviceData device, IEnumerable<Stream> splitAPKs, string packageName, params string[] arguments) =>
            client.InstallMultipleAsync(device, splitAPKs, packageName, default, arguments);

        /// <summary>
        /// Asynchronously push multiple APKs to the device and install them.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="device">The device on which to install the application.</param>
        /// <param name="baseAPK">A <see cref="Stream"/> which represents the base APK to install.</param>
        /// <param name="splitAPKs"><see cref="Stream"/>s which represents the split APKs to install.</param>
        /// <param name="arguments">The arguments to pass to <c>adb install-create</c>.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public static Task InstallMultipleAsync(this IAdbClient client, DeviceData device, Stream baseAPK, IEnumerable<Stream> splitAPKs, params string[] arguments) =>
            client.InstallMultipleAsync(device, baseAPK, splitAPKs, default, arguments);

        /// <summary>
        /// Like "install", but starts an install session.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="device">The device on which to install the application.</param>
        /// <param name="packageName">The package name of the baseAPK to install.</param>
        /// <param name="arguments">The arguments to pass to <c>adb install-create</c>.</param>
        /// <returns>A <see cref="Task"/> which return the session ID</returns>
        public static Task<string> InstallCreateAsync(this IAdbClient client, DeviceData device, string packageName = null, params string[] arguments) =>
            client.InstallCreateAsync(device, packageName, default, arguments);

        /// <summary>
        /// Clear the input text. The input should be in focus. Use <see cref="Element.ClearInputAsync(int, CancellationToken)"/>  if the element isn't focused.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="device">The device on which to clear the input text.</param>
        /// <param name="charCount">The length of text to clear.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public static async Task ClearInputAsync(this IAdbClient client, DeviceData device, int charCount, CancellationToken cancellationToken = default)
        {
            await client.SendKeyEventAsync(device, "KEYCODE_MOVE_END", cancellationToken).ConfigureAwait(false);
            await client.SendKeyEventAsync(device, StringExtensions.Join(" ", Enumerable.Repeat("KEYCODE_DEL", charCount)), cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Click BACK button.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="device">The device on which to click BACK button.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public static Task ClickBackButtonAsync(this IAdbClient client, DeviceData device, CancellationToken cancellationToken = default) => client.SendKeyEventAsync(device, "KEYCODE_BACK", cancellationToken);

        /// <summary>
        /// Click HOME button.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="device">The device on which to click HOME button.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public static Task ClickHomeButtonAsync(this IAdbClient client, DeviceData device, CancellationToken cancellationToken = default) => client.SendKeyEventAsync(device, "KEYCODE_HOME", cancellationToken);
    }
}
#endif