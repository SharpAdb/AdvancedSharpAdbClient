#if HAS_TASK
// <copyright file="AdbClient.Async.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace AdvancedSharpAdbClient
{
    public partial class AdbClient
    {
        /// <inheritdoc/>
        public virtual async Task<int> GetAdbVersionAsync(CancellationToken cancellationToken = default)
        {
            using IAdbSocket socket = adbSocketFactory(EndPoint);

            await socket.SendAdbRequestAsync("host:version", cancellationToken).ConfigureAwait(false);
            _ = await socket.ReadAdbResponseAsync(cancellationToken).ConfigureAwait(false);
            string version = await socket.ReadStringAsync(cancellationToken).ConfigureAwait(false);

            return int.Parse(version, NumberStyles.HexNumber);
        }

        /// <inheritdoc/>
        public virtual async Task KillAdbAsync(CancellationToken cancellationToken = default)
        {
            using IAdbSocket socket = adbSocketFactory(EndPoint);
            await socket.SendAdbRequestAsync("host:kill", cancellationToken).ConfigureAwait(false);

            // The host will immediately close the connection after the kill
            // command has been sent; no need to read the response.
        }

        /// <inheritdoc/>
        public virtual async Task<IEnumerable<DeviceData>> GetDevicesAsync(CancellationToken cancellationToken = default)
        {
            using IAdbSocket socket = adbSocketFactory(EndPoint);

            await socket.SendAdbRequestAsync("host:devices-l", cancellationToken).ConfigureAwait(false);
            _ = await socket.ReadAdbResponseAsync(cancellationToken).ConfigureAwait(false);
            string reply = await socket.ReadStringAsync(cancellationToken).ConfigureAwait(false);

            string[] data = reply.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            return data.Select(x => new DeviceData(x));
        }

        /// <inheritdoc/>
        public virtual async Task<int> CreateForwardAsync(DeviceData device, string local, string remote, bool allowRebind, CancellationToken cancellationToken = default)
        {
            EnsureDevice(device);

            using IAdbSocket socket = adbSocketFactory(EndPoint);
            string rebind = allowRebind ? string.Empty : "norebind:";

            await socket.SendAdbRequestAsync($"host-serial:{device.Serial}:forward:{rebind}{local};{remote}", cancellationToken).ConfigureAwait(false);
            _ = await socket.ReadAdbResponseAsync(cancellationToken).ConfigureAwait(false);
            _ = await socket.ReadAdbResponseAsync(cancellationToken).ConfigureAwait(false);

            string portString = await socket.ReadStringAsync(cancellationToken).ConfigureAwait(false);
            return portString != null && int.TryParse(portString, out int port) ? port : 0;
        }

        /// <inheritdoc/>
        public virtual async Task<int> CreateReverseForwardAsync(DeviceData device, string remote, string local, bool allowRebind, CancellationToken cancellationToken = default)
        {
            EnsureDevice(device);

            using IAdbSocket socket = adbSocketFactory(EndPoint);

            await socket.SetDeviceAsync(device, cancellationToken).ConfigureAwait(false);
            string rebind = allowRebind ? string.Empty : "norebind:";

            await socket.SendAdbRequestAsync($"reverse:forward:{rebind}{remote};{local}", cancellationToken).ConfigureAwait(false);
            _ = await socket.ReadAdbResponseAsync(cancellationToken).ConfigureAwait(false);
            _ = await socket.ReadAdbResponseAsync(cancellationToken).ConfigureAwait(false);

            string portString = await socket.ReadStringAsync(cancellationToken).ConfigureAwait(false);
            return portString != null && int.TryParse(portString, out int port) ? port : 0;
        }

        /// <inheritdoc/>
        public virtual async Task RemoveReverseForwardAsync(DeviceData device, string remote, CancellationToken cancellationToken = default)
        {
            EnsureDevice(device);

            using IAdbSocket socket = adbSocketFactory(EndPoint);
            await socket.SetDeviceAsync(device, cancellationToken).ConfigureAwait(false);

            await socket.SendAdbRequestAsync($"reverse:killforward:{remote}", cancellationToken).ConfigureAwait(false);
            _ = await socket.ReadAdbResponseAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public virtual async Task RemoveAllReverseForwardsAsync(DeviceData device, CancellationToken cancellationToken = default)
        {
            EnsureDevice(device);

            using IAdbSocket socket = adbSocketFactory(EndPoint);
            await socket.SetDeviceAsync(device, cancellationToken).ConfigureAwait(false);

            await socket.SendAdbRequestAsync($"reverse:killforward-all", cancellationToken).ConfigureAwait(false);
            _ = await socket.ReadAdbResponseAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public virtual async Task RemoveForwardAsync(DeviceData device, int localPort, CancellationToken cancellationToken = default)
        {
            EnsureDevice(device);

            using IAdbSocket socket = adbSocketFactory(EndPoint);
            await socket.SendAdbRequestAsync($"host-serial:{device.Serial}:killforward:tcp:{localPort}", cancellationToken).ConfigureAwait(false);
            _ = await socket.ReadAdbResponseAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public virtual async Task RemoveAllForwardsAsync(DeviceData device, CancellationToken cancellationToken = default)
        {
            EnsureDevice(device);

            using IAdbSocket socket = adbSocketFactory(EndPoint);
            await socket.SendAdbRequestAsync($"host-serial:{device.Serial}:killforward-all", cancellationToken).ConfigureAwait(false);
            _ = await socket.ReadAdbResponseAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public virtual async Task<IEnumerable<ForwardData>> ListForwardAsync(DeviceData device, CancellationToken cancellationToken = default)
        {
            EnsureDevice(device);

            using IAdbSocket socket = adbSocketFactory(EndPoint);
            await socket.SendAdbRequestAsync($"host-serial:{device.Serial}:list-forward", cancellationToken).ConfigureAwait(false);
            _ = await socket.ReadAdbResponseAsync(cancellationToken).ConfigureAwait(false);
            string data = await socket.ReadStringAsync(cancellationToken).ConfigureAwait(false);

            string[] parts = data.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            return parts.Select(x => ForwardData.FromString(x));
        }

        /// <inheritdoc/>
        public virtual async Task<IEnumerable<ForwardData>> ListReverseForwardAsync(DeviceData device, CancellationToken cancellationToken = default)
        {
            EnsureDevice(device);

            using IAdbSocket socket = adbSocketFactory(EndPoint);
            await socket.SetDeviceAsync(device, cancellationToken).ConfigureAwait(false);

            await socket.SendAdbRequestAsync($"reverse:list-forward", cancellationToken).ConfigureAwait(false);
            _ = await socket.ReadAdbResponseAsync(cancellationToken).ConfigureAwait(false);
            string data = await socket.ReadStringAsync(cancellationToken).ConfigureAwait(false);

            string[] parts = data.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            return parts.Select(x => ForwardData.FromString(x));
        }

        /// <inheritdoc/>
        public virtual async Task ExecuteServerCommandAsync(string target, string command, Encoding encoding, CancellationToken cancellationToken = default)
        {
            ExceptionExtensions.ThrowIfNull(encoding);
            using IAdbSocket socket = adbSocketFactory(EndPoint);
            await ExecuteServerCommandAsync(target, command, socket, encoding, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task ExecuteServerCommandAsync(string target, string command, IAdbSocket socket, Encoding encoding, CancellationToken cancellationToken = default)
        {
            ExceptionExtensions.ThrowIfNull(encoding);

            StringBuilder request = new();
            if (!StringExtensions.IsNullOrWhiteSpace(target))
            {
                _ = request.AppendFormat("{0}:", target);
            }
            _ = request.Append(command);

            await socket.SendAdbRequestAsync(request.ToString(), cancellationToken);
            await socket.ReadAdbResponseAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public virtual async Task ExecuteRemoteCommandAsync(string command, DeviceData device, Encoding encoding, CancellationToken cancellationToken = default)
        {
            EnsureDevice(device);
            ExceptionExtensions.ThrowIfNull(encoding);

            using IAdbSocket socket = adbSocketFactory(EndPoint);
            await socket.SetDeviceAsync(device, cancellationToken);

            await ExecuteServerCommandAsync("shell", command, socket, encoding, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task ExecuteServerCommandAsync(string target, string command, IShellOutputReceiver receiver, Encoding encoding, CancellationToken cancellationToken = default)
        {
            ExceptionExtensions.ThrowIfNull(encoding);
            using IAdbSocket socket = adbSocketFactory(EndPoint);
            await ExecuteServerCommandAsync(target, command, socket, receiver, encoding, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task ExecuteServerCommandAsync(string target, string command, IAdbSocket socket, IShellOutputReceiver receiver, Encoding encoding, CancellationToken cancellationToken = default)
        {
            ExceptionExtensions.ThrowIfNull(encoding);

            StringBuilder request = new();
            if (!StringExtensions.IsNullOrWhiteSpace(target))
            {
                _ = request.AppendFormat("{0}:", target);
            }
            _ = request.Append(command);

            await socket.SendAdbRequestAsync(request.ToString(), cancellationToken);
            _ = await socket.ReadAdbResponseAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                cancellationToken.Register(socket.Dispose);
                using StreamReader reader = new(socket.GetShellStream(), encoding);
                // Previously, we would loop while reader.Peek() >= 0. Turns out that this would
                // break too soon in certain cases (about every 10 loops, so it appears to be a timing
                // issue). Checking for reader.ReadLine() to return null appears to be much more robust
                // -- one of the integration test fetches output 1000 times and found no truncations.
                while (!cancellationToken.IsCancellationRequested)
                {
                    string? line = await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false);
                    if (line == null) { break; }
                    if (receiver?.AddOutput(line) is false) { break; }
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
        public virtual async Task ExecuteRemoteCommandAsync(string command, DeviceData device, IShellOutputReceiver receiver, Encoding encoding, CancellationToken cancellationToken = default)
        {
            EnsureDevice(device);
            ExceptionExtensions.ThrowIfNull(encoding);

            using IAdbSocket socket = adbSocketFactory(EndPoint);
            await socket.SetDeviceAsync(device, cancellationToken);

            await ExecuteServerCommandAsync("shell", command, socket, receiver, encoding, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<Framebuffer> GetFrameBufferAsync(DeviceData device, CancellationToken cancellationToken = default)
        {
            EnsureDevice(device);

            Framebuffer framebuffer = CreateRefreshableFramebuffer(device);
            await framebuffer.RefreshAsync(true, cancellationToken).ConfigureAwait(false);

            // Convert the framebuffer to an image, and return that.
            return framebuffer;
        }

        /// <inheritdoc/>
        public virtual async Task RunLogServiceAsync(DeviceData device, Action<LogEntry> messageSink, CancellationToken cancellationToken, params LogId[] logNames)
        {
            EnsureDevice(device);
            ExceptionExtensions.ThrowIfNull(messageSink);

            // The 'log' service has been deprecated, see
            // https://android.googlesource.com/platform/system/core/+/7aa39a7b199bb9803d3fd47246ee9530b4a96177
            using IAdbSocket socket = adbSocketFactory(EndPoint);
            await socket.SetDeviceAsync(device, cancellationToken).ConfigureAwait(false);

            StringBuilder request = new StringBuilder().Append("shell:logcat -B");

            foreach (LogId logName in logNames)
            {
                _ = request.AppendFormat(" -b {0}", logName.ToString().ToLowerInvariant());
            }

            await socket.SendAdbRequestAsync(request.ToString(), cancellationToken).ConfigureAwait(false);
            _ = await socket.ReadAdbResponseAsync(cancellationToken).ConfigureAwait(false);

#if NETCOREAPP3_0_OR_GREATER
            await
#endif
            using Stream stream = socket.GetShellStream();
            LogReader reader = new(stream);

            while (!cancellationToken.IsCancellationRequested)
            {
                LogEntry? entry = null;

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
        public virtual async Task RebootAsync(string into, DeviceData device, CancellationToken cancellationToken = default)
        {
            EnsureDevice(device);

            using IAdbSocket socket = adbSocketFactory(EndPoint);
            await socket.SetDeviceAsync(device, cancellationToken).ConfigureAwait(false);

            await socket.SendAdbRequestAsync($"reboot:{into}", cancellationToken).ConfigureAwait(false);
            _ = await socket.ReadAdbResponseAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public virtual async Task<string> PairAsync(DnsEndPoint endpoint, string code, CancellationToken cancellationToken = default)
        {
            ExceptionExtensions.ThrowIfNull(endpoint);

            using IAdbSocket socket = adbSocketFactory(EndPoint);
            await socket.SendAdbRequestAsync($"host:pair:{code}:{endpoint.Host}:{endpoint.Port}", cancellationToken).ConfigureAwait(false);
            _ = await socket.ReadAdbResponseAsync(cancellationToken).ConfigureAwait(false);

            return await socket.ReadStringAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public virtual async Task<string> ConnectAsync(DnsEndPoint endpoint, CancellationToken cancellationToken = default)
        {
            ExceptionExtensions.ThrowIfNull(endpoint);

            using IAdbSocket socket = adbSocketFactory(EndPoint);
            await socket.SendAdbRequestAsync($"host:connect:{endpoint.Host}:{endpoint.Port}", cancellationToken).ConfigureAwait(false);
            _ = await socket.ReadAdbResponseAsync(cancellationToken).ConfigureAwait(false);

            return await socket.ReadStringAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public virtual async Task<string> DisconnectAsync(DnsEndPoint endpoint, CancellationToken cancellationToken = default)
        {
            ExceptionExtensions.ThrowIfNull(endpoint);

            using IAdbSocket socket = adbSocketFactory(EndPoint);
            await socket.SendAdbRequestAsync($"host:disconnect:{endpoint.Host}:{endpoint.Port}", cancellationToken).ConfigureAwait(false);
            _ = await socket.ReadAdbResponseAsync(cancellationToken).ConfigureAwait(false);

            return await socket.ReadStringAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public virtual Task RootAsync(DeviceData device, CancellationToken cancellationToken = default) => RootAsync("root:", device, cancellationToken);

        /// <inheritdoc/>
        public virtual Task UnrootAsync(DeviceData device, CancellationToken cancellationToken = default) => RootAsync("unroot:", device, cancellationToken);

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
            await socket.SetDeviceAsync(device, cancellationToken).ConfigureAwait(false);

            await socket.SendAdbRequestAsync(request, cancellationToken).ConfigureAwait(false);
            _ = await socket.ReadAdbResponseAsync(cancellationToken).ConfigureAwait(false);

            // ADB will send some additional data
            byte[] buffer = new byte[1024];
            int read = await socket.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);

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
                await Extensions.Delay(3000, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <inheritdoc/>
        public virtual async Task InstallAsync(DeviceData device, Stream apk, IProgress<InstallProgressEventArgs>? progress = null, CancellationToken cancellationToken = default, params string[] arguments)
        {
            progress?.Report(new InstallProgressEventArgs(PackageInstallProgressState.Preparing));

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
            _ = requestBuilder.Append($" -S {apk.Length}");

            using IAdbSocket socket = adbSocketFactory(EndPoint);
            await socket.SetDeviceAsync(device, cancellationToken).ConfigureAwait(false);

            await socket.SendAdbRequestAsync(requestBuilder.ToString(), cancellationToken).ConfigureAwait(false);
            _ = await socket.ReadAdbResponseAsync(cancellationToken).ConfigureAwait(false);

            byte[] buffer = new byte[32 * 1024];
            int read = 0;

            long totalBytesToProcess = apk.Length;
            long totalBytesRead = 0;

#if HAS_BUFFERS
            while ((read = await apk.ReadAsync(buffer, cancellationToken).ConfigureAwait(false)) > 0)
            {
                await socket.SendAsync(buffer.AsMemory(0, read), cancellationToken).ConfigureAwait(false);
#else
            while ((read = await apk.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false)) > 0)
            {
                await socket.SendAsync(buffer, read, cancellationToken).ConfigureAwait(false);
#endif
                totalBytesRead += read;
                progress?.Report(new InstallProgressEventArgs(0, 1, totalBytesToProcess == 0L ? 0.0 : totalBytesRead * 100.0 / totalBytesToProcess));
            }
            progress?.Report(new InstallProgressEventArgs(1, 1, 100d));

            progress?.Report(new InstallProgressEventArgs(PackageInstallProgressState.Installing));
            read = await socket.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
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
            progress?.Report(new InstallProgressEventArgs(PackageInstallProgressState.Finished));
        }

        /// <inheritdoc/>
        public virtual async Task InstallMultipleAsync(DeviceData device, IEnumerable<Stream> splitAPKs, string packageName, IProgress<InstallProgressEventArgs>? progress = null, CancellationToken cancellationToken = default, params string[] arguments)
        {
            progress?.Report(new InstallProgressEventArgs(PackageInstallProgressState.Preparing));

            EnsureDevice(device);
            ExceptionExtensions.ThrowIfNull(splitAPKs);
            ExceptionExtensions.ThrowIfNull(packageName);

            if (splitAPKs.Any(apk => apk == null || !apk.CanRead || !apk.CanSeek))
            {
                throw new ArgumentOutOfRangeException(nameof(splitAPKs), "The apk stream must be a readable and seekable stream");
            }

            progress?.Report(new InstallProgressEventArgs(PackageInstallProgressState.CreateSession));
            string session = await InstallCreateAsync(device, packageName, cancellationToken, arguments).ConfigureAwait(false);

            int progressCount = 0;
            int splitAPKsCount = splitAPKs.Count();
            Dictionary<string, double> status = new(splitAPKsCount);
            void OnSyncProgressChanged(string? sender, double args)
            {
                lock (status)
                {
                    if (sender is null)
                    {
                        progressCount++;
                    }
                    else if (sender is string path)
                    {
                        status[path] = args;
                    }
                    progress?.Report(new InstallProgressEventArgs(progressCount, splitAPKsCount, status.Values.Select(x => x / splitAPKsCount).Sum()));
                }
            }

            int i = 0;
            await Extensions.WhenAll(splitAPKs.Select(splitAPK => InstallWriteAsync(device, splitAPK, $"{nameof(splitAPK)}{i++}", session, OnSyncProgressChanged, cancellationToken))).ConfigureAwait(false);

            progress?.Report(new InstallProgressEventArgs(PackageInstallProgressState.Installing));
            await InstallCommitAsync(device, session, cancellationToken).ConfigureAwait(false);
            progress?.Report(new InstallProgressEventArgs(PackageInstallProgressState.Finished));
        }

        /// <inheritdoc/>
        public virtual async Task InstallMultipleAsync(DeviceData device, Stream baseAPK, IEnumerable<Stream> splitAPKs, IProgress<InstallProgressEventArgs>? progress = null, CancellationToken cancellationToken = default, params string[] arguments)
        {
            progress?.Report(new InstallProgressEventArgs(PackageInstallProgressState.Preparing));

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

            progress?.Report(new InstallProgressEventArgs(PackageInstallProgressState.CreateSession));
            string session = await InstallCreateAsync(device, null, cancellationToken, arguments).ConfigureAwait(false);

            int splitAPKsCount = splitAPKs.Count();
            void OnMainSyncProgressChanged(string? sender, double args) =>
                progress?.Report(new InstallProgressEventArgs(sender is null ? 1 : 0, splitAPKsCount + 1, args / 2));

            await InstallWriteAsync(device, baseAPK, nameof(baseAPK), session, OnMainSyncProgressChanged, cancellationToken).ConfigureAwait(false);

            int progressCount = 1;
            Dictionary<string, double> status = new(splitAPKsCount);
            void OnSplitSyncProgressChanged(string? sender, double args)
            {
                lock (status)
                {
                    if (sender is null)
                    {
                        progressCount++;
                    }
                    else if (sender is string path)
                    {
                        status[path] = args;
                    }
                    progress?.Report(new InstallProgressEventArgs(progressCount, splitAPKsCount + 1, (status.Values.Select(x => x / splitAPKsCount).Sum() + 100) / 2));
                }
            }

            int i = 0;
            await Extensions.WhenAll(splitAPKs.Select(splitAPK => InstallWriteAsync(device, splitAPK, $"{nameof(splitAPK)}{i++}", session, OnSplitSyncProgressChanged, cancellationToken))).ConfigureAwait(false);

            progress?.Report(new InstallProgressEventArgs(PackageInstallProgressState.Installing));
            await InstallCommitAsync(device, session, cancellationToken).ConfigureAwait(false);
            progress?.Report(new InstallProgressEventArgs(PackageInstallProgressState.Finished));
        }

        /// <inheritdoc/>
        public virtual async Task<string> InstallCreateAsync(DeviceData device, string? packageName, CancellationToken cancellationToken, params string[] arguments)
        {
            EnsureDevice(device);

            StringBuilder requestBuilder = new StringBuilder().Append("exec:cmd package 'install-create'");

            if (!StringExtensions.IsNullOrWhiteSpace(packageName))
            {
                requestBuilder.AppendFormat(" -p {0}", packageName);
            }

            if (arguments != null)
            {
                foreach (string argument in arguments)
                {
                    _ = requestBuilder.AppendFormat(" {0}", argument);
                }
            }

            using IAdbSocket socket = adbSocketFactory(EndPoint);
            await socket.SetDeviceAsync(device, cancellationToken).ConfigureAwait(false);

            await socket.SendAdbRequestAsync(requestBuilder.ToString(), cancellationToken).ConfigureAwait(false);
            _ = await socket.ReadAdbResponseAsync(cancellationToken).ConfigureAwait(false);

            using StreamReader reader = new(socket.GetShellStream(), Encoding);
            string result = await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false)
                ?? throw new AdbException($"The {nameof(result)} of {nameof(InstallCreateAsync)} is null.");

            if (!result.Contains("Success"))
            {
                throw new AdbException(await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false));
            }

            int arr = result.IndexOf(']') - 1 - result.IndexOf('[');
            string session = result.Substring(result.IndexOf('[') + 1, arr);
            return session;
        }

        /// <inheritdoc/>
        public virtual async Task InstallWriteAsync(DeviceData device, Stream apk, string apkName, string session, IProgress<double>? progress = null, CancellationToken cancellationToken = default)
        {
            progress?.Report(0d);

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

            using IAdbSocket socket = adbSocketFactory(EndPoint);
            await socket.SetDeviceAsync(device, cancellationToken).ConfigureAwait(false);

            await socket.SendAdbRequestAsync(requestBuilder.ToString(), cancellationToken).ConfigureAwait(false);
            _ = await socket.ReadAdbResponseAsync(cancellationToken).ConfigureAwait(false);

            byte[] buffer = new byte[32 * 1024];
            int read = 0;

            long totalBytesToProcess = apk.Length;
            long totalBytesRead = 0;

#if HAS_BUFFERS
            while ((read = await apk.ReadAsync(buffer, cancellationToken).ConfigureAwait(false)) > 0)
            {
                await socket.SendAsync(buffer.AsMemory(0, read), cancellationToken).ConfigureAwait(false);
#else
            while ((read = await apk.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false)) > 0)
            {
                await socket.SendAsync(buffer, read, cancellationToken).ConfigureAwait(false);
#endif
                totalBytesRead += read;
                progress?.Report(totalBytesToProcess == 0L ? 0.0 : totalBytesRead * 100.0 / totalBytesToProcess);
            }

            read = await socket.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
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
            progress?.Report(100d);
        }

        /// <summary>
        /// Asynchronously write an apk into the given install session.
        /// </summary>
        /// <param name="device">The device on which to install the application.</param>
        /// <param name="apk">A <see cref="Stream"/> which represents the application to install.</param>
        /// <param name="apkName">The name of the application.</param>
        /// <param name="session">The session ID of the install session.</param>
        /// <param name="progress">An optional parameter which, when specified, returns progress notifications.
        /// The progress is reported as a value between 0 and 100, representing the percentage of the apk which has been transferred.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        protected virtual async Task InstallWriteAsync(DeviceData device, Stream apk, string apkName, string session, Action<string?, double>? progress, CancellationToken cancellationToken = default)
        {
            progress?.Invoke(apkName, 0d);

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

            using IAdbSocket socket = adbSocketFactory(EndPoint);
            await socket.SetDeviceAsync(device, cancellationToken).ConfigureAwait(false);

            await socket.SendAdbRequestAsync(requestBuilder.ToString(), cancellationToken).ConfigureAwait(false);
            _ = await socket.ReadAdbResponseAsync(cancellationToken).ConfigureAwait(false);

            byte[] buffer = new byte[32 * 1024];
            int read = 0;

            long totalBytesToProcess = apk.Length;
            long totalBytesRead = 0;

#if HAS_BUFFERS
            while ((read = await apk.ReadAsync(buffer, cancellationToken).ConfigureAwait(false)) > 0)
            {
                await socket.SendAsync(buffer.AsMemory(0, read), cancellationToken).ConfigureAwait(false);
#else
            while ((read = await apk.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false)) > 0)
            {
                await socket.SendAsync(buffer, read, cancellationToken).ConfigureAwait(false);
#endif
                totalBytesRead += read;
                progress?.Invoke(apkName, totalBytesToProcess == 0L ? 0.0 : totalBytesRead * 100.0 / totalBytesToProcess);
            }
            progress?.Invoke(apkName, 100d);

            read = await socket.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
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
            progress?.Invoke(null, 100d);
        }

        /// <inheritdoc/>
        public virtual async Task InstallCommitAsync(DeviceData device, string session, CancellationToken cancellationToken = default)
        {
            using IAdbSocket socket = adbSocketFactory(EndPoint);
            await socket.SetDeviceAsync(device, cancellationToken).ConfigureAwait(false);

            await socket.SendAdbRequestAsync($"exec:cmd package 'install-commit' {session}", cancellationToken).ConfigureAwait(false);
            _ = await socket.ReadAdbResponseAsync(cancellationToken).ConfigureAwait(false);

            using StreamReader reader = new(socket.GetShellStream(), Encoding);
            string? result = await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false);
            if (result?.Contains("Success") != true)
            {
                throw new AdbException(await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false));
            }
        }

        /// <inheritdoc/>
        public virtual async Task UninstallAsync(DeviceData device, string packageName, CancellationToken cancellationToken, params string[] arguments)
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

            using IAdbSocket socket = adbSocketFactory(EndPoint);
            await socket.SetDeviceAsync(device, cancellationToken).ConfigureAwait(false);

            await socket.SendAdbRequestAsync(requestBuilder.ToString(), cancellationToken).ConfigureAwait(false);
            _ = await socket.ReadAdbResponseAsync(cancellationToken).ConfigureAwait(false);

            using StreamReader reader = new(socket.GetShellStream(), Encoding);
            string? result = await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false);
            if (result?.Contains("Success") != true)
            {
                throw new AdbException(await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false));
            }
        }

        /// <inheritdoc/>
        public virtual async Task<IEnumerable<string>> GetFeatureSetAsync(DeviceData device, CancellationToken cancellationToken = default)
        {
            EnsureDevice(device);

            using IAdbSocket socket = adbSocketFactory(EndPoint);
            await socket.SendAdbRequestAsync($"host-serial:{device.Serial}:features", cancellationToken).ConfigureAwait(false);
            _ = await socket.ReadAdbResponseAsync(cancellationToken).ConfigureAwait(false);
            string features = await socket.ReadStringAsync(cancellationToken).ConfigureAwait(false);

            IEnumerable<string> featureList = features.Trim().Split('\n', ',');
            return featureList;
        }
    }
}
#endif