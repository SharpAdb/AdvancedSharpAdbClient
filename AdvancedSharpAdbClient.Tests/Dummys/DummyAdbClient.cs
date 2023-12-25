using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AdvancedSharpAdbClient.Tests
{
    /// <summary>
    /// A mock implementation of the <see cref="IAdbClient"/> class.
    /// </summary>
    internal class DummyAdbClient : IAdbClient
    {
        public Dictionary<string, string> Commands { get; } = [];

        public List<string> ReceivedCommands { get; } = [];

        public EndPoint EndPoint { get; init; }

        public void ExecuteRemoteCommand(string command, DeviceData device, Encoding encoding) =>
            ExecuteServerCommand("shell", command, encoding);

        public void ExecuteRemoteCommand(string command, DeviceData device, IShellOutputReceiver receiver, Encoding encoding) =>
            ExecuteServerCommand("shell", command, receiver, encoding);

        public Task ExecuteRemoteCommandAsync(string command, DeviceData device, Encoding encoding, CancellationToken cancellationToken = default) =>
            ExecuteServerCommandAsync("shell", command, encoding, cancellationToken);

        public Task ExecuteRemoteCommandAsync(string command, DeviceData device, IShellOutputReceiver receiver, Encoding encoding, CancellationToken cancellationToken = default) =>
            ExecuteServerCommandAsync("shell", command, receiver, encoding, cancellationToken);

        public void ExecuteServerCommand(string target, string command, Encoding encoding)
        {
            StringBuilder requestBuilder = new();
            if (!StringExtensions.IsNullOrWhiteSpace(target))
            {
                _ = requestBuilder.AppendFormat("{0}:", target);
            }
            _ = requestBuilder.Append(command);

            string request = requestBuilder.ToString();
            ReceivedCommands.Add(request);
        }

        public void ExecuteServerCommand(string target, string command, IAdbSocket socket, Encoding encoding) =>
            ExecuteServerCommand(target, command, encoding);

        public void ExecuteServerCommand(string target, string command, IShellOutputReceiver receiver, Encoding encoding)
        {
            StringBuilder requestBuilder = new();
            if (!StringExtensions.IsNullOrWhiteSpace(target))
            {
                _ = requestBuilder.AppendFormat("{0}:", target);
            }
            _ = requestBuilder.Append(command);

            string request = requestBuilder.ToString();
            ReceivedCommands.Add(request);

            if (Commands.TryGetValue(request, out string value))
            {
                if (receiver != null)
                {
                    StringReader reader = new(value);

                    while (reader.Peek() != -1)
                    {
                        receiver.AddOutput(reader.ReadLine());
                    }

                    receiver.Flush();
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(command), $"The command '{request}' was unexpected");
            }
        }

        public void ExecuteServerCommand(string target, string command, IAdbSocket socket, IShellOutputReceiver receiver, Encoding encoding) =>
            ExecuteServerCommand(target, command, receiver, encoding);

        public async Task ExecuteServerCommandAsync(string target, string command, Encoding encoding, CancellationToken cancellationToken = default)
        {
            await Task.Yield();

            StringBuilder requestBuilder = new();
            if (!StringExtensions.IsNullOrWhiteSpace(target))
            {
                _ = requestBuilder.AppendFormat("{0}:", target);
            }
            _ = requestBuilder.Append(command);

            string request = requestBuilder.ToString();
            ReceivedCommands.Add(request);
        }

        public Task ExecuteServerCommandAsync(string target, string command, IAdbSocket socket, Encoding encoding, CancellationToken cancellationToken) =>
            ExecuteServerCommandAsync(target, command, encoding, cancellationToken);

        public async Task ExecuteServerCommandAsync(string target, string command, IShellOutputReceiver receiver, Encoding encoding, CancellationToken cancellationToken = default)
        {
            StringBuilder requestBuilder = new();
            if (!StringExtensions.IsNullOrWhiteSpace(target))
            {
                _ = requestBuilder.AppendFormat("{0}:", target);
            }
            _ = requestBuilder.Append(command);

            string request = requestBuilder.ToString();
            ReceivedCommands.Add(request);

            if (Commands.TryGetValue(request, out string value))
            {
                if (receiver != null)
                {
                    StringReader reader = new(value);

                    while (reader.Peek() != -1)
                    {
                        receiver.AddOutput(await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false));
                    }

                    receiver.Flush();
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(command), $"The command '{request}' was unexpected");
            }
        }

        public Task ExecuteServerCommandAsync(string target, string command, IAdbSocket socket, IShellOutputReceiver receiver, Encoding encoding, CancellationToken cancellationToken) =>
            ExecuteServerCommandAsync(target, command, receiver, encoding, cancellationToken);

        #region Not Implemented

        string IAdbClient.Connect(DnsEndPoint endpoint) => throw new NotImplementedException();

        Task<string> IAdbClient.ConnectAsync(DnsEndPoint endpoint, CancellationToken cancellationToken) => throw new NotImplementedException();

        int IAdbClient.CreateForward(DeviceData device, string local, string remote, bool allowRebind) => throw new NotImplementedException();

        Task<int> IAdbClient.CreateForwardAsync(DeviceData device, string local, string remote, bool allowRebind, CancellationToken cancellationToken) => throw new NotImplementedException();

        Framebuffer IAdbClient.CreateRefreshableFramebuffer(DeviceData device) => throw new NotImplementedException();

        int IAdbClient.CreateReverseForward(DeviceData device, string remote, string local, bool allowRebind) => throw new NotImplementedException();

        Task<int> IAdbClient.CreateReverseForwardAsync(DeviceData device, string remote, string local, bool allowRebind, CancellationToken cancellationToken) => throw new NotImplementedException();

        string IAdbClient.Disconnect(DnsEndPoint endpoint) => throw new NotImplementedException();

        Task<string> IAdbClient.DisconnectAsync(DnsEndPoint endpoint, CancellationToken cancellationToken) => throw new NotImplementedException();

        int IAdbClient.GetAdbVersion() => throw new NotImplementedException();

        Task<int> IAdbClient.GetAdbVersionAsync(CancellationToken cancellationToken) => throw new NotImplementedException();

        IEnumerable<DeviceData> IAdbClient.GetDevices() => throw new NotImplementedException();

        Task<IEnumerable<DeviceData>> IAdbClient.GetDevicesAsync(CancellationToken cancellationToken) => throw new NotImplementedException();

        IEnumerable<string> IAdbClient.GetFeatureSet(DeviceData device) => throw new NotImplementedException();

        Task<IEnumerable<string>> IAdbClient.GetFeatureSetAsync(DeviceData device, CancellationToken cancellationToken) => throw new NotImplementedException();

        Framebuffer IAdbClient.GetFrameBuffer(DeviceData device) => throw new NotImplementedException();

        Task<Framebuffer> IAdbClient.GetFrameBufferAsync(DeviceData device, CancellationToken cancellationToken) => throw new NotImplementedException();

        void IAdbClient.Install(DeviceData device, Stream apk, Action<InstallProgressEventArgs> progress, params string[] arguments) => throw new NotImplementedException();

        Task IAdbClient.InstallAsync(DeviceData device, Stream apk, Action<InstallProgressEventArgs> progress, CancellationToken cancellationToken, params string[] arguments) => throw new NotImplementedException();

        void IAdbClient.InstallCommit(DeviceData device, string session) => throw new NotImplementedException();

        Task IAdbClient.InstallCommitAsync(DeviceData device, string session, CancellationToken cancellationToken) => throw new NotImplementedException();

        string IAdbClient.InstallCreate(DeviceData device, string packageName, params string[] arguments) => throw new NotImplementedException();

        Task<string> IAdbClient.InstallCreateAsync(DeviceData device, string packageName, CancellationToken cancellationToken, params string[] arguments) => throw new NotImplementedException();

        void IAdbClient.InstallMultiple(DeviceData device, IEnumerable<Stream> splitAPKs, string packageName, Action<InstallProgressEventArgs> progress, params string[] arguments) => throw new NotImplementedException();

        void IAdbClient.InstallMultiple(DeviceData device, Stream baseAPK, IEnumerable<Stream> splitAPKs, Action<InstallProgressEventArgs> progress, params string[] arguments) => throw new NotImplementedException();

        Task IAdbClient.InstallMultipleAsync(DeviceData device, IEnumerable<Stream> splitAPKs, string packageName, Action<InstallProgressEventArgs> progress, CancellationToken cancellationToken, params string[] arguments) => throw new NotImplementedException();

        Task IAdbClient.InstallMultipleAsync(DeviceData device, Stream baseAPK, IEnumerable<Stream> splitAPKs, Action<InstallProgressEventArgs> progress, CancellationToken cancellationToken, params string[] arguments) => throw new NotImplementedException();

        void IAdbClient.InstallWrite(DeviceData device, Stream apk, string apkName, string session, Action<double> progress) => throw new NotImplementedException();

        Task IAdbClient.InstallWriteAsync(DeviceData device, Stream apk, string apkName, string session, Action<double> progress, CancellationToken cancellationToken) => throw new NotImplementedException();

        void IAdbClient.KillAdb() => throw new NotImplementedException();

        Task IAdbClient.KillAdbAsync(CancellationToken cancellationToken) => throw new NotImplementedException();

        IEnumerable<ForwardData> IAdbClient.ListForward(DeviceData device) => throw new NotImplementedException();

        Task<IEnumerable<ForwardData>> IAdbClient.ListForwardAsync(DeviceData device, CancellationToken cancellationToken) => throw new NotImplementedException();

        IEnumerable<ForwardData> IAdbClient.ListReverseForward(DeviceData device) => throw new NotImplementedException();

        Task<IEnumerable<ForwardData>> IAdbClient.ListReverseForwardAsync(DeviceData device, CancellationToken cancellationToken) => throw new NotImplementedException();

        string IAdbClient.Pair(DnsEndPoint endpoint, string code) => throw new NotImplementedException();

        Task<string> IAdbClient.PairAsync(DnsEndPoint endpoint, string code, CancellationToken cancellationToken) => throw new NotImplementedException();

        void IAdbClient.Reboot(string into, DeviceData device) => throw new NotImplementedException();

        Task IAdbClient.RebootAsync(string into, DeviceData device, CancellationToken cancellationToken) => throw new NotImplementedException();

        void IAdbClient.RemoveAllForwards(DeviceData device) => throw new NotImplementedException();

        Task IAdbClient.RemoveAllForwardsAsync(DeviceData device, CancellationToken cancellationToken) => throw new NotImplementedException();

        void IAdbClient.RemoveAllReverseForwards(DeviceData device) => throw new NotImplementedException();

        Task IAdbClient.RemoveAllReverseForwardsAsync(DeviceData device, CancellationToken cancellationToken) => throw new NotImplementedException();

        void IAdbClient.RemoveForward(DeviceData device, int localPort) => throw new NotImplementedException();

        Task IAdbClient.RemoveForwardAsync(DeviceData device, int localPort, CancellationToken cancellationToken) => throw new NotImplementedException();

        void IAdbClient.RemoveReverseForward(DeviceData device, string remote) => throw new NotImplementedException();

        Task IAdbClient.RemoveReverseForwardAsync(DeviceData device, string remote, CancellationToken cancellationToken) => throw new NotImplementedException();

        void IAdbClient.Root(DeviceData device) => throw new NotImplementedException();

        Task IAdbClient.RootAsync(DeviceData device, CancellationToken cancellationToken) => throw new NotImplementedException();

        void IAdbClient.RunLogService(DeviceData device, Action<LogEntry> messageSink, in bool isCancelled, params LogId[] logNames) => throw new NotImplementedException();

        Task IAdbClient.RunLogServiceAsync(DeviceData device, Action<LogEntry> messageSink, CancellationToken cancellationToken, params LogId[] logNames) => throw new NotImplementedException();

        void IAdbClient.Uninstall(DeviceData device, string packageName, params string[] arguments) => throw new NotImplementedException();

        Task IAdbClient.UninstallAsync(DeviceData device, string packageName, CancellationToken cancellationToken, params string[] arguments) => throw new NotImplementedException();

        void IAdbClient.Unroot(DeviceData device) => throw new NotImplementedException();

        Task IAdbClient.UnrootAsync(DeviceData device, CancellationToken cancellationToken) => throw new NotImplementedException();

        #endregion
    }
}
