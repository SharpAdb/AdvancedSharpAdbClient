#if HAS_TASK
// <copyright file="IAdbClient.Async.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using AdvancedSharpAdbClient.Exceptions;
using AdvancedSharpAdbClient.Logs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Xml;

namespace AdvancedSharpAdbClient
{
    public partial interface IAdbClient
    {
        /// <summary>
        /// Ask the ADB server for its internal version number.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which return the ADB version number.</returns>
        Task<int> GetAdbVersionAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Ask the ADB server to quit immediately. This is used when the
        /// ADB client detects that an obsolete server is running after an
        /// upgrade.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        Task KillAdbAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Gets the devices that are available for communication.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which return the list of devices that are connected.</returns>
        Task<IEnumerable<DeviceData>> GetDevicesAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Asks the ADB server to forward local connections from <paramref name="local"/>
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
        /// Asks the ADB server to reverse forward local connections from <paramref name="remote"/>
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
        /// Remove a reverse port forwarding between a remote and a local port.
        /// </summary>
        /// <param name="device">The device on which to remove the reverse port forwarding</param>
        /// <param name="remote">Specification of the remote that was forwarded</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        Task RemoveReverseForwardAsync(DeviceData device, string remote, CancellationToken cancellationToken);

        /// <summary>
        /// Removes all reverse forwards for a given device.
        /// </summary>
        /// <param name="device">The device on which to remove all reverse port forwarding</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        Task RemoveAllReverseForwardsAsync(DeviceData device, CancellationToken cancellationToken);

        /// <summary>
        /// Remove a port forwarding between a local and a remote port.
        /// </summary>
        /// <param name="device">The device on which to remove the port forwarding.</param>
        /// <param name="localPort">Specification of the local port that was forwarded.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        Task RemoveForwardAsync(DeviceData device, int localPort, CancellationToken cancellationToken);

        /// <summary>
        /// Removes all forwards for a given device.
        /// </summary>
        /// <param name="device">The device on which to remove the port forwarding.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        Task RemoveAllForwardsAsync(DeviceData device, CancellationToken cancellationToken);

        /// <summary>
        /// List all existing forward connections from this server.
        /// </summary>
        /// <param name="device">The device for which to list the existing forward connections.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which return the <see cref="ForwardData"/> entry for each existing forward connection.</returns>
        Task<IEnumerable<ForwardData>> ListForwardAsync(DeviceData device, CancellationToken cancellationToken);

        /// <summary>
        /// List all existing reverse forward connections from this server.
        /// </summary>
        /// <param name="device">The device for which to list the existing reverse foward connections.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which return the <see cref="ForwardData"/> entry for each existing reverse forward connection.</returns>
        Task<IEnumerable<ForwardData>> ListReverseForwardAsync(DeviceData device, CancellationToken cancellationToken);

        /// <summary>
        /// Executes a command on the adb server.
        /// </summary>
        /// <param name="target">The target of command, such as <c>shell</c>, <c>remount</c>, <c>dev</c>, <c>tcp</c>, <c>local</c>,
        /// <c>localreserved</c>, <c>localabstract</c>, <c>jdwp</c>, <c>track-jdwp</c>, <c>sync</c>, <c>reverse</c> and so on.</param>
        /// <param name="command">The command to execute.</param>
        /// <param name="encoding">The encoding to use when parsing the command output.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        Task ExecuteServerCommandAsync(string target, string command, Encoding encoding, CancellationToken cancellationToken);

        /// <summary>
        /// Executes a command on the adb server.
        /// </summary>
        /// <param name="target">The target of command, such as <c>shell</c>, <c>remount</c>, <c>dev</c>, <c>tcp</c>, <c>local</c>,
        /// <c>localreserved</c>, <c>localabstract</c>, <c>jdwp</c>, <c>track-jdwp</c>, <c>sync</c>, <c>reverse</c> and so on.</param>
        /// <param name="command">The command to execute.</param>
        /// <param name="socket">The <see cref="IAdbSocket"/> to send command.</param>
        /// <param name="encoding">The encoding to use when parsing the command output.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        Task ExecuteServerCommandAsync(string target, string command, IAdbSocket socket, Encoding encoding, CancellationToken cancellationToken);

        /// <summary>
        /// Executes a command on the device.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <param name="device">The device on which to run the command.</param>
        /// <param name="encoding">The encoding to use when parsing the command output.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        Task ExecuteRemoteCommandAsync(string command, DeviceData device, Encoding encoding, CancellationToken cancellationToken);

        /// <summary>
        /// Executes a command on the adb server.
        /// </summary>
        /// <param name="target">The target of command, such as <c>shell</c>, <c>remount</c>, <c>dev</c>, <c>tcp</c>, <c>local</c>,
        /// <c>localreserved</c>, <c>localabstract</c>, <c>jdwp</c>, <c>track-jdwp</c>, <c>sync</c>, <c>reverse</c> and so on.</param>
        /// <param name="command">The command to execute.</param>
        /// <param name="receiver">Optionally, a <see cref="IShellOutputReceiver"/> that processes the command output.</param>
        /// <param name="encoding">The encoding to use when parsing the command output.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        Task ExecuteServerCommandAsync(string target, string command, IShellOutputReceiver receiver, Encoding encoding, CancellationToken cancellationToken);

        /// <summary>
        /// Executes a command on the adb server.
        /// </summary>
        /// <param name="target">The target of command, such as <c>shell</c>, <c>remount</c>, <c>dev</c>, <c>tcp</c>, <c>local</c>,
        /// <c>localreserved</c>, <c>localabstract</c>, <c>jdwp</c>, <c>track-jdwp</c>, <c>sync</c>, <c>reverse</c> and so on.</param>
        /// <param name="command">The command to execute.</param>
        /// <param name="socket">The <see cref="IAdbSocket"/> to send command.</param>
        /// <param name="receiver">Optionally, a <see cref="IShellOutputReceiver"/> that processes the command output.</param>
        /// <param name="encoding">The encoding to use when parsing the command output.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        Task ExecuteServerCommandAsync(string target, string command, IAdbSocket socket, IShellOutputReceiver receiver, Encoding encoding, CancellationToken cancellationToken);

        /// <summary>
        /// Executes a command on the device.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <param name="device">The device on which to run the command.</param>
        /// <param name="receiver">Optionally, a <see cref="IShellOutputReceiver"/> that processes the command output.</param>
        /// <param name="encoding">The encoding to use when parsing the command output.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        Task ExecuteRemoteCommandAsync(string command, DeviceData device, IShellOutputReceiver receiver, Encoding encoding, CancellationToken cancellationToken);

        /// <summary>
        /// Gets the frame buffer from the specified end point.
        /// </summary>
        /// <param name="device">The device for which to get the framebuffer.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the asynchronous task.</param>
        /// <returns>A <see cref="Task"/> which returns the raw frame buffer.</returns>
        /// <exception cref="AdbException">failed asking for frame buffer</exception>
        /// <exception cref="AdbException">failed nudging</exception>
        Task<Framebuffer> GetFrameBufferAsync(DeviceData device, CancellationToken cancellationToken);

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
        /// Reboots the specified device in to the specified mode.
        /// </summary>
        /// <param name="into">The mode into which to reboot the device.</param>
        /// <param name="device">The device to reboot.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        Task RebootAsync(string into, DeviceData device, CancellationToken cancellationToken);

        /// <summary>
        /// Pair with a device for secure TCP/IP communication
        /// </summary>
        /// <param name="endpoint">The DNS endpoint at which the <c>adb</c> server on the device is running.</param>
        /// <param name="code">The pairing code.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which return the results from adb.</returns>
        Task<string> PairAsync(DnsEndPoint endpoint, string code, CancellationToken cancellationToken);

        /// <summary>
        /// Connect to a device via TCP/IP.
        /// </summary>
        /// <param name="endpoint">The DNS endpoint at which the <c>adb</c> server on the device is running.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which return the results from adb.</returns>
        Task<string> ConnectAsync(DnsEndPoint endpoint, CancellationToken cancellationToken);

        /// <summary>
        /// Disconnects a remote device from this local ADB server.
        /// </summary>
        /// <param name="endpoint">The endpoint of the remote device to disconnect.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which return the results from adb.</returns>
        Task<string> DisconnectAsync(DnsEndPoint endpoint, CancellationToken cancellationToken);

        /// <summary>
        /// Restarts the ADB daemon running on the device with root privileges.
        /// </summary>
        /// <param name="device">The device on which to restart ADB with root privileges.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        Task RootAsync(DeviceData device, CancellationToken cancellationToken);

        /// <summary>
        /// Restarts the ADB daemon running on the device without root privileges.
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
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <param name="arguments">The arguments to pass to <c>adb install</c>.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        Task InstallAsync(DeviceData device, Stream apk, CancellationToken cancellationToken, params string[] arguments);

        /// <summary>
        /// Asynchronously push multiple APKs to the device and install them.
        /// </summary>
        /// <param name="device">The device on which to install the application.</param>
        /// <param name="splitAPKs"><see cref="Stream"/>s which represents the split APKs to install.</param>
        /// <param name="packageName">The package name of the base APK to install.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <param name="arguments">The arguments to pass to <c>adb install-create</c>.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        Task InstallMultipleAsync(DeviceData device, IEnumerable<Stream> splitAPKs, string packageName, CancellationToken cancellationToken, params string[] arguments);

        /// <summary>
        /// Asynchronously push multiple APKs to the device and install them.
        /// </summary>
        /// <param name="device">The device on which to install the application.</param>
        /// <param name="baseAPK">A <see cref="Stream"/> which represents the base APK to install.</param>
        /// <param name="splitAPKs"><see cref="Stream"/>s which represents the split APKs to install.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <param name="arguments">The arguments to pass to <c>adb install-create</c>.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        Task InstallMultipleAsync(DeviceData device, Stream baseAPK, IEnumerable<Stream> splitAPKs, CancellationToken cancellationToken, params string[] arguments);

        /// <summary>
        /// Like "install", but starts an install session.
        /// </summary>
        /// <param name="device">The device on which to install the application.</param>
        /// <param name="packageName">The package name of the baseAPK to install.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <param name="arguments">The arguments to pass to <c>adb install-create</c>.</param>
        /// <returns>A <see cref="Task"/> which return the session ID</returns>
        Task<string> InstallCreateAsync(DeviceData device, string packageName, CancellationToken cancellationToken, params string[] arguments);

        /// <summary>
        /// Write an apk into the given install session.
        /// </summary>
        /// <param name="device">The device on which to install the application.</param>
        /// <param name="apk">A <see cref="Stream"/> which represents the application to install.</param>
        /// <param name="apkName">The name of the application.</param>
        /// <param name="session">The session ID of the install session.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        Task InstallWriteAsync(DeviceData device, Stream apk, string apkName, string session, CancellationToken cancellationToken);

        /// <summary>
        /// Commit the given active install session, installing the app.
        /// </summary>
        /// <param name="device">The device on which to install the application.</param>
        /// <param name="session">The session ID of the install session.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        Task InstallCommitAsync(DeviceData device, string session, CancellationToken cancellationToken);

        /// <summary>
        /// Lists all features supported by the current device.
        /// </summary>
        /// <param name="device">The device for which to get the list of features supported.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which return the list of all features supported by the current device.</returns>
        Task<IEnumerable<string>> GetFeatureSetAsync(DeviceData device, CancellationToken cancellationToken);

        /// <summary>
        /// Gets the current device screen snapshot asynchronously.
        /// </summary>
        /// <param name="device">The device for which to get the screen snapshot.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which return a <see cref="string"/> containing current hierarchy.
        /// Failed if start with <c>ERROR</c> or <c>java.lang.Exception</c>.</returns>
        Task<string> DumpScreenStringAsync(DeviceData device, CancellationToken cancellationToken);

        /// <summary>
        /// Gets the current device screen snapshot asynchronously.
        /// </summary>
        /// <param name="device">The device for which to get the screen snapshot.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which return a <see cref="XmlDocument"/> containing current hierarchy.</returns>
        Task<XmlDocument> DumpScreenAsync(DeviceData device, CancellationToken cancellationToken);

#if WINDOWS_UWP || WINDOWS10_0_17763_0_OR_GREATER
        /// <summary>
        /// Gets the current device screen snapshot asynchronously.
        /// </summary>
        /// <param name="device">The device for which to get the screen snapshot.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which return a <see cref="Windows.Data.Xml.Dom.XmlDocument"/> containing current hierarchy.</returns>
        Task<Windows.Data.Xml.Dom.XmlDocument> DumpScreenWinRTAsync(DeviceData device, CancellationToken cancellationToken);
#endif

        /// <summary>
        /// Clicks on the specified coordinates.
        /// </summary>
        /// <param name="device">The device on which to click.</param>
        /// <param name="cords">The <see cref="Cords"/> to click.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        Task ClickAsync(DeviceData device, Cords cords, CancellationToken cancellationToken);

        /// <summary>
        /// Clicks on the specified coordinates.
        /// </summary>
        /// <param name="device">The device on which to click.</param>
        /// <param name="x">The X co-ordinate to click.</param>
        /// <param name="y">The Y co-ordinate to click.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        Task ClickAsync(DeviceData device, int x, int y, CancellationToken cancellationToken);

        /// <summary>
        /// Generates a swipe gesture from first element to second element Specify the speed in ms.
        /// </summary>
        /// <param name="device">The device on which to swipe.</param>
        /// <param name="first">The start element.</param>
        /// <param name="second">The end element.</param>
        /// <param name="speed">The time spent in swiping.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        Task SwipeAsync(DeviceData device, Element first, Element second, long speed, CancellationToken cancellationToken);

        /// <summary>
        /// Generates a swipe gesture from co-ordinates x1,y1 to x2,y2 with speed Specify the speed in ms.
        /// </summary>
        /// <param name="device">The device on which to swipe.</param>
        /// <param name="x1">The start X co-ordinate.</param>
        /// <param name="y1">The start Y co-ordinate.</param>
        /// <param name="x2">The end X co-ordinate.</param>
        /// <param name="y2">The end Y co-ordinate.</param>
        /// <param name="speed">The time spent in swiping.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        Task SwipeAsync(DeviceData device, int x1, int y1, int x2, int y2, long speed, CancellationToken cancellationToken);

        /// <summary>
        /// Check if the app is running in foreground.
        /// </summary>
        /// <param name="device">The device on which to check.</param>
        /// <param name="packageName">The package name of the app to check.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which return the result. <see langword="true"/> if the app is running in foreground; otherwise, <see langword="false"/>.</returns>
        Task<bool> IsAppInForegroundAsync(DeviceData device, string packageName, CancellationToken cancellationToken);

        /// <summary>
        /// Check if the app is running in background.
        /// </summary>
        /// <param name="device">The device on which to check.</param>
        /// <param name="packageName">The package name of the app to check.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which return the result. <see langword="true"/> if the app is running in background; otherwise, <see langword="false"/>.</returns>
        Task<bool> IsAppRunningAsync(DeviceData device, string packageName, CancellationToken cancellationToken);

        /// <summary>
        /// Get the <see cref="AppStatus"/> of the app.
        /// </summary>
        /// <param name="device">The device on which to get status.</param>
        /// <param name="packageName">The package name of the app to check.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which return the <see cref="AppStatus"/> of the app. Foreground, stopped or running in background.</returns>
        Task<AppStatus> GetAppStatusAsync(DeviceData device, string packageName, CancellationToken cancellationToken);

        /// <summary>
        /// Get element by xpath asynchronously. You can specify the waiting time in timeout.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="xpath"></param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// Only check once if <see langword="default"/>. Or it will continue check until <see cref="CancellationToken.IsCancellationRequested"/> is <see langword="true"/>.</param>
        /// <returns>A <see cref="Task"/> which return the <see cref="Element"/> of <paramref name="xpath"/>.</returns>
        Task<Element> FindElementAsync(DeviceData device, string xpath, CancellationToken cancellationToken);

        /// <summary>
        /// Get elements by xpath asynchronously. You can specify the waiting time in timeout.
        /// </summary>
        /// <param name="device">The device on which to get elements.</param>
        /// <param name="xpath">The xpath of the elements.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// Only check once if <see langword="default"/>. Or it will continue check until <see cref="CancellationToken.IsCancellationRequested"/> is <see langword="true"/>.</param>
        /// <returns>A <see cref="Task"/> which return the <see cref="List{Element}"/> of <see cref="Element"/> has got.</returns>
        Task<IEnumerable<Element>> FindElementsAsync(DeviceData device, string xpath, CancellationToken cancellationToken);

#if NETCOREAPP3_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        /// <summary>
        /// Get elements by xpath asynchronously. You can specify the waiting time in timeout.
        /// </summary>
        /// <param name="device">The device on which to get elements.</param>
        /// <param name="xpath">The xpath of the elements.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.
        /// Only check once if <see langword="default"/>. Or it will continue check until <see cref="CancellationToken.IsCancellationRequested"/> is <see langword="true"/>.</param>
        /// <returns>A <see cref="Task"/> which return the <see cref="IAsyncEnumerable{Element}"/> of <see cref="Element"/> has got.</returns>
        IAsyncEnumerable<Element> FindAsyncElements(DeviceData device, string xpath, CancellationToken cancellationToken);
#endif

        /// <summary>
        /// Send key event to specific. You can see key events here https://developer.android.com/reference/android/view/KeyEvent.
        /// </summary>
        /// <param name="device">The device on which to send key event.</param>
        /// <param name="key">The key event to send.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        Task SendKeyEventAsync(DeviceData device, string key, CancellationToken cancellationToken);

        /// <summary>
        /// Send text to device. Doesn't support Russian.
        /// </summary>
        /// <param name="device">The device on which to send text.</param>
        /// <param name="text">The text to send.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        Task SendTextAsync(DeviceData device, string text, CancellationToken cancellationToken);

        /// <summary>
        /// Start an Android application on device.
        /// </summary>
        /// <param name="device">The device on which to start an application.</param>
        /// <param name="packageName">The package name of the application to start.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        Task StartAppAsync(DeviceData device, string packageName, CancellationToken cancellationToken);

        /// <summary>
        /// Stop an Android application on device.
        /// </summary>
        /// <param name="device">The device on which to stop an application.</param>
        /// <param name="packageName">The package name of the application to stop.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        Task StopAppAsync(DeviceData device, string packageName, CancellationToken cancellationToken);
    }
}
#endif