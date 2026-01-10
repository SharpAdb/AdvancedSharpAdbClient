#if HAS_TASK
// <copyright file="AdbClient.Async.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace AdvancedSharpAdbClient
{
    public partial class AdbClient
    {
        /// <inheritdoc/>
        public async Task<int> GetAdbVersionAsync(CancellationToken cancellationToken = default)
        {
            using IAdbSocket socket = CreateAdbSocket();

            await socket.SendAdbRequestAsync("host:version", cancellationToken).ConfigureAwait(false);
            _ = await socket.ReadAdbResponseAsync(cancellationToken).ConfigureAwait(false);
            string version = await socket.ReadStringAsync(cancellationToken).ConfigureAwait(false);

            return int.Parse(version, NumberStyles.HexNumber);
        }

        /// <inheritdoc/>
        public async Task KillAdbAsync(CancellationToken cancellationToken = default)
        {
            using IAdbSocket socket = CreateAdbSocket();
            await socket.SendAdbRequestAsync("host:kill", cancellationToken).ConfigureAwait(false);

            // The host will immediately close the connection after the kill
            // command has been sent; no need to read the response.
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<DeviceData>> GetDevicesAsync(CancellationToken cancellationToken = default)
        {
            using IAdbSocket socket = CreateAdbSocket();

            await socket.SendAdbRequestAsync("host:devices-l", cancellationToken).ConfigureAwait(false);
            _ = await socket.ReadAdbResponseAsync(cancellationToken).ConfigureAwait(false);
            string reply = await socket.ReadStringAsync(cancellationToken).ConfigureAwait(false);

            string[] data = reply.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            return data.Select(x => new DeviceData(x));
        }

        /// <inheritdoc/>
        public async Task<int> CreateForwardAsync(DeviceData device, string local, string remote, bool allowRebind, CancellationToken cancellationToken = default)
        {
            EnsureDevice(device);

            using IAdbSocket socket = CreateAdbSocket();
            string rebind = allowRebind ? string.Empty : "norebind:";

            await socket.SendAdbRequestAsync($"host-serial:{device.Serial}:forward:{rebind}{local};{remote}", cancellationToken).ConfigureAwait(false);
            _ = await socket.ReadAdbResponseAsync(cancellationToken).ConfigureAwait(false);
            _ = await socket.ReadAdbResponseAsync(cancellationToken).ConfigureAwait(false);

            string portString = await socket.ReadStringAsync(cancellationToken).ConfigureAwait(false);
            return portString != null && int.TryParse(portString, out int port) ? port : 0;
        }

        /// <inheritdoc/>
        public async Task<int> CreateReverseForwardAsync(DeviceData device, string remote, string local, bool allowRebind, CancellationToken cancellationToken = default)
        {
            EnsureDevice(device);

            using IAdbSocket socket = CreateAdbSocket();

            await socket.SetDeviceAsync(device, cancellationToken).ConfigureAwait(false);
            string rebind = allowRebind ? string.Empty : "norebind:";

            await socket.SendAdbRequestAsync($"reverse:forward:{rebind}{remote};{local}", cancellationToken).ConfigureAwait(false);
            _ = await socket.ReadAdbResponseAsync(cancellationToken).ConfigureAwait(false);
            _ = await socket.ReadAdbResponseAsync(cancellationToken).ConfigureAwait(false);

            string portString = await socket.ReadStringAsync(cancellationToken).ConfigureAwait(false);
            return portString != null && int.TryParse(portString, out int port) ? port : 0;
        }

        /// <inheritdoc/>
        public async Task RemoveReverseForwardAsync(DeviceData device, string remote, CancellationToken cancellationToken = default)
        {
            EnsureDevice(device);

            using IAdbSocket socket = CreateAdbSocket();
            await socket.SetDeviceAsync(device, cancellationToken).ConfigureAwait(false);

            await socket.SendAdbRequestAsync($"reverse:killforward:{remote}", cancellationToken).ConfigureAwait(false);
            _ = await socket.ReadAdbResponseAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task RemoveAllReverseForwardsAsync(DeviceData device, CancellationToken cancellationToken = default)
        {
            EnsureDevice(device);

            using IAdbSocket socket = CreateAdbSocket();
            await socket.SetDeviceAsync(device, cancellationToken).ConfigureAwait(false);

            await socket.SendAdbRequestAsync($"reverse:killforward-all", cancellationToken).ConfigureAwait(false);
            _ = await socket.ReadAdbResponseAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task RemoveForwardAsync(DeviceData device, int localPort, CancellationToken cancellationToken = default)
        {
            EnsureDevice(device);

            using IAdbSocket socket = CreateAdbSocket();
            await socket.SendAdbRequestAsync($"host-serial:{device.Serial}:killforward:tcp:{localPort}", cancellationToken).ConfigureAwait(false);
            _ = await socket.ReadAdbResponseAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task RemoveAllForwardsAsync(DeviceData device, CancellationToken cancellationToken = default)
        {
            EnsureDevice(device);

            using IAdbSocket socket = CreateAdbSocket();
            await socket.SendAdbRequestAsync($"host-serial:{device.Serial}:killforward-all", cancellationToken).ConfigureAwait(false);
            _ = await socket.ReadAdbResponseAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ForwardData>> ListForwardAsync(DeviceData device, CancellationToken cancellationToken = default)
        {
            EnsureDevice(device);

            using IAdbSocket socket = CreateAdbSocket();
            await socket.SendAdbRequestAsync($"host-serial:{device.Serial}:list-forward", cancellationToken).ConfigureAwait(false);
            _ = await socket.ReadAdbResponseAsync(cancellationToken).ConfigureAwait(false);
            string data = await socket.ReadStringAsync(cancellationToken).ConfigureAwait(false);

            string[] parts = data.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            return parts.Select(ForwardData.FromString).OfType<ForwardData>();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ForwardData>> ListReverseForwardAsync(DeviceData device, CancellationToken cancellationToken = default)
        {
            EnsureDevice(device);

            using IAdbSocket socket = CreateAdbSocket();
            await socket.SetDeviceAsync(device, cancellationToken).ConfigureAwait(false);

            await socket.SendAdbRequestAsync($"reverse:list-forward", cancellationToken).ConfigureAwait(false);
            _ = await socket.ReadAdbResponseAsync(cancellationToken).ConfigureAwait(false);
            string data = await socket.ReadStringAsync(cancellationToken).ConfigureAwait(false);

            string[] parts = data.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            return parts.Select(ForwardData.FromString).OfType<ForwardData>();
        }

        /// <inheritdoc/>
        public async Task ExecuteServerCommandAsync(string target, string command, CancellationToken cancellationToken = default)
        {
            using IAdbSocket socket = CreateAdbSocket();
            await ExecuteServerCommandAsync(target, command, socket, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task ExecuteServerCommandAsync(string target, string command, IAdbSocket socket, CancellationToken cancellationToken = default)
        {
            DefaultInterpolatedStringHandler request = new(1, 2);
            if (!string.IsNullOrWhiteSpace(target))
            {
                request.AppendLiteral(target);
                request.AppendFormatted(':');
            }
            request.AppendLiteral(command);

            await socket.SendAdbRequestAsync(request.ToStringAndClear(), cancellationToken);
            await socket.ReadAdbResponseAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                using StreamReader reader = new(socket.GetShellStream());
                // Previously, we would loop while reader.Peek() >= 0. Turns out that this would
                // break too soon in certain cases (about every 10 loops, so it appears to be a timing
                // issue). Checking for reader.ReadLine() to return null appears to be much more robust
                // -- one of the integration test fetches output 1000 times and found no truncations.
                while (!cancellationToken.IsCancellationRequested)
                {
                    if (await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false) == null) { break; }
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
        }

        /// <inheritdoc/>
        public async Task ExecuteRemoteCommandAsync(string command, DeviceData device, CancellationToken cancellationToken = default)
        {
            EnsureDevice(device);

            using IAdbSocket socket = CreateAdbSocket();
            await socket.SetDeviceAsync(device, cancellationToken);

            await ExecuteServerCommandAsync("shell", command, socket, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task ExecuteServerCommandAsync(string target, string command, IShellOutputReceiver? receiver, Encoding encoding, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(encoding);
            using IAdbSocket socket = CreateAdbSocket();
            await ExecuteServerCommandAsync(target, command, socket, receiver, encoding, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task ExecuteServerCommandAsync(string target, string command, IAdbSocket socket, IShellOutputReceiver? receiver, Encoding encoding, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(encoding);

            DefaultInterpolatedStringHandler request = new(1, 2);
            if (!string.IsNullOrWhiteSpace(target))
            {
                request.AppendLiteral(target);
                request.AppendFormatted(':');
            }
            request.AppendLiteral(command);

            await socket.SendAdbRequestAsync(request.ToStringAndClear(), cancellationToken);
            _ = await socket.ReadAdbResponseAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                using StreamReader reader = new(socket.GetShellStream(), encoding);
                // Previously, we would loop while reader.Peek() >= 0. Turns out that this would
                // break too soon in certain cases (about every 10 loops, so it appears to be a timing
                // issue). Checking for reader.ReadLine() to return null appears to be much more robust
                // -- one of the integration test fetches output 1000 times and found no truncations.
                while (!cancellationToken.IsCancellationRequested)
                {
                    string? line = await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false);
                    if (line == null) { break; }
                    if (receiver != null && await receiver.AddOutputAsync(line, cancellationToken) == false) { break; }
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
        public async Task ExecuteRemoteCommandAsync(string command, DeviceData device, IShellOutputReceiver? receiver, Encoding encoding, CancellationToken cancellationToken = default)
        {
            EnsureDevice(device);
            ArgumentNullException.ThrowIfNull(encoding);

            using IAdbSocket socket = CreateAdbSocket();
            await socket.SetDeviceAsync(device, cancellationToken);

            await ExecuteServerCommandAsync("shell", command, socket, receiver, encoding, cancellationToken);
        }

#if COMP_NETSTANDARD2_1
        /// <inheritdoc/>
        public async IAsyncEnumerable<string> ExecuteServerEnumerableAsync(string target, string command, Encoding encoding, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(encoding);
            using IAdbSocket socket = CreateAdbSocket();
            await foreach (string? line in ExecuteServerEnumerableAsync(target, command, socket, encoding, cancellationToken).ConfigureAwait(false))
            {
                yield return line;
            }
        }

        /// <inheritdoc/>
        public virtual async IAsyncEnumerable<string> ExecuteServerEnumerableAsync(string target, string command, IAdbSocket socket, Encoding encoding, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(encoding);

            DefaultInterpolatedStringHandler request = new(1, 2);
            if (!string.IsNullOrWhiteSpace(target))
            {
                request.AppendLiteral(target);
                request.AppendFormatted(':');
            }
            request.AppendLiteral(command);

            await socket.SendAdbRequestAsync(request.ToStringAndClear(), cancellationToken);
            _ = await socket.ReadAdbResponseAsync(cancellationToken).ConfigureAwait(false);

            using StreamReader reader = new(socket.GetShellStream(), encoding);
            // Previously, we would loop while reader.Peek() >= 0. Turns out that this would
            // break too soon in certain cases (about every 10 loops, so it appears to be a timing
            // issue). Checking for reader.ReadLine() to return null appears to be much more robust
            // -- one of the integration test fetches output 1000 times and found no truncations.
            while (!cancellationToken.IsCancellationRequested)
            {
                string? line = null;
                try
                {
                    line = await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false);
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
                if (line == null) { yield break; }
                yield return line;
            }
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<string> ExecuteRemoteEnumerableAsync(string command, DeviceData device, Encoding encoding, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            EnsureDevice(device);
            ArgumentNullException.ThrowIfNull(encoding);

            using IAdbSocket socket = CreateAdbSocket();
            await socket.SetDeviceAsync(device, cancellationToken);

            await foreach (string? line in ExecuteServerEnumerableAsync("shell", command, socket, encoding, cancellationToken).ConfigureAwait(false))
            {
                yield return line;
            }
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<LogEntry> RunLogServiceAsync(DeviceData device, [EnumeratorCancellation] CancellationToken cancellationToken = default, params LogId[] logNames)
        {
            EnsureDevice(device);

            // The 'log' service has been deprecated, see
            // https://android.googlesource.com/platform/system/core/+/7aa39a7b199bb9803d3fd47246ee9530b4a96177
            using IAdbSocket socket = CreateAdbSocket();
            await socket.SetDeviceAsync(device, cancellationToken).ConfigureAwait(false);

            DefaultInterpolatedStringHandler request = new(19, logNames.Length);
            request.AppendLiteral("shell:logcat -B");

            foreach (LogId logName in logNames)
            {
                request.AppendLiteral(" -b ");
                request.AppendLiteral(logName.ToString().ToLowerInvariant());
            }

            await socket.SendAdbRequestAsync(request.ToStringAndClear(), cancellationToken).ConfigureAwait(false);
            _ = await socket.ReadAdbResponseAsync(cancellationToken).ConfigureAwait(false);

#if COMP_NETSTANDARD2_1
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
                    yield return entry;
                }
                else
                {
                    yield break;
                }
            }
        }
#endif

        /// <inheritdoc/>
        public async Task RunLogServiceAsync(DeviceData device, Action<LogEntry> messageSink, CancellationToken cancellationToken = default, params LogId[] logNames)
        {
            EnsureDevice(device);
            ArgumentNullException.ThrowIfNull(messageSink);

            // The 'log' service has been deprecated, see
            // https://android.googlesource.com/platform/system/core/+/7aa39a7b199bb9803d3fd47246ee9530b4a96177
            using IAdbSocket socket = CreateAdbSocket();
            await socket.SetDeviceAsync(device, cancellationToken).ConfigureAwait(false);

            DefaultInterpolatedStringHandler request = new(19, logNames.Length);
            request.AppendLiteral("shell:logcat -B");

            foreach (LogId logName in logNames)
            {
                request.AppendLiteral(" -b ");
                request.AppendLiteral(logName.ToString().ToLowerInvariant());
            }

            await socket.SendAdbRequestAsync(request.ToStringAndClear(), cancellationToken).ConfigureAwait(false);
            _ = await socket.ReadAdbResponseAsync(cancellationToken).ConfigureAwait(false);

#if COMP_NETSTANDARD2_1
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
        public async Task<Framebuffer> GetFrameBufferAsync(DeviceData device, CancellationToken cancellationToken = default)
        {
            EnsureDevice(device);

            Framebuffer framebuffer = CreateFramebuffer(device);
            await framebuffer.RefreshAsync(true, cancellationToken).ConfigureAwait(false);

            // Convert the framebuffer to an image, and return that.
            return framebuffer;
        }

        /// <inheritdoc/>
        public async Task RebootAsync(string into, DeviceData device, CancellationToken cancellationToken = default)
        {
            EnsureDevice(device);

            using IAdbSocket socket = CreateAdbSocket();
            await socket.SetDeviceAsync(device, cancellationToken).ConfigureAwait(false);

            await socket.SendAdbRequestAsync($"reboot:{into}", cancellationToken).ConfigureAwait(false);
            _ = await socket.ReadAdbResponseAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<string> PairAsync(string host, int port, string code, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(host);

            using IAdbSocket socket = CreateAdbSocket();
            string address = host.Contains(':') ? host : $"{host}:{port}";
            await socket.SendAdbRequestAsync($"host:pair:{code}:{address}", cancellationToken).ConfigureAwait(false);
            _ = await socket.ReadAdbResponseAsync(cancellationToken).ConfigureAwait(false);

            return await socket.ReadStringAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<string> ConnectAsync(string host, int port = DefaultPort, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(host);

            using IAdbSocket socket = CreateAdbSocket();
            string address = host.Contains(':') ? host : $"{host}:{port}";
            await socket.SendAdbRequestAsync($"host:connect:{address}", cancellationToken).ConfigureAwait(false);
            _ = await socket.ReadAdbResponseAsync(cancellationToken).ConfigureAwait(false);

            return await socket.ReadStringAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<string> DisconnectAsync(string host, int port = DefaultPort, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(host);

            using IAdbSocket socket = CreateAdbSocket();
            string address = host.Contains(':') ? host : $"{host}:{port}";
            await socket.SendAdbRequestAsync($"host:disconnect:{address}", cancellationToken).ConfigureAwait(false);
            _ = await socket.ReadAdbResponseAsync(cancellationToken).ConfigureAwait(false);

            return await socket.ReadStringAsync(cancellationToken).ConfigureAwait(false);
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
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        protected async Task RootAsync(string request, DeviceData device, CancellationToken cancellationToken = default)
        {
            EnsureDevice(device);

            using IAdbSocket socket = CreateAdbSocket();
            await socket.SetDeviceAsync(device, cancellationToken).ConfigureAwait(false);

            await socket.SendAdbRequestAsync(request, cancellationToken).ConfigureAwait(false);
            _ = await socket.ReadAdbResponseAsync(cancellationToken).ConfigureAwait(false);

            // ADB will send some additional data
            byte[] buffer = new byte[1024];
            int read = await socket.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);

            string responseMessage =
#if HAS_BUFFERS
                Encoding.GetString(buffer.AsSpan(0, read));
#else
                Encoding.GetString(buffer, 0, read);
#endif

            // see https://android.googlesource.com/platform/packages/modules/adb/+/refs/heads/main/daemon/restart_service.cpp
            // for possible return strings
            if (!responseMessage.Contains("restarting", StringComparison.OrdinalIgnoreCase))
            {
                throw new AdbException(responseMessage);
            }
            else
            {
                // Give adbd some time to kill itself and come back up.
                // We can't use wait-for-device because devices (e.g. adb over network) might not come back.
                await Task.Delay(3000, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <inheritdoc/>
        public virtual async Task InstallAsync(DeviceData device, Stream apk, Action<InstallProgressEventArgs>? callback = null, CancellationToken cancellationToken = default, params string[] arguments)
        {
            callback?.Invoke(new InstallProgressEventArgs(PackageInstallProgressState.Preparing));

            EnsureDevice(device);
            ArgumentNullException.ThrowIfNull(apk);

            if (!apk.CanRead || !apk.CanSeek)
            {
                throw new ArgumentOutOfRangeException(nameof(apk), "The apk stream must be a readable and seekable stream");
            }

            using IAdbSocket socket = CreateAdbSocket();
            await socket.SetDeviceAsync(device, cancellationToken).ConfigureAwait(false);

            DefaultInterpolatedStringHandler requestBuilder = new(31, (arguments?.Length ?? 0) + 1);
            requestBuilder.AppendLiteral("exec:cmd package 'install'");

            if (arguments != null)
            {
                foreach (string argument in arguments)
                {
                    requestBuilder.AppendFormatted(' ');
                    requestBuilder.AppendLiteral(argument);
                }
            }

            // add size parameter [required for streaming installs]
            // do last to override any user specified value
            requestBuilder.AppendLiteral(" -S ");
            requestBuilder.AppendFormatted(apk.Length);

            await socket.SendAdbRequestAsync(requestBuilder.ToStringAndClear(), cancellationToken).ConfigureAwait(false);
            _ = await socket.ReadAdbResponseAsync(cancellationToken).ConfigureAwait(false);

            byte[] buffer = new byte[32 * 1024];
            int read;

            long totalBytesToProcess = apk.Length;
            long totalBytesRead = 0;

#if HAS_BUFFERS
            while ((read = await apk.ReadAsync(buffer, cancellationToken).ConfigureAwait(false)) > 0)
#else
            while ((read = await apk.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false)) > 0)
#endif
            {
#if COMP_NETSTANDARD2_1
                await socket.SendAsync(buffer.AsMemory(0, read), cancellationToken).ConfigureAwait(false);
#else
                await socket.SendAsync(buffer, read, cancellationToken).ConfigureAwait(false);
#endif
                totalBytesRead += read;
                callback?.Invoke(new InstallProgressEventArgs(0, 1, totalBytesToProcess == 0 ? 0 : totalBytesRead * 100d / totalBytesToProcess));
            }
            callback?.Invoke(new InstallProgressEventArgs(1, 1, 100));

            callback?.Invoke(new InstallProgressEventArgs(PackageInstallProgressState.Installing));
            read = await socket.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
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
        public async Task InstallMultipleAsync(DeviceData device, Stream baseAPK, IEnumerable<Stream> splitAPKs, Action<InstallProgressEventArgs>? callback = null, CancellationToken cancellationToken = default, params string[] arguments)
        {
            callback?.Invoke(new InstallProgressEventArgs(PackageInstallProgressState.Preparing));

            EnsureDevice(device);
            ArgumentNullException.ThrowIfNull(baseAPK);
            ArgumentNullException.ThrowIfNull(splitAPKs);

            if (!baseAPK.CanRead || !baseAPK.CanSeek)
            {
                throw new ArgumentOutOfRangeException(nameof(baseAPK), "The apk stream must be a readable and seekable stream");
            }

            if (splitAPKs.Any(apk => apk == null || !apk.CanRead || !apk.CanSeek))
            {
                throw new ArgumentOutOfRangeException(nameof(splitAPKs), "The apk stream must be a readable and seekable stream");
            }

            callback?.Invoke(new InstallProgressEventArgs(PackageInstallProgressState.CreateSession));
            string session = await InstallCreateAsync(device, cancellationToken, arguments).ConfigureAwait(false);

            int splitAPKsCount = splitAPKs.Count();
            void OnMainSyncProgressChanged(string? sender, double args) =>
                callback?.Invoke(new InstallProgressEventArgs(sender == null ? 1 : 0, splitAPKsCount + 1, args / 2));

            await InstallWriteAsync(device, baseAPK, nameof(baseAPK), session, OnMainSyncProgressChanged, cancellationToken).ConfigureAwait(false);

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

            await splitAPKs.Select((splitAPK, index) => InstallWriteAsync(device, splitAPK, $"{nameof(splitAPK)}{index}", session, OnSplitSyncProgressChanged, cancellationToken)).WhenAll().ConfigureAwait(false);

            callback?.Invoke(new InstallProgressEventArgs(PackageInstallProgressState.Installing));
            await InstallCommitAsync(device, session, cancellationToken).ConfigureAwait(false);
            callback?.Invoke(new InstallProgressEventArgs(PackageInstallProgressState.Finished));
        }

        /// <inheritdoc/>
        public async Task InstallMultipleAsync(DeviceData device, IEnumerable<Stream> splitAPKs, string packageName, Action<InstallProgressEventArgs>? callback = null, CancellationToken cancellationToken = default, params string[] arguments)
        {
            callback?.Invoke(new InstallProgressEventArgs(PackageInstallProgressState.Preparing));

            EnsureDevice(device);
            ArgumentNullException.ThrowIfNull(splitAPKs);
            ArgumentNullException.ThrowIfNull(packageName);

            if (splitAPKs.Any(apk => apk == null || !apk.CanRead || !apk.CanSeek))
            {
                throw new ArgumentOutOfRangeException(nameof(splitAPKs), "The apk stream must be a readable and seekable stream");
            }

            callback?.Invoke(new InstallProgressEventArgs(PackageInstallProgressState.CreateSession));
            string session = await InstallCreateAsync(device, packageName, cancellationToken, arguments).ConfigureAwait(false);

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

            await splitAPKs.Select((splitAPK, index) => InstallWriteAsync(device, splitAPK, $"{nameof(splitAPK)}{index}", session, OnSyncProgressChanged, cancellationToken)).WhenAll().ConfigureAwait(false);

            callback?.Invoke(new InstallProgressEventArgs(PackageInstallProgressState.Installing));
            await InstallCommitAsync(device, session, cancellationToken).ConfigureAwait(false);
            callback?.Invoke(new InstallProgressEventArgs(PackageInstallProgressState.Finished));
        }

        /// <inheritdoc/>
        public async Task<string> InstallCreateAsync(DeviceData device, CancellationToken cancellationToken = default, params string[] arguments)
        {
            EnsureDevice(device);

            using IAdbSocket socket = CreateAdbSocket();
            await socket.SetDeviceAsync(device, cancellationToken).ConfigureAwait(false);

            DefaultInterpolatedStringHandler requestBuilder = new(33, arguments?.Length ?? 0);
            requestBuilder.AppendLiteral("exec:cmd package 'install-create'");

            if (arguments != null)
            {
                foreach (string argument in arguments)
                {
                    requestBuilder.AppendFormatted(' ');
                    requestBuilder.AppendLiteral(argument);
                }
            }

            await socket.SendAdbRequestAsync(requestBuilder.ToStringAndClear(), cancellationToken).ConfigureAwait(false);
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
        public async Task<string> InstallCreateAsync(DeviceData device, string packageName, CancellationToken cancellationToken = default, params string[] arguments)
        {
            EnsureDevice(device);
            ArgumentNullException.ThrowIfNull(packageName);

            using IAdbSocket socket = CreateAdbSocket();
            await socket.SetDeviceAsync(device, cancellationToken).ConfigureAwait(false);

            DefaultInterpolatedStringHandler requestBuilder = new(37 + packageName.Length, arguments?.Length ?? 0);
            requestBuilder.AppendLiteral("exec:cmd package 'install-create'");
            requestBuilder.AppendFormatted(" -p ");
            requestBuilder.AppendLiteral(packageName);

            if (arguments != null)
            {
                foreach (string argument in arguments)
                {
                    requestBuilder.AppendFormatted(' ');
                    requestBuilder.AppendLiteral(argument);
                }
            }

            await socket.SendAdbRequestAsync(requestBuilder.ToStringAndClear(), cancellationToken).ConfigureAwait(false);
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
        public virtual async Task InstallWriteAsync(DeviceData device, Stream apk, string apkName, string session, Action<double>? callback = null, CancellationToken cancellationToken = default)
        {
            callback?.Invoke(0);

            EnsureDevice(device);
            ArgumentNullException.ThrowIfNull(apk);
            ArgumentNullException.ThrowIfNull(apkName);
            ArgumentNullException.ThrowIfNull(session);

            if (!apk.CanRead || !apk.CanSeek)
            {
                throw new ArgumentOutOfRangeException(nameof(apk), "The apk stream must be a readable and seekable stream");
            }

            using IAdbSocket socket = CreateAdbSocket();
            await socket.SetDeviceAsync(device, cancellationToken).ConfigureAwait(false);

            await socket.SendAdbRequestAsync($"exec:cmd package 'install-write' -S {apk.Length} {session} {apkName}.apk", cancellationToken).ConfigureAwait(false);
            _ = await socket.ReadAdbResponseAsync(cancellationToken).ConfigureAwait(false);

            byte[] buffer = new byte[32 * 1024];
            int read;

            long totalBytesToProcess = apk.Length;
            long totalBytesRead = 0;

#if HAS_BUFFERS
            while ((read = await apk.ReadAsync(buffer, cancellationToken).ConfigureAwait(false)) > 0)
#else
            while ((read = await apk.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false)) > 0)
#endif
            {
#if COMP_NETSTANDARD2_1
                await socket.SendAsync(buffer.AsMemory(0, read), cancellationToken).ConfigureAwait(false);
#else
                await socket.SendAsync(buffer, read, cancellationToken).ConfigureAwait(false);
#endif
                totalBytesRead += read;
                callback?.Invoke(totalBytesToProcess == 0 ? 0 : totalBytesRead * 100d / totalBytesToProcess);
            }

            read = await socket.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
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
        protected virtual async Task InstallWriteAsync(DeviceData device, Stream apk, string apkName, string session, Action<string?, double>? callback, CancellationToken cancellationToken = default)
        {
            callback?.Invoke(apkName, 0);

            EnsureDevice(device);
            ArgumentNullException.ThrowIfNull(apk);
            ArgumentNullException.ThrowIfNull(apkName);
            ArgumentNullException.ThrowIfNull(session);

            if (!apk.CanRead || !apk.CanSeek)
            {
                throw new ArgumentOutOfRangeException(nameof(apk), "The apk stream must be a readable and seekable stream");
            }

            using IAdbSocket socket = CreateAdbSocket();
            await socket.SetDeviceAsync(device, cancellationToken).ConfigureAwait(false);

            await socket.SendAdbRequestAsync($"exec:cmd package 'install-write' -S {apk.Length} {session} {apkName}.apk", cancellationToken).ConfigureAwait(false);
            _ = await socket.ReadAdbResponseAsync(cancellationToken).ConfigureAwait(false);

            byte[] buffer = new byte[32 * 1024];
            int read;

            long totalBytesToProcess = apk.Length;
            long totalBytesRead = 0;

#if COMP_NETSTANDARD2_1
            while ((read = await apk.ReadAsync(buffer, cancellationToken).ConfigureAwait(false)) > 0)
            {
                await socket.SendAsync(buffer.AsMemory(0, read), cancellationToken).ConfigureAwait(false);
#else
            while ((read = await apk.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false)) > 0)
            {
                await socket.SendAsync(buffer, read, cancellationToken).ConfigureAwait(false);
#endif
                totalBytesRead += read;
                callback?.Invoke(apkName, totalBytesToProcess == 0 ? 0 : totalBytesRead * 100d / totalBytesToProcess);
            }
            callback?.Invoke(apkName, 100);

            read = await socket.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
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
        public async Task InstallCommitAsync(DeviceData device, string session, CancellationToken cancellationToken = default)
        {
            using IAdbSocket socket = CreateAdbSocket();
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

#if HAS_WINRT
        /// <inheritdoc/>
        public virtual async Task InstallAsync(DeviceData device, IRandomAccessStream apk, Action<InstallProgressEventArgs>? callback = null, CancellationToken cancellationToken = default, params string[] arguments)
        {
            callback?.Invoke(new InstallProgressEventArgs(PackageInstallProgressState.Preparing));

            EnsureDevice(device);
            ArgumentNullException.ThrowIfNull(apk);

            if (!apk.CanRead)
            {
                throw new ArgumentOutOfRangeException(nameof(apk), "The apk stream must be a readable stream");
            }

            using IAdbSocket socket = CreateAdbSocket();
            await socket.SetDeviceAsync(device, cancellationToken).ConfigureAwait(false);

            DefaultInterpolatedStringHandler requestBuilder = new(30, (arguments?.Length ?? 0) + 1);
            requestBuilder.AppendLiteral("exec:cmd package 'install'");

            if (arguments != null)
            {
                foreach (string argument in arguments)
                {
                    requestBuilder.AppendFormatted(' ');
                    requestBuilder.AppendLiteral(argument);
                }
            }

            // add size parameter [required for streaming installs]
            // do last to override any user specified value
            requestBuilder.AppendLiteral(" -S ");
            requestBuilder.AppendFormatted(apk.Size);

            await socket.SendAdbRequestAsync(requestBuilder.ToStringAndClear(), cancellationToken).ConfigureAwait(false);
            _ = await socket.ReadAdbResponseAsync(cancellationToken).ConfigureAwait(false);

            byte[] buffer = new byte[32 * 1024];

            ulong totalBytesToProcess = apk.Size;
            ulong totalBytesRead = 0;

            while (true)
            {
                IBuffer results = await apk.ReadAsync(buffer.AsBuffer(), (uint)buffer.Length, InputStreamOptions.None).AsTask(cancellationToken).ConfigureAwait(false);
                if (results.Length == 0) { break; }
#if HAS_BUFFERS
                await socket.SendAsync(buffer.AsMemory(0, (int)results.Length), cancellationToken).ConfigureAwait(false);
#else
                await socket.SendAsync(buffer, (int)results.Length, cancellationToken).ConfigureAwait(false);
#endif
                totalBytesRead += results.Length;
                callback?.Invoke(new InstallProgressEventArgs(0, 1, totalBytesToProcess == 0 ? 0 : totalBytesRead * 100d / totalBytesToProcess));
            }
            callback?.Invoke(new InstallProgressEventArgs(1, 1, 100));

            callback?.Invoke(new InstallProgressEventArgs(PackageInstallProgressState.Installing));
            int read = await socket.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
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
        public async Task InstallMultipleAsync(DeviceData device, IRandomAccessStream baseAPK, IEnumerable<IRandomAccessStream> splitAPKs, Action<InstallProgressEventArgs>? callback = null, CancellationToken cancellationToken = default, params string[] arguments)
        {
            callback?.Invoke(new InstallProgressEventArgs(PackageInstallProgressState.Preparing));

            EnsureDevice(device);
            ArgumentNullException.ThrowIfNull(baseAPK);
            ArgumentNullException.ThrowIfNull(splitAPKs);

            if (!baseAPK.CanRead)
            {
                throw new ArgumentOutOfRangeException(nameof(baseAPK), "The apk stream must be a readable stream");
            }

            if (splitAPKs.Any(apk => apk == null || !apk.CanRead))
            {
                throw new ArgumentOutOfRangeException(nameof(splitAPKs), "The apk stream must be a readable stream");
            }

            callback?.Invoke(new InstallProgressEventArgs(PackageInstallProgressState.CreateSession));
            string session = await InstallCreateAsync(device, cancellationToken, arguments).ConfigureAwait(false);

            int splitAPKsCount = splitAPKs.Count();
            void OnMainSyncProgressChanged(string? sender, double args) =>
                callback?.Invoke(new InstallProgressEventArgs(sender == null ? 1 : 0, splitAPKsCount + 1, args / 2));

            await InstallWriteAsync(device, baseAPK, nameof(baseAPK), session, OnMainSyncProgressChanged, cancellationToken).ConfigureAwait(false);

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

            await splitAPKs.Select((splitAPK, index) => InstallWriteAsync(device, splitAPK, $"{nameof(splitAPK)}{index}", session, OnSplitSyncProgressChanged, cancellationToken)).WhenAll().ConfigureAwait(false);

            callback?.Invoke(new InstallProgressEventArgs(PackageInstallProgressState.Installing));
            await InstallCommitAsync(device, session, cancellationToken).ConfigureAwait(false);
            callback?.Invoke(new InstallProgressEventArgs(PackageInstallProgressState.Finished));
        }

        /// <inheritdoc/>
        public async Task InstallMultipleAsync(DeviceData device, IEnumerable<IRandomAccessStream> splitAPKs, string packageName, Action<InstallProgressEventArgs>? callback = null, CancellationToken cancellationToken = default, params string[] arguments)
        {
            callback?.Invoke(new InstallProgressEventArgs(PackageInstallProgressState.Preparing));

            EnsureDevice(device);
            ArgumentNullException.ThrowIfNull(splitAPKs);
            ArgumentNullException.ThrowIfNull(packageName);

            if (splitAPKs.Any(apk => apk == null || !apk.CanRead))
            {
                throw new ArgumentOutOfRangeException(nameof(splitAPKs), "The apk stream must be a readable stream");
            }

            callback?.Invoke(new InstallProgressEventArgs(PackageInstallProgressState.CreateSession));
            string session = await InstallCreateAsync(device, packageName, cancellationToken, arguments).ConfigureAwait(false);

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

            await splitAPKs.Select((splitAPK, index) => InstallWriteAsync(device, splitAPK, $"{nameof(splitAPK)}{index}", session, OnSyncProgressChanged, cancellationToken)).WhenAll().ConfigureAwait(false);

            callback?.Invoke(new InstallProgressEventArgs(PackageInstallProgressState.Installing));
            await InstallCommitAsync(device, session, cancellationToken).ConfigureAwait(false);
            callback?.Invoke(new InstallProgressEventArgs(PackageInstallProgressState.Finished));
        }

        /// <inheritdoc/>
        public virtual async Task InstallWriteAsync(DeviceData device, IRandomAccessStream apk, string apkName, string session, Action<double>? callback = null, CancellationToken cancellationToken = default)
        {
            callback?.Invoke(0);

            EnsureDevice(device);
            ArgumentNullException.ThrowIfNull(apk);
            ArgumentNullException.ThrowIfNull(apkName);
            ArgumentNullException.ThrowIfNull(session);

            if (!apk.CanRead)
            {
                throw new ArgumentOutOfRangeException(nameof(apk), "The apk stream must be a readable stream");
            }

            using IAdbSocket socket = CreateAdbSocket();
            await socket.SetDeviceAsync(device, cancellationToken).ConfigureAwait(false);

            await socket.SendAdbRequestAsync($"exec:cmd package 'install-write' -S {apk.Size} {session} {apkName}.apk", cancellationToken).ConfigureAwait(false);
            _ = await socket.ReadAdbResponseAsync(cancellationToken).ConfigureAwait(false);

            byte[] buffer = new byte[32 * 1024];

            ulong totalBytesToProcess = apk.Size;
            ulong totalBytesRead = 0;

            while (true)
            {
                IBuffer results = await apk.ReadAsync(buffer.AsBuffer(), (uint)buffer.Length, InputStreamOptions.None).AsTask(cancellationToken).ConfigureAwait(false);
                if (results.Length == 0) { break; }
#if HAS_BUFFERS
                await socket.SendAsync(buffer.AsMemory(0, (int)results.Length), cancellationToken).ConfigureAwait(false);
#else
                await socket.SendAsync(buffer, (int)results.Length, cancellationToken).ConfigureAwait(false);
#endif
                totalBytesRead += results.Length;
                callback?.Invoke(totalBytesToProcess == 0 ? 0 : totalBytesRead * 100d / totalBytesToProcess);
            }

            int read = await socket.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
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
        protected virtual async Task InstallWriteAsync(DeviceData device, IRandomAccessStream apk, string apkName, string session, Action<string?, double>? callback, CancellationToken cancellationToken = default)
        {
            callback?.Invoke(apkName, 0);

            EnsureDevice(device);
            ArgumentNullException.ThrowIfNull(apk);
            ArgumentNullException.ThrowIfNull(apkName);
            ArgumentNullException.ThrowIfNull(session);

            if (!apk.CanRead)
            {
                throw new ArgumentOutOfRangeException(nameof(apk), "The apk stream must be a readable stream");
            }

            using IAdbSocket socket = CreateAdbSocket();
            await socket.SetDeviceAsync(device, cancellationToken).ConfigureAwait(false);

            await socket.SendAdbRequestAsync($"exec:cmd package 'install-write' -S {apk.Size} {session} {apkName}.apk", cancellationToken).ConfigureAwait(false);
            _ = await socket.ReadAdbResponseAsync(cancellationToken).ConfigureAwait(false);

            byte[] buffer = new byte[32 * 1024];

            ulong totalBytesToProcess = apk.Size;
            ulong totalBytesRead = 0;

            while (true)
            {
                IBuffer results = await apk.ReadAsync(buffer.AsBuffer(), (uint)buffer.Length, InputStreamOptions.None).AsTask(cancellationToken).ConfigureAwait(false);
                if (results.Length == 0) { break; }
#if HAS_BUFFERS
                await socket.SendAsync(buffer.AsMemory(0, (int)results.Length), cancellationToken).ConfigureAwait(false);
#else
                await socket.SendAsync(buffer, (int)results.Length, cancellationToken).ConfigureAwait(false);
#endif
                totalBytesRead += results.Length;
                callback?.Invoke(apkName, totalBytesToProcess == 0 ? 0 : totalBytesRead * 100d / totalBytesToProcess);
            }
            callback?.Invoke(apkName, 100);

            int read = await socket.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
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
#endif

        /// <inheritdoc/>
        public async Task UninstallAsync(DeviceData device, string packageName, CancellationToken cancellationToken = default, params string[] arguments)
        {
            EnsureDevice(device);

            using IAdbSocket socket = CreateAdbSocket();
            await socket.SetDeviceAsync(device, cancellationToken).ConfigureAwait(false);

            DefaultInterpolatedStringHandler requestBuilder = new(29, (arguments?.Length ?? 0) + 1);
            requestBuilder.AppendLiteral("exec:cmd package 'uninstall'");

            if (arguments != null)
            {
                foreach (string argument in arguments)
                {
                    requestBuilder.AppendFormatted(' ');
                    requestBuilder.AppendLiteral(argument);
                }
            }

            requestBuilder.AppendFormatted(' ');
            requestBuilder.AppendLiteral(packageName);

            await socket.SendAdbRequestAsync(requestBuilder.ToStringAndClear(), cancellationToken).ConfigureAwait(false);
            _ = await socket.ReadAdbResponseAsync(cancellationToken).ConfigureAwait(false);

            using StreamReader reader = new(socket.GetShellStream(), Encoding);
            string? result = await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false);
            if (result?.Contains("Success") != true)
            {
                throw new AdbException(await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false));
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<string>> GetFeatureSetAsync(DeviceData device, CancellationToken cancellationToken = default)
        {
            EnsureDevice(device);

            using IAdbSocket socket = CreateAdbSocket();
            await socket.SendAdbRequestAsync($"host-serial:{device.Serial}:features", cancellationToken).ConfigureAwait(false);
            _ = await socket.ReadAdbResponseAsync(cancellationToken).ConfigureAwait(false);
            string features = await socket.ReadStringAsync(cancellationToken).ConfigureAwait(false);

            IEnumerable<string> featureList = features.Trim().Split('\n', ',');
            return featureList;
        }
    }
}
#endif