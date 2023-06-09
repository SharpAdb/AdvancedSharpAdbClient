using AdvancedSharpAdbClient.Exceptions;
using AdvancedSharpAdbClient.Logs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Xunit;

namespace AdvancedSharpAdbClient.Tests
{
    public partial class AdbClientTests
    {
        [Fact]
        public async void GetAdbVersionAsyncTest()
        {
            string[] responseMessages = new string[]
            {
                "0020"
            };

            string[] requests = new string[]
            {
                "host:version"
            };

            int version = 0;

            await RunTestAsync(
                OkResponse,
                responseMessages,
                requests,
                async () => version = await TestClient.GetAdbVersionAsync());

            // Make sure and the correct value is returned.
            Assert.Equal(32, version);
        }

        [Fact]
        public async void KillAdbAsyncTest()
        {
            string[] requests = new string[]
            {
                "host:kill"
            };

            await RunTestAsync(
                NoResponses,
                NoResponseMessages,
                requests,
                () => TestClient.KillAdbAsync());
        }

        [Fact]
        public async void GetDevicesAsyncTest()
        {
            string[] responseMessages = new string[]
            {
                "169.254.109.177:5555   device product:VS Emulator 5\" KitKat (4.4) XXHDPI Phone model:5__KitKat__4_4__XXHDPI_Phone device:donatello\n"
            };

            string[] requests = new string[]
            {
                "host:devices-l"
            };

            IEnumerable<DeviceData> devices = null;

            await RunTestAsync(
                OkResponse,
                responseMessages,
                requests,
                async () => devices = await TestClient.GetDevicesAsync());

            // Make sure and the correct value is returned.
            Assert.NotNull(devices);
            Assert.Single(devices);

            DeviceData device = devices.Single();

            Assert.Equal("169.254.109.177:5555", device.Serial);
            Assert.Equal(DeviceState.Online, device.State);
            Assert.Equal("5__KitKat__4_4__XXHDPI_Phone", device.Model);
            Assert.Equal("donatello", device.Name);
        }

        [Fact]
        public async void CreateForwardAsyncTest() =>
            await RunCreateForwardAsyncTest(
                (device) => TestClient.CreateForwardAsync(device, "tcp:1", "tcp:2", true),
                "tcp:1;tcp:2");


        [Fact]
        public async void CreateReverseAsyncTest() =>
            await RunCreateReverseAsyncTest(
                (device) => TestClient.CreateReverseForwardAsync(device, "tcp:1", "tcp:2", true),
                "tcp:1;tcp:2");

        [Fact]
        public async void CreateTcpForwardAsyncTest() =>
            await RunCreateForwardAsyncTest(
                (device) => TestClient.CreateForwardAsync(device, 3, 4),
                "tcp:3;tcp:4");

        [Fact]
        public async void CreateSocketForwardAsyncTest() =>
            await RunCreateForwardAsyncTest(
                (device) => TestClient.CreateForwardAsync(device, 5, "/socket/1"),
                "tcp:5;local:/socket/1");

        [Fact]
        public async void CreateDuplicateForwardAsyncTest()
        {
            AdbResponse[] responses = new AdbResponse[]
            {
                AdbResponse.FromError("cannot rebind existing socket")
            };

            string[] requests = new string[]
            {
                "host-serial:169.254.109.177:5555:forward:norebind:tcp:1;tcp:2"
            };

            _ = await Assert.ThrowsAsync<AdbException>(() =>
            RunTestAsync(
                responses,
                NoResponseMessages,
                requests,
                () => TestClient.CreateForwardAsync(Device, "tcp:1", "tcp:2", false)));
        }

        [Fact]
        public async void RemoveForwardAsyncTest()
        {
            string[] requests = new string[]
            {
                "host-serial:169.254.109.177:5555:killforward:tcp:1"
            };

            await RunTestAsync(
                OkResponse,
                NoResponseMessages,
                requests,
                () => TestClient.RemoveForwardAsync(Device, 1));
        }

        [Fact]
        public async void RemoveReverseForwardAsyncTest()
        {
            string[] requests = new string[]
            {
                "host:transport:169.254.109.177:5555",
                "reverse:killforward:localabstract:test"
            };

            AdbResponse[] responses = new AdbResponse[]
            {
                AdbResponse.OK,
                AdbResponse.OK,
            };

            await RunTestAsync(
                responses,
                NoResponseMessages,
                requests,
                () => TestClient.RemoveReverseForwardAsync(Device, "localabstract:test"));
        }

        [Fact]
        public async void RemoveAllForwardsAsyncTest()
        {
            string[] requests = new string[]
            {
                "host-serial:169.254.109.177:5555:killforward-all"
            };

            await RunTestAsync(
                OkResponse,
                NoResponseMessages,
                requests,
                () => TestClient.RemoveAllForwardsAsync(Device));
        }

        [Fact]
        public async void RemoveAllReversesAsyncTest()
        {
            string[] requests = new string[]
            {
                "host:transport:169.254.109.177:5555",
                "reverse:killforward-all"
            };

            AdbResponse[] responses = new AdbResponse[]
            {
                AdbResponse.OK,
                AdbResponse.OK,
            };

            await RunTestAsync(
                responses,
                NoResponseMessages,
                requests,
                () => TestClient.RemoveAllReverseForwardsAsync(Device));
        }

        [Fact]
        public async void ListForwardAsyncTest()
        {
            string[] responseMessages = new string[]
            {
                "169.254.109.177:5555 tcp:1 tcp:2\n169.254.109.177:5555 tcp:3 tcp:4\n169.254.109.177:5555 tcp:5 local:/socket/1\n"
            };

            string[] requests = new string[]
            {
                "host-serial:169.254.109.177:5555:list-forward"
            };

            ForwardData[] forwards = null;

            await RunTestAsync(
                OkResponse,
                responseMessages,
                requests,
                async () => forwards = (await TestClient.ListForwardAsync(Device)).ToArray());

            Assert.NotNull(forwards);
            Assert.Equal(3, forwards.Length);
            Assert.Equal("169.254.109.177:5555", forwards[0].SerialNumber);
            Assert.Equal("tcp:1", forwards[0].Local);
            Assert.Equal("tcp:2", forwards[0].Remote);
        }

        [Fact]
        public async void ListReverseForwardAsyncTest()
        {
            string[] responseMessages = new string[]
            {
                "(reverse) localabstract:scrcpy tcp:100\n(reverse) localabstract: scrcpy2 tcp:100\n(reverse) localabstract: scrcpy3 tcp:100\n"
            };
            AdbResponse[] responses = new AdbResponse[]
            {
                AdbResponse.OK,
                AdbResponse.OK,
            };

            string[] requests = new string[]
            {
                "host:transport:169.254.109.177:5555",
                "reverse:list-forward"
            };

            ForwardData[] forwards = null;

            await RunTestAsync(
                responses,
                responseMessages,
                requests,
                async () => forwards = (await TestClient.ListReverseForwardAsync(Device)).ToArray());

            Assert.NotNull(forwards);
            Assert.Equal(3, forwards.Length);
            Assert.Equal("(reverse)", forwards[0].SerialNumber);
            Assert.Equal("localabstract:scrcpy", forwards[0].Local);
            Assert.Equal("tcp:100", forwards[0].Remote);
        }

        [Fact]
        public async void ExecuteRemoteCommandAsyncTest()
        {
            DeviceData device = new()
            {
                Serial = "169.254.109.177:5555",
                State = DeviceState.Online
            };

            AdbResponse[] responses = new AdbResponse[]
            {
                AdbResponse.OK,
                AdbResponse.OK
            };

            string[] responseMessages = Array.Empty<string>();

            string[] requests = new string[]
            {
                "host:transport:169.254.109.177:5555",
                "shell:echo Hello, World"
            };

            byte[] streamData = Encoding.ASCII.GetBytes("Hello, World\r\n");
            using MemoryStream shellStream = new(streamData);

            ConsoleOutputReceiver receiver = new();

            await RunTestAsync(
                responses,
                responseMessages,
                requests,
                shellStream,
                () => TestClient.ExecuteRemoteCommandAsync("echo Hello, World", device, receiver));

            Assert.Equal("Hello, World\r\n", receiver.ToString(), ignoreLineEndingDifferences: true);
        }

        [Fact]
        public async void ExecuteRemoteCommandAsyncUnresponsiveTest()
        {
            DeviceData device = new()
            {
                Serial = "169.254.109.177:5555",
                State = DeviceState.Online
            };

            AdbResponse[] responses = new AdbResponse[]
            {
                AdbResponse.OK,
                AdbResponse.OK
            };

            string[] responseMessages = Array.Empty<string>();

            string[] requests = new string[]
            {
                "host:transport:169.254.109.177:5555",
                "shell:echo Hello, World"
            };

            ConsoleOutputReceiver receiver = new();

            _ = await Assert.ThrowsAsync<ShellCommandUnresponsiveException>(() =>
            RunTestAsync(
                responses,
                responseMessages,
                requests,
                null,
                () => TestClient.ExecuteRemoteCommandAsync("echo Hello, World", device, receiver, CancellationToken.None)));
        }

        [Fact]
        public async void RunLogServiceAsyncTest()
        {
            DeviceData device = new()
            {
                Serial = "169.254.109.177:5555",
                State = DeviceState.Online
            };

            AdbResponse[] responses = new AdbResponse[]
            {
                AdbResponse.OK,
                AdbResponse.OK
            };

            string[] responseMessages = Array.Empty<string>();

            string[] requests = new string[]
            {
                "host:transport:169.254.109.177:5555",
                "shell:logcat -B -b system"
            };

            ConsoleOutputReceiver receiver = new();

            using Stream stream = File.OpenRead("Assets/logcat.bin");
            using ShellStream shellStream = new(stream, false);
            Collection<LogEntry> logs = new();
            Action<LogEntry> sink = logs.Add;

            await RunTestAsync(
                responses,
                responseMessages,
                requests,
                shellStream,
                () => TestClient.RunLogServiceAsync(device, sink, CancellationToken.None, LogId.System));

            Assert.Equal(3, logs.Count);
        }

        [Fact]
        public async void RebootAsyncTest()
        {
            string[] requests = new string[]
            {
                "host:transport:169.254.109.177:5555",
                "reboot:"
            };

            await RunTestAsync(
                new AdbResponse[] { AdbResponse.OK, AdbResponse.OK },
                NoResponseMessages,
                requests,
                () => TestClient.RebootAsync(Device));
        }

        [Fact]
        public async void PairAsyncIPAddressTest() =>
            await RunPairAsyncTest(
                () => TestClient.PairAsync(IPAddress.Loopback, "114514"),
                "127.0.0.1:5555",
                "114514");

        [Fact]
        public async void PairAsyncDnsEndpointTest() =>
            await RunPairAsyncTest(
                () => TestClient.PairAsync(new DnsEndPoint("localhost", 1234), "114514"),
                "localhost:1234",
                "114514");

        [Fact]
        public async void PairAsyncIPEndpointTest() =>
            await RunPairAsyncTest(
                () => TestClient.PairAsync(new IPEndPoint(IPAddress.Loopback, 4321), "114514"),
                "127.0.0.1:4321",
                "114514");

        [Fact]
        public async void PairAsyncHostEndpointTest() =>
            await RunPairAsyncTest(
                () => TestClient.PairAsync("localhost:9926", "114514"),
                "localhost:9926",
                "114514");

        [Fact]
        public async void PairAsyncIPAddressNullTest() =>
            _ = await Assert.ThrowsAsync<ArgumentNullException>(() => TestClient.PairAsync((IPAddress)null, "114514"));

        [Fact]
        public async void PairAsyncDnsEndpointNullTest() =>
            _ = await Assert.ThrowsAsync<ArgumentNullException>(() => TestClient.PairAsync(null, "114514"));

        [Fact]
        public async void PairAsyncIPEndpointNullTest() =>
            _ = await Assert.ThrowsAsync<ArgumentNullException>(() => TestClient.PairAsync((IPEndPoint)null, "114514"));

        [Fact]
        public async void PairAsyncHostEndpointNullTest() =>
            _ = await Assert.ThrowsAsync<ArgumentNullException>(() => TestClient.PairAsync((string)null, "114514"));

        [Fact]
        public async void ConnectAsyncIPAddressTest() =>
            await RunConnectAsyncTest(
                () => TestClient.ConnectAsync(IPAddress.Loopback),
                "127.0.0.1:5555");

        [Fact]
        public async void ConnectAsyncDnsEndpointTest() =>
            await RunConnectAsyncTest(
                () => TestClient.ConnectAsync(new DnsEndPoint("localhost", 1234)),
                "localhost:1234");

        [Fact]
        public async void ConnectAsyncIPEndpointTest() =>
            await RunConnectAsyncTest(
                () => TestClient.ConnectAsync(new IPEndPoint(IPAddress.Loopback, 4321)),
                "127.0.0.1:4321");

        [Fact]
        public async void ConnectAsyncHostEndpointTest() =>
            await RunConnectAsyncTest(
                () => TestClient.ConnectAsync("localhost:9926"),
                "localhost:9926");

        [Fact]
        public async void ConnectAsyncIPAddressNullTest() =>
            _ = await Assert.ThrowsAsync<ArgumentNullException>(() => TestClient.ConnectAsync((IPAddress)null));

        [Fact]
        public async void ConnectAsyncDnsEndpointNullTest() =>
            _ = await Assert.ThrowsAsync<ArgumentNullException>(() => TestClient.ConnectAsync(null));

        [Fact]
        public async void ConnectAsyncIPEndpointNullTest() =>
            _ = await Assert.ThrowsAsync<ArgumentNullException>(() => TestClient.ConnectAsync((IPEndPoint)null));

        [Fact]
        public async void ConnectAsyncHostEndpointNullTest() =>
            _ = await Assert.ThrowsAsync<ArgumentNullException>(() => TestClient.ConnectAsync((string)null));

        [Fact]
        public async void DisconnectAsyncTest()
        {
            string[] requests = new string[] { "host:disconnect:localhost:5555" };
            string[] responseMessages = new string[] { "disconnected 127.0.0.1:5555" };

            await RunTestAsync(
                OkResponse,
                responseMessages,
                requests,
                () => TestClient.DisconnectAsync(new DnsEndPoint("localhost", 5555)));
        }

        [Fact]
        public async void RootAsyncTest()
        {
            DeviceData device = new()
            {
                Serial = "009d1cd696d5194a",
                State = DeviceState.Online
            };

            string[] requests = new string[]
            {
                "host:transport:009d1cd696d5194a",
                "root:"
            };

            byte[] expectedData = new byte[1024];
            byte[] expectedString = Encoding.UTF8.GetBytes("adbd cannot run as root in production builds\n");
            Buffer.BlockCopy(expectedString, 0, expectedData, 0, expectedString.Length);

            _ = await Assert.ThrowsAsync<AdbException>(() =>
            RunTestAsync(
                new AdbResponse[] { AdbResponse.OK, AdbResponse.OK },
                NoResponseMessages,
                requests,
                Array.Empty<(SyncCommand, string)>(),
                Array.Empty<SyncCommand>(),
                new byte[][] { expectedData },
                Array.Empty<byte[]>(),
                () => TestClient.RootAsync(device)));
        }

        [Fact]
        public async void UnrootAsyncTest()
        {
            DeviceData device = new()
            {
                Serial = "009d1cd696d5194a",
                State = DeviceState.Online
            };

            string[] requests = new string[]
            {
                "host:transport:009d1cd696d5194a",
                "unroot:"
            };

            byte[] expectedData = new byte[1024];
            byte[] expectedString = Encoding.UTF8.GetBytes("adbd not running as root\n");
            Buffer.BlockCopy(expectedString, 0, expectedData, 0, expectedString.Length);

            _ = await Assert.ThrowsAsync<AdbException>(() =>
            RunTestAsync(
                new AdbResponse[] { AdbResponse.OK, AdbResponse.OK },
                NoResponseMessages,
                requests,
                Array.Empty<(SyncCommand, string)>(),
                Array.Empty<SyncCommand>(),
                new byte[][] { expectedData },
                Array.Empty<byte[]>(),
                () => TestClient.UnrootAsync(device)));
        }

        [Fact]
        public async void InstallAsyncTest()
        {
            DeviceData device = new()
            {
                Serial = "009d1cd696d5194a",
                State = DeviceState.Online
            };

            string[] requests = new string[]
            {
                "host:transport:009d1cd696d5194a",
                "exec:cmd package 'install' -S 205774"
            };

            // The app data is sent in chunks of 32 kb
            Collection<byte[]> applicationDataChuncks = new();

            using (Stream stream = File.OpenRead("Assets/testapp.apk"))
            {
                while (true)
                {
                    byte[] buffer = new byte[32 * 1024];
                    int read = await stream.ReadAsync(buffer);

                    if (read == 0)
                    {
                        break;
                    }
                    else
                    {
                        buffer = buffer.Take(read).ToArray();
                        applicationDataChuncks.Add(buffer);
                    }
                }
            }

            byte[] response = Encoding.UTF8.GetBytes("Success\n");

            using (Stream stream = File.OpenRead("Assets/testapp.apk"))
            {
                await RunTestAsync(
                    new AdbResponse[]
                    {
                        AdbResponse.OK,
                        AdbResponse.OK,
                    },
                    NoResponseMessages,
                    requests,
                    Array.Empty<(SyncCommand, string)>(),
                    Array.Empty<SyncCommand>(),
                    new byte[][] { response },
                    applicationDataChuncks.ToArray(),
                    () => TestClient.InstallAsync(device, stream));
            }
        }

        [Fact]
        public async void InstallCreateAsyncTest()
        {
            DeviceData device = new()
            {
                Serial = "009d1cd696d5194a",
                State = DeviceState.Online
            };

            string[] requests = new string[]
            {
                "host:transport:009d1cd696d5194a",
                "exec:cmd package 'install-create' -p com.google.android.gms"
            };

            byte[] streamData = Encoding.ASCII.GetBytes("Success: created install session [936013062]\r\n");
            using MemoryStream shellStream = new(streamData);

            string session = string.Empty;

            await RunTestAsync(
                new AdbResponse[]
                {
                    AdbResponse.OK,
                    AdbResponse.OK,
                },
                NoResponseMessages,
                requests,
                shellStream,
                async () => session = await TestClient.InstallCreateAsync(device, "com.google.android.gms"));

            Assert.Equal("936013062", session);
        }

        [Fact]
        public async void InstallWriteAsyncTest()
        {
            DeviceData device = new()
            {
                Serial = "009d1cd696d5194a",
                State = DeviceState.Online
            };

            string[] requests = new string[]
            {
                "host:transport:009d1cd696d5194a",
                "exec:cmd package 'install-write' -S 205774 936013062 base.apk"
            };

            // The app data is sent in chunks of 32 kb
            Collection<byte[]> applicationDataChuncks = new();

            using (Stream stream = File.OpenRead("Assets/testapp.apk"))
            {
                while (true)
                {
                    byte[] buffer = new byte[32 * 1024];
                    int read = await stream.ReadAsync(buffer);

                    if (read == 0)
                    {
                        break;
                    }
                    else
                    {
                        buffer = buffer.Take(read).ToArray();
                        applicationDataChuncks.Add(buffer);
                    }
                }
            }

            byte[] response = Encoding.UTF8.GetBytes("Success: streamed 205774 bytes\n");

            using (Stream stream = File.OpenRead("Assets/testapp.apk"))
            {
                await RunTestAsync(
                    new AdbResponse[]
                    {
                        AdbResponse.OK,
                        AdbResponse.OK,
                    },
                    NoResponseMessages,
                    requests,
                    Array.Empty<(SyncCommand, string)>(),
                    Array.Empty<SyncCommand>(),
                    new byte[][] { response },
                    applicationDataChuncks.ToArray(),
                    () => TestClient.InstallWriteAsync(device, stream, "base", "936013062"));
            }
        }

        [Fact]
        public async void InstallCommitAsyncTest()
        {
            DeviceData device = new()
            {
                Serial = "009d1cd696d5194a",
                State = DeviceState.Online
            };

            string[] requests = new string[]
            {
                "host:transport:009d1cd696d5194a",
                "exec:cmd package 'install-commit' 936013062"
            };

            byte[] streamData = Encoding.ASCII.GetBytes("Success\r\n");
            using MemoryStream shellStream = new(streamData);

            await RunTestAsync(
                new AdbResponse[]
                {
                    AdbResponse.OK,
                    AdbResponse.OK,
                },
                NoResponseMessages,
                requests,
                shellStream,
                () => TestClient.InstallCommitAsync(device, "936013062"));
        }

        [Fact]
        public async void GetFeatureSetAsyncTest()
        {
            DeviceData device = new()
            {
                Serial = "009d1cd696d5194a",
                State = DeviceState.Online
            };

            string[] requests = new string[]
            {
                "host-serial:009d1cd696d5194a:features"
            };

            string[] responses = new string[]
            {
                "sendrecv_v2_brotli,remount_shell,sendrecv_v2,abb_exec,fixed_push_mkdir,fixed_push_symlink_timestamp,abb,shell_v2,cmd,ls_v2,apex,stat_v2\r\n"
            };

            IEnumerable<string> features = null;

            await RunTestAsync(
                new AdbResponse[]
                {
                    AdbResponse.OK,
                },
                responses,
                requests,
                async () => features = await TestClient.GetFeatureSetAsync(device));

            Assert.Equal(12, features.Count());
            Assert.Equal("sendrecv_v2_brotli", features.First());
            Assert.Equal("stat_v2", features.Last());
        }

        [Fact]
        public async void DumpScreenStringAsyncTest()
        {
            DeviceData device = new()
            {
                Serial = "009d1cd696d5194a",
                State = DeviceState.Online
            };

            string[] requests = new string[]
            {
                "host:transport:009d1cd696d5194a",
                "shell:uiautomator dump /dev/tty"
            };

            string dump = File.ReadAllText(@"Assets/dumpscreen.txt");
            byte[] streamData = Encoding.UTF8.GetBytes(dump);
            using MemoryStream shellStream = new(streamData);

            string xml = string.Empty;

            await RunTestAsync(
                new AdbResponse[]
                {
                    AdbResponse.OK,
                    AdbResponse.OK,
                },
                NoResponseMessages,
                requests,
                shellStream,
                async () => xml = await TestClient.DumpScreenStringAsync(device));

            Assert.Equal(dump.Replace("Events injected: 1\r\n", "").Replace("UI hierchary dumped to: /dev/tty", "").Trim(), xml);
        }

        [Fact]
        public async void DumpScreenAsyncTest()
        {
            DeviceData device = new()
            {
                Serial = "009d1cd696d5194a",
                State = DeviceState.Online
            };

            string[] requests = new string[]
            {
                "host:transport:009d1cd696d5194a",
                "shell:uiautomator dump /dev/tty"
            };

            string dump = File.ReadAllText(@"Assets/dumpscreen.txt");
            byte[] streamData = Encoding.UTF8.GetBytes(dump);
            using MemoryStream shellStream = new(streamData);

            XmlDocument xml = null;

            await RunTestAsync(
                new AdbResponse[]
                {
                    AdbResponse.OK,
                    AdbResponse.OK,
                },
                NoResponseMessages,
                requests,
                shellStream,
                async () => xml = await TestClient.DumpScreenAsync(device));

            XmlDocument doc = new();
            doc.LoadXml(dump.Replace("Events injected: 1\r\n", "").Replace("UI hierchary dumped to: /dev/tty", "").Trim());

            Assert.Equal(doc, xml);
        }

        [Fact]
        public async void FindElementAsyncTest()
        {
            DeviceData device = new()
            {
                Serial = "009d1cd696d5194a",
                State = DeviceState.Online
            };

            string[] requests = new string[]
            {
                "host:transport:009d1cd696d5194a",
                "shell:uiautomator dump /dev/tty"
            };

            string dump = File.ReadAllText(@"Assets/dumpscreen.txt");
            byte[] streamData = Encoding.UTF8.GetBytes(dump);
            using MemoryStream shellStream = new(streamData);

            await RunTestAsync(
                new AdbResponse[]
                {
                    AdbResponse.OK,
                    AdbResponse.OK,
                },
                NoResponseMessages,
                requests,
                shellStream,
                async () =>
                {
                    Element element = await TestClient.FindElementAsync(device);
                    Assert.Equal(144, element.GetChildCount());
                    element = element[0][0][0][0][0][0][0][0][2][1][0][0];
                    Assert.Equal("where-where", element.Attributes["text"]);
                    Assert.Equal(Area.FromLTRB(45, 889, 427, 973), element.Area);
                });
        }

        [Fact]
        public async void FindElementsAsyncTest()
        {
            DeviceData device = new()
            {
                Serial = "009d1cd696d5194a",
                State = DeviceState.Online
            };

            string[] requests = new string[]
            {
                "host:transport:009d1cd696d5194a",
                "shell:uiautomator dump /dev/tty"
            };

            string dump = File.ReadAllText(@"Assets/dumpscreen.txt");
            byte[] streamData = Encoding.UTF8.GetBytes(dump);
            using MemoryStream shellStream = new(streamData);

            await RunTestAsync(
                new AdbResponse[]
                {
                    AdbResponse.OK,
                    AdbResponse.OK,
                },
                NoResponseMessages,
                requests,
                shellStream,
                async () =>
                {
                    List<Element> elements = await TestClient.FindElementsAsync(device);
                    int childCount = elements.Count;
                    elements.ForEach(x => childCount += x.GetChildCount());
                    Assert.Equal(145, childCount);
                    Element element = elements[0][0][0][0][0][0][0][0][0][2][1][0][0];
                    Assert.Equal("where-where", element.Attributes["text"]);
                    Assert.Equal(Area.FromLTRB(45, 889, 427, 973), element.Area);
                });
        }

        private Task RunConnectAsyncTest(Func<Task> test, string connectString)
        {
            string[] requests = new string[]
            {
                $"host:connect:{connectString}"
            };

            string[] responseMessages = new string[]
            {
                $"connected to {connectString}"
            };

            return RunTestAsync(
                OkResponse,
                responseMessages,
                requests,
                test);
        }

        private Task RunPairAsyncTest(Func<Task> test, string connectString, string code)
        {
            string[] requests = new string[]
            {
                $"host:pair:{code}:{connectString}"
            };

            string[] responseMessages = new string[]
            {
                $"Successfully paired to {connectString} [guid=adb-996198a3-xPRwsQ]"
            };

            return RunTestAsync(
                OkResponse,
                responseMessages,
                requests,
                test);
        }

        private Task RunCreateReverseAsyncTest(Func<DeviceData, Task> test, string reverseString)
        {
            string[] requests = new string[]
            {
                "host:transport:169.254.109.177:5555",
                $"reverse:forward:{reverseString}",
            };

            return RunTestAsync(
                new AdbResponse[]
                {
                    AdbResponse.OK,
                    AdbResponse.OK,
                    AdbResponse.OK
                },
                new string[]
                {
                    null
                },
                requests,
                () => test(Device));
        }

        private Task RunCreateForwardAsyncTest(Func<DeviceData, Task> test, string forwardString)
        {
            string[] requests = new string[]
            {
                $"host-serial:169.254.109.177:5555:forward:{forwardString}"
            };

            return RunTestAsync(
                new AdbResponse[]
                {
                    AdbResponse.OK,
                    AdbResponse.OK
                },
                new string[]
                {
                    null
                },
                requests,
                () => test(Device));
        }
    }
}
