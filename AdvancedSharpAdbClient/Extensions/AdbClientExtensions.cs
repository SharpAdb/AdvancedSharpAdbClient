// <copyright file="AdbClientExtensions.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

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
            client.CreateForward(device, local.ToString(), remote.ToString(), allowRebind);

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
            client.CreateReverseForward(device, remote.ToString(), local.ToString(), allowRebind);

        /// <summary>
        /// Remove a reverse port forwarding between a remote and a local port.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="device">The device on which to remove the reverse port forwarding</param>
        /// <param name="remote">Specification of the remote that was forwarded</param>
        public static void RemoveReverseForward(this IAdbClient client, DeviceData device, ForwardSpec remote) =>
            client.RemoveReverseForward(device, remote.ToString());

        /// <summary>
        /// Executes a command on the adb server.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="target">The target of command, such as <c>shell</c>, <c>remount</c>, <c>dev</c>, <c>tcp</c>, <c>local</c>,
        /// <c>localreserved</c>, <c>localabstract</c>, <c>jdwp</c>, <c>track-jdwp</c>, <c>sync</c>, <c>reverse</c> and so on.</param>
        /// <param name="command">The command to execute.</param>
        /// <param name="receiver">Optionally, a <see cref="IShellOutputReceiver"/> that processes the command output.</param>
        public static void ExecuteServerCommand(this IAdbClient client, string target, string command, IShellOutputReceiver? receiver) =>
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
        public static void ExecuteServerCommand(this IAdbClient client, string target, string command, IAdbSocket socket, IShellOutputReceiver? receiver) =>
            client.ExecuteServerCommand(target, command, socket, receiver, AdbClient.Encoding);

        /// <summary>
        /// Executes a shell command on the device.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="command">The command to execute.</param>
        /// <param name="device">The device on which to run the command.</param>
        /// <param name="receiver">Optionally, a <see cref="IShellOutputReceiver"/> that processes the command output.</param>
        public static void ExecuteRemoteCommand(this IAdbClient client, string command, DeviceData device, IShellOutputReceiver? receiver) =>
            client.ExecuteRemoteCommand(command, device, receiver, AdbClient.Encoding);

        /// <summary>
        /// Executes a command on the adb server.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="target">The target of command, such as <c>shell</c>, <c>remount</c>, <c>dev</c>, <c>tcp</c>, <c>local</c>,
        /// <c>localreserved</c>, <c>localabstract</c>, <c>jdwp</c>, <c>track-jdwp</c>, <c>sync</c>, <c>reverse</c> and so on.</param>
        /// <param name="command">The command to execute.</param>
        /// <param name="predicate">Optionally, a <see cref="Func{String, Boolean}"/> that processes the command output.</param>
        public static void ExecuteServerCommand(this IAdbClient client, string target, string command, Func<string, bool>? predicate) =>
            client.ExecuteServerCommand(target, command, predicate.AsShellOutputReceiver(), AdbClient.Encoding);

        /// <summary>
        /// Executes a command on the adb server.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="target">The target of command, such as <c>shell</c>, <c>remount</c>, <c>dev</c>, <c>tcp</c>, <c>local</c>,
        /// <c>localreserved</c>, <c>localabstract</c>, <c>jdwp</c>, <c>track-jdwp</c>, <c>sync</c>, <c>reverse</c> and so on.</param>
        /// <param name="command">The command to execute.</param>
        /// <param name="socket">The <see cref="IAdbSocket"/> to send command.</param>
        /// <param name="predicate">Optionally, a <see cref="Func{String, Boolean}"/> that processes the command output.</param>
        public static void ExecuteServerCommand(this IAdbClient client, string target, string command, IAdbSocket socket, Func<string, bool>? predicate) =>
            client.ExecuteServerCommand(target, command, socket, predicate.AsShellOutputReceiver(), AdbClient.Encoding);

        /// <summary>
        /// Executes a shell command on the device.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="command">The command to execute.</param>
        /// <param name="device">The device on which to run the command.</param>
        /// <param name="predicate">Optionally, a <see cref="Func{String, Boolean}"/> that processes the command output.</param>
        public static void ExecuteRemoteCommand(this IAdbClient client, string command, DeviceData device, Func<string, bool>? predicate) =>
            client.ExecuteRemoteCommand(command, device, predicate.AsShellOutputReceiver(), AdbClient.Encoding);

        /// <summary>
        /// Executes a command on the adb server.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="target">The target of command, such as <c>shell</c>, <c>remount</c>, <c>dev</c>, <c>tcp</c>, <c>local</c>,
        /// <c>localreserved</c>, <c>localabstract</c>, <c>jdwp</c>, <c>track-jdwp</c>, <c>sync</c>, <c>reverse</c> and so on.</param>
        /// <param name="command">The command to execute.</param>
        /// <param name="predicate">Optionally, a <see cref="Func{String, Boolean}"/> that processes the command output.</param>
        /// <param name="encoding">The encoding to use when parsing the command output.</param>
        public static void ExecuteServerCommand(this IAdbClient client, string target, string command, Func<string, bool>? predicate, Encoding encoding) =>
            client.ExecuteServerCommand(target, command, predicate.AsShellOutputReceiver(), encoding);

        /// <summary>
        /// Executes a command on the adb server.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="target">The target of command, such as <c>shell</c>, <c>remount</c>, <c>dev</c>, <c>tcp</c>, <c>local</c>,
        /// <c>localreserved</c>, <c>localabstract</c>, <c>jdwp</c>, <c>track-jdwp</c>, <c>sync</c>, <c>reverse</c> and so on.</param>
        /// <param name="command">The command to execute.</param>
        /// <param name="socket">The <see cref="IAdbSocket"/> to send command.</param>
        /// <param name="predicate">Optionally, a <see cref="Func{String, Boolean}"/> that processes the command output.</param>
        /// <param name="encoding">The encoding to use when parsing the command output.</param>
        public static void ExecuteServerCommand(this IAdbClient client, string target, string command, IAdbSocket socket, Func<string, bool>? predicate, Encoding encoding) =>
            client.ExecuteServerCommand(target, command, socket, predicate.AsShellOutputReceiver(), encoding);

        /// <summary>
        /// Executes a shell command on the device.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="command">The command to execute.</param>
        /// <param name="device">The device on which to run the command.</param>
        /// <param name="predicate">Optionally, a <see cref="Func{String, Boolean}"/> that processes the command output.</param>
        /// <param name="encoding">The encoding to use when parsing the command output.</param>
        public static void ExecuteRemoteCommand(this IAdbClient client, string command, DeviceData device, Func<string, bool>? predicate, Encoding encoding) =>
            client.ExecuteRemoteCommand(command, device, predicate.AsShellOutputReceiver(), encoding);

        /// <summary>
        /// Executes a command on the adb server and returns the output.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="target">The target of command, such as <c>shell</c>, <c>remount</c>, <c>dev</c>, <c>tcp</c>, <c>local</c>,
        /// <c>localreserved</c>, <c>localabstract</c>, <c>jdwp</c>, <c>track-jdwp</c>, <c>sync</c>, <c>reverse</c> and so on.</param>
        /// <param name="command">The command to execute.</param>
        /// <returns>A <see cref="IEnumerable{String}"/> of strings, each representing a line of output from the command.</returns>
        public static IEnumerable<string> ExecuteServerCommand(this IAdbClient client, string target, string command) =>
            client.ExecuteServerCommand(target, command, AdbClient.Encoding);

        /// <summary>
        /// Executes a command on the adb server and returns the output.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="target">The target of command, such as <c>shell</c>, <c>remount</c>, <c>dev</c>, <c>tcp</c>, <c>local</c>,
        /// <c>localreserved</c>, <c>localabstract</c>, <c>jdwp</c>, <c>track-jdwp</c>, <c>sync</c>, <c>reverse</c> and so on.</param>
        /// <param name="command">The command to execute.</param>
        /// <param name="socket">The <see cref="IAdbSocket"/> to send command.</param>
        /// <returns>A <see cref="IEnumerable{String}"/> of strings, each representing a line of output from the command.</returns>
        public static IEnumerable<string> ExecuteServerCommand(this IAdbClient client, string target, string command, IAdbSocket socket) =>
            client.ExecuteServerCommand(target, command, socket, AdbClient.Encoding);

        /// <summary>
        /// Executes a shell command on the device and returns the output.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="command">The command to execute.</param>
        /// <param name="device">The device on which to run the command.</param>
        /// <returns>A <see cref="IEnumerable{String}"/> of strings, each representing a line of output from the command.</returns>
        public static IEnumerable<string> ExecuteRemoteCommand(this IAdbClient client, string command, DeviceData device) =>
            client.ExecuteRemoteCommand(command, device, AdbClient.Encoding);

        /// <summary>
        /// Runs the event log service on a device.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="device">The device on which to run the event log service.</param>
        /// <param name="messageSink">A callback which will receive the event log messages as they are received.</param>
        /// <param name="logNames">Optionally, the names of the logs to receive.</param>
        public static void RunLogService(this IAdbClient client, DeviceData device, Action<LogEntry> messageSink, params LogId[] logNames) =>
            client.RunLogService(device, messageSink, false, logNames);

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
                : client.Pair(address.ToString(), AdbClient.DefaultPort, code);

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
                : client.Pair(endpoint.Address.ToString(), endpoint.Port, code);

        /// <summary>
        /// Pair with a device for secure TCP/IP communication.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="host">The host address of the remote device.</param>
        /// <param name="code">The pairing code.</param>
        /// <returns>The results from adb.</returns>
        public static string Pair(this IAdbClient client, string host, string code) =>
            client.Pair(host, AdbClient.DefaultPort, code);

        /// <summary>
        /// Connect to a device via TCP/IP.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="address">The IP address of the remote device.</param>
        /// <returns>The results from adb.</returns>
        public static string Connect(this IAdbClient client, IPAddress address) =>
            address == null
                ? throw new ArgumentNullException(nameof(address))
                : client.Connect(address.ToString(), AdbClient.DefaultPort);

        /// <summary>
        /// Connect to a device via TCP/IP.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="endpoint">The IP endpoint at which the <c>adb</c> server on the device is running.</param>
        /// <returns>The results from adb.</returns>
        public static string Connect(this IAdbClient client, IPEndPoint endpoint) =>
            endpoint == null
                ? throw new ArgumentNullException(nameof(endpoint))
                : client.Connect(endpoint.Address.ToString(), endpoint.Port);

        /// <summary>
        /// Connect to a device via TCP/IP.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="host">The host address of the remote device.</param>
        /// <returns>The results from adb.</returns>
        public static string Connect(this IAdbClient client, string host) =>
            client.Connect(host, AdbClient.DefaultPort);

        /// <summary>
        /// Disconnects a remote device from this local ADB server.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="address">The IP address of the remote device.</param>
        /// <returns>The results from adb.</returns>
        public static string Disconnect(this IAdbClient client, IPAddress address) =>
            address == null
                ? throw new ArgumentNullException(nameof(address))
                : client.Disconnect(address.ToString(), AdbClient.DefaultPort);

        /// <summary>
        /// Disconnects a remote device from this local ADB server.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="endpoint">The IP endpoint at which the <c>adb</c> server on the device is running.</param>
        /// <returns>The results from adb.</returns>
        public static string Disconnect(this IAdbClient client, IPEndPoint endpoint) =>
            endpoint == null
                ? throw new ArgumentNullException(nameof(endpoint))
                : client.Disconnect(endpoint.Address.ToString(), endpoint.Port);

        /// <summary>
        /// Disconnects a remote device from this local ADB server.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="host">The host address of the remote device.</param>
        /// <returns>The results from adb.</returns>
        public static string Disconnect(this IAdbClient client, string host) =>
            client.Disconnect(host, AdbClient.DefaultPort);

        /// <summary>
        /// Creates a new instance of the <see cref="DeviceClient"/> class, which can be used to interact with a device.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="device">The device on which to process command.</param>
        /// <returns>A new instance of the <see cref="DeviceClient"/> class.</returns>
        public static DeviceClient CreateDeviceClient(this IAdbClient client, DeviceData device) => new(client, device);

        /// <summary>
        /// Creates a new instance of the <see cref="DeviceClient"/> class, which can be used to get information about packages that are installed on a device.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="device">The device on which to look for packages.</param>
        /// <param name="arguments">The arguments to pass to <c>pm list packages</c>.</param>
        /// <returns>A new instance of the <see cref="PackageManager"/> class.</returns>
        public static PackageManager CreatePackageManager(this IAdbClient client, DeviceData device, params string[] arguments) => new(client, device, arguments);

#if !NETFRAMEWORK || NET40_OR_GREATER
        /// <summary>
        /// Runs the event log service on a device.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="device">The device on which to run the event log service.</param>
        /// <param name="progress">A callback which will receive the event log messages as they are received.</param>
        /// <param name="logNames">Optionally, the names of the logs to receive.</param>
        public static void RunLogService(this IAdbClient client, DeviceData device, IProgress<LogEntry> progress, params LogId[] logNames) =>
            client.RunLogService(device, progress, false, logNames);

        /// <summary>
        /// Runs the event log service on a device.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="device">The device on which to run the event log service.</param>
        /// <param name="progress">A callback which will receive the event log messages as they are received.</param>
        /// <param name="isCancelled">A <see cref="bool"/> that can be used to cancel the task.</param>
        /// <param name="logNames">Optionally, the names of the logs to receive.</param>
        public static void RunLogService(this IAdbClient client, DeviceData device, IProgress<LogEntry> progress, in bool isCancelled, params LogId[] logNames) =>
            client.RunLogService(device, progress.Report, isCancelled, logNames);

        /// <summary>
        /// Installs an Android application on an device.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="device">The device on which to install the application.</param>
        /// <param name="apk">A <see cref="Stream"/> which represents the application to install.</param>
        /// <param name="progress">An optional parameter which, when specified, returns progress notifications.
        /// The progress is reported as <see cref="InstallProgressEventArgs"/>, representing the state of installation.</param>
        /// <param name="arguments">The arguments to pass to <c>adb install</c>.</param>
        public static void Install(this IAdbClient client, DeviceData device, Stream apk, IProgress<InstallProgressEventArgs>? progress = null, params string[] arguments) =>
            client.Install(device, apk, progress.AsAction(), arguments);

        /// <summary>
        /// Push multiple APKs to the device and install them.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="device">The device on which to install the application.</param>
        /// <param name="baseAPK">A <see cref="Stream"/> which represents the base APK to install.</param>
        /// <param name="splitAPKs"><see cref="Stream"/>s which represents the split APKs to install.</param>
        /// <param name="progress">An optional parameter which, when specified, returns progress notifications.
        /// The progress is reported as <see cref="InstallProgressEventArgs"/>, representing the state of installation.</param>
        /// <param name="arguments">The arguments to pass to <c>adb install-create</c>.</param>
        public static void InstallMultiple(this IAdbClient client, DeviceData device, Stream baseAPK, IEnumerable<Stream> splitAPKs, IProgress<InstallProgressEventArgs>? progress = null, params string[] arguments) =>
            client.InstallMultiple(device, baseAPK, splitAPKs, progress.AsAction(), arguments);

        /// <summary>
        /// Push multiple APKs to the device and install them.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="device">The device on which to install the application.</param>
        /// <param name="splitAPKs"><see cref="Stream"/>s which represents the split APKs to install.</param>
        /// <param name="packageName">The package name of the base APK to install.</param>
        /// <param name="progress">An optional parameter which, when specified, returns progress notifications.
        /// The progress is reported as <see cref="InstallProgressEventArgs"/>, representing the state of installation.</param>
        /// <param name="arguments">The arguments to pass to <c>adb install-create</c>.</param>
        public static void InstallMultiple(this IAdbClient client, DeviceData device, IEnumerable<Stream> splitAPKs, string packageName, IProgress<InstallProgressEventArgs>? progress = null, params string[] arguments) =>
            client.InstallMultiple(device, splitAPKs, packageName, progress.AsAction(), arguments);

        /// <summary>
        /// Write an apk into the given install session.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="device">The device on which to install the application.</param>
        /// <param name="apk">A <see cref="Stream"/> which represents the application to install.</param>
        /// <param name="apkName">The name of the application.</param>
        /// <param name="session">The session ID of the install session.</param>
        /// <param name="progress">An optional parameter which, when specified, returns progress notifications.
        /// The progress is reported as a value between 0 and 100, representing the percentage of the apk which has been transferred.</param>
        public static void InstallWrite(this IAdbClient client, DeviceData device, Stream apk, string apkName, string session, IProgress<double>? progress) =>
            client.InstallWrite(device, apk, apkName, session, progress.AsAction());

        /// <summary>
        /// Pair with a device for secure TCP/IP communication.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="endpoint">The DNS endpoint at which the <c>adb</c> server on the device is running.</param>
        /// <param name="code">The pairing code.</param>
        /// <returns>The results from adb.</returns>
        public static string Pair(this IAdbClient client, DnsEndPoint endpoint, string code) =>
            endpoint == null
                ? throw new ArgumentNullException(nameof(endpoint))
                : client.Pair(endpoint.Host, endpoint.Port, code);

        /// <summary>
        /// Connect to a device via TCP/IP.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="endpoint">The DNS endpoint at which the <c>adb</c> server on the device is running.</param>
        /// <returns>The results from adb.</returns>
        public static string Connect(this IAdbClient client, DnsEndPoint endpoint) =>
            endpoint == null
                ? throw new ArgumentNullException(nameof(endpoint))
                : client.Connect(endpoint.Host, endpoint.Port);

        /// <summary>
        /// Disconnects a remote device from this local ADB server.
        /// </summary>
        /// <param name="client">An instance of a class that implements the <see cref="IAdbClient"/> interface.</param>
        /// <param name="endpoint">The DNS endpoint at which the <c>adb</c> server on the device is running.</param>
        /// <returns>The results from adb.</returns>
        public static string Disconnect(this IAdbClient client, DnsEndPoint endpoint) =>
            endpoint == null
                ? throw new ArgumentNullException(nameof(endpoint))
                : client.Disconnect(endpoint.Host, endpoint.Port);
#endif
    }
}
