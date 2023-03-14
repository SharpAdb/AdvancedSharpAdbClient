﻿// <copyright file="IAdbClient.Async.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using AdvancedSharpAdbClient.Exceptions;
using AdvancedSharpAdbClient.Logs;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace AdvancedSharpAdbClient
{
    public partial interface IAdbClient
    {
        /// <summary>
        /// Ask the ADB server for its internal version number.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>An <see cref="Task"/> which return the ADB version number.</returns>
        Task<int> GetAdbVersionAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Gets the devices that are available for communication.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>An <see cref="Task"/> which return the list of devices that are connected.</returns>
        Task<List<DeviceData>> GetDevicesAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Asks the ADB server to forward local connections from <paramref name="local"/>
        /// to the <paramref name="remote"/> address on the <paramref name="device"/>.
        /// </summary>
        /// <param name="device">The device to which to forward the connections.</param>
        /// <param name="local">
        /// <para>
        /// The local address to forward. This value can be in one of:
        /// </para>
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
        /// <para>
        /// The remote address to forward. This value can be in one of:
        /// </para>
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
        /// <param name="allowRebind">
        /// If set to <see langword="true"/>, the request will fail if there is already a forward
        /// connection from <paramref name="local"/>.
        /// </param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.
        /// If your requested to start forwarding to local port TCP:0, the port number of the TCP port
        /// which has been opened. In all other cases, <c>0</c>.</returns>
        Task<int> CreateForwardAsync(DeviceData device, string local, string remote, bool allowRebind, CancellationToken cancellationToken);

        /// <summary>
        /// Asks the ADB server to forward local connections from <paramref name="local"/>
        /// to the <paramref name="remote"/> address on the <paramref name="device"/>.
        /// </summary>
        /// <param name="device">The device to which to forward the connections.</param>
        /// <param name="local">
        /// <para>
        /// The local address to forward. This value can be in one of:
        /// </para>
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
        /// <para>
        /// The remote address to forward. This value can be in one of:
        /// </para>
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
        Task<int> CreateForwardAsync(DeviceData device, ForwardSpec local, ForwardSpec remote, bool allowRebind, CancellationToken cancellationToken);

        /// <summary>
        /// Asks the ADB server to reverse forward local connections from <paramref name="remote"/>
        /// to the <paramref name="local"/> address on the <paramref name="device"/>.
        /// </summary>
        /// <param name="device">The device to which to reverse forward the connections.</param>
        /// <param name="remote">
        /// <para>
        /// The remote address to reverse forward. This value can be in one of:
        /// </para>
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
        /// <para>
        /// The local address to reverse forward. This value can be in one of:
        /// </para>
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
        /// List all existing forward connections from this server.
        /// </summary>
        /// <param name="device">The device for which to list the existing forward connections.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>An <see cref="Task"/> which return the <see cref="ForwardData"/> entry for each existing forward connection.</returns>
        Task<IEnumerable<ForwardData>> ListForwardAsync(DeviceData device, CancellationToken cancellationToken);

        /// <summary>
        /// List all existing reverse forward connections from this server.
        /// </summary>
        /// <param name="device">The device for which to list the existing reverse foward connections.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>An <see cref="Task"/> which return the <see cref="ForwardData"/> entry for each existing reverse forward connection.</returns>
        Task<IEnumerable<ForwardData>> ListReverseForwardAsync(DeviceData device, CancellationToken cancellationToken);

        /// <summary>
        /// Executes a command on the device.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <param name="device">The device on which to run the command.</param>
        /// <param name="receiver">The receiver which will get the command output.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        Task ExecuteRemoteCommandAsync(string command, DeviceData device, IShellOutputReceiver receiver, CancellationToken cancellationToken);

        /// <summary>
        /// Executes a command on the device.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <param name="device">The device on which to run the command.</param>
        /// <param name="receiver">The receiver which will get the command output.</param>
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
        Task<Image> GetFrameBufferAsync(DeviceData device, CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously runs the event log service on a device.
        /// </summary>
        /// <param name="device">The device on which to run the event log service.</param>
        /// <param name="messageSink">A callback which will receive the event log messages as they are received.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the event log service. Use this to stop reading from the event log.</param>
        /// <param name="logNames">Optionally, the names of the logs to receive.</param>
        /// <returns>An <see cref="Task"/> which represents the asynchronous operation.</returns>
        Task RunLogServiceAsync(DeviceData device, Action<LogEntry> messageSink, CancellationToken cancellationToken, params LogId[] logNames);

        /// <summary>
        /// Pair with a device for secure TCP/IP communication
        /// </summary>
        /// <param name="endpoint">The DNS endpoint at which the <c>adb</c> server on the device is running.</param>
        /// <param name="code">The pairing code.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>An <see cref="Task"/> which return the results from adb.</returns>
        Task<string> PairAsync(DnsEndPoint endpoint, string code, CancellationToken cancellationToken);

        /// <summary>
        /// Connect to a device via TCP/IP.
        /// </summary>
        /// <param name="endpoint">The DNS endpoint at which the <c>adb</c> server on the device is running.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>An <see cref="Task"/> which return the results from adb.</returns>
        Task<string> ConnectAsync(DnsEndPoint endpoint, CancellationToken cancellationToken);

        /// <summary>
        /// Disconnects a remote device from this local ADB server.
        /// </summary>
        /// <param name="endpoint">The endpoint of the remote device to disconnect.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>An <see cref="Task"/> which return the results from adb.</returns>
        Task<string> DisconnectAsync(DnsEndPoint endpoint, CancellationToken cancellationToken);

        /// <summary>
        /// Lists all features supported by the current device.
        /// </summary>
        /// <param name="device">The device for which to get the list of features supported.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>An <see cref="Task"/> which return the list of all features supported by the current device.</returns>
        Task<List<string>> GetFeatureSetAsync(DeviceData device, CancellationToken cancellationToken);

        /// <summary>
        /// Gets the current device screen snapshot asynchronously.
        /// </summary>
        /// <param name="device">The device for which to get the screen snapshot.</param>
        /// <returns>An <see cref="Task"/> which return the Xml containing current hierarchy.</returns>
        Task<XmlDocument> DumpScreenAsync(DeviceData device);

        /// <summary>
        /// Clicks on the specified coordinates.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="cords"></param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        Task ClickAsync(DeviceData device, Cords cords);

        /// <summary>
        /// Clicks on the specified coordinates.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        Task ClickAsync(DeviceData device, int x, int y);

        /// <summary>
        /// Generates a swipe gesture from first element to second element Specify the speed in ms.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <param name="speed"></param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        Task SwipeAsync(DeviceData device, Element first, Element second, long speed);

        /// <summary>
        /// Generates a swipe gesture from co-ordinates x1,y1 to x2,y2 with speed Specify the speed in ms.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="speed"></param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        Task SwipeAsync(DeviceData device, int x1, int y1, int x2, int y2, long speed);

        /// <summary>
        /// Get element by xpath asynchronously. You can specify the waiting time in timeout.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="xpath"></param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        Task<Element> FindElementAsync(DeviceData device, string xpath, CancellationToken cancellationToken);

        /// <summary>
        /// Get elements by xpath asynchronously. You can specify the waiting time in timeout.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="xpath"></param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        Task<Element[]> FindElementsAsync(DeviceData device, string xpath, CancellationToken cancellationToken);

        /// <summary>
        /// Send keyevent to specific. You can see keyevents here https://developer.android.com/reference/android/view/KeyEvent.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="key"></param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        Task SendKeyEventAsync(DeviceData device, string key);

        /// <summary>
        /// Send text to device. Doesn't support Russian.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="text"></param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        Task SendTextAsync(DeviceData device, string text);

        /// <summary>
        /// Clear the input text. The input should be in focus. Use el.ClearInput() if the element isn't focused.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="charcount"></param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        Task ClearInputAsync(DeviceData device, int charcount, CancellationToken cancellationToken);

        /// <summary>
        /// Start an Android application on device.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="packagename"></param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        Task StartAppAsync(DeviceData device, string packagename, CancellationToken cancellationToken);

        /// <summary>
        /// Stop an Android application on device.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="packagename"></param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        Task StopAppAsync(DeviceData device, string packagename, CancellationToken cancellationToken);

        /// <summary>
        /// Click BACK button.
        /// </summary>
        /// <param name="device"></param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        Task BackBtnAsync(DeviceData device);

        /// <summary>
        /// Click HOME button.
        /// </summary>
        /// <param name="device"></param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        Task HomeBtnAsync(DeviceData device);
    }
}