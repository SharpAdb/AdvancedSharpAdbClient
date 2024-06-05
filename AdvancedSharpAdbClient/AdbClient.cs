// <copyright file="AdbClient.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

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
    [DebuggerDisplay($"{nameof(AdbClient)} \\{{ {nameof(EndPoint)} = {{{nameof(EndPoint)}}} }}")]
    public partial class AdbClient : IAdbClient, ICloneable<AdbClient>, ICloneable
#if HAS_WINRT
        , IAdbClient.IWinRT
#endif
    {
        /// <summary>
        /// The <see cref="Array"/> of <see cref="char"/>s that represent a new line.
        /// </summary>
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
        /// The <see cref="Func{EndPoint, IAdbSocket}"/> to create <see cref="IAdbSocket"/>.
        /// </summary>
        protected readonly Func<EndPoint, IAdbSocket> AdbSocketFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdbClient"/> class.
        /// </summary>
        public AdbClient()
            : this(AdbServerEndPoint, Factories.AdbSocketFactory)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AdbClient"/> class.
        /// </summary>
        /// <param name="endPoint">The <see cref="System.Net.EndPoint"/> at which the adb server is listening.</param>
        public AdbClient(EndPoint endPoint)
            : this(endPoint, Factories.AdbSocketFactory)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AdbClient"/> class.
        /// </summary>
        /// <param name="host">The host address at which the adb server is listening.</param>
        /// <param name="port">The port at which the adb server is listening.</param>
        public AdbClient(string host, int port)
            : this(Extensions.CreateDnsEndPoint(host, port), Factories.AdbSocketFactory)
        {
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
            AdbSocketFactory = adbSocketFactory ?? throw new ArgumentNullException(nameof(adbSocketFactory));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AdbClient"/> class.
        /// </summary>
        /// <param name="host">The host address at which the adb server is listening.</param>
        /// <param name="port">The port at which the adb server is listening.</param>
        /// <param name="adbSocketFactory">The <see cref="Func{EndPoint, IAdbSocket}"/> to create <see cref="IAdbSocket"/>.</param>
        public AdbClient(string host, int port, Func<EndPoint, IAdbSocket> adbSocketFactory)
            : this(Extensions.CreateDnsEndPoint(host, port), adbSocketFactory)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AdbClient"/> class.
        /// </summary>
        /// <param name="adbSocketFactory">The <see cref="Func{EndPoint, IAdbSocket}"/> to create <see cref="IAdbSocket"/>.</param>
        public AdbClient(Func<EndPoint, IAdbSocket> adbSocketFactory)
            : this(AdbServerEndPoint, adbSocketFactory)
        {
        }

        /// <summary>
        /// Gets a new instance of the <see cref="AdbClient"/> class.
        /// </summary>
        public static IAdbClient Instance => Factories.AdbClientFactory(AdbServerEndPoint);

        /// <summary>
        /// Gets or sets default encoding.
        /// </summary>
        public static Encoding Encoding { get; set; } = Encoding.UTF8;

        /// <summary>
        /// Gets the current port at which the Android Debug Bridge server listens.
        /// </summary>
        public static int AdbServerPort => int.TryParse(TryGetEnvironmentVariable("ANDROID_ADB_SERVER_PORT"), out int result) ? result : DefaultAdbServerPort;

        /// <summary>
        /// Gets the default <see cref="IPEndPoint"/> which to use when connecting to a device over TCP/IP.
        /// </summary>
        public static IPEndPoint DefaultEndPoint => new(IPAddress.Loopback, DefaultPort);

        /// <summary>
        /// Gets the <see cref="IPEndPoint"/> at which the Android Debug Bridge server listens by default.
        /// </summary>
        public static IPEndPoint AdbServerEndPoint => new(IPAddress.Loopback, AdbServerPort);

        /// <summary>
        /// Gets the <see cref="System.Net.EndPoint"/> at which the adb server is listening.
        /// </summary>
        public EndPoint EndPoint { get; init; }

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
            using IAdbSocket socket = CreateAdbSocket();

            socket.SendAdbRequest("host:version");
            _ = socket.ReadAdbResponse();
            string version = socket.ReadString();

            return int.Parse(version, NumberStyles.HexNumber);
        }

        /// <inheritdoc/>
        public void KillAdb()
        {
            using IAdbSocket socket = CreateAdbSocket();
            socket.SendAdbRequest("host:kill");

            // The host will immediately close the connection after the kill
            // command has been sent; no need to read the response.
        }

        /// <inheritdoc/>
        public IEnumerable<DeviceData> GetDevices()
        {
            using IAdbSocket socket = CreateAdbSocket();

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

            using IAdbSocket socket = CreateAdbSocket();
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

            using IAdbSocket socket = CreateAdbSocket();

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

            using IAdbSocket socket = CreateAdbSocket();
            socket.SetDevice(device);

            socket.SendAdbRequest($"reverse:killforward:{remote}");
            _ = socket.ReadAdbResponse();
        }

        /// <inheritdoc/>
        public void RemoveAllReverseForwards(DeviceData device)
        {
            EnsureDevice(device);

            using IAdbSocket socket = CreateAdbSocket();
            socket.SetDevice(device);

            socket.SendAdbRequest($"reverse:killforward-all");
            _ = socket.ReadAdbResponse();
        }

        /// <inheritdoc/>
        public void RemoveForward(DeviceData device, int localPort)
        {
            EnsureDevice(device);

            using IAdbSocket socket = CreateAdbSocket();
            socket.SendAdbRequest($"host-serial:{device.Serial}:killforward:tcp:{localPort}");
            _ = socket.ReadAdbResponse();
        }

        /// <inheritdoc/>
        public void RemoveAllForwards(DeviceData device)
        {
            EnsureDevice(device);

            using IAdbSocket socket = CreateAdbSocket();
            socket.SendAdbRequest($"host-serial:{device.Serial}:killforward-all");
            _ = socket.ReadAdbResponse();
        }

        /// <inheritdoc/>
        public IEnumerable<ForwardData> ListForward(DeviceData device)
        {
            EnsureDevice(device);

            using IAdbSocket socket = CreateAdbSocket();
            socket.SendAdbRequest($"host-serial:{device.Serial}:list-forward");
            _ = socket.ReadAdbResponse();
            string data = socket.ReadString();

            string[] parts = data.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            return parts.Select(ForwardData.FromString).OfType<ForwardData>();
        }

        /// <inheritdoc/>
        public IEnumerable<ForwardData> ListReverseForward(DeviceData device)
        {
            EnsureDevice(device);

            using IAdbSocket socket = CreateAdbSocket();
            socket.SetDevice(device);

            socket.SendAdbRequest($"reverse:list-forward");
            _ = socket.ReadAdbResponse();
            string data = socket.ReadString();

            string[] parts = data.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            return parts.Select(ForwardData.FromString).OfType<ForwardData>();
        }

        /// <inheritdoc/>
        public void ExecuteServerCommand(string target, string command)
        {
            using IAdbSocket socket = CreateAdbSocket();
            ExecuteServerCommand(target, command, socket);
        }

        /// <inheritdoc/>
        public virtual void ExecuteServerCommand(string target, string command, IAdbSocket socket)
        {
            StringBuilder request = new();
            if (!StringExtensions.IsNullOrWhiteSpace(target))
            {
                _ = request.Append(target).Append(':');
            }
            _ = request.Append(command);

            socket.SendAdbRequest(request.ToString());
            _ = socket.ReadAdbResponse();
        }

        /// <inheritdoc/>
        public void ExecuteRemoteCommand(string command, DeviceData device)
        {
            EnsureDevice(device);

            using IAdbSocket socket = CreateAdbSocket();
            socket.SetDevice(device);

            ExecuteServerCommand("shell", command, socket);
        }

        /// <inheritdoc/>
        public void ExecuteServerCommand(string target, string command, IShellOutputReceiver? receiver, Encoding encoding)
        {
            ExceptionExtensions.ThrowIfNull(encoding);
            using IAdbSocket socket = CreateAdbSocket();
            ExecuteServerCommand(target, command, socket, receiver, encoding);
        }

        /// <inheritdoc/>
        public virtual void ExecuteServerCommand(string target, string command, IAdbSocket socket, IShellOutputReceiver? receiver, Encoding encoding)
        {
            ExceptionExtensions.ThrowIfNull(encoding);

            StringBuilder request = new();
            if (!StringExtensions.IsNullOrWhiteSpace(target))
            {
                _ = request.Append(target).Append(':');
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
                    string? line = reader.ReadLine();
                    if (line == null) { break; }
                    if (receiver?.AddOutput(line) == false) { break; }
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
        public void ExecuteRemoteCommand(string command, DeviceData device, IShellOutputReceiver? receiver, Encoding encoding)
        {
            EnsureDevice(device);

            using IAdbSocket socket = CreateAdbSocket();
            socket.SetDevice(device);

            ExecuteServerCommand("shell", command, socket, receiver, encoding);
        }

        /// <inheritdoc/>
        public IEnumerable<string> ExecuteServerCommand(string target, string command, Encoding encoding)
        {
            ExceptionExtensions.ThrowIfNull(encoding);
            using IAdbSocket socket = CreateAdbSocket();
            foreach (string line in ExecuteServerCommand(target, command, socket, encoding))
            {
                yield return line;
            }
        }

        /// <inheritdoc/>
        public virtual IEnumerable<string> ExecuteServerCommand(string target, string command, IAdbSocket socket, Encoding encoding)
        {
            ExceptionExtensions.ThrowIfNull(encoding);

            StringBuilder request = new();
            if (!StringExtensions.IsNullOrWhiteSpace(target))
            {
                _ = request.Append(target).Append(':');
            }
            _ = request.Append(command);

            socket.SendAdbRequest(request.ToString());
            _ = socket.ReadAdbResponse();

            using StreamReader reader = new(socket.GetShellStream(), encoding);
            // Previously, we would loop while reader.Peek() >= 0. Turns out that this would
            // break too soon in certain cases (about every 10 loops, so it appears to be a timing
            // issue). Checking for reader.ReadLine() to return null appears to be much more robust
            // -- one of the integration test fetches output 1000 times and found no truncations.
            while (true)
            {
                string? line;
                try
                {
                    line = reader.ReadLine();
                }
                catch (Exception e)
                {
                    throw new ShellCommandUnresponsiveException(e);
                }
                if (line == null) { yield break; }
                yield return line;
            }
        }

        /// <inheritdoc/>
        public IEnumerable<string> ExecuteRemoteCommand(string command, DeviceData device, Encoding encoding)
        {
            EnsureDevice(device);

            using IAdbSocket socket = CreateAdbSocket();
            socket.SetDevice(device);

            foreach (string line in ExecuteServerCommand("shell", command, socket, encoding))
            {
                yield return line;
            }
        }

        /// <inheritdoc/>
        public IEnumerable<LogEntry> RunLogService(DeviceData device, params LogId[] logNames)
        {
            EnsureDevice(device);

            // The 'log' service has been deprecated, see
            // https://android.googlesource.com/platform/system/core/+/7aa39a7b199bb9803d3fd47246ee9530b4a96177
            using IAdbSocket socket = CreateAdbSocket();
            socket.SetDevice(device);

            StringBuilder request = new("shell:logcat -B");

            foreach (LogId logName in logNames)
            {
                _ = request.Append(" -b ").Append(logName.ToString().ToLowerInvariant());
            }

            socket.SendAdbRequest(request.ToString());
            _ = socket.ReadAdbResponse();

            using Stream stream = socket.GetShellStream();
            LogReader reader = new(stream);

            LogEntry? entry = null;
            while (true)
            {
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
                    yield return entry;
                }
                else
                {
                    yield break;
                }
            }
        }

        /// <inheritdoc/>
        public void RunLogService(DeviceData device, Action<LogEntry> messageSink, in bool isCancelled = false, params LogId[] logNames)
        {
            EnsureDevice(device);
            ExceptionExtensions.ThrowIfNull(messageSink);

            // The 'log' service has been deprecated, see
            // https://android.googlesource.com/platform/system/core/+/7aa39a7b199bb9803d3fd47246ee9530b4a96177
            using IAdbSocket socket = CreateAdbSocket();
            socket.SetDevice(device);

            StringBuilder request = new("shell:logcat -B");

            foreach (LogId logName in logNames)
            {
                _ = request.Append(" -b ").Append(logName.ToString().ToLowerInvariant());
            }

            socket.SendAdbRequest(request.ToString());
            _ = socket.ReadAdbResponse();

            using Stream stream = socket.GetShellStream();
            LogReader reader = new(stream);

            LogEntry? entry = null;
            while (!isCancelled)
            {
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
        public Framebuffer CreateFramebuffer(DeviceData device)
        {
            EnsureDevice(device);
            return new Framebuffer(device, this, AdbSocketFactory);
        }

        /// <inheritdoc/>
        public Framebuffer GetFrameBuffer(DeviceData device)
        {
            EnsureDevice(device);

            Framebuffer framebuffer = CreateFramebuffer(device);
            framebuffer.Refresh(true);

            // Convert the framebuffer to an image, and return that.
            return framebuffer;
        }

        /// <inheritdoc/>
        public void Reboot(string into, DeviceData device)
        {
            EnsureDevice(device);

            using IAdbSocket socket = CreateAdbSocket();
            socket.SetDevice(device);

            socket.SendAdbRequest($"reboot:{into}");
            _ = socket.ReadAdbResponse();
        }

        /// <inheritdoc/>
        public string Pair(string host, int port, string code)
        {
            ExceptionExtensions.ThrowIfNull(host);

            using IAdbSocket socket = CreateAdbSocket();
            string address = host.Contains(':') ? host : $"{host}:{port}";
            socket.SendAdbRequest($"host:pair:{code}:{address}");
            _ = socket.ReadAdbResponse();

            return socket.ReadString();
        }

        /// <inheritdoc/>
        public string Connect(string host, int port = DefaultPort)
        {
            ExceptionExtensions.ThrowIfNull(host);

            using IAdbSocket socket = CreateAdbSocket();
            string address = host.Contains(':') ? host : $"{host}:{port}";
            socket.SendAdbRequest($"host:connect:{address}");
            _ = socket.ReadAdbResponse();

            return socket.ReadString();
        }

        /// <inheritdoc/>
        public string Disconnect(string host, int port = DefaultPort)
        {
            ExceptionExtensions.ThrowIfNull(host);

            using IAdbSocket socket = CreateAdbSocket();
            string address = host.Contains(':') ? host : $"{host}:{port}";
            socket.SendAdbRequest($"host:disconnect:{address}");
            _ = socket.ReadAdbResponse();

            return socket.ReadString();
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

            using IAdbSocket socket = CreateAdbSocket();
            socket.SetDevice(device);
            socket.SendAdbRequest(request);
            _ = socket.ReadAdbResponse();

            // ADB will send some additional data
            byte[] buffer = new byte[1024];
            int read = socket.Read(buffer);

            string responseMessage =
#if HAS_BUFFERS
                Encoding.GetString(buffer.AsSpan(0, read));
#else
                Encoding.GetString(buffer, 0, read);
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
                TaskExExtensions.Delay(3000).AwaitByTaskCompleteSource();
#endif
            }
        }

        /// <inheritdoc/>
        public virtual void Install(DeviceData device, Stream apk, Action<InstallProgressEventArgs>? callback = null, params string[] arguments)
        {
            callback?.Invoke(new InstallProgressEventArgs(PackageInstallProgressState.Preparing));

            EnsureDevice(device);
            ExceptionExtensions.ThrowIfNull(apk);

            if (!apk.CanRead || !apk.CanSeek)
            {
                throw new ArgumentOutOfRangeException(nameof(apk), "The apk stream must be a readable and seekable stream");
            }

            StringBuilder requestBuilder = new("exec:cmd package 'install'");

            if (arguments != null)
            {
                foreach (string argument in arguments)
                {
                    _ = requestBuilder.Append(' ').Append(argument);
                }
            }

            // add size parameter [required for streaming installs]
            // do last to override any user specified value
            _ = requestBuilder.Append(" -S ").Append(apk.Length);

            using IAdbSocket socket = CreateAdbSocket();
            socket.SetDevice(device);

            socket.SendAdbRequest(requestBuilder.ToString());
            _ = socket.ReadAdbResponse();

            byte[] buffer = new byte[32 * 1024];
            int read;

            long totalBytesToProcess = apk.Length;
            long totalBytesRead = 0;

#if HAS_BUFFERS
            while ((read = apk.Read(buffer)) > 0)
            {
                socket.Send(buffer.AsSpan(0, read));
#else
            while ((read = apk.Read(buffer, 0, buffer.Length)) > 0)
            {
                socket.Send(buffer, read);
#endif
                totalBytesRead += read;
                callback?.Invoke(new InstallProgressEventArgs(0, 1, totalBytesToProcess == 0 ? 0 : totalBytesRead * 100d / totalBytesToProcess));
            }
            callback?.Invoke(new InstallProgressEventArgs(1, 1, 100));

            callback?.Invoke(new InstallProgressEventArgs(PackageInstallProgressState.Installing));
            read = socket.Read(buffer);
            string value =
#if HAS_BUFFERS
                Encoding.GetString(buffer.AsSpan(0, read));
#else
                Encoding.GetString(buffer, 0, read);
#endif

            if (!value.Contains("Success"))
            {
                throw new AdbException(value);
            }
            callback?.Invoke(new InstallProgressEventArgs(PackageInstallProgressState.Finished));
        }

        /// <inheritdoc/>
        public void InstallMultiple(DeviceData device, Stream baseAPK, IEnumerable<Stream> splitAPKs, Action<InstallProgressEventArgs>? callback = null, params string[] arguments)
        {
            callback?.Invoke(new InstallProgressEventArgs(PackageInstallProgressState.Preparing));

            EnsureDevice(device);
            ExceptionExtensions.ThrowIfNull(baseAPK);
            ExceptionExtensions.ThrowIfNull(splitAPKs);

            if (!baseAPK.CanRead || !baseAPK.CanSeek)
            {
                throw new ArgumentOutOfRangeException(nameof(baseAPK), "The apk stream must be a readable and seekable stream");
            }

            if (splitAPKs.Any(apk => apk == null || !apk.CanRead || !apk.CanSeek))
            {
                throw new ArgumentOutOfRangeException(nameof(splitAPKs), "The apk stream must be a readable and seekable stream");
            }

            callback?.Invoke(new InstallProgressEventArgs(PackageInstallProgressState.CreateSession));
            string session = InstallCreate(device, arguments);

            int splitAPKsCount = splitAPKs.Count();
            void OnMainSyncProgressChanged(string? sender, double args) =>
                callback?.Invoke(new InstallProgressEventArgs(sender == null ? 1 : 0, splitAPKsCount + 1, args / 2));

            InstallWrite(device, baseAPK, nameof(baseAPK), session, OnMainSyncProgressChanged);

            int progressCount = 1;
            Dictionary<string, double> status = new(splitAPKsCount);
            void OnSplitSyncProgressChanged(string? sender, double args)
            {
                lock (status)
                {
                    if (sender == null)
                    {
                        progressCount++;
                    }
                    else if (sender is string path)
                    {
                        status[path] = args;
                    }
                    callback?.Invoke(new InstallProgressEventArgs(progressCount, splitAPKsCount + 1, (status.Values.Select(x => x / splitAPKsCount).Sum() + 100) / 2));
                }
            }

            int i = 0;
            foreach (Stream splitAPK in splitAPKs)
            {
                InstallWrite(device, splitAPK, $"{nameof(splitAPK)}{i++}", session, OnSplitSyncProgressChanged);
            }

            callback?.Invoke(new InstallProgressEventArgs(PackageInstallProgressState.Installing));
            InstallCommit(device, session);
            callback?.Invoke(new InstallProgressEventArgs(PackageInstallProgressState.Finished));
        }

        /// <inheritdoc/>
        public void InstallMultiple(DeviceData device, IEnumerable<Stream> splitAPKs, string packageName, Action<InstallProgressEventArgs>? callback = null, params string[] arguments)
        {
            callback?.Invoke(new InstallProgressEventArgs(PackageInstallProgressState.Preparing));

            EnsureDevice(device);
            ExceptionExtensions.ThrowIfNull(splitAPKs);
            ExceptionExtensions.ThrowIfNull(packageName);

            if (splitAPKs.Any(apk => apk == null || !apk.CanRead || !apk.CanSeek))
            {
                throw new ArgumentOutOfRangeException(nameof(splitAPKs), "The apk stream must be a readable and seekable stream");
            }

            callback?.Invoke(new InstallProgressEventArgs(PackageInstallProgressState.CreateSession));
            string session = InstallCreate(device, packageName, arguments);

            int progressCount = 0;
            int splitAPKsCount = splitAPKs.Count();
            Dictionary<string, double> status = new(splitAPKsCount);
            void OnSyncProgressChanged(string? sender, double args)
            {
                lock (status)
                {
                    if (sender == null)
                    {
                        progressCount++;
                    }
                    else if (sender is string path)
                    {
                        status[path] = args;
                    }
                    callback?.Invoke(new InstallProgressEventArgs(progressCount, splitAPKsCount, status.Values.Select(x => x / splitAPKsCount).Sum()));
                }
            }

            int i = 0;
            foreach (Stream splitAPK in splitAPKs)
            {
                InstallWrite(device, splitAPK, $"{nameof(splitAPK)}{i++}", session, OnSyncProgressChanged);
            }

            callback?.Invoke(new InstallProgressEventArgs(PackageInstallProgressState.Installing));
            InstallCommit(device, session);
            callback?.Invoke(new InstallProgressEventArgs(PackageInstallProgressState.Finished));
        }

        /// <inheritdoc/>
        public string InstallCreate(DeviceData device, params string[] arguments)
        {
            EnsureDevice(device);

            StringBuilder requestBuilder = new("exec:cmd package 'install-create'");

            if (arguments != null)
            {
                foreach (string argument in arguments)
                {
                    _ = requestBuilder.Append(' ').Append(argument);
                }
            }

            using IAdbSocket socket = CreateAdbSocket();
            socket.SetDevice(device);

            socket.SendAdbRequest(requestBuilder.ToString());
            _ = socket.ReadAdbResponse();

            using StreamReader reader = new(socket.GetShellStream(), Encoding);
            string result = reader.ReadLine() ?? throw new AdbException($"The {nameof(result)} of {nameof(InstallCreate)} is null.");

            if (!result.Contains("Success"))
            {
                throw new AdbException(reader.ReadToEnd());
            }

            int arr = result.IndexOf(']') - 1 - result.IndexOf('[');
            string session = result.Substring(result.IndexOf('[') + 1, arr);
            return session;
        }

        /// <inheritdoc/>
        public string InstallCreate(DeviceData device, string packageName, params string[] arguments)
        {
            EnsureDevice(device);
            ExceptionExtensions.ThrowIfNull(packageName);

            StringBuilder requestBuilder =
                new StringBuilder("exec:cmd package 'install-create'")
                    .Append(" -p ").Append(packageName);

            if (arguments != null)
            {
                foreach (string argument in arguments)
                {
                    _ = requestBuilder.Append(' ').Append(argument);
                }
            }

            using IAdbSocket socket = CreateAdbSocket();
            socket.SetDevice(device);

            socket.SendAdbRequest(requestBuilder.ToString());
            _ = socket.ReadAdbResponse();

            using StreamReader reader = new(socket.GetShellStream(), Encoding);
            string result = reader.ReadLine() ?? throw new AdbException($"The {nameof(result)} of {nameof(InstallCreate)} is null.");

            if (!result.Contains("Success"))
            {
                throw new AdbException(reader.ReadToEnd());
            }

            int arr = result.IndexOf(']') - 1 - result.IndexOf('[');
            string session = result.Substring(result.IndexOf('[') + 1, arr);
            return session;
        }

        /// <inheritdoc/>
        public virtual void InstallWrite(DeviceData device, Stream apk, string apkName, string session, Action<double>? callback = null)
        {
            callback?.Invoke(0);

            EnsureDevice(device);
            ExceptionExtensions.ThrowIfNull(apk);
            ExceptionExtensions.ThrowIfNull(apkName);
            ExceptionExtensions.ThrowIfNull(session);

            if (!apk.CanRead || !apk.CanSeek)
            {
                throw new ArgumentOutOfRangeException(nameof(apk), "The apk stream must be a readable and seekable stream");
            }

            StringBuilder requestBuilder =
                new StringBuilder($"exec:cmd package 'install-write'")
                    // add size parameter [required for streaming installs]
                    // do last to override any user specified value
                    .Append(" -S ").Append(apk.Length)
                    .Append(' ').Append(session).Append(' ')
                    .Append(apkName).Append(".apk");

            using IAdbSocket socket = CreateAdbSocket();
            socket.SetDevice(device);

            socket.SendAdbRequest(requestBuilder.ToString());
            _ = socket.ReadAdbResponse();

            byte[] buffer = new byte[32 * 1024];
            int read;

            long totalBytesToProcess = apk.Length;
            long totalBytesRead = 0;

#if HAS_BUFFERS
            while ((read = apk.Read(buffer)) > 0)
            {
                socket.Send(buffer.AsSpan(0, read));
#else
            while ((read = apk.Read(buffer, 0, buffer.Length)) > 0)
            {
                socket.Send(buffer, read);
#endif
                totalBytesRead += read;
                callback?.Invoke(totalBytesToProcess == 0 ? 0 : totalBytesRead * 100d / totalBytesToProcess);
            }

            read = socket.Read(buffer);
            string value =
#if HAS_BUFFERS
                Encoding.GetString(buffer.AsSpan(0, read));
#else
                Encoding.GetString(buffer, 0, read);
#endif

            if (!value.Contains("Success"))
            {
                throw new AdbException(value);
            }
            callback?.Invoke(100);
        }

        /// <summary>
        /// Write an apk into the given install session.
        /// </summary>
        /// <param name="device">The device on which to install the application.</param>
        /// <param name="apk">A <see cref="Stream"/> which represents the application to install.</param>
        /// <param name="apkName">The name of the application.</param>
        /// <param name="session">The session ID of the install session.</param>
        /// <param name="callback">An optional parameter which, when specified, returns progress notifications. The progress is reported as a value between 0 and 100, representing the percentage of the apk which has been transferred.</param>
        protected virtual void InstallWrite(DeviceData device, Stream apk, string apkName, string session, Action<string?, double>? callback)
        {
            callback?.Invoke(apkName, 0);

            EnsureDevice(device);
            ExceptionExtensions.ThrowIfNull(apk);
            ExceptionExtensions.ThrowIfNull(apkName);
            ExceptionExtensions.ThrowIfNull(session);

            if (!apk.CanRead || !apk.CanSeek)
            {
                throw new ArgumentOutOfRangeException(nameof(apk), "The apk stream must be a readable and seekable stream");
            }

            StringBuilder requestBuilder =
                new StringBuilder($"exec:cmd package 'install-write'")
                    // add size parameter [required for streaming installs]
                    // do last to override any user specified value
                    .Append(" -S ").Append(apk.Length)
                    .Append(' ').Append(session).Append(' ')
                    .Append(apkName).Append(".apk");

            using IAdbSocket socket = CreateAdbSocket();
            socket.SetDevice(device);

            socket.SendAdbRequest(requestBuilder.ToString());
            _ = socket.ReadAdbResponse();

            byte[] buffer = new byte[32 * 1024];
            int read;

            long totalBytesToProcess = apk.Length;
            long totalBytesRead = 0;

#if HAS_BUFFERS
            while ((read = apk.Read(buffer)) > 0)
            {
                socket.Send(buffer.AsSpan(0, read));
#else
            while ((read = apk.Read(buffer, 0, buffer.Length)) > 0)
            {
                socket.Send(buffer, read);
#endif
                totalBytesRead += read;
                callback?.Invoke(apkName, totalBytesToProcess == 0 ? 0 : totalBytesRead * 100d / totalBytesToProcess);
            }
            callback?.Invoke(apkName, 100);

            read = socket.Read(buffer);
            string value =
#if HAS_BUFFERS
                Encoding.GetString(buffer.AsSpan(0, read));
#else
                Encoding.GetString(buffer, 0, read);
#endif

            if (!value.Contains("Success"))
            {
                throw new AdbException(value);
            }
            callback?.Invoke(null, 100);
        }

        /// <inheritdoc/>
        public void InstallCommit(DeviceData device, string session)
        {
            EnsureDevice(device);

            using IAdbSocket socket = CreateAdbSocket();
            socket.SetDevice(device);

            socket.SendAdbRequest($"exec:cmd package 'install-commit' {session}");
            _ = socket.ReadAdbResponse();

            using StreamReader reader = new(socket.GetShellStream(), Encoding);
            string? result = reader.ReadLine();
            if (result?.Contains("Success") != true)
            {
                throw new AdbException(reader.ReadToEnd());
            }
        }

        /// <inheritdoc/>
        public void Uninstall(DeviceData device, string packageName, params string[] arguments)
        {
            EnsureDevice(device);

            StringBuilder requestBuilder = new("exec:cmd package 'uninstall'");

            if (arguments != null)
            {
                foreach (string argument in arguments)
                {
                    _ = requestBuilder.Append(' ').Append(argument);
                }
            }

            _ = requestBuilder.Append(' ').Append(packageName);

            using IAdbSocket socket = CreateAdbSocket();
            socket.SetDevice(device);

            socket.SendAdbRequest(requestBuilder.ToString());
            _ = socket.ReadAdbResponse();

            using StreamReader reader = new(socket.GetShellStream(), Encoding);
            string? result = reader.ReadLine();
            if (result?.Contains("Success") != true)
            {
                throw new AdbException(reader.ReadToEnd());
            }
        }

        /// <inheritdoc/>
        public IEnumerable<string> GetFeatureSet(DeviceData device)
        {
            EnsureDevice(device);

            using IAdbSocket socket = CreateAdbSocket();
            socket.SendAdbRequest($"host-serial:{device.Serial}:features");
            _ = socket.ReadAdbResponse();
            string features = socket.ReadString();

            IEnumerable<string> featureList = features.Trim().Split('\n', ',');
            return featureList;
        }

        /// <summary>
        /// Creates a new <see cref="AdbClient"/> object with the specified <see cref="EndPoint"/>.
        /// </summary>
        /// <returns>A new <see cref="AdbClient"/> object with the specified <see cref="EndPoint"/>.</returns>
        public IAdbSocket CreateAdbSocket() => AdbSocketFactory(EndPoint);

        /// <inheritdoc/>
        public override string ToString() => $"The {nameof(AdbClient)} communicate with adb server at {EndPoint}";

        /// <summary>
        /// Creates a new <see cref="AdbClient"/> object that is a copy of the current instance with new <see cref="EndPoint"/>.
        /// </summary>
        /// <param name="endPoint">The new <see cref="EndPoint"/> to use.</param>
        /// <returns>A new <see cref="AdbClient"/> object that is a copy of this instance with new <see cref="EndPoint"/>.</returns>
        public virtual AdbClient Clone(EndPoint endPoint) => new(endPoint, AdbSocketFactory);

        /// <inheritdoc/>
        public AdbClient Clone() => Clone(EndPoint);

        /// <inheritdoc/>
        object ICloneable.Clone() => Clone();

        /// <summary>
        /// Sets default encoding (default - UTF8).
        /// </summary>
        /// <param name="encoding">The <see cref="System.Text.Encoding"/> to set.</param>
        public static void SetEncoding(Encoding encoding) => Encoding = encoding;

        /// <summary>
        /// Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="device"/> does not have a valid serial number.
        /// </summary>
        /// <param name="device">A <see cref="DeviceData"/> object to validate.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="device"/> corresponds.</param>
        /// <exception cref="ArgumentOutOfRangeException">The <paramref name="device"/> does not have a valid serial number.</exception>
        protected static void EnsureDevice(in DeviceData device, [CallerArgumentExpression(nameof(device))] string? paramName = "device")
        {
            if (device.IsEmpty)
            {
                throw new ArgumentOutOfRangeException(paramName, "You must specific a serial number for the device");
            }
        }

        /// <summary>
        /// Retrieves the value of an environment variable from the current process.
        /// </summary>
        /// <param name="variable">The name of the environment variable.</param>
        /// <returns>The value of the environment variable specified by <paramref name="variable"/>,
        /// or <see langword="null"/> if the environment variable is not found.</returns>
        private static string? TryGetEnvironmentVariable(string variable)
        {
            try
            {
                return Environment.GetEnvironmentVariable(variable);
            }
            catch
            {
                return null;
            }
        }
    }

    /// <summary>
    /// See as the <see cref="AdbClient"/> class.
    /// </summary>
    [Obsolete($"{nameof(AdvancedAdbClient)} is too long to remember. Please use {nameof(AdbClient)} instead.")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class AdvancedAdbClient : AdbClient, IAdvancedAdbClient;
}
