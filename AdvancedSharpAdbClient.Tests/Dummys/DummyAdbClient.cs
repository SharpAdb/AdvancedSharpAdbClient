using AdvancedSharpAdbClient.Logs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace AdvancedSharpAdbClient.Tests
{
    internal class DummyAdbClient : IAdbClient
    {
        public Dictionary<string, string> Commands { get; private set; } = new Dictionary<string, string>();

        public List<string> ReceivedCommands { get; private set; } = new List<string>();

        public EndPoint EndPoint { get; private set; }

        public void ExecuteRemoteCommand(string command, DeviceData device, IShellOutputReceiver receiver) =>
            ExecuteRemoteCommand(command, device, receiver, Encoding.Default);

        public void ExecuteRemoteCommand(string command, DeviceData device, IShellOutputReceiver receiver, Encoding encoding)
        {
            ReceivedCommands.Add(command);

            if (Commands.TryGetValue(command, out string value))
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
                throw new ArgumentOutOfRangeException(nameof(command), $"The command '{command}' was unexpected");
            }
        }

        public Task ExecuteRemoteCommandAsync(string command, DeviceData device, IShellOutputReceiver receiver, CancellationToken cancellationToken = default) =>
            ExecuteRemoteCommandAsync(command, device, receiver, Encoding.Default, cancellationToken);

        public Task ExecuteRemoteCommandAsync(string command, DeviceData device, IShellOutputReceiver receiver, Encoding encoding, CancellationToken cancellationToken = default)
        {
            ReceivedCommands.Add(command);

            if (Commands.TryGetValue(command, out string value))
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
                throw new ArgumentOutOfRangeException(nameof(command), $"The command '{command}' was unexpected");
            }

            return Task.FromResult(true);
        }

        #region Not Implemented

        public void BackBtn(DeviceData device) => throw new NotImplementedException();

        public Task BackBtnAsync(DeviceData device, CancellationToken cancellationToken) => throw new NotImplementedException();

        public void ClearInput(DeviceData device, int charCount) => throw new NotImplementedException();

        public Task ClearInputAsync(DeviceData device, int charCount, CancellationToken cancellationToken) => throw new NotImplementedException();

        public void Click(DeviceData device, Cords cords) => throw new NotImplementedException();

        public void Click(DeviceData device, int x, int y) => throw new NotImplementedException();

        public Task ClickAsync(DeviceData device, Cords cords, CancellationToken cancellationToken) => throw new NotImplementedException();

        public Task ClickAsync(DeviceData device, int x, int y, CancellationToken cancellationToken) => throw new NotImplementedException();

        public string Connect(DnsEndPoint endpoint) => throw new NotImplementedException();

        public Task<string> ConnectAsync(DnsEndPoint endpoint, CancellationToken cancellationToken) => throw new NotImplementedException();

        public int CreateForward(DeviceData device, string local, string remote, bool allowRebind) => throw new NotImplementedException();

        public int CreateForward(DeviceData device, ForwardSpec local, ForwardSpec remote, bool allowRebind) => throw new NotImplementedException();

        public Task<int> CreateForwardAsync(DeviceData device, string local, string remote, bool allowRebind, CancellationToken cancellationToken) => throw new NotImplementedException();

        public Task<int> CreateForwardAsync(DeviceData device, ForwardSpec local, ForwardSpec remote, bool allowRebind, CancellationToken cancellationToken) => throw new NotImplementedException();

        public Framebuffer CreateRefreshableFramebuffer(DeviceData device) => throw new NotImplementedException();

        public int CreateReverseForward(DeviceData device, string remote, string local, bool allowRebind) => throw new NotImplementedException();

        public Task<int> CreateReverseForwardAsync(DeviceData device, string remote, string local, bool allowRebind, CancellationToken cancellationToken) => throw new NotImplementedException();

        public string Disconnect(DnsEndPoint endpoint) => throw new NotImplementedException();

        public Task<string> DisconnectAsync(DnsEndPoint endpoint, CancellationToken cancellationToken) => throw new NotImplementedException();

        public XmlDocument DumpScreen(DeviceData device) => throw new NotImplementedException();

        public Task<XmlDocument> DumpScreenAsync(DeviceData device, CancellationToken cancellationToken) => throw new NotImplementedException();

        public string DumpScreenString(DeviceData device) => throw new NotImplementedException();

        public Task<string> DumpScreenStringAsync(DeviceData device, CancellationToken cancellationToken) => throw new NotImplementedException();

        public IAsyncEnumerable<Element> FindAsyncElements(DeviceData device, string xpath, CancellationToken cancellationToken) => throw new NotImplementedException();

        public Element FindElement(DeviceData device, string xpath, TimeSpan timeout = default) => throw new NotImplementedException();

        public Task<Element> FindElementAsync(DeviceData device, string xpath, CancellationToken cancellationToken) => throw new NotImplementedException();

        public IEnumerable<Element> FindElements(DeviceData device, string xpath, TimeSpan timeout = default) => throw new NotImplementedException();

        public Task<List<Element>> FindElementsAsync(DeviceData device, string xpath, CancellationToken cancellationToken) => throw new NotImplementedException();

        public int GetAdbVersion() => throw new NotImplementedException();

        public Task<int> GetAdbVersionAsync(CancellationToken cancellationToken) => throw new NotImplementedException();

        public AppStatus GetAppStatus(DeviceData device, string packageName) => throw new NotImplementedException();

        public Task<AppStatus> GetAppStatusAsync(DeviceData device, string packageName, CancellationToken cancellationToken) => throw new NotImplementedException();

        public IEnumerable<DeviceData> GetDevices() => throw new NotImplementedException();

        public Task<IEnumerable<DeviceData>> GetDevicesAsync(CancellationToken cancellationToken) => throw new NotImplementedException();

        public IEnumerable<string> GetFeatureSet(DeviceData device) => throw new NotImplementedException();

        public Task<IEnumerable<string>> GetFeatureSetAsync(DeviceData device, CancellationToken cancellationToken) => throw new NotImplementedException();

        public Framebuffer GetFrameBuffer(DeviceData device) => throw new NotImplementedException();

        public Task<Framebuffer> GetFrameBufferAsync(DeviceData device, CancellationToken cancellationToken) => throw new NotImplementedException();

        public void HomeBtn(DeviceData device) => throw new NotImplementedException();

        public Task HomeBtnAsync(DeviceData device, CancellationToken cancellationToken) => throw new NotImplementedException();

        public void Install(DeviceData device, Stream apk, params string[] arguments) => throw new NotImplementedException();

        public Task InstallAsync(DeviceData device, Stream apk, params string[] arguments) => throw new NotImplementedException();

        public Task InstallAsync(DeviceData device, Stream apk, CancellationToken cancellationToken, params string[] arguments) => throw new NotImplementedException();

        public void InstallCommit(DeviceData device, string session) => throw new NotImplementedException();

        public Task InstallCommitAsync(DeviceData device, string session, CancellationToken cancellationToken) => throw new NotImplementedException();

        public string InstallCreate(DeviceData device, string packageName = null, params string[] arguments) => throw new NotImplementedException();

        public Task<string> InstallCreateAsync(DeviceData device, string packageName, params string[] arguments) => throw new NotImplementedException();

        public Task<string> InstallCreateAsync(DeviceData device, string packageName, CancellationToken cancellationToken, params string[] arguments) => throw new NotImplementedException();

        public void InstallMultiple(DeviceData device, IEnumerable<Stream> splitAPKs, string packageName, params string[] arguments) => throw new NotImplementedException();

        public void InstallMultiple(DeviceData device, Stream baseAPK, IEnumerable<Stream> splitAPKs, params string[] arguments) => throw new NotImplementedException();

        public Task InstallMultipleAsync(DeviceData device, IEnumerable<Stream> splitAPKs, string packageName, params string[] arguments) => throw new NotImplementedException();

        public Task InstallMultipleAsync(DeviceData device, IEnumerable<Stream> splitAPKs, string packageName, CancellationToken cancellationToken, params string[] arguments) => throw new NotImplementedException();

        public Task InstallMultipleAsync(DeviceData device, Stream baseAPK, IEnumerable<Stream> splitAPKs, params string[] arguments) => throw new NotImplementedException();

        public Task InstallMultipleAsync(DeviceData device, Stream baseAPK, IEnumerable<Stream> splitAPKs, CancellationToken cancellationToken, params string[] arguments) => throw new NotImplementedException();

        public void InstallWrite(DeviceData device, Stream apk, string apkName, string session) => throw new NotImplementedException();

        public Task InstallWriteAsync(DeviceData device, Stream apk, string apkName, string session, CancellationToken cancellationToken) => throw new NotImplementedException();

        public bool IsAppRunning(DeviceData device, string packageName) => throw new NotImplementedException();

        public Task<bool> IsAppRunningAsync(DeviceData device, string packageName, CancellationToken cancellationToken) => throw new NotImplementedException();

        public bool IsCurrentApp(DeviceData device, string packageName) => throw new NotImplementedException();

        public Task<bool> IsCurrentAppAsync(DeviceData device, string packageName, CancellationToken cancellationToken) => throw new NotImplementedException();

        public void KillAdb() => throw new NotImplementedException();

        public Task KillAdbAsync(CancellationToken cancellationToken) => throw new NotImplementedException();

        public IEnumerable<ForwardData> ListForward(DeviceData device) => throw new NotImplementedException();

        public Task<IEnumerable<ForwardData>> ListForwardAsync(DeviceData device, CancellationToken cancellationToken) => throw new NotImplementedException();

        public IEnumerable<ForwardData> ListReverseForward(DeviceData device) => throw new NotImplementedException();

        public Task<IEnumerable<ForwardData>> ListReverseForwardAsync(DeviceData device, CancellationToken cancellationToken) => throw new NotImplementedException();

        public string Pair(DnsEndPoint endpoint, string code) => throw new NotImplementedException();

        public Task<string> PairAsync(DnsEndPoint endpoint, string code, CancellationToken cancellationToken) => throw new NotImplementedException();

        public void Reboot(string into, DeviceData device) => throw new NotImplementedException();

        public Task RebootAsync(string into, DeviceData device, CancellationToken cancellationToken) => throw new NotImplementedException();

        public void RemoveAllForwards(DeviceData device) => throw new NotImplementedException();

        public Task RemoveAllForwardsAsync(DeviceData device, CancellationToken cancellationToken) => throw new NotImplementedException();

        public void RemoveAllReverseForwards(DeviceData device) => throw new NotImplementedException();

        public Task RemoveAllReverseForwardsAsync(DeviceData device, CancellationToken cancellationToken) => throw new NotImplementedException();

        public void RemoveForward(DeviceData device, int localPort) => throw new NotImplementedException();

        public Task RemoveForwardAsync(DeviceData device, int localPort, CancellationToken cancellationToken) => throw new NotImplementedException();

        public void RemoveReverseForward(DeviceData device, string remote) => throw new NotImplementedException();

        public Task RemoveReverseForwardAsync(DeviceData device, string remote, CancellationToken cancellationToken) => throw new NotImplementedException();

        public void Root(DeviceData device) => throw new NotImplementedException();

        public Task RootAsync(DeviceData device, CancellationToken cancellationToken) => throw new NotImplementedException();

        public void RunLogService(DeviceData device, Action<LogEntry> messageSink, params LogId[] logNames) => throw new NotImplementedException();

        public Task RunLogServiceAsync(DeviceData device, Action<LogEntry> messageSink, params LogId[] logNames) => throw new NotImplementedException();

        public Task RunLogServiceAsync(DeviceData device, Action<LogEntry> messageSink, CancellationToken cancellationToken, params LogId[] logNames) => throw new NotImplementedException();

        public void SendKeyEvent(DeviceData device, string key) => throw new NotImplementedException();

        public Task SendKeyEventAsync(DeviceData device, string key, CancellationToken cancellationToken) => throw new NotImplementedException();

        public void SendText(DeviceData device, string text) => throw new NotImplementedException();

        public Task SendTextAsync(DeviceData device, string text, CancellationToken cancellationToken) => throw new NotImplementedException();

        public void StartApp(DeviceData device, string packageName) => throw new NotImplementedException();

        public Task StartAppAsync(DeviceData device, string packageName, CancellationToken cancellationToken) => throw new NotImplementedException();

        public void StopApp(DeviceData device, string packageName) => throw new NotImplementedException();

        public Task StopAppAsync(DeviceData device, string packageName, CancellationToken cancellationToken) => throw new NotImplementedException();

        public void Swipe(DeviceData device, Element first, Element second, long speed) => throw new NotImplementedException();

        public void Swipe(DeviceData device, int x1, int y1, int x2, int y2, long speed) => throw new NotImplementedException();

        public Task SwipeAsync(DeviceData device, Element first, Element second, long speed, CancellationToken cancellationToken) => throw new NotImplementedException();

        public Task SwipeAsync(DeviceData device, int x1, int y1, int x2, int y2, long speed, CancellationToken cancellationToken) => throw new NotImplementedException();

        public void Unroot(DeviceData device) => throw new NotImplementedException();

        public Task UnrootAsync(DeviceData device, CancellationToken cancellationToken) => throw new NotImplementedException();

        #endregion
    }
}
