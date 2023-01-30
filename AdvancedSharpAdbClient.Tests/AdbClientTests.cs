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
using Xunit;

namespace AdvancedSharpAdbClient.Tests
{
    /// <summary>
    /// Tests the <see cref="AdbClient"/> class.
    /// </summary>
    public class AdbClientTests : SocketBasedTests
    {
        // Toggle the integration test flag to true to run on an actual adb server
        // (and to build/validate the test cases), set to false to use the mocked
        // adb sockets.
        // In release mode, this flag is ignored and the mocked adb sockets are always used.
        public AdbClientTests() : base(integrationTest: false, doDispose: false)
        {
            lock (FactoriesTests.locker)
            {
                Factories.Reset();
            }
        }

        [Fact]
        public void ConstructorNullTest() =>
            _ = Assert.Throws<ArgumentNullException>(() => new AdbClient(null, Factories.AdbSocketFactory));

        [Fact]
        public void ConstructorInvalidEndPointTest() =>
            _ = Assert.Throws<NotSupportedException>(() => new AdbClient(new CustomEndPoint(), Factories.AdbSocketFactory));

        [Fact]
        public void ConstructorTest()
        {
            AdbClient adbClient = new();
            Assert.NotNull(adbClient);
            Assert.NotNull(adbClient.EndPoint);
            Assert.IsType<IPEndPoint>(adbClient.EndPoint);

            IPEndPoint endPoint = (IPEndPoint)adbClient.EndPoint;

            Assert.Equal(IPAddress.Loopback, endPoint.Address);
            Assert.Equal(AdbClient.AdbServerPort, endPoint.Port);
        }

        [Fact]
        public void FormAdbRequestTest()
        {
            Assert.Equal(Encoding.ASCII.GetBytes("0009host:kill"), AdbClient.FormAdbRequest("host:kill"));
            Assert.Equal(Encoding.ASCII.GetBytes("000Chost:version"), AdbClient.FormAdbRequest("host:version"));
        }

        [Fact]
        public void CreateAdbForwardRequestTest()
        {
            Assert.Equal(Encoding.ASCII.GetBytes("0008tcp:1984"), AdbClient.CreateAdbForwardRequest(null, 1984));
            Assert.Equal(Encoding.ASCII.GetBytes("0012tcp:1981:127.0.0.1"), AdbClient.CreateAdbForwardRequest("127.0.0.1", 1981));
        }

        [Fact]
        public void KillAdbTest()
        {
            string[] requests = new string[]
            {
                "host:kill"
            };

            RunTest(
                NoResponses,
                NoResponseMessages,
                requests,
                TestClient.KillAdb);
        }

        [Fact]
        public void GetAdbVersionTest()
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

            RunTest(
                OkResponse,
                responseMessages,
                requests,
                () => version = TestClient.GetAdbVersion());

            // Make sure and the correct value is returned.
            Assert.Equal(32, version);
        }

        [Fact]
        public void GetDevicesTest()
        {
            string[] responseMessages = new string[]
            {
                "169.254.109.177:5555   device product:VS Emulator 5\" KitKat (4.4) XXHDPI Phone model:5__KitKat__4_4__XXHDPI_Phone device:donatello\n"
            };

            string[] requests = new string[]
            {
                "host:devices-l"
            };

            List<DeviceData> devices = null;

            RunTest(
                OkResponse,
                responseMessages,
                requests,
                () => devices = TestClient.GetDevices());

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
        public void SetDeviceTest()
        {
            string[] requests = new string[]
            {
                "host:transport:169.254.109.177:5555"
            };

            RunTest(
                OkResponse,
                NoResponseMessages,
                requests,
                () => Socket.SetDevice(Device));
        }

        [Fact]
        public void SetInvalidDeviceTest()
        {
            string[] requests = new string[]
            {
                "host:transport:169.254.109.177:5555"
            };

            _ = Assert.Throws<DeviceNotFoundException>(() =>
            RunTest(
                new AdbResponse[] { AdbResponse.FromError("device not found") },
                NoResponseMessages,
                requests,
                () => Socket.SetDevice(Device)));
        }

        [Fact]
        public void SetDeviceOtherException()
        {
            string[] requests = new string[]
            {
                "host:transport:169.254.109.177:5555"
            };

            _ = Assert.Throws<AdbException>(() =>
            RunTest(
                new AdbResponse[] { AdbResponse.FromError("Too many cats.") },
                NoResponseMessages,
                requests,
                () => Socket.SetDevice(Device)));
        }

        [Fact]
        public void RebootTest()
        {
            string[] requests = new string[]
            {
                "host:transport:169.254.109.177:5555",
                "reboot:"
            };

            RunTest(
                new AdbResponse[] { AdbResponse.OK, AdbResponse.OK },
                NoResponseMessages,
                requests,
                () =>
                TestClient.Reboot(Device));
        }

        [Fact]
        public void ExecuteRemoteCommandTest()
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
            MemoryStream shellStream = new(streamData);

            ConsoleOutputReceiver receiver = new();

            RunTest(
                responses,
                responseMessages,
                requests,
                shellStream,
                () => TestClient.ExecuteRemoteCommand("echo Hello, World", device, receiver));

            Assert.Equal("Hello, World\r\n", receiver.ToString(), ignoreLineEndingDifferences: true);
        }

        [Fact]
        public void ExecuteRemoteCommandUnresponsiveTest()
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

            _ = Assert.Throws<ShellCommandUnresponsiveException>(() =>
            RunTest(
                responses,
                responseMessages,
                requests,
                null,
                () => TestClient.ExecuteRemoteCommand("echo Hello, World", device, receiver)));
        }

        [Fact]
        public void CreateForwardTest() =>
            RunCreateForwardTest(
                (device) => TestClient.CreateForward(device, "tcp:1", "tcp:2", true),
                "tcp:1;tcp:2");


        [Fact]
        public void CreateReverseTest() =>
            RunCreateReverseTest(
                (device) => TestClient.CreateReverseForward(device, "tcp:1", "tcp:2", true),
                "tcp:1;tcp:2");

        [Fact]
        public void CreateTcpForwardTest() =>
            RunCreateForwardTest(
                (device) => TestClient.CreateForward(device, 3, 4),
                "tcp:3;tcp:4");

        [Fact]
        public void CreateSocketForwardTest() =>
            RunCreateForwardTest(
                (device) => TestClient.CreateForward(device, 5, "/socket/1"),
                "tcp:5;local:/socket/1");

        [Fact]
        public void CreateDuplicateForwardTest()
        {
            AdbResponse[] responses = new AdbResponse[]
            {
                AdbResponse.FromError("cannot rebind existing socket")
            };

            string[] requests = new string[]
            {
                "host-serial:169.254.109.177:5555:forward:norebind:tcp:1;tcp:2"
            };

            _ = Assert.Throws<AdbException>(() =>
            RunTest(
                responses,
                NoResponseMessages,
                requests,
                () => TestClient.CreateForward(Device, "tcp:1", "tcp:2", false)));
        }

        [Fact]
        public void ListForwardTest()
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

            RunTest(
                OkResponse,
                responseMessages,
                requests,
                () => forwards = TestClient.ListForward(Device).ToArray());

            Assert.NotNull(forwards);
            Assert.Equal(3, forwards.Length);
            Assert.Equal("169.254.109.177:5555", forwards[0].SerialNumber);
            Assert.Equal("tcp:1", forwards[0].Local);
            Assert.Equal("tcp:2", forwards[0].Remote);
        }

        [Fact]
        public void ListReverseForwardTest()
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

            RunTest(
                responses,
                responseMessages,
                requests,
                () => forwards = TestClient.ListReverseForward(Device).ToArray());

            Assert.NotNull(forwards);
            Assert.Equal(3, forwards.Length);
            Assert.Equal("(reverse)", forwards[0].SerialNumber);
            Assert.Equal("localabstract:scrcpy", forwards[0].Local);
            Assert.Equal("tcp:100", forwards[0].Remote);
        }

        [Fact]
        public void RemoveForwardTest()
        {
            string[] requests = new string[]
            {
                "host-serial:169.254.109.177:5555:killforward:tcp:1"
            };

            RunTest(
                OkResponse,
                NoResponseMessages,
                requests,
                () => TestClient.RemoveForward(Device, 1));
        }

        [Fact]
        public void RemoveReverseForwardTest()
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

            RunTest(
                responses,
                NoResponseMessages,
                requests,
                () => TestClient.RemoveReverseForward(Device, "localabstract:test"));
        }

        [Fact]
        public void RemoveAllForwardsTest()
        {
            string[] requests = new string[]
            {
                "host-serial:169.254.109.177:5555:killforward-all"
            };

            RunTest(
                OkResponse,
                NoResponseMessages,
                requests,
                () => TestClient.RemoveAllForwards(Device));
        }

        [Fact]
        public void RemoveAllReversesTest()
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

            RunTest(
                responses,
                NoResponseMessages,
                requests,
                () => TestClient.RemoveAllReverseForwards(Device));
        }

        [Fact]
        public void PairIPAddressTest() =>
            RunPairTest(
                () => TestClient.Pair(IPAddress.Loopback, "114514"),
                "127.0.0.1:5555",
                "114514");

        [Fact]
        public void PairDnsEndpointTest() =>
            RunPairTest(
                () => TestClient.Pair(new DnsEndPoint("localhost", 1234), "114514"),
                "localhost:1234",
                "114514");

        [Fact]
        public void PairIPEndpointTest() =>
            RunPairTest(
                () => TestClient.Pair(new IPEndPoint(IPAddress.Loopback, 4321), "114514"),
                "127.0.0.1:4321",
                "114514");

        [Fact]
        public void PairHostEndpointTest() =>
            RunPairTest(
                () => TestClient.Pair("localhost:9926", "114514"),
                "localhost:9926",
                "114514");

        [Fact]
        public async Task PairIPAddressNullTest() =>
            _ = await Assert.ThrowsAsync<ArgumentNullException>(() => TestClient.PairAsync((IPAddress)null, "114514"));

        [Fact]
        public async Task PairDnsEndpointNullTest() =>
            _ = await Assert.ThrowsAsync<ArgumentNullException>(() => TestClient.PairAsync(null, "114514"));

        [Fact]
        public async Task PairIPEndpointNullTest() =>
            _ = await Assert.ThrowsAsync<ArgumentNullException>(() => TestClient.PairAsync((IPEndPoint)null, "114514"));

        [Fact]
        public async Task PairHostEndpointNullTest() =>
            _ = await Assert.ThrowsAsync<ArgumentNullException>(() => TestClient.PairAsync((string)null, "114514"));

        [Fact]
        public void ConnectIPAddressTest() =>
            RunConnectTest(
                () => TestClient.Connect(IPAddress.Loopback),
                "127.0.0.1:5555");

        [Fact]
        public void ConnectDnsEndpointTest() =>
            RunConnectTest(
                () => TestClient.Connect(new DnsEndPoint("localhost", 1234)),
                "localhost:1234");

        [Fact]
        public void ConnectIPEndpointTest() =>
            RunConnectTest(
                () => TestClient.Connect(new IPEndPoint(IPAddress.Loopback, 4321)),
                "127.0.0.1:4321");

        [Fact]
        public void ConnectHostEndpointTest() =>
            RunConnectTest(
                () => TestClient.Connect("localhost:9926"),
                "localhost:9926");

        [Fact]
        public async Task ConnectIPAddressNullTest() =>
            _ = await Assert.ThrowsAsync<ArgumentNullException>(() => TestClient.ConnectAsync((IPAddress)null));

        [Fact]
        public async Task ConnectDnsEndpointNullTest() =>
            _ = await Assert.ThrowsAsync<ArgumentNullException>(() => TestClient.ConnectAsync(null));

        [Fact]
        public async Task ConnectIPEndpointNullTest() =>
            _ = await Assert.ThrowsAsync<ArgumentNullException>(() => TestClient.ConnectAsync((IPEndPoint)null));

        [Fact]
        public async Task ConnectHostEndpointNullTest() =>
            _ = await Assert.ThrowsAsync<ArgumentNullException>(() => TestClient.ConnectAsync((string)null));

        [Fact]
        public void DisconnectTest()
        {
            string[] requests = new string[] { "host:disconnect:localhost:5555" };
            string[] responseMessages = new string[] { "disconnected 127.0.0.1:5555" };

            RunTest(
                OkResponse,
                responseMessages,
                requests,
                () => TestClient.Disconnect(new DnsEndPoint("localhost", 5555)));
        }

        [Fact]
        public void ReadLogTest()
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

            RunTest(
                responses,
                responseMessages,
                requests,
                shellStream,
                () => TestClient.RunLogServiceAsync(device, sink, CancellationToken.None, LogId.System).Wait());

            Assert.Equal(3, logs.Count);
        }

        [Fact]
        public void RootTest()
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

            _ = Assert.Throws<AdbException>(() =>
            RunTest(
                new AdbResponse[] { AdbResponse.OK, AdbResponse.OK },
                NoResponseMessages,
                requests,
                Array.Empty<(SyncCommand, string)>(),
                Array.Empty<SyncCommand>(),
                new byte[][] { expectedData },
                Array.Empty<byte[]>(),
                () => TestClient.Root(device)));
        }

        [Fact]
        public void UnrootTest()
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

            _ = Assert.Throws<AdbException>(() =>
            RunTest(
                new AdbResponse[] { AdbResponse.OK, AdbResponse.OK },
                NoResponseMessages,
                requests,
                Array.Empty<(SyncCommand, string)>(),
                Array.Empty<SyncCommand>(),
                new byte[][] { expectedData },
                Array.Empty<byte[]>(),
                () => TestClient.Unroot(device)));
        }

        [Fact]
        public void InstallTest()
        {
            DeviceData device = new()
            {
                Serial = "009d1cd696d5194a",
                State = DeviceState.Online
            };

            string[] requests = new string[]
            {
                "host:transport:009d1cd696d5194a",
                "exec:cmd package 'install'  -S 205774"
            };

            // The app data is sent in chunks of 32 kb
            Collection<byte[]> applicationDataChuncks = new();

            using (Stream stream = File.OpenRead("Assets/testapp.apk"))
            {
                while (true)
                {
                    byte[] buffer = new byte[32 * 1024];
                    int read = stream.Read(buffer, 0, buffer.Length);

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
                RunTest(
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
                    () => TestClient.Install(device, stream));
            }
        }

        private void RunPairTest(Action test, string connectString, string code)
        {
            string[] requests = new string[]
            {
                $"host:pair:{code}:{connectString}"
            };

            string[] responseMessages = new string[]
            {
                $"Successfully paired to {connectString} [guid=adb-996198a3-xPRwsQ]"
            };

            RunTest(
                OkResponse,
                responseMessages,
                requests,
                test);
        }

        private void RunConnectTest(Action test, string connectString)
        {
            string[] requests = new string[]
            {
                $"host:connect:{connectString}"
            };

            string[] responseMessages = new string[]
            {
                $"connected to {connectString}"
            };

            RunTest(
                OkResponse,
                responseMessages,
                requests,
                test);
        }

        private void RunCreateReverseTest(Action<DeviceData> test, string reverseString)
        {
            string[] requests = new string[]
            {
                "host:transport:169.254.109.177:5555",
                $"reverse:forward:{reverseString}",
            };

            RunTest(
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

        private void RunCreateForwardTest(Action<DeviceData> test, string forwardString)
        {
            string[] requests = new string[]
            {
                $"host-serial:169.254.109.177:5555:forward:{forwardString}"
            };

            RunTest(
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
