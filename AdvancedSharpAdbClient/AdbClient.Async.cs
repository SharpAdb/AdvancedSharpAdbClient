// <copyright file="AdbClient.Async.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using AdvancedSharpAdbClient.Exceptions;
using AdvancedSharpAdbClient.Logs;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

#if NET
using System.Runtime.Versioning;
#endif

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
        public async Task<List<DeviceData>> GetDevicesAsync(CancellationToken cancellationToken = default)
        {
            using IAdbSocket socket = adbSocketFactory(EndPoint);
            await socket.SendAdbRequestAsync("host:devices-l", cancellationToken);
            await socket.ReadAdbResponseAsync(cancellationToken);
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

            await socket.SendAdbRequestAsync($"reverse:forward:{rebind}{remote};{local}", cancellationToken);
            _ = await socket.ReadAdbResponseAsync(cancellationToken);
            _ = await socket.ReadAdbResponseAsync(cancellationToken);
            string portString = await socket.ReadStringAsync(cancellationToken);

            return portString != null && int.TryParse(portString, out int port) ? port : 0;
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
        public async Task<IEnumerable<ForwardData>> ListForwardAsync(DeviceData device, CancellationToken cancellationToken = default)
        {
            EnsureDevice(device);

            using IAdbSocket socket = adbSocketFactory(EndPoint);
            await socket.SendAdbRequestAsync($"host-serial:{device.Serial}:list-forward", cancellationToken);
            AdbResponse response = await socket.ReadAdbResponseAsync(cancellationToken);

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

            await socket.SendAdbRequestAsync($"reverse:list-forward", cancellationToken);
            AdbResponse response = await socket.ReadAdbResponseAsync(cancellationToken);

            string data = await socket.ReadStringAsync(cancellationToken);

            string[] parts = data.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            return parts.Select(ForwardData.FromString);
        }

        /// <inheritdoc/>
        public async Task ExecuteRemoteCommandAsync(string command, DeviceData device, IShellOutputReceiver receiver, Encoding encoding, CancellationToken cancellationToken = default)
        {
            EnsureDevice(device);

            using IAdbSocket socket = adbSocketFactory(EndPoint);
            cancellationToken.Register(socket.Dispose);

            socket.SetDevice(device);
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
                    string line =
#if !NET35
                        await reader.ReadLineAsync().ConfigureAwait(false);
#else
                        await Utilities.Run(reader.ReadLine, cancellationToken).ConfigureAwait(false);
#endif

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

            await socket.SendAdbRequestAsync(request.ToString(), cancellationToken);
            AdbResponse response = await socket.ReadAdbResponseAsync(cancellationToken);

            using Stream stream = socket.GetShellStream();
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
            await socket.SendAdbRequestAsync($"host:pair:{code}:{endpoint.Host}:{endpoint.Port}", cancellationToken);
            AdbResponse response = await socket.ReadAdbResponseAsync(cancellationToken);
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
            await socket.SendAdbRequestAsync($"host:connect:{endpoint.Host}:{endpoint.Port}", cancellationToken);
            AdbResponse response = await socket.ReadAdbResponseAsync(cancellationToken);
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
            await socket.SendAdbRequestAsync($"host:disconnect:{endpoint.Host}:{endpoint.Port}", cancellationToken);
            AdbResponse response = await socket.ReadAdbResponseAsync(cancellationToken);
            string results = await socket.ReadStringAsync(cancellationToken);
            return results;
        }

        /// <inheritdoc/>
        public async Task InstallAsync(DeviceData device, Stream apk, CancellationToken cancellationToken = default, params string[] arguments)
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

            await socket.SendAdbRequestAsync(requestBuilder.ToString(), cancellationToken);
            AdbResponse response = await socket.ReadAdbResponseAsync(cancellationToken);

            byte[] buffer = new byte[32 * 1024];
            int read = 0;

#if !NET35
            while ((read = await apk.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
#else
            while ((read = apk.Read(buffer, 0, buffer.Length)) > 0)
#endif
            {
                await socket.SendAsync(buffer, read, cancellationToken);
            }

            read = await socket.ReadAsync(buffer, buffer.Length, cancellationToken);
            string value = Encoding.UTF8.GetString(buffer, 0, read);

            if (!value.Contains("Success"))
            {
                throw new AdbException(value);
            }
        }

        /// <inheritdoc/>
        public async Task<List<string>> GetFeatureSetAsync(DeviceData device, CancellationToken cancellationToken = default)
        {
            using IAdbSocket socket = adbSocketFactory(EndPoint);
            await socket.SendAdbRequestAsync($"host-serial:{device.Serial}:features", cancellationToken);

            AdbResponse response = await socket.ReadAdbResponseAsync(cancellationToken);
            string features = await socket.ReadStringAsync(cancellationToken);

            List<string> featureList = features.Split(new char[] { '\n', ',' }).ToList();
            return featureList;
        }

        /// <inheritdoc/>
        public async Task<XmlDocument> DumpScreenAsync(DeviceData device, CancellationToken cancellationToken = default)
        {
            XmlDocument doc = new();
            using IAdbSocket socket = adbSocketFactory(EndPoint);
            socket.SetDevice(device);
            await socket.SendAdbRequestAsync("shell:uiautomator dump /dev/tty", cancellationToken);
            AdbResponse response = await socket.ReadAdbResponseAsync(cancellationToken);
            using StreamReader reader = new(socket.GetShellStream(), Encoding);
#if !NET35
            string xmlString = await reader.ReadToEndAsync();
#else
            string xmlString = await Utilities.Run(reader.ReadToEnd, cancellationToken).ConfigureAwait(false);
#endif
            xmlString = xmlString.Replace("Events injected: 1\r\n", "").Replace("UI hierchary dumped to: /dev/tty", "").Trim();
            if (xmlString != "" && !xmlString.StartsWith("ERROR"))
            {
                doc.LoadXml(xmlString);
                return doc;
            }
            return null;
        }
        
        /// <inheritdoc/>
        public async Task ClickAsync(DeviceData device, Cords cords, CancellationToken cancellationToken = default)
        {
            EnsureDevice(device);

            using IAdbSocket socket = adbSocketFactory(EndPoint);
            socket.SetDevice(device);
            await socket.SendAdbRequestAsync(string.Format("shell:input tap {0} {1}", cords.X, cords.Y), cancellationToken);
            AdbResponse response = await socket.ReadAdbResponseAsync(cancellationToken);
            using StreamReader reader = new(socket.GetShellStream(), Encoding);
#if !NET35
            string result = await reader.ReadToEndAsync();
#else
            string result = await Utilities.Run(reader.ReadToEnd, cancellationToken).ConfigureAwait(false);
#endif
            if (result.ToUpper().Contains("ERROR")) // error or ERROR
            {
                throw new ElementNotFoundException("Coordinates of element is invalid");
            }
        }

        /// <inheritdoc/>
        public async Task ClickAsync(DeviceData device, int x, int y, CancellationToken cancellationToken = default)
        {
            EnsureDevice(device);

            using IAdbSocket socket = adbSocketFactory(EndPoint);
            socket.SetDevice(device);
            await socket.SendAdbRequestAsync(string.Format("shell:input tap {0} {1}", x, y), cancellationToken);
            AdbResponse response = await socket.ReadAdbResponseAsync(cancellationToken);
            using StreamReader reader = new(socket.GetShellStream(), Encoding);
#if !NET35
            string result = await reader.ReadToEndAsync();
#else
            string result = await Utilities.Run(reader.ReadToEnd, cancellationToken).ConfigureAwait(false);
#endif
            if (result.ToUpper().Contains("ERROR"))
            {
                throw new ElementNotFoundException("Coordinates of element is invalid");
            }
        }

        /// <inheritdoc/>
        public async Task SwipeAsync(DeviceData device, Element first, Element second, long speed, CancellationToken cancellationToken = default)
        {
            EnsureDevice(device);

            using IAdbSocket socket = adbSocketFactory(EndPoint);
            socket.SetDevice(device);
            await socket.SendAdbRequestAsync(string.Format("shell:input swipe {0} {1} {2} {3} {4}", first.Cords.X, first.Cords.Y, second.Cords.X, second.Cords.Y, speed), cancellationToken);
            AdbResponse response = await socket.ReadAdbResponseAsync(cancellationToken);
            using StreamReader reader = new(socket.GetShellStream(), Encoding);
#if !NET35
            string result = await reader.ReadToEndAsync();
#else
            string result = await Utilities.Run(reader.ReadToEnd, cancellationToken).ConfigureAwait(false);
#endif
            if (result.ToUpper().Contains("ERROR"))
            {
                throw new ElementNotFoundException("Coordinates of element is invalid");
            }
        }

        /// <inheritdoc/>
        public async Task SwipeAsync(DeviceData device, int x1, int y1, int x2, int y2, long speed, CancellationToken cancellationToken = default)
        {
            EnsureDevice(device);

            using IAdbSocket socket = adbSocketFactory(EndPoint);
            socket.SetDevice(device);
            await socket.SendAdbRequestAsync(string.Format("shell:input swipe {0} {1} {2} {3} {4}", x1, y1, x2, y2, speed), cancellationToken);
            AdbResponse response = await socket.ReadAdbResponseAsync(cancellationToken);
            using StreamReader reader = new(socket.GetShellStream(), Encoding);
#if !NET35
            string result = await reader.ReadToEndAsync();
#else
            string result = await Utilities.Run(reader.ReadToEnd, cancellationToken).ConfigureAwait(false);
#endif
            if (result.ToUpper().Contains("ERROR"))
            {
                throw new ElementNotFoundException("Coordinates of element is invalid");
            }
        }
        
        /// <inheritdoc/>
#if NET
        [SupportedOSPlatform("windows")]
#endif
        public async Task<Image> GetFrameBufferAsync(DeviceData device, CancellationToken cancellationToken = default)
        {
            EnsureDevice(device);

            using Framebuffer framebuffer = CreateRefreshableFramebuffer(device);
            await framebuffer.RefreshAsync(cancellationToken).ConfigureAwait(false);

            // Convert the framebuffer to an image, and return that.
            return framebuffer.ToImage();
        }
        
        /// <inheritdoc/>
        public Task<int> CreateForwardAsync(DeviceData device, ForwardSpec local, ForwardSpec remote, bool allowRebind, CancellationToken cancellationToken = default) =>
            CreateForwardAsync(device, local?.ToString(), remote?.ToString(), allowRebind, cancellationToken);

        /// <inheritdoc/>
        public Task ExecuteRemoteCommandAsync(string command, DeviceData device, IShellOutputReceiver receiver, CancellationToken cancellationToken = default) =>
            ExecuteRemoteCommandAsync(command, device, receiver, Encoding, cancellationToken);

        /// <inheritdoc/>
        public async Task<Element> FindElementAsync(DeviceData device, string xpath, CancellationToken cancellationToken = default)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    XmlDocument doc = await DumpScreenAsync(device);
                    if (doc != null)
                    {
                        XmlNode xmlNode = doc.SelectSingleNode(xpath);
                        if (xmlNode != null)
                        {
                            string bounds = xmlNode.Attributes["bounds"].Value;
                            if (bounds != null)
                            {
                                int[] cords = bounds.Replace("][", ",").Replace("[", "").Replace("]", "").Split(',').Select(int.Parse).ToArray(); // x1, y1, x2, y2
                                Dictionary<string, string> attributes = new();
                                foreach (XmlAttribute at in xmlNode.Attributes)
                                {
                                    attributes.Add(at.Name, at.Value);
                                }
                                Cords cord = new((cords[0] + cords[2]) / 2, (cords[1] + cords[3]) / 2); // Average x1, y1, x2, y2
                                return new Element(this, device, cord, attributes);
                            }
                        }
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
        public async Task<Element[]> FindElementsAsync(DeviceData device, string xpath, CancellationToken cancellationToken = default)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    XmlDocument doc = await DumpScreenAsync(device);
                    if (doc != null)
                    {
                        XmlNodeList xmlNodes = doc.SelectNodes(xpath);
                        if (xmlNodes != null)
                        {
                            Element[] elements = new Element[xmlNodes.Count];
                            for (int i = 0; i < elements.Length; i++)
                            {
                                string bounds = xmlNodes[i].Attributes["bounds"].Value;
                                if (bounds != null)
                                {
                                    int[] cords = bounds.Replace("][", ",").Replace("[", "").Replace("]", "").Split(',').Select(int.Parse).ToArray(); // x1, y1, x2, y2
                                    Dictionary<string, string> attributes = new();
                                    foreach (XmlAttribute at in xmlNodes[i].Attributes)
                                    {
                                        attributes.Add(at.Name, at.Value);
                                    }
                                    Cords cord = new((cords[0] + cords[2]) / 2, (cords[1] + cords[3]) / 2); // Average x1, y1, x2, y2
                                    elements[i] = new Element(this, device, cord, attributes);
                                }
                            }
                            return elements.Length == 0 ? null : elements;
                        }
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
        public async Task SendKeyEventAsync(DeviceData device, string key, CancellationToken cancellationToken = default)
        {
            EnsureDevice(device);

            using IAdbSocket socket = adbSocketFactory(EndPoint);
            socket.SetDevice(device);
            await socket.SendAdbRequestAsync(string.Format("shell:input keyevent {0}", key), cancellationToken);
            AdbResponse response = await socket.ReadAdbResponseAsync(cancellationToken);
            using StreamReader reader = new(socket.GetShellStream(), Encoding);
#if !NET35
            string result = await reader.ReadToEndAsync();
#else
            string result = await Utilities.Run(reader.ReadToEnd, cancellationToken).ConfigureAwait(false);
#endif
            if (result.ToUpper().Contains("ERROR"))
            {
                throw new InvalidKeyEventException("KeyEvent is invalid");
            }
        }

        /// <inheritdoc/>
        public async Task SendTextAsync(DeviceData device, string text, CancellationToken cancellationToken = default)
        {
            EnsureDevice(device);

            using IAdbSocket socket = adbSocketFactory(EndPoint);
            socket.SetDevice(device);
            await socket.SendAdbRequestAsync(string.Format("shell:input text {0}", text), cancellationToken);
            AdbResponse response = await socket.ReadAdbResponseAsync(cancellationToken);
            using StreamReader reader = new(socket.GetShellStream(), Encoding);
#if !NET35
            string result = await reader.ReadToEndAsync();
#else
            string result = await Utilities.Run(reader.ReadToEnd, cancellationToken).ConfigureAwait(false);
#endif
            if (result.ToUpper().Contains("ERROR"))
            {
                throw new InvalidTextException();
            }
        }

        /// <inheritdoc/>
        public async Task ClearInputAsync(DeviceData device, int charcount, CancellationToken cancellationToken = default)
        {
            await SendKeyEventAsync(device, "KEYCODE_MOVE_END");
            await ExecuteRemoteCommandAsync("input keyevent " + Utilities.Join(" ", Enumerable.Repeat("KEYCODE_DEL ", charcount)), device, null, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task StartAppAsync(DeviceData device, string packagename, CancellationToken cancellationToken = default) =>
            await ExecuteRemoteCommandAsync($"monkey -p {packagename} 1", device, null, cancellationToken);

        /// <inheritdoc/>
        public async Task StopAppAsync(DeviceData device, string packagename, CancellationToken cancellationToken = default) =>
            await ExecuteRemoteCommandAsync($"am force-stop {packagename}", device, null, cancellationToken);

        /// <inheritdoc/>
        public Task BackBtnAsync(DeviceData device) => SendKeyEventAsync(device, "KEYCODE_BACK");

        /// <inheritdoc/>
        public Task HomeBtnAsync(DeviceData device) => SendKeyEventAsync(device, "KEYCODE_HOME");
    }
}
