#if HAS_TASK
// <copyright file="IAdbClient.Async.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace AdvancedSharpAdbClient
{
    public partial interface IAdbClient
    {
        /// <summary>
        /// Asynchronously ask the ADB server for its internal version number.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task{Int32}"/> which returns the ADB version number.</returns>
        Task<int> GetAdbVersionAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously ask the ADB server to quit immediately. This is used when the
        /// ADB client detects that an obsolete server is running after an upgrade.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        Task KillAdbAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously gets the devices that are available for communication.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task{IEnumerable}"/> which returns the list of devices that are connected.</returns>
        Task<IEnumerable<DeviceData>> GetDevicesAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously asks the ADB server to forward local connections from <paramref name="local"/>
        /// to the <paramref name="remote"/> address on the <paramref name="device"/>.
        /// </summary>
        /// <param name="device">The device on which to forward the connections.</param>
        /// <param name="local">
        /// <para>The local address to forward. This value can be in one of:</para>
        /// <list type="ordered">
        ///   <item>
        ///     <c>tcp:&lt;port&gt;</c>: TCP connection on localhost:&lt;port&gt;
        ///   </item>
        ///   <item>
        ///     <c>local:&lt;path&gt;</c>: Unix local domain socket on &lt;path&gt;
        ///   </item>
        /// </list>
        /// </param>
        /// <param name="remote">
        /// <para>The remote address to forward. This value can be in one of:</para>
        /// <list type="ordered">
        ///   <item>
        ///     <c>tcp:&lt;port&gt;</c>: TCP connection on localhost:&lt;port&gt; on device
        ///   </item>
        ///   <item>
        ///     <c>local:&lt;path&gt;</c>: Unix local domain socket on &lt;path&gt; on device
        ///   </item>
        ///   <item>
        ///     <c>jdwp:&lt;pid&gt;</c>: JDWP thread on VM process &lt;pid&gt; on device.
        ///   </item>
        /// </list>
        /// </param>
        /// <param name="allowRebind">If set to <see langword="true"/>, the request will fail if there is already a forward
        /// connection from <paramref name="local"/>.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.
        /// If your requested to start forwarding to local port TCP:0, the port number of the TCP port
        /// which has been opened. In all other cases, <c>0</c>.</returns>
        Task<int> CreateForwardAsync(DeviceData device, string local, string remote, bool allowRebind, CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously asks the ADB server to reverse forward local connections from <paramref name="remote"/>
        /// to the <paramref name="local"/> address on the <paramref name="device"/>.
        /// </summary>
        /// <param name="device">The device on which to reverse forward the connections.</param>
        /// <param name="remote">
        /// <para>The remote address to reverse forward. This value can be in one of:</para>
        /// <list type="ordered">
        ///   <item>
        ///     <c>tcp:&lt;port&gt;</c>: TCP connection on localhost:&lt;port&gt; on device
        ///   </item>
        ///   <item>
        ///     <c>local:&lt;path&gt;</c>: Unix local domain socket on &lt;path&gt; on device
        ///   </item>
        ///   <item>
        ///     <c>jdwp:&lt;pid&gt;</c>: JDWP thread on VM process &lt;pid&gt; on device.
        ///   </item>
        /// </list>
        /// </param>
        /// <param name="local">
        /// <para>The local address to reverse forward. This value can be in one of:</para>
        /// <list type="ordered">
        ///   <item>
        ///     <c>tcp:&lt;port&gt;</c>: TCP connection on localhost:&lt;port&gt;
        ///   </item>
        ///   <item>
        ///     <c>local:&lt;path&gt;</c>: Unix local domain socket on &lt;path&gt;
        ///   </item>
        /// </list>
        /// </param>
        /// <param name="allowRebind">If set to <see langword="true"/>, the request will fail if if the specified
        /// socket is already bound through a previous reverse command.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.
        /// If your requested to start reverse to remote port TCP:0, the port number of the TCP port
        /// which has been opened. In all other cases, <c>0</c>.</returns>
        Task<int> CreateReverseForwardAsync(DeviceData device, string remote, string local, bool allowRebind, CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously remove a reverse port forwarding between a remote and a local port.
        /// </summary>
        /// <param name="device">The device on which to remove the reverse port forwarding</param>
        /// <param name="remote">Specification of the remote that was forwarded</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        Task RemoveReverseForwardAsync(DeviceData device, string remote, CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously removes all reverse forwards for a given device.
        /// </summary>
        /// <param name="device">The device on which to remove all reverse port forwarding</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        Task RemoveAllReverseForwardsAsync(DeviceData device, CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously remove a port forwarding between a local and a remote port.
        /// </summary>
        /// <param name="device">The device on which to remove the port forwarding.</param>
        /// <param name="localPort">Specification of the local port that was forwarded.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        Task RemoveForwardAsync(DeviceData device, int localPort, CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously removes all forwards for a given device.
        /// </summary>
        /// <param name="device">The device on which to remove the port forwarding.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        Task RemoveAllForwardsAsync(DeviceData device, CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously list all existing forward connections from this server.
        /// </summary>
        /// <param name="device">The device for which to list the existing forward connections.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task{IEnumerable}"/> which returns the <see cref="ForwardData"/> entry for each existing forward connection.</returns>
        Task<IEnumerable<ForwardData>> ListForwardAsync(DeviceData device, CancellationToken cancellationToken);

        /// <summary>
        /// List all existing reverse forward connections from this server.
        /// </summary>
        /// <param name="device">The device for which to list the existing reverse foward connections.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task{IEnumerable}"/> which returns the <see cref="ForwardData"/> entry for each existing reverse forward connection.</returns>
        Task<IEnumerable<ForwardData>> ListReverseForwardAsync(DeviceData device, CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously executes a command on the adb server.
        /// </summary>
        /// <param name="target">The target of command, such as <c>shell</c>, <c>remount</c>, <c>dev</c>, <c>tcp</c>, <c>local</c>,
        /// <c>localreserved</c>, <c>localabstract</c>, <c>jdwp</c>, <c>track-jdwp</c>, <c>sync</c>, <c>reverse</c> and so on.</param>
        /// <param name="command">The command to execute.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        Task ExecuteServerCommandAsync(string target, string command, CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously executes a command on the adb server.
        /// </summary>
        /// <param name="target">The target of command, such as <c>shell</c>, <c>remount</c>, <c>dev</c>, <c>tcp</c>, <c>local</c>,
        /// <c>localreserved</c>, <c>localabstract</c>, <c>jdwp</c>, <c>track-jdwp</c>, <c>sync</c>, <c>reverse</c> and so on.</param>
        /// <param name="command">The command to execute.</param>
        /// <param name="socket">The <see cref="IAdbSocket"/> to send command.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        Task ExecuteServerCommandAsync(string target, string command, IAdbSocket socket, CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously executes a command on the device.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <param name="device">The device on which to run the command.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        Task ExecuteRemoteCommandAsync(string command, DeviceData device, CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously executes a command on the adb server.
        /// </summary>
        /// <param name="target">The target of command, such as <c>shell</c>, <c>remount</c>, <c>dev</c>, <c>tcp</c>, <c>local</c>,
        /// <c>localreserved</c>, <c>localabstract</c>, <c>jdwp</c>, <c>track-jdwp</c>, <c>sync</c>, <c>reverse</c> and so on.</param>
        /// <param name="command">The command to execute.</param>
        /// <param name="receiver">Optionally, a <see cref="IShellOutputReceiver"/> that processes the command output.</param>
        /// <param name="encoding">The encoding to use when parsing the command output.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        Task ExecuteServerCommandAsync(string target, string command, IShellOutputReceiver? receiver, Encoding encoding, CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously executes a command on the adb server.
        /// </summary>
        /// <param name="target">The target of command, such as <c>shell</c>, <c>remount</c>, <c>dev</c>, <c>tcp</c>, <c>local</c>,
        /// <c>localreserved</c>, <c>localabstract</c>, <c>jdwp</c>, <c>track-jdwp</c>, <c>sync</c>, <c>reverse</c> and so on.</param>
        /// <param name="command">The command to execute.</param>
        /// <param name="socket">The <see cref="IAdbSocket"/> to send command.</param>
        /// <param name="receiver">Optionally, a <see cref="IShellOutputReceiver"/> that processes the command output.</param>
        /// <param name="encoding">The encoding to use when parsing the command output.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        Task ExecuteServerCommandAsync(string target, string command, IAdbSocket socket, IShellOutputReceiver? receiver, Encoding encoding, CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously executes a command on the device.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <param name="device">The device on which to run the command.</param>
        /// <param name="receiver">Optionally, a <see cref="IShellOutputReceiver"/> that processes the command output.</param>
        /// <param name="encoding">The encoding to use when parsing the command output.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        Task ExecuteRemoteCommandAsync(string command, DeviceData device, IShellOutputReceiver? receiver, Encoding encoding, CancellationToken cancellationToken);

#if COMP_NETSTANDARD2_1
        /// <summary>
        /// Asynchronously executes a command on the adb server and returns the output.
        /// </summary>
        /// <param name="target">The target of command, such as <c>shell</c>, <c>remount</c>, <c>dev</c>, <c>tcp</c>, <c>local</c>,
        /// <c>localreserved</c>, <c>localabstract</c>, <c>jdwp</c>, <c>track-jdwp</c>, <c>sync</c>, <c>reverse</c> and so on.</param>
        /// <param name="command">The command to execute.</param>
        /// <param name="encoding">The encoding to use when parsing the command output.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="IAsyncEnumerable{String}"/> of strings, each representing a line of output from the command.</returns>
        IAsyncEnumerable<string> ExecuteServerCommandAsync(string target, string command, Encoding encoding, CancellationToken cancellationToken) =>
            ExecuteServerCommand(target, command, encoding).AsEnumerableAsync(cancellationToken);

        /// <summary>
        /// Asynchronously executes a command on the adb server and returns the output.
        /// </summary>
        /// <param name="target">The target of command, such as <c>shell</c>, <c>remount</c>, <c>dev</c>, <c>tcp</c>, <c>local</c>,
        /// <c>localreserved</c>, <c>localabstract</c>, <c>jdwp</c>, <c>track-jdwp</c>, <c>sync</c>, <c>reverse</c> and so on.</param>
        /// <param name="command">The command to execute.</param>
        /// <param name="socket">The <see cref="IAdbSocket"/> to send command.</param>
        /// <param name="encoding">The encoding to use when parsing the command output.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="IAsyncEnumerable{String}"/> of strings, each representing a line of output from the command.</returns>
        IAsyncEnumerable<string> ExecuteServerCommandAsync(string target, string command, IAdbSocket socket, Encoding encoding, CancellationToken cancellationToken) =>
            ExecuteServerCommand(target, command, socket, encoding).AsEnumerableAsync(cancellationToken);

        /// <summary>
        /// Asynchronously executes a command on the device and returns the output.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <param name="device">The device on which to run the command.</param>
        /// <param name="encoding">The encoding to use when parsing the command output.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="IAsyncEnumerable{String}"/> of strings, each representing a line of output from the command.</returns>
        IAsyncEnumerable<string> ExecuteRemoteCommandAsync(string command, DeviceData device, Encoding encoding, CancellationToken cancellationToken) =>
            ExecuteRemoteCommand(command, device, encoding).AsEnumerableAsync(cancellationToken);

        /// <summary>
        /// Asynchronously runs the event log service on a device and returns it.
        /// </summary>
        /// <param name="device">The device on which to run the event log service.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the event log service. Use this to stop reading from the event log.</param>
        /// <param name="logNames">Optionally, the names of the logs to receive.</param>
        /// <returns>A <see cref="IAsyncEnumerable{LogEntry}"/> which contains the log entries.</returns>
        IAsyncEnumerable<LogEntry> RunLogServiceAsync(DeviceData device, CancellationToken cancellationToken, params LogId[] logNames) =>
            RunLogService(device, logNames).AsEnumerableAsync(cancellationToken);
#endif

        /// <summary>
        /// Asynchronously runs the event log service on a device.
        /// </summary>
        /// <param name="device">The device on which to run the event log service.</param>
        /// <param name="messageSink">A callback which will receive the event log messages as they are received.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the event log service. Use this to stop reading from the event log.</param>
        /// <param name="logNames">Optionally, the names of the logs to receive.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        Task RunLogServiceAsync(DeviceData device, Action<LogEntry> messageSink, CancellationToken cancellationToken, params LogId[] logNames);

        /// <summary>
        /// Asynchronously gets the frame buffer from the specified end point.
        /// </summary>
        /// <param name="device">The device for which to get the framebuffer.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the asynchronous task.</param>
        /// <returns>A <see cref="Task{Framebuffer}"/> which returns the raw frame buffer.</returns>
        /// <exception cref="AdbException">failed asking for frame buffer</exception>
        /// <exception cref="AdbException">failed nudging</exception>
        Task<Framebuffer> GetFrameBufferAsync(DeviceData device, CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously reboots the specified device in to the specified mode.
        /// </summary>
        /// <param name="into">The mode into which to reboot the device.</param>
        /// <param name="device">The device to reboot.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        Task RebootAsync(string into, DeviceData device, CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously pair with a device for secure TCP/IP communication
        /// </summary>
        /// <param name="host">The host address of the remote device.</param>
        /// <param name="port">The port of the remote device.</param>
        /// <param name="code">The pairing code.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task{String}"/> which returns the results from adb.</returns>
        Task<string> PairAsync(string host, int port, string code, CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously connect to a device via TCP/IP.
        /// </summary>
        /// <param name="host">The host address of the remote device.</param>
        /// <param name="port">The port of the remote device.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task{String}"/> which returns the results from adb.</returns>
        Task<string> ConnectAsync(string host, int port, CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously disconnects a remote device from this local ADB server.
        /// </summary>
        /// <param name="host">The host address of the remote device.</param>
        /// <param name="port">The port of the remote device.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task{String}"/> which returns the results from adb.</returns>
        Task<string> DisconnectAsync(string host, int port, CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously restarts the ADB daemon running on the device with root privileges.
        /// </summary>
        /// <param name="device">The device on which to restart ADB with root privileges.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        Task RootAsync(DeviceData device, CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously restarts the ADB daemon running on the device without root privileges.
        /// </summary>
        /// <param name="device">The device on which to restart ADB without root privileges.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        Task UnrootAsync(DeviceData device, CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously installs an Android application on an device.
        /// </summary>
        /// <param name="device">The device on which to install the application.</param>
        /// <param name="apk">A <see cref="Stream"/> which represents the application to install.</param>
        /// <param name="callback">An optional parameter which, when specified, returns progress notifications.
        /// The progress is reported as <see cref="InstallProgressEventArgs"/>, representing the state of installation.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <param name="arguments">The arguments to pass to <c>adb install</c>.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        Task InstallAsync(DeviceData device, Stream apk, Action<InstallProgressEventArgs>? callback, CancellationToken cancellationToken, params string[] arguments);

        /// <summary>
        /// Asynchronously push multiple APKs to the device and install them.
        /// </summary>
        /// <param name="device">The device on which to install the application.</param>
        /// <param name="baseAPK">A <see cref="Stream"/> which represents the base APK to install.</param>
        /// <param name="splitAPKs"><see cref="Stream"/>s which represents the split APKs to install.</param>
        /// <param name="callback">An optional parameter which, when specified, returns progress notifications.
        /// The progress is reported as <see cref="InstallProgressEventArgs"/>, representing the state of installation.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <param name="arguments">The arguments to pass to <c>adb install-create</c>.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        Task InstallMultipleAsync(DeviceData device, Stream baseAPK, IEnumerable<Stream> splitAPKs, Action<InstallProgressEventArgs>? callback, CancellationToken cancellationToken, params string[] arguments);

        /// <summary>
        /// Asynchronously push multiple APKs to the device and install them.
        /// </summary>
        /// <param name="device">The device on which to install the application.</param>
        /// <param name="splitAPKs"><see cref="Stream"/>s which represents the split APKs to install.</param>
        /// <param name="packageName">The package name of the base APK to install.</param>
        /// <param name="callback">An optional parameter which, when specified, returns progress notifications.
        /// The progress is reported as <see cref="InstallProgressEventArgs"/>, representing the state of installation.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <param name="arguments">The arguments to pass to <c>adb install-create</c>.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        Task InstallMultipleAsync(DeviceData device, IEnumerable<Stream> splitAPKs, string packageName, Action<InstallProgressEventArgs>? callback, CancellationToken cancellationToken, params string[] arguments);

        /// <summary>
        /// Like "install", but starts an install session asynchronously.
        /// Use <see cref="InstallCreateAsync(DeviceData, string, CancellationToken, string[])"/> if installation dose not have a base APK.
        /// </summary>
        /// <param name="device">The device on which to install the application.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <param name="arguments">The arguments to pass to <c>adb install-create</c>.</param>
        /// <returns>A <see cref="Task{String}"/> which returns the session ID</returns>
        Task<string> InstallCreateAsync(DeviceData device, CancellationToken cancellationToken, params string[] arguments);

        /// <summary>
        /// Like "install", but starts an install session asynchronously.
        /// Use <see cref="InstallCreateAsync(DeviceData, CancellationToken, string[])"/> if installation has a base APK.
        /// </summary>
        /// <param name="device">The device on which to install the application.</param>
        /// <param name="packageName">The package name of the baseAPK to install.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <param name="arguments">The arguments to pass to <c>adb install-create</c>.</param>
        /// <returns>A <see cref="Task{String}"/> which returns the session ID</returns>
        Task<string> InstallCreateAsync(DeviceData device, string packageName, CancellationToken cancellationToken, params string[] arguments);

        /// <summary>
        /// Asynchronously write an apk into the given install session.
        /// </summary>
        /// <param name="device">The device on which to install the application.</param>
        /// <param name="apk">A <see cref="Stream"/> which represents the application to install.</param>
        /// <param name="apkName">The name of the application.</param>
        /// <param name="session">The session ID of the install session.</param>
        /// <param name="callback">An optional parameter which, when specified, returns progress notifications.
        /// The progress is reported as a value between 0 and 100, representing the percentage of the apk which has been transferred.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        Task InstallWriteAsync(DeviceData device, Stream apk, string apkName, string session, Action<double>? callback, CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously commit the given active install session, installing the app.
        /// </summary>
        /// <param name="device">The device on which to install the application.</param>
        /// <param name="session">The session ID of the install session.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        Task InstallCommitAsync(DeviceData device, string session, CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously uninstalls an Android application on an device.
        /// </summary>
        /// <param name="device">The device on which to install the application.</param>
        /// <param name="packageName">The name of the package to uninstall.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <param name="arguments">The arguments to pass to <c>adb uninstall</c>.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        Task UninstallAsync(DeviceData device, string packageName, CancellationToken cancellationToken, params string[] arguments);

        /// <summary>
        /// Asynchronously lists all features supported by the current device.
        /// </summary>
        /// <param name="device">The device for which to get the list of features supported.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task{IEnumerable}"/> which returns the list of all features supported by the current device.</returns>
        Task<IEnumerable<string>> GetFeatureSetAsync(DeviceData device, CancellationToken cancellationToken);

#if HAS_WINRT
        /// <summary>
        /// Provides access to the WinRT specific methods of the <see cref="IAdbClient"/> interface.
        /// </summary>
        public interface IWinRT
        {
            /// <summary>
            /// Asynchronously installs an Android application on an device.
            /// </summary>
            /// <param name="device">The device on which to install the application.</param>
            /// <param name="apk">A <see cref="IRandomAccessStream"/> which represents the application to install.</param>
            /// <param name="callback">An optional parameter which, when specified, returns progress notifications.
            /// The progress is reported as <see cref="InstallProgressEventArgs"/>, representing the state of installation.</param>
            /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
            /// <param name="arguments">The arguments to pass to <c>adb install</c>.</param>
            /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
            [ContractVersion(typeof(UniversalApiContract), 65536u)]
            Task InstallAsync(DeviceData device, IRandomAccessStream apk, Action<InstallProgressEventArgs>? callback, CancellationToken cancellationToken, params string[] arguments);

            /// <summary>
            /// Asynchronously push multiple APKs to the device and install them.
            /// </summary>
            /// <param name="device">The device on which to install the application.</param>
            /// <param name="baseAPK">A <see cref="IRandomAccessStream"/> which represents the base APK to install.</param>
            /// <param name="splitAPKs"><see cref="IRandomAccessStream"/>s which represents the split APKs to install.</param>
            /// <param name="callback">An optional parameter which, when specified, returns progress notifications.
            /// The progress is reported as <see cref="InstallProgressEventArgs"/>, representing the state of installation.</param>
            /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
            /// <param name="arguments">The arguments to pass to <c>adb install-create</c>.</param>
            /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
            [ContractVersion(typeof(UniversalApiContract), 65536u)]
            Task InstallMultipleAsync(DeviceData device, IRandomAccessStream baseAPK, IEnumerable<IRandomAccessStream> splitAPKs, Action<InstallProgressEventArgs>? callback, CancellationToken cancellationToken, params string[] arguments);

            /// <summary>
            /// Asynchronously push multiple APKs to the device and install them.
            /// </summary>
            /// <param name="device">The device on which to install the application.</param>
            /// <param name="splitAPKs"><see cref="IRandomAccessStream"/>s which represents the split APKs to install.</param>
            /// <param name="packageName">The package name of the base APK to install.</param>
            /// <param name="callback">An optional parameter which, when specified, returns progress notifications.
            /// The progress is reported as <see cref="InstallProgressEventArgs"/>, representing the state of installation.</param>
            /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
            /// <param name="arguments">The arguments to pass to <c>adb install-create</c>.</param>
            /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
            [ContractVersion(typeof(UniversalApiContract), 65536u)]
            Task InstallMultipleAsync(DeviceData device, IEnumerable<IRandomAccessStream> splitAPKs, string packageName, Action<InstallProgressEventArgs>? callback, CancellationToken cancellationToken, params string[] arguments);

            /// <summary>
            /// Asynchronously write an apk into the given install session.
            /// </summary>
            /// <param name="device">The device on which to install the application.</param>
            /// <param name="apk">A <see cref="IRandomAccessStream"/> which represents the application to install.</param>
            /// <param name="apkName">The name of the application.</param>
            /// <param name="session">The session ID of the install session.</param>
            /// <param name="callback">An optional parameter which, when specified, returns progress notifications.
            /// The progress is reported as a value between 0 and 100, representing the percentage of the apk which has been transferred.</param>
            /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
            /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
            [ContractVersion(typeof(UniversalApiContract), 65536u)]
            Task InstallWriteAsync(DeviceData device, IRandomAccessStream apk, string apkName, string session, Action<double>? callback, CancellationToken cancellationToken);
        }
#endif
    }
}
#endif