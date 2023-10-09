// <copyright file="AdbClient.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using AdvancedSharpAdbClient.Exceptions;
using AdvancedSharpAdbClient.Logs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;

namespace AdvancedSharpAdbClient
{
    /// <summary>
    /// <para>Implements the <see cref="IAdbClient"/> interface, and allows you to interact with the
    /// adb server and devices that are connected to that adb server.</para>
    /// <para>For example, to fetch a list of all devices that are currently connected to this PC, you can
    /// call the <see cref="GetDevices"/> method.</para>
    /// <para>To run a command on a device, you can use the <see cref="ExecuteRemoteCommand(string, DeviceData, IShellOutputReceiver, Encoding)"/>
    /// method.</para>
    /// </summary>
    /// <remarks>
    /// <para><seealso href="https://github.com/android/platform_system_core/blob/master/adb/SERVICES.TXT">SERVICES.TXT</seealso></para>
    /// <para><seealso href="https://github.com/android/platform_system_core/blob/master/adb/adb_client.c">adb_client.c</seealso></para>
    /// <para><seealso href="https://github.com/android/platform_system_core/blob/master/adb/adb.c">adb.c</seealso></para>
    /// </remarks>
    public partial class AdbClient : IAdbClient
    {
        private static readonly char[] separator = Extensions.NewLineSeparator;

        /// <summary>
        /// The default port to use when connecting to a device over TCP/IP.
        /// </summary>
        public const int DefaultPort = 5555;

        /// <summary>
        /// The port at which the Android Debug Bridge server listens by default.
        /// </summary>
        public const int DefaultAdbServerPort = 5037;

        /// <summary>
        /// Gets a new instance of the <see cref="AdbClient"/> class.
        /// </summary>
        [Obsolete("This function has been removed since SharpAdbClient. Here is a placeholder which function is gets a new instance instead of gets or sets the default instance.")]
        public static IAdbClient Instance => new AdbClient();

        /// <summary>
        /// The <see cref="Func{EndPoint, IAdbSocket}"/> to create <see cref="IAdbSocket"/>.
        /// </summary>
        protected readonly Func<EndPoint, IAdbSocket> adbSocketFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdbClient"/> class.
        /// </summary>
        public AdbClient() : this(new IPEndPoint(IPAddress.Loopback, AdbServerPort), Factories.AdbSocketFactory)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AdbClient"/> class.
        /// </summary>
        /// <param name="endPoint">The <see cref="System.Net.EndPoint"/> at which the adb server is listening.</param>
        public AdbClient(EndPoint endPoint) : this(endPoint, Factories.AdbSocketFactory)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AdbClient"/> class.
        /// </summary>
        /// <param name="host">The host address at which the adb server is listening.</param>
        /// <param name="port">The port at which the adb server is listening.</param>
        public AdbClient(string host, int port) : this(host, port, Factories.AdbSocketFactory)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AdbClient"/> class.
        /// </summary>
        /// <param name="host">The host address at which the adb server is listening.</param>
        /// <param name="port">The port at which the adb server is listening.</param>
        /// <param name="adbSocketFactory">The <see cref="Func{EndPoint, IAdbSocket}"/> to create <see cref="IAdbSocket"/>.</param>
        public AdbClient(string host, int port, Func<EndPoint, IAdbSocket> adbSocketFactory)
        {
            if (string.IsNullOrEmpty(host))
            {
                throw new ArgumentNullException(nameof(host));
            }

            string[] values = host.Split(':');

            EndPoint = values.Length <= 0
                ? throw new ArgumentNullException(nameof(host))
                : new DnsEndPoint(values[0], values.Length > 1 && int.TryParse(values[1], out int _port) ? _port : port);
            this.adbSocketFactory = adbSocketFactory ?? throw new ArgumentNullException(nameof(adbSocketFactory));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AdbClient"/> class.
        /// </summary>
        /// <param name="endPoint">The <see cref="System.Net.EndPoint"/> at which the adb server is listening.</param>
        /// <param name="adbSocketFactory">The <see cref="Func{EndPoint, IAdbSocket}"/> to create <see cref="IAdbSocket"/>.</param>
        public AdbClient(EndPoint endPoint, Func<EndPoint, IAdbSocket> adbSocketFactory)
        {
            ExceptionExtensions.ThrowIfNull(endPoint);

            if (endPoint is not (IPEndPoint or DnsEndPoint))
            {
                throw new NotSupportedException("Only TCP endpoints are supported");
            }

            EndPoint = endPoint;
            this.adbSocketFactory = adbSocketFactory ?? throw new ArgumentNullException(nameof(adbSocketFactory));
        }

        /// <summary>
        /// Get or set default encoding.
        /// </summary>
        public static Encoding Encoding { get; set; } = Encoding.UTF8;

        /// <summary>
        /// Gets the current port at which the Android Debug Bridge server listens.
        /// </summary>
        public static int AdbServerPort => int.TryParse(Environment.GetEnvironmentVariable("ANDROID_ADB_SERVER_PORT"), out int result) ? result : DefaultAdbServerPort;

        /// <summary>
        /// The Default <see cref="System.Net.EndPoint"/> at which the adb server is listening.
        /// </summary>
        public static EndPoint DefaultEndPoint => new IPEndPoint(IPAddress.Loopback, DefaultPort);

        /// <summary>
        /// Gets the <see cref="System.Net.EndPoint"/> at which the adb server is listening.
        /// </summary>
        public EndPoint EndPoint { get; protected set; }

        /// <summary>
        /// Create an ASCII string preceded by four hex digits. The opening "####"
        /// is the length of the rest of the string, encoded as ASCII hex(case
        /// doesn't matter).
        /// </summary>
        /// <param name="req">The request to form.</param>
        /// <returns>An array containing <c>####req</c>.</returns>
        public static byte[] FormAdbRequest(string req)
        {
            int payloadLength = Encoding.GetByteCount(req);
            string resultStr = $"{payloadLength:X4}{req}";
            byte[] result = Encoding.GetBytes(resultStr);
            return result;
        }

        /// <summary>
        /// Creates the adb forward request.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="port">The port.</param>
        /// <returns>This returns an array containing <c>"####tcp:{port}:{addStr}"</c>.</returns>
        public static byte[] CreateAdbForwardRequest(string address, int port)
        {
            string request = address == null ? $"tcp:{port}" : $"tcp:{port}:{address}";
            return FormAdbRequest(request);
        }

        /// <inheritdoc/>
        public int GetAdbVersion()
        {
            using IAdbSocket socket = adbSocketFactory(EndPoint);
            socket.SendAdbRequest("host:version");
            AdbResponse response = socket.ReadAdbResponse();
            string version = socket.ReadString();

            return int.Parse(version, NumberStyles.HexNumber);
        }

        /// <inheritdoc/>
        public void KillAdb()
        {
            using IAdbSocket socket = adbSocketFactory(EndPoint);
            socket.SendAdbRequest("host:kill");

            // The host will immediately close the connection after the kill
            // command has been sent; no need to read the response.
        }

        /// <inheritdoc/>
        public IEnumerable<DeviceData> GetDevices()
        {
            using IAdbSocket socket = adbSocketFactory(EndPoint);
            socket.SendAdbRequest("host:devices-l");
            _ = socket.ReadAdbResponse();
            string reply = socket.ReadString();

            string[] data = reply.Split(separator, StringSplitOptions.RemoveEmptyEntries);

            return data.Select(x => new DeviceData(x));
        }

        /// <inheritdoc/>
        public int CreateForward(DeviceData device, string local, string remote, bool allowRebind)
        {
            EnsureDevice(device);

            using IAdbSocket socket = adbSocketFactory(EndPoint);
            string rebind = allowRebind ? string.Empty : "norebind:";

            socket.SendAdbRequest($"host-serial:{device.Serial}:forward:{rebind}{local};{remote}");
            _ = socket.ReadAdbResponse();
            _ = socket.ReadAdbResponse();
            string portString = socket.ReadString();

            return portString != null && int.TryParse(portString, out int port) ? port : 0;
        }

        /// <inheritdoc/>
        public int CreateReverseForward(DeviceData device, string remote, string local, bool allowRebind)
        {
            EnsureDevice(device);

            using IAdbSocket socket = adbSocketFactory(EndPoint);
            socket.SetDevice(device);

            string rebind = allowRebind ? string.Empty : "norebind:";

            socket.SendAdbRequest($"reverse:forward:{rebind}{remote};{local}");
            _ = socket.ReadAdbResponse();
            _ = socket.ReadAdbResponse();
            string portString = socket.ReadString();

            return portString != null && int.TryParse(portString, out int port) ? port : 0;
        }

        /// <inheritdoc/>
        public void RemoveReverseForward(DeviceData device, string remote)
        {
            EnsureDevice(device);

            using IAdbSocket socket = adbSocketFactory(EndPoint);
            socket.SetDevice(device);

            socket.SendAdbRequest($"reverse:killforward:{remote}");
            AdbResponse response = socket.ReadAdbResponse();
        }

        /// <inheritdoc/>
        public void RemoveAllReverseForwards(DeviceData device)
        {
            EnsureDevice(device);

            using IAdbSocket socket = adbSocketFactory(EndPoint);
            socket.SetDevice(device);

            socket.SendAdbRequest($"reverse:killforward-all");
            AdbResponse response = socket.ReadAdbResponse();
        }

        /// <inheritdoc/>
        public void RemoveForward(DeviceData device, int localPort)
        {
            EnsureDevice(device);

            using IAdbSocket socket = adbSocketFactory(EndPoint);
            socket.SendAdbRequest($"host-serial:{device.Serial}:killforward:tcp:{localPort}");
            AdbResponse response = socket.ReadAdbResponse();
        }

        /// <inheritdoc/>
        public void RemoveAllForwards(DeviceData device)
        {
            EnsureDevice(device);

            using IAdbSocket socket = adbSocketFactory(EndPoint);
            socket.SendAdbRequest($"host-serial:{device.Serial}:killforward-all");
            AdbResponse response = socket.ReadAdbResponse();
        }

        /// <inheritdoc/>
        public IEnumerable<ForwardData> ListForward(DeviceData device)
        {
            EnsureDevice(device);

            using IAdbSocket socket = adbSocketFactory(EndPoint);
            socket.SendAdbRequest($"host-serial:{device.Serial}:list-forward");
            AdbResponse response = socket.ReadAdbResponse();

            string data = socket.ReadString();

            string[] parts = data.Split(separator, StringSplitOptions.RemoveEmptyEntries);

            return parts.Select(x => new ForwardData(x));
        }

        /// <inheritdoc/>
        public IEnumerable<ForwardData> ListReverseForward(DeviceData device)
        {
            EnsureDevice(device);

            using IAdbSocket socket = adbSocketFactory(EndPoint);
            socket.SetDevice(device);

            socket.SendAdbRequest($"reverse:list-forward");
            AdbResponse response = socket.ReadAdbResponse();

            string data = socket.ReadString();

            string[] parts = data.Split(separator, StringSplitOptions.RemoveEmptyEntries);

            return parts.Select(x => new ForwardData(x));
        }

        /// <inheritdoc/>
        public void ExecuteServerCommand(string target, string command, Encoding encoding)
        {
            using IAdbSocket socket = adbSocketFactory(EndPoint);
            ExecuteServerCommand(target, command, socket, encoding);
        }

        /// <inheritdoc/>
        public void ExecuteServerCommand(string target, string command, IAdbSocket socket, Encoding encoding)
        {
            StringBuilder request = new();
            if (!StringExtensions.IsNullOrWhiteSpace(target))
            {
                _ = request.AppendFormat("{0}:", target);
            }
            _ = request.Append(command);

            socket.SendAdbRequest(request.ToString());
            _ = socket.ReadAdbResponse();
        }

        /// <inheritdoc/>
        public void ExecuteRemoteCommand(string command, DeviceData device, Encoding encoding)
        {
            EnsureDevice(device);

            using IAdbSocket socket = adbSocketFactory(EndPoint);
            socket.SetDevice(device);

            ExecuteServerCommand("shell", command, socket, encoding);
        }

        /// <inheritdoc/>
        public void ExecuteServerCommand(string target, string command, IShellOutputReceiver receiver, Encoding encoding)
        {
            using IAdbSocket socket = adbSocketFactory(EndPoint);
            ExecuteServerCommand(target, command, socket, receiver, encoding);
        }

        /// <inheritdoc/>
        public void ExecuteServerCommand(string target, string command, IAdbSocket socket, IShellOutputReceiver receiver, Encoding encoding)
        {
            StringBuilder request = new();
            if (!StringExtensions.IsNullOrWhiteSpace(target))
            {
                _ = request.AppendFormat("{0}:", target);
            }
            _ = request.Append(command);

            socket.SendAdbRequest(request.ToString());
            _ = socket.ReadAdbResponse();

            try
            {
                using StreamReader reader = new(socket.GetShellStream(), encoding);
                // Previously, we would loop while reader.Peek() >= 0. Turns out that this would
                // break too soon in certain cases (about every 10 loops, so it appears to be a timing
                // issue). Checking for reader.ReadLine() to return null appears to be much more robust
                // -- one of the integration test fetches output 1000 times and found no truncations.
                while (true)
                {
                    string line = reader.ReadLine();

                    if (line == null) { break; }

                    receiver?.AddOutput(line);
                }
            }
            catch (Exception e)
            {
                throw new ShellCommandUnresponsiveException(e);
            }
            finally
            {
                receiver?.Flush();
            }
        }

        /// <inheritdoc/>
        public void ExecuteRemoteCommand(string command, DeviceData device, IShellOutputReceiver receiver, Encoding encoding)
        {
            EnsureDevice(device);

            using IAdbSocket socket = adbSocketFactory(EndPoint);
            socket.SetDevice(device);

            ExecuteServerCommand("shell", command, socket, receiver, encoding);
        }

        /// <inheritdoc/>
        public Framebuffer CreateRefreshableFramebuffer(DeviceData device)
        {
            EnsureDevice(device);

            return new Framebuffer(device, this);
        }

        /// <inheritdoc/>
        public Framebuffer GetFrameBuffer(DeviceData device)
        {
            EnsureDevice(device);

            Framebuffer framebuffer = CreateRefreshableFramebuffer(device);
            framebuffer.Refresh(true);

            // Convert the framebuffer to an image, and return that.
            return framebuffer;
        }

        /// <inheritdoc/>
        public void RunLogService(DeviceData device, Action<LogEntry> messageSink, params LogId[] logNames)
        {
            ExceptionExtensions.ThrowIfNull(messageSink);

            EnsureDevice(device);

            // The 'log' service has been deprecated, see
            // https://android.googlesource.com/platform/system/core/+/7aa39a7b199bb9803d3fd47246ee9530b4a96177
            using IAdbSocket socket = adbSocketFactory(EndPoint);
            socket.SetDevice(device);

            StringBuilder request = new StringBuilder().Append("shell:logcat -B");

            foreach (LogId logName in logNames)
            {
                _ = request.Append($" -b {logName.ToString().ToLowerInvariant()}");
            }

            socket.SendAdbRequest(request.ToString());
            AdbResponse response = socket.ReadAdbResponse();

            using Stream stream = socket.GetShellStream();
            LogReader reader = new(stream);

            while (true)
            {
                LogEntry entry = null;

                try
                {
                    entry = reader.ReadEntry();
                }
                catch (EndOfStreamException)
                {
                    // This indicates the end of the stream; the entry will remain null.
                }

                if (entry != null)
                {
                    messageSink(entry);
                }
                else
                {
                    break;
                }
            }
        }

        /// <inheritdoc/>
        public void Reboot(string into, DeviceData device)
        {
            EnsureDevice(device);

            string request = $"reboot:{into}";

            using IAdbSocket socket = adbSocketFactory(EndPoint);
            socket.SetDevice(device);
            socket.SendAdbRequest(request);
            AdbResponse response = socket.ReadAdbResponse();
        }

        /// <inheritdoc/>
        public string Pair(DnsEndPoint endpoint, string code)
        {
            ExceptionExtensions.ThrowIfNull(endpoint);

            using IAdbSocket socket = adbSocketFactory(EndPoint);
            socket.SendAdbRequest($"host:pair:{code}:{endpoint.Host}:{endpoint.Port}");
            AdbResponse response = socket.ReadAdbResponse();
            string results = socket.ReadString();
            return results;
        }

        /// <inheritdoc/>
        public string Connect(DnsEndPoint endpoint)
        {
            ExceptionExtensions.ThrowIfNull(endpoint);

            using IAdbSocket socket = adbSocketFactory(EndPoint);
            socket.SendAdbRequest($"host:connect:{endpoint.Host}:{endpoint.Port}");
            AdbResponse response = socket.ReadAdbResponse();
            string results = socket.ReadString();
            return results;
        }

        /// <inheritdoc/>
        public string Disconnect(DnsEndPoint endpoint)
        {
            ExceptionExtensions.ThrowIfNull(endpoint);

            using IAdbSocket socket = adbSocketFactory(EndPoint);
            socket.SendAdbRequest($"host:disconnect:{endpoint.Host}:{endpoint.Port}");
            AdbResponse response = socket.ReadAdbResponse();
            string results = socket.ReadString();
            return results;
        }

        /// <inheritdoc/>
        public void Root(DeviceData device) => Root("root:", device);

        /// <inheritdoc/>
        public void Unroot(DeviceData device) => Root("unroot:", device);

        /// <summary>
        /// Restarts the ADB daemon running on the device with or without root privileges.
        /// </summary>
        /// <param name="request">The command of root or unroot.</param>
        /// <param name="device">The device on which to restart ADB with root privileges.</param>
        protected void Root(string request, DeviceData device)
        {
            EnsureDevice(device);

            using IAdbSocket socket = adbSocketFactory(EndPoint);
            socket.SetDevice(device);
            socket.SendAdbRequest(request);
            AdbResponse response = socket.ReadAdbResponse();

            // ADB will send some additional data
            byte[] buffer = new byte[1024];
            int read = socket.Read(buffer);

            string responseMessage =
#if HAS_BUFFERS
                Encoding.UTF8.GetString(buffer.AsSpan(0, read));
#else
                Encoding.UTF8.GetString(buffer, 0, read);
#endif

            // see https://android.googlesource.com/platform/packages/modules/adb/+/refs/heads/master/daemon/restart_service.cpp
            // for possible return strings
            if (!responseMessage.Contains("restarting", StringComparison.OrdinalIgnoreCase))
            {
                throw new AdbException(responseMessage);
            }
            else
            {
                // Give adbd some time to kill itself and come back up.
                // We can't use wait-for-device because devices (e.g. adb over network) might not come back.
#if HAS_PROCESS && !WINDOWS_UWP
                Thread.Sleep(3000);
#else
                Extensions.Delay(3000).GetAwaiter().GetResult();
#endif
            }
        }

        /// <inheritdoc/>
        public void Install(DeviceData device, Stream apk, params string[] arguments)
        {
            EnsureDevice(device);

            ExceptionExtensions.ThrowIfNull(apk);

            if (!apk.CanRead || !apk.CanSeek)
            {
                throw new ArgumentOutOfRangeException(nameof(apk), "The apk stream must be a readable and seekable stream");
            }

            StringBuilder requestBuilder = new StringBuilder().Append("exec:cmd package 'install'");

            if (arguments != null)
            {
                foreach (string argument in arguments)
                {
                    _ = requestBuilder.Append($" {argument}");
                }
            }

            // add size parameter [required for streaming installs]
            // do last to override any user specified value
            _ = requestBuilder.Append($" -S {apk.Length}");

            using IAdbSocket socket = adbSocketFactory(EndPoint);
            socket.SetDevice(device);

            socket.SendAdbRequest(requestBuilder.ToString());
            AdbResponse response = socket.ReadAdbResponse();

            byte[] buffer = new byte[32 * 1024];
            int read = 0;

            while ((read = apk.Read(buffer)) > 0)
            {
#if HAS_BUFFERS
                socket.Send(buffer.AsSpan(0, read));
#else
                socket.Send(buffer, read);
#endif
            }

            read = socket.Read(buffer);
            string value =
#if HAS_BUFFERS
                Encoding.UTF8.GetString(buffer.AsSpan(0, read));
#else
                Encoding.UTF8.GetString(buffer, 0, read);
#endif

            if (!value.Contains("Success"))
            {
                throw new AdbException(value);
            }
        }

        /// <inheritdoc/>
        public void InstallMultiple(DeviceData device, IEnumerable<Stream> splitAPKs, string packageName, params string[] arguments)
        {
            EnsureDevice(device);

            ExceptionExtensions.ThrowIfNull(packageName);

            string session = InstallCreate(device, packageName, arguments);

            int i = 0;
            foreach (Stream splitAPK in splitAPKs)
            {
                if (splitAPK == null || !splitAPK.CanRead || !splitAPK.CanSeek)
                {
                    Debug.WriteLine("The apk stream must be a readable and seekable stream");
                    continue;
                }

                try
                {
                    InstallWrite(device, splitAPK, $"{nameof(splitAPK)}{i++}", session);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }

            InstallCommit(device, session);
        }

        /// <inheritdoc/>
        public void InstallMultiple(DeviceData device, Stream baseAPK, IEnumerable<Stream> splitAPKs, params string[] arguments)
        {
            EnsureDevice(device);

            ExceptionExtensions.ThrowIfNull(baseAPK);

            if (!baseAPK.CanRead || !baseAPK.CanSeek)
            {
                throw new ArgumentOutOfRangeException(nameof(baseAPK), "The apk stream must be a readable and seekable stream");
            }

            string session = InstallCreate(device, null, arguments);

            InstallWrite(device, baseAPK, nameof(baseAPK), session);

            int i = 0;
            foreach (Stream splitAPK in splitAPKs)
            {
                if (splitAPK == null || !splitAPK.CanRead || !splitAPK.CanSeek)
                {
                    Debug.WriteLine("The apk stream must be a readable and seekable stream");
                    continue;
                }

                try
                {
                    InstallWrite(device, splitAPK, $"{nameof(splitAPK)}{i++}", session);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }

            InstallCommit(device, session);
        }

        /// <inheritdoc/>
        public string InstallCreate(DeviceData device, string packageName = null, params string[] arguments)
        {
            EnsureDevice(device);

            StringBuilder requestBuilder = new StringBuilder().Append("exec:cmd package 'install-create'");

            if (!StringExtensions.IsNullOrWhiteSpace(packageName))
            {
                requestBuilder.Append($" -p {packageName}");
            }

            if (arguments != null)
            {
                foreach (string argument in arguments)
                {
                    _ = requestBuilder.Append($" {argument}");
                }
            }

            using IAdbSocket socket = adbSocketFactory(EndPoint);
            socket.SetDevice(device);

            socket.SendAdbRequest(requestBuilder.ToString());
            AdbResponse response = socket.ReadAdbResponse();

            using StreamReader reader = new(socket.GetShellStream(), Encoding);
            string result = reader.ReadLine();

            if (!result.Contains("Success"))
            {
                throw new AdbException(reader.ReadToEnd());
            }

            int arr = result.IndexOf(']') - 1 - result.IndexOf('[');
            string session = result.Substring(result.IndexOf('[') + 1, arr);
            return session;
        }

        /// <inheritdoc/>
        public void InstallWrite(DeviceData device, Stream apk, string apkName, string session)
        {
            EnsureDevice(device);

            ExceptionExtensions.ThrowIfNull(apk);

            if (!apk.CanRead || !apk.CanSeek)
            {
                throw new ArgumentOutOfRangeException(nameof(apk), "The apk stream must be a readable and seekable stream");
            }

            ExceptionExtensions.ThrowIfNull(session);

            ExceptionExtensions.ThrowIfNull(apkName);

            StringBuilder requestBuilder = new();
            requestBuilder.Append($"exec:cmd package 'install-write'");

            // add size parameter [required for streaming installs]
            // do last to override any user specified value
            requestBuilder.Append($" -S {apk.Length}");

            requestBuilder.Append($" {session} {apkName}.apk");

            using IAdbSocket socket = adbSocketFactory(EndPoint);
            socket.SetDevice(device);

            socket.SendAdbRequest(requestBuilder.ToString());
            AdbResponse response = socket.ReadAdbResponse();

            byte[] buffer = new byte[32 * 1024];
            int read = 0;

            while ((read = apk.Read(buffer)) > 0)
            {
#if HAS_BUFFERS
                socket.Send(buffer.AsSpan(0, read));
#else
                socket.Send(buffer, read);
#endif
            }

            read = socket.Read(buffer);
            string value =
#if HAS_BUFFERS
                Encoding.UTF8.GetString(buffer.AsSpan(0, read));
#else
                Encoding.UTF8.GetString(buffer, 0, read);
#endif

            if (!value.Contains("Success"))
            {
                throw new AdbException(value);
            }
        }

        /// <inheritdoc/>
        public void InstallCommit(DeviceData device, string session)
        {
            EnsureDevice(device);

            using IAdbSocket socket = adbSocketFactory(EndPoint);
            socket.SetDevice(device);

            socket.SendAdbRequest($"exec:cmd package 'install-commit' {session}");
            AdbResponse response = socket.ReadAdbResponse();

            using StreamReader reader = new(socket.GetShellStream(), Encoding);
            string result = reader.ReadLine();
            if (!result.Contains("Success"))
            {
                throw new AdbException(reader.ReadToEnd());
            }
        }

        /// <inheritdoc/>
        public IEnumerable<string> GetFeatureSet(DeviceData device)
        {
            EnsureDevice(device);

            using IAdbSocket socket = adbSocketFactory(EndPoint);
            socket.SendAdbRequest($"host-serial:{device.Serial}:features");

            AdbResponse response = socket.ReadAdbResponse();
            string features = socket.ReadString();

            IEnumerable<string> featureList = features.Trim().Split('\n', ',');
            return featureList;
        }

        /// <inheritdoc/>
        public string DumpScreenString(DeviceData device)
        {
            EnsureDevice(device);
            using IAdbSocket socket = adbSocketFactory(EndPoint);
            socket.SetDevice(device);
            socket.SendAdbRequest("shell:uiautomator dump /dev/tty");
            AdbResponse response = socket.ReadAdbResponse();
            using StreamReader reader = new(socket.GetShellStream(), Encoding);
            string xmlString = reader.ReadToEnd()
                .Replace("Events injected: 1\r\n", string.Empty)
                .Replace("UI hierchary dumped to: /dev/tty", string.Empty)
                .Trim();
            if (string.IsNullOrEmpty(xmlString) || xmlString.StartsWith("<?xml"))
            {
                return xmlString;
            }
            Match xmlMatch = GetXmlRegex().Match(xmlString);
            return !xmlMatch.Success ? throw new XmlException("An error occurred while receiving xml: " + xmlString) : xmlMatch.Value;
        }

        /// <inheritdoc/>
        public XmlDocument DumpScreen(DeviceData device)
        {
            XmlDocument doc = new();
            string xmlString = DumpScreenString(device);
            if (!string.IsNullOrEmpty(xmlString))
            {
                doc.LoadXml(xmlString);
                return doc;
            }
            return null;
        }

#if WINDOWS_UWP || WINDOWS10_0_17763_0_OR_GREATER
        /// <inheritdoc/>
        public Windows.Data.Xml.Dom.XmlDocument DumpScreenWinRT(DeviceData device)
        {
            Windows.Data.Xml.Dom.XmlDocument doc = new();
            string xmlString = DumpScreenString(device);
            if (!string.IsNullOrEmpty(xmlString))
            {
                doc.LoadXml(xmlString);
                return doc;
            }
            return null;
        }
#endif

        /// <inheritdoc/>
        public void Click(DeviceData device, Cords cords)
        {
            EnsureDevice(device);

            using IAdbSocket socket = adbSocketFactory(EndPoint);
            socket.SetDevice(device);
            socket.SendAdbRequest($"shell:input tap {cords.X} {cords.Y}");
            AdbResponse response = socket.ReadAdbResponse();
            using StreamReader reader = new(socket.GetShellStream(), Encoding);
            string result = reader.ReadToEnd().TrimStart();
            if (result.StartsWith("java.lang."))
            {
                throw JavaException.Parse(result);
            }
            else if (result.Contains("ERROR", StringComparison.OrdinalIgnoreCase)) // error or ERROR
            {
                throw new ElementNotFoundException("Coordinates of element is invalid");
            }
        }

        /// <inheritdoc/>
        public void Click(DeviceData device, int x, int y)
        {
            EnsureDevice(device);

            using IAdbSocket socket = adbSocketFactory(EndPoint);
            socket.SetDevice(device);
            socket.SendAdbRequest($"shell:input tap {x} {y}");
            AdbResponse response = socket.ReadAdbResponse();
            using StreamReader reader = new(socket.GetShellStream(), Encoding);
            string result = reader.ReadToEnd().TrimStart();
            if (result.StartsWith("java.lang."))
            {
                throw JavaException.Parse(result);
            }
            else if (result.Contains("ERROR", StringComparison.OrdinalIgnoreCase)) // error or ERROR
            {
                throw new ElementNotFoundException("Coordinates of element is invalid");
            }
        }

        /// <inheritdoc/>
        public void Swipe(DeviceData device, Element first, Element second, long speed)
        {
            EnsureDevice(device);

            using IAdbSocket socket = adbSocketFactory(EndPoint);
            socket.SetDevice(device);
            socket.SendAdbRequest($"shell:input swipe {first.Center.X} {first.Center.Y} {second.Center.X} {second.Center.Y} {speed}");
            AdbResponse response = socket.ReadAdbResponse();
            using StreamReader reader = new(socket.GetShellStream(), Encoding);
            string result = reader.ReadToEnd().TrimStart();
            if (result.StartsWith("java.lang."))
            {
                throw JavaException.Parse(result);
            }
            else if (result.Contains("ERROR", StringComparison.OrdinalIgnoreCase)) // error or ERROR
            {
                throw new ElementNotFoundException("Coordinates of element is invalid");
            }
        }

        /// <inheritdoc/>
        public void Swipe(DeviceData device, int x1, int y1, int x2, int y2, long speed)
        {
            EnsureDevice(device);

            using IAdbSocket socket = adbSocketFactory(EndPoint);
            socket.SetDevice(device);
            socket.SendAdbRequest($"shell:input swipe {x1} {y1} {x2} {y2} {speed}");
            AdbResponse response = socket.ReadAdbResponse();
            using StreamReader reader = new(socket.GetShellStream(), Encoding);
            string result = reader.ReadToEnd().TrimStart();
            if (result.StartsWith("java.lang."))
            {
                throw JavaException.Parse(result);
            }
            else if (result.Contains("ERROR", StringComparison.OrdinalIgnoreCase)) // error or ERROR
            {
                throw new ElementNotFoundException("Coordinates of element is invalid");
            }
        }

        /// <inheritdoc/>
        public bool IsAppRunning(DeviceData device, string packageName)
        {
            EnsureDevice(device);

            using IAdbSocket socket = adbSocketFactory(EndPoint);
            socket.SetDevice(device);
            socket.SendAdbRequest($"shell:pidof {packageName}");
            AdbResponse response = socket.ReadAdbResponse();
            using StreamReader reader = new(socket.GetShellStream(), Encoding);
            string result = reader.ReadToEnd().TrimStart().Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
            bool intParsed = int.TryParse(result, out int pid);
            return intParsed && pid > 0;
        }

        /// <inheritdoc/>
        public bool IsAppInForeground(DeviceData device, string packageName)
        {
            EnsureDevice(device);

            using IAdbSocket socket = adbSocketFactory(EndPoint);
            socket.SetDevice(device);
            socket.SendAdbRequest($"shell:dumpsys activity activities | grep mResumedActivity");
            AdbResponse response = socket.ReadAdbResponse();
            using StreamReader reader = new(socket.GetShellStream(), Encoding);
            string result = reader.ReadToEnd();
            return result.Contains(packageName);
        }

        /// <inheritdoc/>
        public AppStatus GetAppStatus(DeviceData device, string packageName)
        {
            EnsureDevice(device);

            // Check if the app is in foreground
            bool currentApp = IsAppInForeground(device, packageName);
            if (currentApp)
            {
                return AppStatus.Foreground;
            }

            // Check if the app is running in background
            bool isAppRunning = IsAppRunning(device, packageName);
            return isAppRunning ? AppStatus.Background : AppStatus.Stopped;
        }

        /// <inheritdoc/>
        public Element FindElement(DeviceData device, string xpath = "hierarchy/node", TimeSpan timeout = default)
        {
            EnsureDevice(device);
            Stopwatch stopwatch = new();
            stopwatch.Start();
            while (timeout == TimeSpan.Zero || stopwatch.Elapsed < timeout)
            {
                XmlDocument doc = DumpScreen(device);
                if (doc != null)
                {
                    XmlNode xmlNode = doc.SelectSingleNode(xpath);
                    if (xmlNode != null)
                    {
                        Element element = Element.FromXmlNode(this, device, xmlNode);
                        if (element != null)
                        {
                            return element;
                        }
                    }
                }
                if (timeout == TimeSpan.Zero)
                {
                    break;
                }
            }
            return null;
        }

        /// <inheritdoc/>
        public IEnumerable<Element> FindElements(DeviceData device, string xpath = "hierarchy/node", TimeSpan timeout = default)
        {
            EnsureDevice(device);
            Stopwatch stopwatch = new();
            stopwatch.Start();
            while (timeout == TimeSpan.Zero || stopwatch.Elapsed < timeout)
            {
                XmlDocument doc = DumpScreen(device);
                if (doc != null)
                {
                    XmlNodeList xmlNodes = doc.SelectNodes(xpath);
                    if (xmlNodes != null)
                    {
                        for (int i = 0; i < xmlNodes.Count; i++)
                        {
                            Element element = Element.FromXmlNode(this, device, xmlNodes[i]);
                            if (element != null)
                            {
                                yield return element;
                            }
                        }
                        break;
                    }
                }
                if (timeout == TimeSpan.Zero) { break; }
            }
        }

        /// <inheritdoc/>
        public void SendKeyEvent(DeviceData device, string key)
        {
            EnsureDevice(device);

            using IAdbSocket socket = adbSocketFactory(EndPoint);
            socket.SetDevice(device);
            socket.SendAdbRequest($"shell:input keyevent {key}");
            AdbResponse response = socket.ReadAdbResponse();
            using StreamReader reader = new(socket.GetShellStream(), Encoding);
            string result = reader.ReadToEnd().TrimStart();
            if (result.StartsWith("java.lang."))
            {
                throw JavaException.Parse(result);
            }
            else if (result.Contains("ERROR", StringComparison.OrdinalIgnoreCase)) // error or ERROR
            {
                throw new InvalidKeyEventException("KeyEvent is invalid");
            }
        }

        /// <inheritdoc/>
        public void SendText(DeviceData device, string text)
        {
            EnsureDevice(device);

            using IAdbSocket socket = adbSocketFactory(EndPoint);
            socket.SetDevice(device);
            socket.SendAdbRequest($"shell:input text {text}");
            AdbResponse response = socket.ReadAdbResponse();
            using StreamReader reader = new(socket.GetShellStream(), Encoding);
            string result = reader.ReadToEnd().TrimStart();
            if (result.StartsWith("java.lang."))
            {
                throw JavaException.Parse(result);
            }
            else if (result.Contains("ERROR", StringComparison.OrdinalIgnoreCase)) // error or ERROR
            {
                throw new InvalidTextException();
            }
        }
        /// <inheritdoc/>
        public void StartApp(DeviceData device, string packageName)
        {
            EnsureDevice(device);

            using IAdbSocket socket = adbSocketFactory(EndPoint);
            socket.SetDevice(device);
            socket.SendAdbRequest($"shell:monkey -p {packageName} 1");
            AdbResponse response = socket.ReadAdbResponse();
        }

        /// <inheritdoc/>
        public void StopApp(DeviceData device, string packageName)
        {
            EnsureDevice(device);

            using IAdbSocket socket = adbSocketFactory(EndPoint);
            socket.SetDevice(device);
            socket.SendAdbRequest($"shell:am force-stop {packageName}");
            AdbResponse response = socket.ReadAdbResponse();
        }

        /// <summary>
        /// Sets default encoding (default - UTF8).
        /// </summary>
        /// <param name="encoding">The <see cref="System.Text.Encoding"/> to set.</param>
        public static void SetEncoding(Encoding encoding) => Encoding = encoding;

        /// <summary>
        /// Throws an <see cref="ArgumentNullException"/> if the <paramref name="device"/>
        /// parameter is <see langword="null"/>, and a <see cref="ArgumentOutOfRangeException"/>
        /// if <paramref name="device"/> does not have a valid serial number.
        /// </summary>
        /// <param name="device">A <see cref="DeviceData"/> object to validate.</param>
        protected static void EnsureDevice(DeviceData device)
        {
            ExceptionExtensions.ThrowIfNull(device);

            if (string.IsNullOrEmpty(device.Serial))
            {
                throw new ArgumentOutOfRangeException(nameof(device), "You must specific a serial number for the device");
            }
        }

#if NET7_0_OR_GREATER
        [GeneratedRegex("<\\?xml(.?)*")]
        private static partial Regex GetXmlRegex();
#else
        private static Regex GetXmlRegex() => new("<\\?xml(.?)*");
#endif
    }

    /// <summary>
    /// See as the <see cref="AdbClient"/> class.
    /// </summary>
    [Obsolete("AdvancedAdbClient is too long to remember. Please use AdbClient instead.")]
    public class AdvancedAdbClient : AdbClient
    {
    }
}
