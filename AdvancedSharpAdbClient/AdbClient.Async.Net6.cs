using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using AdvancedSharpAdbClient.Exceptions;
using AdvancedSharpAdbClient.Logs;

namespace AdvancedSharpAdbClient;

#if NET6_0_OR_GREATER
public partial class AdbClient
{
        /// <inheritdoc/>
        public async Task<int> GetAdbVersionAsync(CancellationToken cancellationToken = default)
        {
            using IAdbSocket socket = adbSocketFactory(EndPoint);
            await socket.SendAdbRequestAsync("host:version");
            AdbResponse response = await socket.ReadAdbResponseAsync();
            string version = await socket.ReadStringAsync(cancellationToken);

            return int.Parse(version, NumberStyles.HexNumber);
        }

        /// <inheritdoc/>
        public async Task<List<DeviceData>> GetDevicesAsync(CancellationToken cancellationToken = default)
        {
            using IAdbSocket socket = adbSocketFactory(EndPoint);
            await socket.SendAdbRequestAsync("host:devices-l");
            await socket.ReadAdbResponseAsync();
            string reply = await socket.ReadStringAsync(cancellationToken);

            string[] data = reply.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            return data.Select(DeviceData.CreateFromAdbData).ToList();
        }

        /// <inheritdoc/>
        public async Task<int> CreateReverseForwardAsync(DeviceData device, string remote, string local, bool allowRebind, CancellationToken cancellationToken = default)
        {
            EnsureDevice(device);

            using IAdbSocket socket = adbSocketFactory(EndPoint);
            socket.SetDevice(device);

            string rebind = allowRebind ? string.Empty : "norebind:";

            await socket.SendAdbRequestAsync($"reverse:forward:{rebind}{remote};{local}");
            _ = await socket.ReadAdbResponseAsync();
            _ = await socket.ReadAdbResponseAsync();
            string portString = await socket.ReadStringAsync(cancellationToken);

            return portString != null && int.TryParse(portString, out int port) ? port : 0;
        }

        /// <inheritdoc/>
        public async Task<int> CreateForwardAsync(DeviceData device, string local, string remote, bool allowRebind, CancellationToken cancellationToken = default)
        {
            EnsureDevice(device);

            using IAdbSocket socket = adbSocketFactory(EndPoint);
            string rebind = allowRebind ? string.Empty : "norebind:";

            await socket.SendAdbRequestAsync($"host-serial:{device.Serial}:forward:{rebind}{local};{remote}");
            _ = await socket.ReadAdbResponseAsync();
            _ = await socket.ReadAdbResponseAsync();
            string portString = await socket.ReadStringAsync(cancellationToken);

            return portString != null && int.TryParse(portString, out int port) ? port : 0;
        }
        
        /// <inheritdoc/>
        public async Task ExecuteRemoteCommandAsync(string command, DeviceData device, IShellOutputReceiver receiver, Encoding encoding, CancellationToken cancellationToken = default)
        {
            EnsureDevice(device);

            using IAdbSocket socket = adbSocketFactory(EndPoint);
            cancellationToken.Register(socket.Dispose);

            socket.SetDevice(device);
            await socket.SendAdbRequestAsync($"shell:{command}");
            AdbResponse response = await socket.ReadAdbResponseAsync();

            try
            {
                using StreamReader reader = new(socket.GetShellStream(), encoding);
                // Previously, we would loop while reader.Peek() >= 0. Turns out that this would
                // break too soon in certain cases (about every 10 loops, so it appears to be a timing
                // issue). Checking for reader.ReadLine() to return null appears to be much more robust
                // -- one of the integration test fetches output 1000 times and found no truncations.
                while (!cancellationToken.IsCancellationRequested)
                {
                    string line =
                        await reader.ReadLineAsync().ConfigureAwait(false);
                    
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
        public async Task InstallAsync(DeviceData device, Stream apk, CancellationToken ct = default, params string[] arguments)
        {
            EnsureDevice(device);

            if (apk == null)
            {
                throw new ArgumentNullException(nameof(apk));
            }

            if (!apk.CanRead || !apk.CanSeek)
            {
                throw new ArgumentOutOfRangeException(nameof(apk), "The apk stream must be a readable and seekable stream");
            }

            StringBuilder requestBuilder = new();
            _ = requestBuilder.Append("exec:cmd package 'install' ");

            if (arguments != null)
            {
                foreach (string argument in arguments)
                {
                    _ = requestBuilder.Append(' ');
                    _ = requestBuilder.Append(argument);
                }
            }

            // add size parameter [required for streaming installs]
            // do last to override any user specified value
            _ = requestBuilder.Append($" -S {apk.Length}");

            using IAdbSocket socket = adbSocketFactory(EndPoint);
            socket.SetDevice(device);

            await socket.SendAdbRequestAsync(requestBuilder.ToString());
            AdbResponse response = await socket.ReadAdbResponseAsync();

            byte[] buffer = new byte[32 * 1024];
            int read = 0;

            while ((read = await apk.ReadAsync(buffer, 0, buffer.Length, ct)) > 0)
            {
                await socket.SendAsync(buffer, read);
            }

            read = await socket.ReadAsync(buffer, buffer.Length, ct);
            string value = Encoding.UTF8.GetString(buffer, 0, read);

            if (!value.Contains("Success"))
            {
                throw new AdbException(value);
            }
        }
        
        /// <inheritdoc/>
        public async Task SendKeyEventAsync(DeviceData device, string key)
        {
            EnsureDevice(device);

            using IAdbSocket socket = adbSocketFactory(EndPoint);
            socket.SetDevice(device);
            await socket.SendAdbRequestAsync(string.Format("shell:input keyevent {0}", key));
            AdbResponse response = await socket.ReadAdbResponseAsync();
            using StreamReader reader = new(socket.GetShellStream(), Encoding);
            string result = await reader.ReadToEndAsync();

            if (result.ToUpper().Contains("ERROR"))
            {
                throw new InvalidKeyEventException("KeyEvent is invalid");
            }
        }

        /// <inheritdoc/>
        public async Task SendTextAsync(DeviceData device, string text)
        {
            EnsureDevice(device);

            using IAdbSocket socket = adbSocketFactory(EndPoint);
            socket.SetDevice(device);
            await socket.SendAdbRequestAsync(string.Format("shell:input text {0}", text));
            AdbResponse response = await socket.ReadAdbResponseAsync();
            using StreamReader reader = new(socket.GetShellStream(), Encoding);
            string result = await reader.ReadToEndAsync();
            if (result.ToUpper().Contains("ERROR"))
            {
                throw new InvalidTextException();
            }
        }
        
        /// <inheritdoc/>
        public async Task RunLogServiceAsync(DeviceData device, Action<LogEntry> messageSink, CancellationToken cancellationToken = default, params LogId[] logNames)
        {
            if (messageSink == null)
            {
                throw new ArgumentNullException(nameof(messageSink));
            }

            EnsureDevice(device);

            // The 'log' service has been deprecated, see
            // https://android.googlesource.com/platform/system/core/+/7aa39a7b199bb9803d3fd47246ee9530b4a96177
            using IAdbSocket socket = adbSocketFactory(EndPoint);
            socket.SetDevice(device);

            StringBuilder request = new();
            request.Append("shell:logcat -B");

            foreach (LogId logName in logNames)
            {
                request.Append($" -b {logName.ToString().ToLowerInvariant()}");
            }

            await socket.SendAdbRequestAsync(request.ToString());
            AdbResponse response = await socket.ReadAdbResponseAsync();

            await using Stream stream = socket.GetShellStream();
            LogReader reader = new(stream);

            while (!cancellationToken.IsCancellationRequested)
            {
                LogEntry entry = null;

                try
                {
                    entry = await reader.ReadEntry(cancellationToken).ConfigureAwait(false);
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
        public async Task<string> PairAsync(DnsEndPoint endpoint, string code, CancellationToken cancellationToken = default)
        {
            if (endpoint == null)
            {
                throw new ArgumentNullException(nameof(endpoint));
            }

            using IAdbSocket socket = adbSocketFactory(EndPoint);
            await socket.SendAdbRequestAsync($"host:pair:{code}:{endpoint.Host}:{endpoint.Port}");
            AdbResponse response = await socket.ReadAdbResponseAsync();
            string results = await socket.ReadStringAsync(cancellationToken);
            return results;
        }

        /// <inheritdoc/>
        public async Task<string> ConnectAsync(DnsEndPoint endpoint, CancellationToken cancellationToken = default)
        {
            if (endpoint == null)
            {
                throw new ArgumentNullException(nameof(endpoint));
            }

            using IAdbSocket socket = adbSocketFactory(EndPoint);
            await socket.SendAdbRequestAsync($"host:connect:{endpoint.Host}:{endpoint.Port}");
            AdbResponse response = await socket.ReadAdbResponseAsync();
            string results = await socket.ReadStringAsync(cancellationToken);
            return results;
        }

        /// <inheritdoc/>
        public async Task<string> DisconnectAsync(DnsEndPoint endpoint, CancellationToken cancellationToken = default)
        {
            if (endpoint == null)
            {
                throw new ArgumentNullException(nameof(endpoint));
            }

            using IAdbSocket socket = adbSocketFactory(EndPoint);
            await socket.SendAdbRequestAsync($"host:disconnect:{endpoint.Host}:{endpoint.Port}");
            AdbResponse response = await socket.ReadAdbResponseAsync();
            string results = await socket.ReadStringAsync(cancellationToken);
            return results;
        }

        /// <inheritdoc/>
        public async Task<List<string>> GetFeatureSetAsync(DeviceData device, CancellationToken cancellationToken = default)
        {
            using IAdbSocket socket = adbSocketFactory(EndPoint);
            await socket.SendAdbRequestAsync($"host-serial:{device.Serial}:features");

            AdbResponse response = await socket.ReadAdbResponseAsync();
            string features = await socket.ReadStringAsync(cancellationToken);

            List<string> featureList = features.Split(new char[] { '\n', ',' }).ToList();
            return featureList;
        }

        /// <inheritdoc/>
        public async Task<XmlDocument> DumpScreenAsync(DeviceData device)
        {
            XmlDocument doc = new();
            using IAdbSocket socket = adbSocketFactory(EndPoint);
            socket.SetDevice(device);
            await socket.SendAdbRequestAsync("shell:uiautomator dump /dev/tty");
            AdbResponse response = await socket.ReadAdbResponseAsync();
            using StreamReader reader = new(socket.GetShellStream(), Encoding);
            string xmlString = await reader.ReadToEndAsync();

            xmlString = xmlString.Replace("Events injected: 1\r\n", "").Replace("UI hierchary dumped to: /dev/tty", "").Trim();
            if (xmlString != "" && !xmlString.StartsWith("ERROR"))
            {
                doc.LoadXml(xmlString);
                return doc;
            }
            return null;
        }
        
        /// <inheritdoc/>
        public async Task ClickAsync(DeviceData device, Cords cords)
        {
            EnsureDevice(device);

            using IAdbSocket socket = adbSocketFactory(EndPoint);
            socket.SetDevice(device);
            await socket.SendAdbRequestAsync(string.Format("shell:input tap {0} {1}", cords.X, cords.Y));
            AdbResponse response = await socket.ReadAdbResponseAsync();
            using StreamReader reader = new(socket.GetShellStream(), Encoding);
            string result = await reader.ReadToEndAsync();

            if (result.ToUpper().Contains("ERROR")) // error or ERROR
            {
                throw new ElementNotFoundException("Coordinates of element is invalid");
            }
        }

        /// <inheritdoc/>
        public async Task ClickAsync(DeviceData device, int x, int y)
        {
            EnsureDevice(device);

            using IAdbSocket socket = adbSocketFactory(EndPoint);
            socket.SetDevice(device);
            await socket.SendAdbRequestAsync(string.Format("shell:input tap {0} {1}", x, y));
            AdbResponse response = await socket.ReadAdbResponseAsync();
            using StreamReader reader = new(socket.GetShellStream(), Encoding);
            string result = await reader.ReadToEndAsync();

            if (result.ToUpper().Contains("ERROR"))
            {
                throw new ElementNotFoundException("Coordinates of element is invalid");
            }
        }

        /// <inheritdoc/>
        public async Task SwipeAsync(DeviceData device, Element first, Element second, long speed)
        {
            EnsureDevice(device);

            using IAdbSocket socket = adbSocketFactory(EndPoint);
            socket.SetDevice(device);
            await socket.SendAdbRequestAsync(string.Format("shell:input swipe {0} {1} {2} {3} {4}", first.Cords.X, first.Cords.Y, second.Cords.X, second.Cords.Y, speed));
            AdbResponse response = await socket.ReadAdbResponseAsync();
            using StreamReader reader = new(socket.GetShellStream(), Encoding);
            string result = await reader.ReadToEndAsync();

            if (result.ToUpper().Contains("ERROR"))
            {
                throw new ElementNotFoundException("Coordinates of element is invalid");
            }
        }

        /// <inheritdoc/>
        public async Task SwipeAsync(DeviceData device, int x1, int y1, int x2, int y2, long speed)
        {
            EnsureDevice(device);

            using IAdbSocket socket = adbSocketFactory(EndPoint);
            socket.SetDevice(device);
            await socket.SendAdbRequestAsync(string.Format("shell:input swipe {0} {1} {2} {3} {4}", x1, y1, x2, y2, speed));
            AdbResponse response = await socket.ReadAdbResponseAsync();
            using StreamReader reader = new(socket.GetShellStream(), Encoding);
            string result = await reader.ReadToEndAsync();

            if (result.ToUpper().Contains("ERROR"))
            {
                throw new ElementNotFoundException("Coordinates of element is invalid");
            }
        }
        
        /// <inheritdoc/>
        public async Task<IEnumerable<ForwardData>> ListForwardAsync(DeviceData device, CancellationToken cancellationToken = default)
        {
            EnsureDevice(device);

            using IAdbSocket socket = adbSocketFactory(EndPoint);
            await socket.SendAdbRequestAsync($"host-serial:{device.Serial}:list-forward");
            AdbResponse response = await socket.ReadAdbResponseAsync();

            string data = await socket.ReadStringAsync(cancellationToken);

            string[] parts = data.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            return parts.Select(ForwardData.FromString);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ForwardData>> ListReverseForwardAsync(DeviceData device, CancellationToken cancellationToken = default)
        {
            EnsureDevice(device);

            using IAdbSocket socket = adbSocketFactory(EndPoint);
            socket.SetDevice(device);

            await socket.SendAdbRequestAsync($"reverse:list-forward");
            AdbResponse response = await socket.ReadAdbResponseAsync();

            string data = await socket.ReadStringAsync(cancellationToken);

            string[] parts = data.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            return parts.Select(ForwardData.FromString);
        }
}
#endif