// <copyright file="AdbClient.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using AdvancedSharpAdbClient.WinRT.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.Foundation;
using Windows.Foundation.Metadata;
using Windows.Storage.Streams;

namespace AdvancedSharpAdbClient.WinRT
{
    /// <summary>
    /// <para>
    /// Implements the <see cref="IAdbClient"/> interface, and allows you to interact with the
    /// adb server and devices that are connected to that adb server.
    /// </para>
    /// <para>
    /// For example, to fetch a list of all devices that are currently connected to this PC, you can
    /// call the <see cref="GetDevices"/> method.
    /// </para>
    /// <para>
    /// To run a command on a device, you can use the <see cref="ExecuteRemoteCommandAsync(string, DeviceData, IShellOutputReceiver, CancellationToken)"/>
    /// method.
    /// </para>
    /// </summary>
    /// <remarks><para><seealso href="https://github.com/android/platform_system_core/blob/master/adb/SERVICES.TXT">SERVICES.TXT</seealso></para>
    /// <para><seealso href="https://github.com/android/platform_system_core/blob/master/adb/adb_client.c">adb_client.c</seealso></para>
    /// <para><seealso href="https://github.com/android/platform_system_core/blob/master/adb/adb.c">adb.c</seealso></para></remarks>
    public sealed class AdbClient
    {
        internal readonly AdvancedSharpAdbClient.AdbClient adbClient;

        /// <summary>
        /// The port at which the Android Debug Bridge server listens by default.
        /// </summary>
        public static int AdbServerPort => 5037;

        /// <summary>
        /// The default port to use when connecting to a device over TCP/IP.
        /// </summary>
        public static int DefaultPort => 5555;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdbClient"/> class.
        /// </summary>
        public AdbClient()
        {
            adbClient = new(new IPEndPoint(IPAddress.Loopback, AdbServerPort), Factories.AdbSocketFactory);
        }

        /// <summary>
        /// Create an ASCII string preceded by four hex digits. The opening "####"
        /// is the length of the rest of the string, encoded as ASCII hex(case
        /// doesn't matter).
        /// </summary>
        /// <param name="req">The request to form.</param>
        /// <returns>An array containing <c>####req</c>.</returns>
        public static byte[] FormAdbRequest(string req) => AdvancedSharpAdbClient.AdbClient.FormAdbRequest(req);

        /// <summary>
        /// Creates the adb forward request.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="port">The port.</param>
        /// <returns>This returns an array containing <c>"####tcp:{port}:{addStr}"</c>.</returns>
        public static byte[] CreateAdbForwardRequest(string address, int port) => AdvancedSharpAdbClient.AdbClient.CreateAdbForwardRequest(address, port);

        /// <summary>
        /// Ask the ADB server for its internal version number.
        /// </summary>
        /// <returns>The ADB version number.</returns>
        public int GetAdbVersion() => adbClient.GetAdbVersion();

        /// <summary>
        /// Ask the ADB server for its internal version number.
        /// </summary>
        /// <returns>An <see cref="IAsyncOperation{TResult}"/> which return the ADB version number.</returns>
        public IAsyncOperation<int> GetAdbVersionAsync() => adbClient.GetAdbVersionAsync().AsAsyncOperation();

        /// <summary>
        /// Ask the ADB server for its internal version number.
        /// </summary>
        /// <param name="timeout">A <see cref="System.TimeSpan"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>An <see cref="IAsyncOperation{TResult}"/> which return the ADB version number.</returns>
        public IAsyncOperation<int> GetAdbVersionAsync(TimeSpan timeout) => adbClient.GetAdbVersionAsync(timeout.GetCancellationToken()).AsAsyncOperation();

        /// <summary>
        /// Ask the ADB server to quit immediately. This is used when the
        /// ADB client detects that an obsolete server is running after an
        /// upgrade.
        /// </summary>
        public void KillAdb() => adbClient.KillAdb();

        /// <summary>
        /// Ask the ADB server to quit immediately. This is used when the
        /// ADB client detects that an obsolete server is running after an
        /// upgrade.
        /// </summary>
        /// <returns>A <see cref="IAsyncAction"/> which represents the asynchronous operation.</returns>
        public IAsyncAction KillAdbAsync() => adbClient.KillAdbAsync().AsAsyncAction();

        /// <summary>
        /// Ask the ADB server to quit immediately. This is used when the
        /// ADB client detects that an obsolete server is running after an
        /// upgrade.
        /// </summary>
        /// <param name="timeout">A <see cref="System.TimeSpan"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="IAsyncAction"/> which represents the asynchronous operation.</returns>
        public IAsyncAction KillAdbAsync(TimeSpan timeout) => adbClient.KillAdbAsync(timeout.GetCancellationToken()).AsAsyncAction();

        /// <summary>
        /// Gets the devices that are available for communication.
        /// </summary>
        /// <returns>A list of devices that are connected.</returns>
        /// <example>
        /// <para>
        /// The following example list all Android devices that are currently connected to this PC:
        /// </para>
        /// <code>
        /// var devices = AdbClient.Instance.GetDevices();
        /// 
        /// foreach(var device in devices)
        /// {
        ///     Console.WriteLine(device.Name);
        /// }
        /// </code>
        /// </example>
        public IEnumerable<DeviceData> GetDevices() => adbClient.GetDevices().Select(DeviceData.GetDeviceData);

        /// <summary>
        /// Gets the devices that are available for communication.
        /// </summary>
        /// <returns>An <see cref="IAsyncOperation{TResult}"/> which return the list of devices that are connected.</returns>
        public IAsyncOperation<IEnumerable<DeviceData>> GetDevicesAsync() => Task.Run(async () => (await adbClient.GetDevicesAsync()).Select(DeviceData.GetDeviceData)).AsAsyncOperation();

        /// <summary>
        /// Gets the devices that are available for communication.
        /// </summary>
        /// <param name="timeout">A <see cref="System.TimeSpan"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>An <see cref="IAsyncOperation{TResult}"/> which return the list of devices that are connected.</returns>
        public IAsyncOperation<IEnumerable<DeviceData>> GetDevicesAsync(TimeSpan timeout) => Task.Run(async () => (await adbClient.GetDevicesAsync(timeout.GetCancellationToken())).Select(DeviceData.GetDeviceData)).AsAsyncOperation();

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
        /// <returns>If your requested to start forwarding to local port TCP:0, the port number of the TCP port
        /// which has been opened. In all other cases, <c>0</c>.</returns>
        public int CreateForward(DeviceData device, string local, string remote, bool allowRebind) => adbClient.CreateForward(device.deviceData, local, remote, allowRebind);

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
        /// <returns>If your requested to start forwarding to local port TCP:0, the port number of the TCP port
        /// which has been opened. In all other cases, <c>0</c>.</returns>
        [DefaultOverload]
        public int CreateForward(DeviceData device, ForwardSpec local, ForwardSpec remote, bool allowRebind) => adbClient.CreateForward(device.deviceData, local.forwardSpec, remote.forwardSpec, allowRebind);

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
        /// <param name="allowRebind">If set to <see langword="true"/>, the request will fail if if the specified socket
        /// is already bound through a previous reverse command.</param>
        /// <returns>If your requested to start reverse to remote port TCP:0, the port number of the TCP port
        /// which has been opened. In all other cases, <c>0</c>.</returns>
        public int CreateReverseForward(DeviceData device, string remote, string local, bool allowRebind) => adbClient.CreateReverseForward(device.deviceData, remote, local, allowRebind);

        /// <summary>
        /// Remove a reverse port forwarding between a remote and a local port.
        /// </summary>
        /// <param name="device">The device on which to remove the reverse port forwarding</param>
        /// <param name="remote">Specification of the remote that was forwarded</param>
        public void RemoveReverseForward(DeviceData device, string remote) => adbClient.RemoveReverseForward(device.deviceData, remote);

        /// <summary>
        /// Removes all reverse forwards for a given device.
        /// </summary>
        /// <param name="device">The device on which to remove all reverse port forwarding</param>
        public void RemoveAllReverseForwards(DeviceData device) => adbClient.RemoveAllReverseForwards(device.deviceData);

        /// <summary>
        /// Remove a port forwarding between a local and a remote port.
        /// </summary>
        /// <param name="device">The device on which to remove the port forwarding.</param>
        /// <param name="localPort">Specification of the local port that was forwarded.</param>
        public void RemoveForward(DeviceData device, int localPort) => adbClient.RemoveForward(device.deviceData, localPort);

        /// <summary>
        /// Removes all forwards for a given device.
        /// </summary>
        /// <param name="device">The device on which to remove the port forwarding.</param>
        public void RemoveAllForwards(DeviceData device) => adbClient.RemoveAllForwards(device.deviceData);

        /// <summary>
        /// List all existing forward connections from this server.
        /// </summary>
        /// <param name="device">The device for which to list the existing forward connections.</param>
        /// <returns>A <see cref="ForwardData"/> entry for each existing forward connection.</returns>
        public IEnumerable<ForwardData> ListForward(DeviceData device) => adbClient.ListForward(device.deviceData).Select(ForwardData.GetForwardData);

        /// <summary>
        /// List all existing reverse forward connections from this server.
        /// </summary>
        /// <param name="device">The device for which to list the existing reverse forward connections.</param>
        /// <returns>A <see cref="ForwardData"/> entry for each existing reverse forward connection.</returns>
        public IEnumerable<ForwardData> ListReverseForward(DeviceData device) => adbClient.ListReverseForward(device.deviceData).Select(ForwardData.GetForwardData);

        /// <summary>
        /// Executes a command on the device.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <param name="device">The device on which to run the command.</param>
        /// <param name="receiver">The receiver which will get the command output.</param>
        public void ExecuteRemoteCommand(string command, DeviceData device, IShellOutputReceiver receiver) => adbClient.ExecuteRemoteCommand(command, device.deviceData, WinRTOutputReceiver.GetShellOutputReceiver(receiver));

        /// <summary>
        /// Gets a <see cref="Framebuffer"/> which contains the framebuffer data for this device. The framebuffer data can be refreshed,
        /// giving you high performance access to the device's framebuffer.
        /// </summary>
        /// <param name="device">The device for which to get the framebuffer.</param>
        /// <returns>A <see cref="Framebuffer"/> object which can be used to get the framebuffer of the device.</returns>
        public Framebuffer CreateRefreshableFramebuffer(DeviceData device) => Framebuffer.GetFramebuffer(adbClient.CreateRefreshableFramebuffer(device.deviceData));

        /// <summary>
        /// Reboots the specified adb socket address.
        /// </summary>
        /// <param name="device">The device to reboot.</param>
        public void Reboot(DeviceData device) => adbClient.Reboot(device.deviceData);

        /// <summary>
        /// Reboots the specified device in to the specified mode.
        /// </summary>
        /// <param name="into">The mode into which to reboot the device.</param>
        /// <param name="device">The device to reboot.</param>
        public void Reboot(string into, DeviceData device) => adbClient.Reboot(into, device.deviceData);

        /// <summary>
        /// Pair with a device for secure TCP/IP communication.
        /// </summary>
        /// <param name="host">The host address of the remote device.</param>
        /// <param name="code">The pairing code.</param>
        /// <returns>The results from adb.</returns>
        public string Pair(string host, string code) => adbClient.Pair(host, code);

        /// <summary>
        /// Pair with a device for secure TCP/IP communication.
        /// </summary>
        /// <param name="host">The host address of the remote device.</param>
        /// <param name="port">The port of the remote device.</param>
        /// <param name="code">The pairing code.</param>
        /// <returns>The results from adb.</returns>
        public string Pair(string host, int port, string code) => adbClient.Pair(host, port, code);

        /// <summary>
        /// Connect to a device via TCP/IP.
        /// </summary>
        /// <param name="host">The host address of the remote device.</param>
        /// <returns>The results from adb.</returns>
        public string Connect(string host) => adbClient.Connect(host);

        /// <summary>
        /// Connect to a device via TCP/IP.
        /// </summary>
        /// <param name="host">The host address of the remote device.</param>
        /// <param name="port">The port of the remote device.</param>
        /// <returns>The results from adb.</returns>
        public string Connect(string host, int port) => adbClient.Connect(host, port);

        /// <summary>
        /// Disconnects a remote device from this local ADB server.
        /// </summary>
        /// <param name="host">The host address of the remote device.</param>
        /// <returns>The results from adb.</returns>
        public string Disconnect(string host) => Disconnect(host, AdvancedSharpAdbClient.AdbClient.DefaultPort);

        /// <summary>
        /// Disconnects a remote device from this local ADB server.
        /// </summary>
        /// <param name="host">The host address of the remote device.</param>
        /// <param name="port">The port of the remote device.</param>
        /// <returns>The results from adb.</returns>
        public string Disconnect(string host, int port)
        {
            if (string.IsNullOrEmpty(host))
            {
                throw new ArgumentNullException(nameof(host));
            }

            string[] values = host.Split(':');

            return values.Length <= 0
                ? throw new ArgumentNullException(nameof(host))
                : adbClient.Connect(new DnsEndPoint(values[0], values.Length > 1 && int.TryParse(values[1], out int _port) ? _port : port));
        }

        /// <summary>
        /// Restarts the ADB daemon running on the device with root privileges.
        /// </summary>
        /// <param name="device">The device on which to restart ADB with root privileges.</param>
        public void Root(DeviceData device) => adbClient.Root(device.deviceData);

        /// <summary>
        /// Restarts the ADB daemon running on the device without root privileges.
        /// </summary>
        /// <param name="device">The device on which to restart ADB without root privileges.</param>
        public void Unroot(DeviceData device) => adbClient.Unroot(device.deviceData);

        /// <summary>
        /// Installs an Android application on an device.
        /// </summary>
        /// <param name="device">The device on which to install the application.</param>
        /// <param name="apk">A <see cref="IInputStream"/> which represents the application to install.</param>
        /// <param name="arguments">The arguments to pass to <c>adb install</c>.</param>
        public void Install(DeviceData device, IInputStream apk, [ReadOnlyArray] params string[] arguments) => adbClient.Install(device.deviceData, apk.AsStreamForRead(), arguments);

        /// <summary>
        /// Push multiple APKs to the device and install them.
        /// </summary>
        /// <param name="device">The device on which to install the application.</param>
        /// <param name="splitAPKs"><see cref="IInputStream"/>s which represents the split APKs to install.</param>
        /// <param name="packageName">The packageName of the base APK to install.</param>
        /// <param name="arguments">The arguments to pass to <c>adb install-create</c>.</param>
        public void InstallMultiple(DeviceData device, IEnumerable<IInputStream> splitAPKs, string packageName, [ReadOnlyArray] params string[] arguments) => adbClient.InstallMultiple(device.deviceData, splitAPKs.Select((x) => x.AsStreamForRead()).ToArray(), packageName, arguments);

        /// <summary>
        /// Push multiple APKs to the device and install them.
        /// </summary>
        /// <param name="device">The device on which to install the application.</param>
        /// <param name="baseAPK">A <see cref="IInputStream"/> which represents the base APK to install.</param>
        /// <param name="splitAPKs"><see cref="IInputStream"/>s which represents the split APKs to install.</param>
        /// <param name="arguments">The arguments to pass to <c>adb install-create</c>.</param>
        [DefaultOverload]
        public void InstallMultiple(DeviceData device, IInputStream baseAPK, IEnumerable<IInputStream> splitAPKs, [ReadOnlyArray] params string[] arguments) => adbClient.InstallMultiple(device.deviceData, baseAPK.AsStreamForRead(), splitAPKs.Select((x) => x.AsStreamForRead()).ToArray(), arguments);

        /// <summary>
        /// Like "install", but starts an install session.
        /// </summary>
        /// <param name="device">The device on which to install the application.</param>
        /// <param name="arguments">The arguments to pass to <c>adb install-create</c>.</param>
        /// <returns>Session ID</returns>
        public string InstallCreate(DeviceData device, [ReadOnlyArray] params string[] arguments) => adbClient.InstallCreate(device.deviceData, arguments: arguments);

        /// <summary>
        /// Like "install", but starts an install session.
        /// </summary>
        /// <param name="device">The device on which to install the application.</param>
        /// <param name="packageName">The packageName of the baseAPK to install.</param>
        /// <param name="arguments">The arguments to pass to <c>adb install-create</c>.</param>
        /// <returns>Session ID</returns>
        public string InstallCreate(DeviceData device, string packageName, [ReadOnlyArray] params string[] arguments) => adbClient.InstallCreate(device.deviceData, packageName, arguments);

        /// <summary>
        /// Write an apk into the given install session.
        /// </summary>
        /// <param name="device">The device on which to install the application.</param>
        /// <param name="apk">A <see cref="IInputStream"/> which represents the application to install.</param>
        /// <param name="apkName">The name of the application.</param>
        /// <param name="session">The session ID of the install session.</param>
        public void InstallWrite(DeviceData device, IInputStream apk, string apkName, string session) => adbClient.InstallWrite(device.deviceData, apk.AsStreamForRead(), apkName, session);

        /// <summary>
        /// Commit the given active install session, installing the app.
        /// </summary>
        /// <param name="device">The device on which to install the application.</param>
        /// <param name="session">The session ID of the install session.</param>
        public void InstallCommit(DeviceData device, string session) => adbClient.InstallCommit(device.deviceData, session);

        /// <summary>
        /// Lists all features supported by the current device.
        /// </summary>
        /// <param name="device">The device for which to get the list of features supported.</param>
        /// <returns>A list of all features supported by the current device.</returns>
        public IList<string> GetFeatureSet(DeviceData device) => adbClient.GetFeatureSet(device.deviceData);

        /// <summary>
        /// Gets the current device screen snapshot.
        /// </summary>
        /// <param name="device">The device for which to get the screen snapshot.</param>
        /// <returns>Xml containing current hierarchy.</returns>
        public XmlDocument DumpScreen(DeviceData device)
        {
            string xmlString = adbClient.DumpScreen(device.deviceData)?.OuterXml;
            if (!string.IsNullOrEmpty(xmlString))
            {
                XmlDocument doc = new();
                doc.LoadXml(xmlString);
                return doc;
            }
            return null;
        }

        /// <summary>
        /// Clicks on the specified coordinates.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="cords"></param>
        public void Click(DeviceData device, Cords cords) => adbClient.Click(device.deviceData, cords.cords);

        /// <summary>
        /// Clicks on the specified coordinates.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void Click(DeviceData device, int x, int y) => adbClient.Click(device.deviceData, x, y);

        /// <summary>
        /// Generates a swipe gesture from first element to second element Specify the speed in ms.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <param name="speed"></param>
        public void Swipe(DeviceData device, Element first, Element second, long speed) => adbClient.Swipe(device.deviceData, first.element, second.element, speed);

        /// <summary>
        /// Generates a swipe gesture from co-ordinates x1,y1 to x2,y2 with speed Specify the speed in ms.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="speed"></param>
        public void Swipe(DeviceData device, int x1, int y1, int x2, int y2, long speed) => adbClient.Swipe(device.deviceData, x1, y1, x2, y2, speed);

        /// <summary>
        /// Get element by xpath. You can specify the waiting time in timeout.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="xpath"></param>
        /// <returns>The <see cref="Element"/> class</returns>
        public Element FindElement(DeviceData device, string xpath) => Element.GetElement(adbClient.FindElement(device.deviceData, xpath));

        /// <summary>
        /// Get element by xpath. You can specify the waiting time in timeout.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="xpath"></param>
        /// <param name="timeout"></param>
        /// <returns>The <see cref="Element"/> class</returns>
        public Element FindElement(DeviceData device, string xpath, TimeSpan timeout) => Element.GetElement(adbClient.FindElement(device.deviceData, xpath, timeout));

        /// <summary>
        /// Get elements by xpath. You can specify the waiting time in timeout.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="xpath"></param>
        /// <returns>The <see cref="Element"/> class</returns>
        public IEnumerable<Element> FindElements(DeviceData device, string xpath) => adbClient.FindElements(device.deviceData, xpath).Select(Element.GetElement);

        /// <summary>
        /// Get elements by xpath. You can specify the waiting time in timeout.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="xpath"></param>
        /// <param name="timeout"></param>
        /// <returns>The <see cref="Element"/> class</returns>
        public IEnumerable<Element> FindElements(DeviceData device, string xpath, TimeSpan timeout) => adbClient.FindElements(device.deviceData, xpath, timeout).Select(Element.GetElement);

        /// <summary>
        /// Send key event to specific. You can see key events here https://developer.android.com/reference/android/view/KeyEvent.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="key"></param>
        public void SendKeyEvent(DeviceData device, string key) => adbClient.SendKeyEvent(device.deviceData, key);

        /// <summary>
        /// Send text to device. Doesn't support Russian.
        /// </summary>
        public void SendText(DeviceData device, string text) => adbClient.SendText(device.deviceData, text);

        /// <summary>
        /// Clear the input text. The input should be in focus. Use <see cref="Element.ClearInput(int)"/> if the element isn't focused.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="charCount"></param>
        public void ClearInput(DeviceData device, int charCount) => adbClient.ClearInput(device.deviceData, charCount);

        /// <summary>
        /// Start an Android application on device.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="packageName"></param>
        public void StartApp(DeviceData device, string packageName) => adbClient.StartApp(device.deviceData, packageName);

        /// <summary>
        /// Stop an Android application on device.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="packageName"></param>
        public void StopApp(DeviceData device, string packageName) => adbClient.StopApp(device.deviceData, packageName);

        /// <summary>
        /// Click BACK button.
        /// </summary>
        /// <param name="device"></param>
        public void BackBtn(DeviceData device) => adbClient.BackBtn(device.deviceData);

        /// <summary>
        /// Click HOME button.
        /// </summary>
        /// <param name="device"></param>
        public void HomeBtn(DeviceData device) => adbClient.HomeBtn(device.deviceData);
    }
}