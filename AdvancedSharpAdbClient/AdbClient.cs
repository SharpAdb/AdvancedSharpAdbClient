// <copyright file="AdbClient.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
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
    public partial class AdbClient : IAdbClient
#if WINDOWS_UWP || WINDOWS10_0_17763_0_OR_GREATER
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
            : this(new IPEndPoint(IPAddress.Loopback, AdbServerPort), Factories.AdbSocketFactory)
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
            : this(new IPEndPoint(IPAddress.Loopback, AdbServerPort), adbSocketFactory)
        {
        }

        /// <summary>
        /// Gets a new instance of the <see cref="AdbClient"/> class.
        /// </summary>
        [Obsolete("This function has been removed since SharpAdbClient. Here is a placeholder which function is gets a new instance instead of gets or sets the default instance.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static IAdbClient Instance => new AdbClient();

        /// <summary>
        /// Gets or sets default encoding.
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
        public virtual int GetAdbVersion()
        {
            using IAdbSocket socket = AdbSocketFactory(EndPoint);

            socket.SendAdbRequest("host:version");
            _ = socket.ReadAdbResponse();
            string version = socket.ReadString();

            return int.Parse(version, NumberStyles.HexNumber);
        }

        /// <inheritdoc/>
        public virtual void KillAdb()
        {
            using IAdbSocket socket = AdbSocketFactory(EndPoint);
            socket.SendAdbRequest("host:kill");

            // The host will immediately close the connection after the kill
            // command has been sent; no need to read the response.
        }

        /// <inheritdoc/>
        public virtual IEnumerable<DeviceData> GetDevices()
        {
            using IAdbSocket socket = AdbSocketFactory(EndPoint);

            socket.SendAdbRequest("host:devices-l");
            _ = socket.ReadAdbResponse();
            string reply = socket.ReadString();

            string[] data = reply.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            return data.Select(x => new DeviceData(x));
        }

        /// <inheritdoc/>
        public virtual int CreateForward(DeviceData device, string local, string remote, bool allowRebind)
        {
            EnsureDevice(device);

            using IAdbSocket socket = AdbSocketFactory(EndPoint);
            string rebind = allowRebind ? string.Empty : "norebind:";

            socket.SendAdbRequest($"host-serial:{device.Serial}:forward:{rebind}{local};{remote}");
            _ = socket.ReadAdbResponse();
            _ = socket.ReadAdbResponse();

            string portString = socket.ReadString();
            return portString != null && int.TryParse(portString, out int port) ? port : 0;
        }

        /// <inheritdoc/>
        public virtual int CreateReverseForward(DeviceData device, string remote, string local, bool allowRebind)
        {
            EnsureDevice(device);

            using IAdbSocket socket = AdbSocketFactory(EndPoint);

            socket.SetDevice(device);
            string rebind = allowRebind ? string.Empty : "norebind:";

            socket.SendAdbRequest($"reverse:forward:{rebind}{remote};{local}");
            _ = socket.ReadAdbResponse();
            _ = socket.ReadAdbResponse();

            string portString = socket.ReadString();
            return portString != null && int.TryParse(portString, out int port) ? port : 0;
        }

        /// <inheritdoc/>
        public virtual void RemoveReverseForward(DeviceData device, string remote)
        {
            EnsureDevice(device);

            using IAdbSocket socket = AdbSocketFactory(EndPoint);
            socket.SetDevice(device);

            socket.SendAdbRequest($"reverse:killforward:{remote}");
            _ = socket.ReadAdbResponse();
        }

        /// <inheritdoc/>
        public virtual void RemoveAllReverseForwards(DeviceData device)
        {
            EnsureDevice(device);

            using IAdbSocket socket = AdbSocketFactory(EndPoint);
            socket.SetDevice(device);

            socket.SendAdbRequest($"reverse:killforward-all");
            _ = socket.ReadAdbResponse();
        }

        /// <inheritdoc/>
        public virtual void RemoveForward(DeviceData device, int localPort)
        {
            EnsureDevice(device);

            using IAdbSocket socket = AdbSocketFactory(EndPoint);
            socket.SendAdbRequest($"host-serial:{device.Serial}:killforward:tcp:{localPort}");
            _ = socket.ReadAdbResponse();
        }

        /// <inheritdoc/>
        public virtual void RemoveAllForwards(DeviceData device)
        {
            EnsureDevice(device);

            using IAdbSocket socket = AdbSocketFactory(EndPoint);
            socket.SendAdbRequest($"host-serial:{device.Serial}:killforward-all");
            _ = socket.ReadAdbResponse();
        }

        /// <inheritdoc/>
        public virtual IEnumerable<ForwardData> ListForward(DeviceData device)
        {
            EnsureDevice(device);

            using IAdbSocket socket = AdbSocketFactory(EndPoint);
            socket.SendAdbRequest($"host-serial:{device.Serial}:list-forward");
            _ = socket.ReadAdbResponse();
            string data = socket.ReadString();

            string[] parts = data.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            return parts.Select(x => ForwardData.FromString(x));
        }

        /// <inheritdoc/>
        public virtual IEnumerable<ForwardData> ListReverseForward(DeviceData device)
        {
            EnsureDevice(device);

            using IAdbSocket socket = AdbSocketFactory(EndPoint);
            socket.SetDevice(device);

            socket.SendAdbRequest($"reverse:list-forward");
            _ = socket.ReadAdbResponse();
            string data = socket.ReadString();

            string[] parts = data.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            return parts.Select(x => ForwardData.FromString(x));
        }

        /// <inheritdoc/>
        public virtual void ExecuteServerCommand(string target, string command)
        {
            using IAdbSocket socket = AdbSocketFactory(EndPoint);
            ExecuteServerCommand(target, command, socket);
        }

        /// <inheritdoc/>
        public virtual void ExecuteServerCommand(string target, string command, IAdbSocket socket)
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
        public virtual void ExecuteRemoteCommand(string command, DeviceData device)
        {
            EnsureDevice(device);

            using IAdbSocket socket = AdbSocketFactory(EndPoint);
            socket.SetDevice(device);

            ExecuteServerCommand("shell", command, socket);
        }

        /// <inheritdoc/>
        public virtual void ExecuteServerCommand(string target, string command, IShellOutputReceiver receiver, Encoding encoding)
        {
            ExceptionExtensions.ThrowIfNull(encoding);
            using IAdbSocket socket = AdbSocketFactory(EndPoint);
            ExecuteServerCommand(target, command, socket, receiver, encoding);
        }

        /// <inheritdoc/>
        public virtual void ExecuteServerCommand(string target, string command, IAdbSocket socket, IShellOutputReceiver receiver, Encoding encoding)
        {
            ExceptionExtensions.ThrowIfNull(encoding);

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
        public virtual void ExecuteRemoteCommand(string command, DeviceData device, IShellOutputReceiver receiver, Encoding encoding)
        {
            EnsureDevice(device);

            using IAdbSocket socket = AdbSocketFactory(EndPoint);
            socket.SetDevice(device);

            ExecuteServerCommand("shell", command, socket, receiver, encoding);
        }

        /// <inheritdoc/>
        public virtual IEnumerable<string> ExecuteServerCommand(string target, string command, Encoding encoding)
        {
            ExceptionExtensions.ThrowIfNull(encoding);
            using IAdbSocket socket = AdbSocketFactory(EndPoint);
            return ExecuteServerCommand(target, command, socket, encoding);
        }

        /// <inheritdoc/>
        public virtual IEnumerable<string> ExecuteServerCommand(string target, string command, IAdbSocket socket, Encoding encoding)
        {
            ExceptionExtensions.ThrowIfNull(encoding);

            StringBuilder request = new();
            if (!StringExtensions.IsNullOrWhiteSpace(target))
            {
                _ = request.AppendFormat("{0}:", target);
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
        public virtual IEnumerable<string> ExecuteRemoteCommand(string command, DeviceData device, Encoding encoding)
        {
            EnsureDevice(device);

            using IAdbSocket socket = AdbSocketFactory(EndPoint);
            socket.SetDevice(device);

            return ExecuteServerCommand("shell", command, socket, encoding);
        }

        /// <inheritdoc/>
        public virtual IEnumerable<LogEntry> RunLogService(DeviceData device, params LogId[] logNames)
        {
            EnsureDevice(device);

            // The 'log' service has been deprecated, see
            // https://android.googlesource.com/platform/system/core/+/7aa39a7b199bb9803d3fd47246ee9530b4a96177
            using IAdbSocket socket = AdbSocketFactory(EndPoint);
            socket.SetDevice(device);

            StringBuilder request = new StringBuilder().Append("shell:logcat -B");

            foreach (LogId logName in logNames)
            {
                _ = request.AppendFormat(" -b {0}", logName.ToString().ToLowerInvariant());
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
        public virtual void RunLogService(DeviceData device, Action<LogEntry> messageSink, in bool isCancelled = false, params LogId[] logNames)
        {
            EnsureDevice(device);
            ExceptionExtensions.ThrowIfNull(messageSink);

            // The 'log' service has been deprecated, see
            // https://android.googlesource.com/platform/system/core/+/7aa39a7b199bb9803d3fd47246ee9530b4a96177
            using IAdbSocket socket = AdbSocketFactory(EndPoint);
            socket.SetDevice(device);

            StringBuilder request = new StringBuilder().Append("shell:logcat -B");

            foreach (LogId logName in logNames)
            {
                _ = request.AppendFormat(" -b {0}", logName.ToString().ToLowerInvariant());
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
        public virtual Framebuffer CreateFramebuffer(DeviceData device)
        {
            EnsureDevice(device);
            return new Framebuffer(device, this, AdbSocketFactory);
        }

        /// <inheritdoc/>
        public virtual Framebuffer GetFrameBuffer(DeviceData device)
        {
            EnsureDevice(device);

            Framebuffer framebuffer = CreateFramebuffer(device);
            framebuffer.Refresh(true);

            // Convert the framebuffer to an image, and return that.
            return framebuffer;
        }

        /// <inheritdoc/>
        public virtual void Reboot(string into, DeviceData device)
        {
            EnsureDevice(device);

            using IAdbSocket socket = AdbSocketFactory(EndPoint);
            socket.SetDevice(device);

            socket.SendAdbRequest($"reboot:{into}");
            _ = socket.ReadAdbResponse();
        }

        /// <inheritdoc/>
        public virtual string Pair(DnsEndPoint endpoint, string code)
        {
            ExceptionExtensions.ThrowIfNull(endpoint);

            using IAdbSocket socket = AdbSocketFactory(EndPoint);
            socket.SendAdbRequest($"host:pair:{code}:{endpoint.Host}:{endpoint.Port}");
            _ = socket.ReadAdbResponse();

            return socket.ReadString();
        }

        /// <inheritdoc/>
        public virtual string Connect(DnsEndPoint endpoint)
        {
            ExceptionExtensions.ThrowIfNull(endpoint);

            using IAdbSocket socket = AdbSocketFactory(EndPoint);
            socket.SendAdbRequest($"host:connect:{endpoint.Host}:{endpoint.Port}");
            _ = socket.ReadAdbResponse();

            return socket.ReadString();
        }

        /// <inheritdoc/>
        public virtual string Disconnect(DnsEndPoint endpoint)
        {
            ExceptionExtensions.ThrowIfNull(endpoint);

            using IAdbSocket socket = AdbSocketFactory(EndPoint);
            socket.SendAdbRequest($"host:disconnect:{endpoint.Host}:{endpoint.Port}");
            _ = socket.ReadAdbResponse();

            return socket.ReadString();
        }

        /// <inheritdoc/>
        public virtual void Root(DeviceData device) => Root("root:", device);

        /// <inheritdoc/>
        public virtual void Unroot(DeviceData device) => Root("unroot:", device);

        /// <summary>
        /// Restarts the ADB daemon running on the device with or without root privileges.
        /// </summary>
        /// <param name="request">The command of root or unroot.</param>
        /// <param name="device">The device on which to restart ADB with root privileges.</param>
        protected virtual void Root(string request, DeviceData device)
        {
            EnsureDevice(device);

            using IAdbSocket socket = AdbSocketFactory(EndPoint);
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

            StringBuilder requestBuilder = new StringBuilder().Append("exec:cmd package 'install'");

            if (arguments != null)
            {
                foreach (string argument in arguments)
                {
                    _ = requestBuilder.AppendFormat(" {0}", argument);
                }
            }

            // add size parameter [required for streaming installs]
            // do last to override any user specified value
            _ = requestBuilder.AppendFormat(" -S {0}", apk.Length);

            using IAdbSocket socket = AdbSocketFactory(EndPoint);
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
        public virtual void InstallMultiple(DeviceData device, Stream baseAPK, IEnumerable<Stream> splitAPKs, Action<InstallProgressEventArgs>? callback = null, params string[] arguments)
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
        public virtual void InstallMultiple(DeviceData device, IEnumerable<Stream> splitAPKs, string packageName, Action<InstallProgressEventArgs>? callback = null, params string[] arguments)
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
        public virtual string InstallCreate(DeviceData device, params string[] arguments)
        {
            EnsureDevice(device);

            StringBuilder requestBuilder = new StringBuilder().Append("exec:cmd package 'install-create'");

            if (arguments != null)
            {
                foreach (string argument in arguments)
                {
                    _ = requestBuilder.AppendFormat(" {0}", argument);
                }
            }

            using IAdbSocket socket = AdbSocketFactory(EndPoint);
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
        public virtual string InstallCreate(DeviceData device, string packageName, params string[] arguments)
        {
            EnsureDevice(device);
            ExceptionExtensions.ThrowIfNull(packageName);

            StringBuilder requestBuilder =
                new StringBuilder().Append("exec:cmd package 'install-create'")
                                   .AppendFormat(" -p {0}", packageName);

            if (arguments != null)
            {
                foreach (string argument in arguments)
                {
                    _ = requestBuilder.AppendFormat(" {0}", argument);
                }
            }

            using IAdbSocket socket = AdbSocketFactory(EndPoint);
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
                new StringBuilder().Append($"exec:cmd package 'install-write'")
                                   // add size parameter [required for streaming installs]
                                   // do last to override any user specified value
                                   .AppendFormat(" -S {0}", apk.Length)
                                   .AppendFormat(" {0} {1}.apk", session, apkName);

            using IAdbSocket socket = AdbSocketFactory(EndPoint);
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
                new StringBuilder().Append($"exec:cmd package 'install-write'")
                                   // add size parameter [required for streaming installs]
                                   // do last to override any user specified value
                                   .AppendFormat(" -S {0}", apk.Length)
                                   .AppendFormat(" {0} {1}.apk", session, apkName);

            using IAdbSocket socket = AdbSocketFactory(EndPoint);
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
        public virtual void InstallCommit(DeviceData device, string session)
        {
            EnsureDevice(device);

            using IAdbSocket socket = AdbSocketFactory(EndPoint);
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
        public virtual void Uninstall(DeviceData device, string packageName, params string[] arguments)
        {
            EnsureDevice(device);

            StringBuilder requestBuilder = new StringBuilder().Append("exec:cmd package 'uninstall'");

            if (arguments != null)
            {
                foreach (string argument in arguments)
                {
                    _ = requestBuilder.AppendFormat(" {0}", argument);
                }
            }

            _ = requestBuilder.AppendFormat(" {0}", packageName);

            using IAdbSocket socket = AdbSocketFactory(EndPoint);
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
        public virtual IEnumerable<string> GetFeatureSet(DeviceData device)
        {
            EnsureDevice(device);

            using IAdbSocket socket = AdbSocketFactory(EndPoint);
            socket.SendAdbRequest($"host-serial:{device.Serial}:features");
            _ = socket.ReadAdbResponse();
            string features = socket.ReadString();

            IEnumerable<string> featureList = features.Trim().Split('\n', ',');
            return featureList;
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
        protected static void EnsureDevice([NotNull] DeviceData? device)
        {
            ExceptionExtensions.ThrowIfNull(device);
            if (string.IsNullOrEmpty(device.Serial))
            {
                throw new ArgumentOutOfRangeException(nameof(device), "You must specific a serial number for the device");
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
