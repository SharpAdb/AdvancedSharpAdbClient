using AdvancedSharpAdbClient.Logs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using System.Xml;

namespace AdvancedSharpAdbClient.Tests
{
    internal class DummyAdbClient : IAdbClient
    {
        public Dictionary<string, string> Commands
        { get; private set; } = new Dictionary<string, string>();

        public Collection<string> ReceivedCommands
        { get; private set; } = new Collection<string>();

        public EndPoint EndPoint
        { get; private set; }

        public void Connect(DnsEndPoint endpoint)
        {
            throw new NotImplementedException();
        }

        public int CreateForward(DeviceData device, ForwardSpec local, ForwardSpec remote, bool allowRebind)
        {
            throw new NotImplementedException();
        }

        public int CreateForward(DeviceData device, string local, string remote, bool allowRebind)
        {
            throw new NotImplementedException();
        }

        public int CreateReverseForward(DeviceData device, string remote, string local, bool allowRebind)
        {
            throw new NotImplementedException();
        }


        public Task ExecuteRemoteCommandAsync(string command, DeviceData device, IShellOutputReceiver receiver, CancellationToken cancellationToken)
        {
            this.ReceivedCommands.Add(command);

            if (this.Commands.ContainsKey(command))
            {
                if (receiver != null)
                {
                    StringReader reader = new StringReader(this.Commands[command]);

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

        public int GetAdbVersion()
        {
            throw new NotImplementedException();
        }

        public List<DeviceData> GetDevices()
        {
            throw new NotImplementedException();
        }

        public Task<Image> GetFrameBufferAsync(DeviceData device, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Framebuffer CreateRefreshableFramebuffer(DeviceData device)
        {
            throw new NotImplementedException();
        }

        public void KillAdb()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ForwardData> ListReverseForward(DeviceData device)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ForwardData> ListForward(DeviceData device)
        {
            throw new NotImplementedException();
        }

        public void Reboot(string into, DeviceData device)
        {
            throw new NotImplementedException();
        }

        public void RemoveAllForwards(DeviceData device)
        {
            throw new NotImplementedException();
        }

        public void RemoveForward(DeviceData device, int localPort)
        {
            throw new NotImplementedException();
        }

        public void RemoveAllReverseForwards(DeviceData device)
        {
            throw new NotImplementedException();
        }

        public void RemoveReverseForward(DeviceData device, string remote)
        {
            throw new NotImplementedException();
        }

        public Task RunLogServiceAsync(DeviceData device, Action<LogEntry> sink, CancellationToken cancellationToken, params LogId[] logNames)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<LogEntry> RunLogService(DeviceData device, CancellationToken cancellationToken, params LogId[] logNames)
        {
            throw new NotImplementedException();
        }

        public void Root(DeviceData device)
        {
            throw new NotImplementedException();
        }

        public void Unroot(DeviceData device)
        {
            throw new NotImplementedException();
        }

        public void Disconnect(DnsEndPoint endpoint)
        {
            throw new NotImplementedException();
        }

        public void Install(DeviceData device, Stream apk, params string[] arguments)
        {
            throw new NotImplementedException();
        }

        public List<string> GetFeatureSet(DeviceData device)
        {
            throw new NotImplementedException();
        }

        public Task ExecuteRemoteCommandAsync(string command, DeviceData device, IShellOutputReceiver receiver, Encoding encoding, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public string InstallCreated(DeviceData device, string packageName = null, params string[] arguments)
        {
            throw new NotImplementedException();
        }

        public void InstallWrite(DeviceData device, Stream apk, string apkname, string session)
        {
            throw new NotImplementedException();
        }

        public void InstallCommit(DeviceData device, string session)
        {
            throw new NotImplementedException();
        }

        public XmlDocument DumpScreen(DeviceData device)
        {
            throw new NotImplementedException();
        }

        public void Click(DeviceData device, Cords cords)
        {
            throw new NotImplementedException();
        }

        public void Click(DeviceData device, int x, int y)
        {
            throw new NotImplementedException();
        }

        public void Swipe(DeviceData device, Element first, Element second, long speed)
        {
            throw new NotImplementedException();
        }

        public void Swipe(DeviceData device, int x1, int y1, int x2, int y2, long speed)
        {
            throw new NotImplementedException();
        }

        public Element FindElement(DeviceData device, string xpath, TimeSpan timeout = default)
        {
            throw new NotImplementedException();
        }

        public Element[] FindElements(DeviceData device, string xpath, TimeSpan timeout = default)
        {
            throw new NotImplementedException();
        }

        public void SendKeyEvent(DeviceData device, string key)
        {
            throw new NotImplementedException();
        }

        public void SendText(DeviceData device, string text)
        {
            throw new NotImplementedException();
        }

        public void ClearInput(DeviceData device, int charcount)
        {
            throw new NotImplementedException();
        }

        public void StartApp(DeviceData device, string packagename)
        {
            throw new NotImplementedException();
        }

        public void StopApp(DeviceData device, string packagename)
        {
            throw new NotImplementedException();
        }

        public void BackBtn(DeviceData device)
        {
            throw new NotImplementedException();
        }

        public void HomeBtn(DeviceData device)
        {
            throw new NotImplementedException();
        }
    }
}
