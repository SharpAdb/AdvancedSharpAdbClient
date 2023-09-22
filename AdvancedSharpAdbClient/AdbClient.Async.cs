#if HAS_TASK
// <copyright file="AdbClient.Async.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
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
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;

namespace AdvancedSharpAdbClient
{
    public partial class AdbClient
    {
        /// <inheritdoc/>
        public async Task<int> GetAdbVersionAsync(CancellationToken cancellationToken = default)
        {
            using IAdbSocket socket = adbSocketFactory(EndPoint);
            await socket.SendAdbRequestAsync("host:version", cancellationToken);
            AdbResponse response = await socket.ReadAdbResponseAsync(cancellationToken);
            string version = await socket.ReadStringAsync(cancellationToken);

            return int.Parse(version, NumberStyles.HexNumber);
        }

        /// <inheritdoc/>
        public async Task KillAdbAsync(CancellationToken cancellationToken = default)
        {
            using IAdbSocket socket = adbSocketFactory(EndPoint);
            await socket.SendAdbRequestAsync("host:kill", cancellationToken);

            // The host will immediately close the connection after the kill
            // command has been sent; no need to read the response.
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<DeviceData>> GetDevicesAsync(CancellationToken cancellationToken = default)
        {
            using IAdbSocket socket = adbSocketFactory(EndPoint);
            await socket.SendAdbRequestAsync("host:devices-l", cancellationToken);
            await socket.ReadAdbResponseAsync(cancellationToken);
            string reply = await socket.ReadStringAsync(cancellationToken);

            string[] data = reply.Split(separator, StringSplitOptions.RemoveEmptyEntries);

            return data.Select(DeviceData.CreateFromAdbData);
        }

        /// <inheritdoc/>
        public async Task<int> CreateForwardAsync(DeviceData device, string local, string remote, bool allowRebind, CancellationToken cancellationToken = default)
        {
            EnsureDevice(device);

            using IAdbSocket socket = adbSocketFactory(EndPoint);
            string rebind = allowRebind ? string.Empty : "norebind:";

            await socket.SendAdbRequestAsync($"host-serial:{device.Serial}:forward:{rebind}{local};{remote}", cancellationToken);
            _ = await socket.ReadAdbResponseAsync(cancellationToken);
            _ = await socket.ReadAdbResponseAsync(cancellationToken);
            string portString = await socket.ReadStringAsync(cancellationToken);

            return portString != null && int.TryParse(portString, out int port) ? port : 0;
        }

        /// <inheritdoc/>
        public Task<int> CreateForwardAsync(DeviceData device, ForwardSpec local, ForwardSpec remote, bool allowRebind, CancellationToken cancellationToken = default) =>
            CreateForwardAsync(device, local?.ToString(), remote?.ToString(), allowRebind, cancellationToken);

        /// <inheritdoc/>
        public async Task<int> CreateReverseForwardAsync(DeviceData device, string remote, string local, bool allowRebind, CancellationToken cancellationToken = default)
        {
            EnsureDevice(device);

            using IAdbSocket socket = adbSocketFactory(EndPoint);
            await socket.SetDeviceAsync(device, cancellationToken);

            string rebind = allowRebind ? string.Empty : "norebind:";

            await socket.SendAdbRequestAsync($"reverse:forward:{rebind}{remote};{local}", cancellationToken);
            _ = await socket.ReadAdbResponseAsync(cancellationToken);
            _ = await socket.ReadAdbResponseAsync(cancellationToken);
            string portString = await socket.ReadStringAsync(cancellationToken);

            return portString != null && int.TryParse(portString, out int port) ? port : 0;
        }

        /// <inheritdoc/>
        public async Task RemoveReverseForwardAsync(DeviceData device, string remote, CancellationToken cancellationToken = default)
        {
            EnsureDevice(device);

            using IAdbSocket socket = adbSocketFactory(EndPoint);
            await socket.SetDeviceAsync(device, cancellationToken);

            await socket.SendAdbRequestAsync($"reverse:killforward:{remote}", cancellationToken);
            AdbResponse response = socket.ReadAdbResponse();
        }

        /// <inheritdoc/>
        public async Task RemoveAllReverseForwardsAsync(DeviceData device, CancellationToken cancellationToken = default)
        {
            EnsureDevice(device);

            using IAdbSocket socket = adbSocketFactory(EndPoint);
            await socket.SetDeviceAsync(device, cancellationToken);

            await socket.SendAdbRequestAsync($"reverse:killforward-all", cancellationToken);
            AdbResponse response = await socket.ReadAdbResponseAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task RemoveForwardAsync(DeviceData device, int localPort, CancellationToken cancellationToken = default)
        {
            EnsureDevice(device);

            using IAdbSocket socket = adbSocketFactory(EndPoint);
            await socket.SendAdbRequestAsync($"host-serial:{device.Serial}:killforward:tcp:{localPort}", cancellationToken);
            AdbResponse response = await socket.ReadAdbResponseAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task RemoveAllForwardsAsync(DeviceData device, CancellationToken cancellationToken = default)
        {
            EnsureDevice(device);

            using IAdbSocket socket = adbSocketFactory(EndPoint);
            await socket.SendAdbRequestAsync($"host-serial:{device.Serial}:killforward-all", cancellationToken);
            AdbResponse response = await socket.ReadAdbResponseAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ForwardData>> ListForwardAsync(DeviceData device, CancellationToken cancellationToken = default)
        {
            EnsureDevice(device);

            using IAdbSocket socket = adbSocketFactory(EndPoint);
            await socket.SendAdbRequestAsync($"host-serial:{device.Serial}:list-forward", cancellationToken);
            AdbResponse response = await socket.ReadAdbResponseAsync(cancellationToken);

            string data = await socket.ReadStringAsync(cancellationToken);

            string[] parts = data.Split(separator, StringSplitOptions.RemoveEmptyEntries);

            return parts.Select(ForwardData.FromString);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ForwardData>> ListReverseForwardAsync(DeviceData device, CancellationToken cancellationToken = default)
        {
            EnsureDevice(device);

            using IAdbSocket socket = adbSocketFactory(EndPoint);
            await socket.SetDeviceAsync(device, cancellationToken);

            await socket.SendAdbRequestAsync($"reverse:list-forward", cancellationToken);
            AdbResponse response = await socket.ReadAdbResponseAsync(cancellationToken);

            string data = await socket.ReadStringAsync(cancellationToken);

            string[] parts = data.Split(separator, StringSplitOptions.RemoveEmptyEntries);

            return parts.Select(ForwardData.FromString);
        }

        /// <inheritdoc/>
        public Task ExecuteRemoteCommandAsync(string command, DeviceData device, IShellOutputReceiver receiver, CancellationToken cancellationToken = default) =>
            ExecuteRemoteCommandAsync(command, device, receiver, Encoding, cancellationToken);

        /// <inheritdoc/>
        public async Task ExecuteRemoteCommandAsync(string command, DeviceData device, IShellOutputReceiver receiver, Encoding encoding, CancellationToken cancellationToken = default)
        {
            EnsureDevice(device);

            using IAdbSocket socket = adbSocketFactory(EndPoint);
            cancellationToken.Register(socket.Dispose);

            await socket.SetDeviceAsync(device, cancellationToken);
            await socket.SendAdbRequestAsync($"shell:{command}", cancellationToken);
            AdbResponse response = await socket.ReadAdbResponseAsync(cancellationToken);

            try
            {
                using StreamReader reader = new(socket.GetShellStream(), encoding);
                // Previously, we would loop while reader.Peek() >= 0. Turns out that this would
                // break too soon in certain cases (about every 10 loops, so it appears to be a timing
                // issue). Checking for reader.ReadLine() to return null appears to be much more robust
                // -- one of the integration test fetches output 1000 times and found no truncations.
                while (!cancellationToken.IsCancellationRequested)
                {
                    string line = await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false);

                    if (line == null) { break; }

                    receiver?.AddOutput(line);
                }
            }
            catch (Exception e)
            {
                // If a cancellation was requested, this main loop is interrupted with an exception
                // because the socket is closed. In that case, we don't need to throw a ShellCommandUnresponsiveException.
                // In all other cases, something went wrong, and we want to report it to the user.
                if (!cancellationToken.IsCancellationRequested)
                {
                    throw new ShellCommandUnresponsiveException(e);
                }
            }
            finally
            {
                receiver?.Flush();
            }
        }

        /// <inheritdoc/>
        public async Task<Framebuffer> GetFrameBufferAsync(DeviceData device, CancellationToken cancellationToken = default)
        {
            EnsureDevice(device);

            Framebuffer framebuffer = CreateRefreshableFramebuffer(device);
            await framebuffer.RefreshAsync(true, cancellationToken).ConfigureAwait(false);

            // Convert the framebuffer to an image, and return that.
            return framebuffer;
        }

        /// <inheritdoc/>
        public Task RunLogServiceAsync(DeviceData device, Action<LogEntry> messageSink, params LogId[] logNames) =>
            RunLogServiceAsync(device, messageSink, default, logNames);

        /// <inheritdoc/>
        public async Task RunLogServiceAsync(DeviceData device, Action<LogEntry> messageSink, CancellationToken cancellationToken, params LogId[] logNames)
        {
            ExceptionExtensions.ThrowIfNull(messageSink);

            EnsureDevice(device);

            // The 'log' service has been deprecated, see
            // https://android.googlesource.com/platform/system/core/+/7aa39a7b199bb9803d3fd47246ee9530b4a96177
            using IAdbSocket socket = adbSocketFactory(EndPoint);
            await socket.SetDeviceAsync(device, cancellationToken);

            StringBuilder request = new();
            request.Append("shell:logcat -B");

            foreach (LogId logName in logNames)
            {
                request.Append($" -b {logName.ToString().ToLowerInvariant()}");
            }

            await socket.SendAdbRequestAsync(request.ToString(), cancellationToken);
            AdbResponse response = await socket.ReadAdbResponseAsync(cancellationToken);

#if NETCOREAPP3_0_OR_GREATER
            await
#endif
            using Stream stream = socket.GetShellStream();
            LogReader reader = new(stream);

            while (!cancellationToken.IsCancellationRequested)
            {
                LogEntry entry = null;

                try
                {
                    entry = await reader.ReadEntryAsync(cancellationToken).ConfigureAwait(false);
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
        public async Task RebootAsync(string into, DeviceData device, CancellationToken cancellationToken = default)
        {
            EnsureDevice(device);

            string request = $"reboot:{into}";

            using IAdbSocket socket = adbSocketFactory(EndPoint);
            await socket.SetDeviceAsync(device, cancellationToken);
            await socket.SendAdbRequestAsync(request, cancellationToken);
            AdbResponse response = await socket.ReadAdbResponseAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<string> PairAsync(DnsEndPoint endpoint, string code, CancellationToken cancellationToken = default)
        {
            ExceptionExtensions.ThrowIfNull(endpoint);

            using IAdbSocket socket = adbSocketFactory(EndPoint);
            await socket.SendAdbRequestAsync($"host:pair:{code}:{endpoint.Host}:{endpoint.Port}", cancellationToken);
            AdbResponse response = await socket.ReadAdbResponseAsync(cancellationToken);
            string results = await socket.ReadStringAsync(cancellationToken);
            return results;
        }

        /// <inheritdoc/>
        public async Task<string> ConnectAsync(DnsEndPoint endpoint, CancellationToken cancellationToken = default)
        {
            ExceptionExtensions.ThrowIfNull(endpoint);

            using IAdbSocket socket = adbSocketFactory(EndPoint);
            await socket.SendAdbRequestAsync($"host:connect:{endpoint.Host}:{endpoint.Port}", cancellationToken);
            AdbResponse response = await socket.ReadAdbResponseAsync(cancellationToken);
            string results = await socket.ReadStringAsync(cancellationToken);
            return results;
        }

        /// <inheritdoc/>
        public async Task<string> DisconnectAsync(DnsEndPoint endpoint, CancellationToken cancellationToken = default)
        {
            ExceptionExtensions.ThrowIfNull(endpoint);

            using IAdbSocket socket = adbSocketFactory(EndPoint);
            await socket.SendAdbRequestAsync($"host:disconnect:{endpoint.Host}:{endpoint.Port}", cancellationToken);
            AdbResponse response = await socket.ReadAdbResponseAsync(cancellationToken);
            string results = await socket.ReadStringAsync(cancellationToken);
            return results;
        }

        /// <inheritdoc/>
        public Task RootAsync(DeviceData device, CancellationToken cancellationToken = default) => RootAsync("root:", device, cancellationToken);

        /// <inheritdoc/>
        public Task UnrootAsync(DeviceData device, CancellationToken cancellationToken = default) => RootAsync("unroot:", device, cancellationToken);

        /// <summary>
        /// Restarts the ADB daemon running on the device with or without root privileges.
        /// </summary>
        /// <param name="request">The command of root or unroot.</param>
        /// <param name="device">The device on which to restart ADB with root privileges.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which return the results from adb.</returns>
        protected async Task RootAsync(string request, DeviceData device, CancellationToken cancellationToken = default)
        {
            EnsureDevice(device);

            using IAdbSocket socket = adbSocketFactory(EndPoint);
            await socket.SetDeviceAsync(device, cancellationToken);
            await socket.SendAdbRequestAsync(request, cancellationToken);
            AdbResponse response = await socket.ReadAdbResponseAsync(cancellationToken);

            // ADB will send some additional data
            byte[] buffer = new byte[1024];
            int read = socket.Read(buffer);

            string responseMessage = Encoding.UTF8.GetString(buffer, 0, read);

            // see https://android.googlesource.com/platform/packages/modules/adb/+/refs/heads/master/daemon/restart_service.cpp
            // for possible return strings
#if NETCOREAPP2_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER
            if (!responseMessage.Contains("restarting", StringComparison.OrdinalIgnoreCase))
#else
            if (responseMessage.IndexOf("restarting", StringComparison.OrdinalIgnoreCase) == -1)
#endif
            {
                throw new AdbException(responseMessage);
            }
            else
            {
                // Give adbd some time to kill itself and come back up.
                // We can't use wait-for-device because devices (e.g. adb over network) might not come back.
                Utilities.Delay(3000, cancellationToken).GetAwaiter().GetResult();
            }
        }

        /// <inheritdoc/>
        public Task InstallAsync(DeviceData device, Stream apk, params string[] arguments) => InstallAsync(device, apk, default, arguments);

        /// <inheritdoc/>
        public async Task InstallAsync(DeviceData device, Stream apk, CancellationToken cancellationToken, params string[] arguments)
        {
            EnsureDevice(device);

            ExceptionExtensions.ThrowIfNull(apk);

            if (!apk.CanRead || !apk.CanSeek)
            {
                throw new ArgumentOutOfRangeException(nameof(apk), "The apk stream must be a readable and seekable stream");
            }

            StringBuilder requestBuilder = new();
            _ = requestBuilder.Append("exec:cmd package 'install'");

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
            await socket.SetDeviceAsync(device, cancellationToken);

            await socket.SendAdbRequestAsync(requestBuilder.ToString(), cancellationToken);
            AdbResponse response = await socket.ReadAdbResponseAsync(cancellationToken);

            byte[] buffer = new byte[32 * 1024];
            int read = 0;

#if NETCOREAPP2_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER
            while ((read = await apk.ReadAsync(buffer, cancellationToken)) > 0)
#elif !NET35
            while ((read = await apk.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
#else
            while ((read = apk.Read(buffer, 0, buffer.Length)) > 0)
#endif
            {
                await socket.SendAsync(buffer, read, cancellationToken);
            }

            read = await socket.ReadAsync(buffer, cancellationToken);
            string value = Encoding.UTF8.GetString(buffer, 0, read);

            if (!value.Contains("Success"))
            {
                throw new AdbException(value);
            }
        }

        /// <inheritdoc/>
        public Task InstallMultipleAsync(DeviceData device, IEnumerable<Stream> splitAPKs, string packageName, params string[] arguments) =>
            InstallMultipleAsync(device, splitAPKs, packageName, default, arguments);

        /// <inheritdoc/>
        public async Task InstallMultipleAsync(DeviceData device, IEnumerable<Stream> splitAPKs, string packageName, CancellationToken cancellationToken, params string[] arguments)
        {
            EnsureDevice(device);

            ExceptionExtensions.ThrowIfNull(packageName);

            string session = await InstallCreateAsync(device, packageName, cancellationToken, arguments);

            int i = 0;
            IEnumerable<Task> tasks = splitAPKs.Select(async (splitAPK) =>
            {
                if (splitAPK == null || !splitAPK.CanRead || !splitAPK.CanSeek)
                {
                    Debug.WriteLine("The apk stream must be a readable and seekable stream");
                    return;
                }

                try
                {
                    await InstallWriteAsync(device, splitAPK, $"{nameof(splitAPK)}{i++}", session, cancellationToken);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            });
            foreach (Task task in tasks)
            {
                await task;
            }

            await InstallCommitAsync(device, session, cancellationToken);
        }

        /// <inheritdoc/>
        public Task InstallMultipleAsync(DeviceData device, Stream baseAPK, IEnumerable<Stream> splitAPKs, params string[] arguments) =>
            InstallMultipleAsync(device, baseAPK, splitAPKs, default, arguments);

        /// <inheritdoc/>
        public async Task InstallMultipleAsync(DeviceData device, Stream baseAPK, IEnumerable<Stream> splitAPKs, CancellationToken cancellationToken, params string[] arguments)
        {
            EnsureDevice(device);

            ExceptionExtensions.ThrowIfNull(baseAPK);

            if (!baseAPK.CanRead || !baseAPK.CanSeek)
            {
                throw new ArgumentOutOfRangeException(nameof(baseAPK), "The apk stream must be a readable and seekable stream");
            }

            string session = await InstallCreateAsync(device, null, cancellationToken, arguments);

            await InstallWriteAsync(device, baseAPK, nameof(baseAPK), session, cancellationToken);

            int i = 0;
            IEnumerable<Task> tasks = splitAPKs.Select(async (splitAPK) =>
            {
                if (splitAPK == null || !splitAPK.CanRead || !splitAPK.CanSeek)
                {
                    Debug.WriteLine("The apk stream must be a readable and seekable stream");
                    return;
                }

                try
                {
                    await InstallWriteAsync(device, splitAPK, $"{nameof(splitAPK)}{i++}", session, cancellationToken);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            });
            foreach (Task task in tasks)
            {
                await task;
            }

            await InstallCommitAsync(device, session, cancellationToken);
        }

        /// <inheritdoc/>
        public Task<string> InstallCreateAsync(DeviceData device, string packageName = null, params string[] arguments) =>
            InstallCreateAsync(device, packageName, default, arguments);


        /// <inheritdoc/>
        public async Task<string> InstallCreateAsync(DeviceData device, string packageName, CancellationToken cancellationToken, params string[] arguments)
        {
            EnsureDevice(device);

            StringBuilder requestBuilder = new();
            _ = requestBuilder.Append("exec:cmd package 'install-create'");
            _ = requestBuilder.Append(packageName.IsNullOrWhiteSpace() ? string.Empty : $" -p {packageName}");

            if (arguments != null)
            {
                foreach (string argument in arguments)
                {
                    _ = requestBuilder.Append($" {argument}");
                }
            }

            using IAdbSocket socket = adbSocketFactory(EndPoint);
            await socket.SetDeviceAsync(device, cancellationToken);

            await socket.SendAdbRequestAsync(requestBuilder.ToString(), cancellationToken);
            AdbResponse response = await socket.ReadAdbResponseAsync(cancellationToken);

            using StreamReader reader = new(socket.GetShellStream(), Encoding);
            string result = await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false);

            if (!result.Contains("Success"))
            {
                throw new AdbException(await reader.ReadToEndAsync(cancellationToken));
            }

            int arr = result.IndexOf(']') - 1 - result.IndexOf('[');
            string session = result.Substring(result.IndexOf('[') + 1, arr);
            return session;
        }

        /// <inheritdoc/>
        public async Task InstallWriteAsync(DeviceData device, Stream apk, string apkName, string session, CancellationToken cancellationToken = default)
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
            await socket.SetDeviceAsync(device, cancellationToken);

            await socket.SendAdbRequestAsync(requestBuilder.ToString(), cancellationToken);
            AdbResponse response = await socket.ReadAdbResponseAsync(cancellationToken);

            byte[] buffer = new byte[32 * 1024];
            int read = 0;

#if NETCOREAPP2_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER
            while ((read = await apk.ReadAsync(buffer, cancellationToken)) > 0)
#elif !NET35
            while ((read = await apk.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
#else
            while ((read = apk.Read(buffer, 0, buffer.Length)) > 0)
#endif
            {
                await socket.SendAsync(buffer, read, cancellationToken);
            }

            read = await socket.ReadAsync(buffer, cancellationToken);
            string value = Encoding.UTF8.GetString(buffer, 0, read);

            if (!value.Contains("Success"))
            {
                throw new AdbException(value);
            }
        }

        /// <inheritdoc/>
        public async Task InstallCommitAsync(DeviceData device, string session, CancellationToken cancellationToken = default)
        {
            using IAdbSocket socket = adbSocketFactory(EndPoint);
            await socket.SetDeviceAsync(device, cancellationToken);

            await socket.SendAdbRequestAsync($"exec:cmd package 'install-commit' {session}", cancellationToken);
            AdbResponse response = await socket.ReadAdbResponseAsync(cancellationToken);

            using StreamReader reader = new(socket.GetShellStream(), Encoding);
            string result = await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false);
            if (!result.Contains("Success"))
            {
                throw new AdbException(await reader.ReadToEndAsync(cancellationToken));
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<string>> GetFeatureSetAsync(DeviceData device, CancellationToken cancellationToken = default)
        {
            using IAdbSocket socket = adbSocketFactory(EndPoint);
            await socket.SendAdbRequestAsync($"host-serial:{device.Serial}:features", cancellationToken);

            AdbResponse response = await socket.ReadAdbResponseAsync(cancellationToken);
            string features = await socket.ReadStringAsync(cancellationToken);

            IEnumerable<string> featureList = features.Trim().Split('\n', ',');
            return featureList;
        }

        /// <inheritdoc/>
        public async Task<string> DumpScreenStringAsync(DeviceData device, CancellationToken cancellationToken = default)
        {
            EnsureDevice(device);
            using IAdbSocket socket = adbSocketFactory(EndPoint);
            await socket.SetDeviceAsync(device, cancellationToken);
            await socket.SendAdbRequestAsync("shell:uiautomator dump /dev/tty", cancellationToken);
            AdbResponse response = await socket.ReadAdbResponseAsync(cancellationToken);
            using StreamReader reader = new(socket.GetShellStream(), Encoding);
            string xmlString = reader.ReadToEnd()
                .Replace("Events injected: 1\r\n", string.Empty)
                .Replace("UI hierchary dumped to: /dev/tty", string.Empty)
                .Trim();
            if (string.IsNullOrEmpty(xmlString) || xmlString.StartsWith("<?xml"))
            {
                return xmlString;
            }
            Match xmlMatch = GetXMLRegex().Match(xmlString);
            return !xmlMatch.Success ? throw new XmlException("An error occurred while receiving xml: " + xmlString) : xmlMatch.Value;
        }

        /// <inheritdoc/>
        public async Task<XmlDocument> DumpScreenAsync(DeviceData device, CancellationToken cancellationToken = default)
        {
            XmlDocument doc = new();
            string xmlString = await DumpScreenStringAsync(device, cancellationToken);
            if (!string.IsNullOrEmpty(xmlString))
            {
                doc.LoadXml(xmlString);
                return doc;
            }
            return null;
        }

#if WINDOWS_UWP || WINDOWS10_0_17763_0_OR_GREATER
        /// <inheritdoc/>
        public async Task<Windows.Data.Xml.Dom.XmlDocument> DumpScreenWinRTAsync(DeviceData device, CancellationToken cancellationToken = default)
        {
            Windows.Data.Xml.Dom.XmlDocument doc = new();
            string xmlString = await DumpScreenStringAsync(device, cancellationToken);
            if (!string.IsNullOrEmpty(xmlString))
            {
                doc.LoadXml(xmlString);
                return doc;
            }
            return null;
        }
#endif

        /// <inheritdoc/>
        public async Task ClickAsync(DeviceData device, Cords cords, CancellationToken cancellationToken = default)
        {
            EnsureDevice(device);

            using IAdbSocket socket = adbSocketFactory(EndPoint);
            await socket.SetDeviceAsync(device, cancellationToken);
            await socket.SendAdbRequestAsync($"shell:input tap {cords.X} {cords.Y}", cancellationToken);
            AdbResponse response = await socket.ReadAdbResponseAsync(cancellationToken);
            using StreamReader reader = new(socket.GetShellStream(), Encoding);
            string result = (await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false)).TrimStart();
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
        public async Task ClickAsync(DeviceData device, int x, int y, CancellationToken cancellationToken = default)
        {
            EnsureDevice(device);

            using IAdbSocket socket = adbSocketFactory(EndPoint);
            await socket.SetDeviceAsync(device, cancellationToken);
            await socket.SendAdbRequestAsync($"shell:input tap {x} {y}", cancellationToken);
            AdbResponse response = await socket.ReadAdbResponseAsync(cancellationToken);
            using StreamReader reader = new(socket.GetShellStream(), Encoding);
            string result = (await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false)).TrimStart();
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
        public async Task SwipeAsync(DeviceData device, Element first, Element second, long speed, CancellationToken cancellationToken = default)
        {
            EnsureDevice(device);

            using IAdbSocket socket = adbSocketFactory(EndPoint);
            await socket.SetDeviceAsync(device, cancellationToken);
            await socket.SendAdbRequestAsync($"shell:input swipe {first.Cords.X} {first.Cords.Y} {second.Cords.X} {second.Cords.Y} {speed}", cancellationToken);
            AdbResponse response = await socket.ReadAdbResponseAsync(cancellationToken);
            using StreamReader reader = new(socket.GetShellStream(), Encoding);
            string result = (await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false)).TrimStart();
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
        public async Task SwipeAsync(DeviceData device, int x1, int y1, int x2, int y2, long speed, CancellationToken cancellationToken = default)
        {
            EnsureDevice(device);

            using IAdbSocket socket = adbSocketFactory(EndPoint);
            await socket.SetDeviceAsync(device, cancellationToken);
            await socket.SendAdbRequestAsync($"shell:input swipe {x1} {y1} {x2} {y2} {speed}", cancellationToken);
            AdbResponse response = await socket.ReadAdbResponseAsync(cancellationToken);
            using StreamReader reader = new(socket.GetShellStream(), Encoding);
            string result = (await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false)).TrimStart();
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
        public async Task<bool> IsCurrentAppAsync(DeviceData device, string packageName, CancellationToken cancellationToken = default)
        {
            ConsoleOutputReceiver receiver = new();
            await ExecuteRemoteCommandAsync($"dumpsys activity activities | grep mResumedActivity", device, receiver, cancellationToken);
            string response = receiver.ToString().Trim();
            return response.ToString().Contains(packageName);
        }

        /// <inheritdoc/>
        public async Task<bool> IsAppRunningAsync(DeviceData device, string packageName, CancellationToken cancellationToken = default)
        {
            ConsoleOutputReceiver receiver = new();
            await ExecuteRemoteCommandAsync($"pidof {packageName}", device, receiver, cancellationToken);
            string response = receiver.ToString().Trim();
            bool intParsed = int.TryParse(response, out int pid);
            return intParsed && pid > 0;
        }

        /// <inheritdoc/>
        public async Task<AppStatus> GetAppStatusAsync(DeviceData device, string packageName, CancellationToken cancellationToken = default)
        {
            // Check if the app is in foreground
            bool currentApp = await IsCurrentAppAsync(device, packageName, cancellationToken);
            if (currentApp)
            {
                return AppStatus.Foreground;
            }

            // Check if the app is running in background
            bool isAppRunning = await IsAppRunningAsync(device, packageName, cancellationToken);
            return isAppRunning ? AppStatus.Background : AppStatus.Stopped;
        }

        /// <inheritdoc/>
        public async Task<Element> FindElementAsync(DeviceData device, string xpath = "hierarchy/node", CancellationToken cancellationToken = default)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    XmlDocument doc = await DumpScreenAsync(device, cancellationToken);
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
                    if (cancellationToken == default)
                    {
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                // If a cancellation was requested, this main loop is interrupted with an exception
                // because the socket is closed. In that case, we don't need to throw a ShellCommandUnresponsiveException.
                // In all other cases, something went wrong, and we want to report it to the user.
                if (!cancellationToken.IsCancellationRequested)
                {
                    throw new ShellCommandUnresponsiveException(e);
                }
            }
            return null;
        }

        /// <inheritdoc/>
        public async Task<List<Element>> FindElementsAsync(DeviceData device, string xpath = "hierarchy/node", CancellationToken cancellationToken = default)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    XmlDocument doc = await DumpScreenAsync(device, cancellationToken);
                    if (doc != null)
                    {
                        XmlNodeList xmlNodes = doc.SelectNodes(xpath);
                        if (xmlNodes != null)
                        {
                            List<Element> elements = new(xmlNodes.Count);
                            for (int i = 0; i < xmlNodes.Count; i++)
                            {
                                Element element = Element.FromXmlNode(this, device, xmlNodes[i]);
                                if (element != null)
                                {
                                    elements.Add(element);
                                }
                            }
                            return elements.Count == 0 ? null : elements;
                        }
                    }
                    if (cancellationToken == default)
                    {
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                // If a cancellation was requested, this main loop is interrupted with an exception
                // because the socket is closed. In that case, we don't need to throw a ShellCommandUnresponsiveException.
                // In all other cases, something went wrong, and we want to report it to the user.
                if (!cancellationToken.IsCancellationRequested)
                {
                    throw new ShellCommandUnresponsiveException(e);
                }
            }
            return null;
        }

#if NETCOREAPP3_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        /// <inheritdoc/>
        public async IAsyncEnumerable<Element> FindAsyncElements(DeviceData device, string xpath = "hierarchy/node", [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                XmlDocument doc = await DumpScreenAsync(device, cancellationToken);
                if (doc != null)
                {
                    XmlNodeList xmlNodes = doc.SelectNodes(xpath);
                    if (xmlNodes != null)
                    {
                        bool isBreak = false;
                        for (int i = 0; i < xmlNodes.Count; i++)
                        {
                            Element element = Element.FromXmlNode(this, device, xmlNodes[i]);
                            if (element != null)
                            {
                                isBreak = true;
                                yield return element;
                            }
                        }
                        if (isBreak)
                        {
                            break;
                        }
                    }
                }
                if (cancellationToken == default)
                {
                    break;
                }
            }
        }
#endif

        /// <inheritdoc/>
        public async Task SendKeyEventAsync(DeviceData device, string key, CancellationToken cancellationToken = default)
        {
            EnsureDevice(device);

            using IAdbSocket socket = adbSocketFactory(EndPoint);
            await socket.SetDeviceAsync(device, cancellationToken);
            await socket.SendAdbRequestAsync($"shell:input keyevent {key}", cancellationToken);
            AdbResponse response = await socket.ReadAdbResponseAsync(cancellationToken);
            using StreamReader reader = new(socket.GetShellStream(), Encoding);
            string result = (await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false)).TrimStart();
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
        public async Task SendTextAsync(DeviceData device, string text, CancellationToken cancellationToken = default)
        {
            EnsureDevice(device);

            using IAdbSocket socket = adbSocketFactory(EndPoint);
            await socket.SetDeviceAsync(device, cancellationToken);
            await socket.SendAdbRequestAsync($"shell:input text {text}", cancellationToken);
            AdbResponse response = await socket.ReadAdbResponseAsync(cancellationToken);
            using StreamReader reader = new(socket.GetShellStream(), Encoding);
            string result = (await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false)).TrimStart();
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
        public async Task ClearInputAsync(DeviceData device, int charCount, CancellationToken cancellationToken = default)
        {
            await SendKeyEventAsync(device, "KEYCODE_MOVE_END", cancellationToken);
            await ExecuteRemoteCommandAsync("input keyevent " + Utilities.Join(" ", Enumerable.Repeat("KEYCODE_DEL ", charCount)), device, null, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task StartAppAsync(DeviceData device, string packageName, CancellationToken cancellationToken = default) =>
            await ExecuteRemoteCommandAsync($"monkey -p {packageName} 1", device, null, cancellationToken);

        /// <inheritdoc/>
        public async Task StopAppAsync(DeviceData device, string packageName, CancellationToken cancellationToken = default) =>
            await ExecuteRemoteCommandAsync($"am force-stop {packageName}", device, null, cancellationToken);

        /// <inheritdoc/>
        public Task BackBtnAsync(DeviceData device) => SendKeyEventAsync(device, "KEYCODE_BACK");

        /// <inheritdoc/>
        public Task HomeBtnAsync(DeviceData device) => SendKeyEventAsync(device, "KEYCODE_HOME");
    }
}
#endif