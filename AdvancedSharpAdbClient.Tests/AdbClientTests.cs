using AdvancedSharpAdbClient.Exceptions;
using AdvancedSharpAdbClient.Logs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;
using Xunit;
using System.Data.Common;

namespace AdvancedSharpAdbClient.Tests
{
    /// <summary>
    /// Tests the <see cref="AdbClient"/> class.
    /// </summary>
    public partial class AdbClientTests : SocketBasedTests
    {
        /// <summary>
        /// Toggle the integration test flag to true to run on an actual adb server
        /// (and to build/validate the test cases), set to false to use the mocked
        /// adb sockets.
        /// In release mode, this flag is ignored and the mocked adb sockets are always used.
        /// </summary>
        public AdbClientTests() : base(integrationTest: false, doDispose: false)
        {
        }

        /// <summary>
        /// Tests the <see cref="AdbClient(EndPoint, Func{EndPoint, IAdbSocket})"/> method.
        /// </summary>
        [Fact]
        public void ConstructorNullTest() =>
            _ = Assert.Throws<ArgumentNullException>(() => new AdbClient(null, Factories.AdbSocketFactory));

        /// <summary>
        /// Tests the <see cref="AdbClient(EndPoint, Func{EndPoint, IAdbSocket})"/> method.
        /// </summary>
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

        /// <summary>
        /// Tests the <see cref="AdbClient.FormAdbRequest(string)"/> method.
        /// </summary>
        [Fact]
        public void FormAdbRequestTest()
        {
            Assert.Equal(Encoding.ASCII.GetBytes("0009host:kill"), AdbClient.FormAdbRequest("host:kill"));
            Assert.Equal(Encoding.ASCII.GetBytes("000Chost:version"), AdbClient.FormAdbRequest("host:version"));
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.CreateAdbForwardRequest(string, int)"/> method.
        /// </summary>
        [Fact]
        public void CreateAdbForwardRequestTest()
        {
            Assert.Equal(Encoding.ASCII.GetBytes("0008tcp:1984"), AdbClient.CreateAdbForwardRequest(null, 1984));
            Assert.Equal(Encoding.ASCII.GetBytes("0012tcp:1981:127.0.0.1"), AdbClient.CreateAdbForwardRequest("127.0.0.1", 1981));
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.GetAdbVersion"/> method.
        /// </summary>
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

        /// <summary>
        /// Tests the <see cref="AdbClient.KillAdb"/> method.
        /// </summary>
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

        /// <summary>
        /// Tests the <see cref="AdbClient.GetDevices"/> method.
        /// </summary>
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

            IEnumerable<DeviceData> devices = null;

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

        /// <summary>
        /// Tests the <see cref="IAdbSocket.SetDevice(DeviceData)"/> method.
        /// </summary>
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

        /// <summary>
        /// Tests the <see cref="IAdbSocket.SetDevice(DeviceData)"/> method.
        /// </summary>
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

        /// <summary>
        /// Tests the <see cref="IAdbSocket.SetDevice(DeviceData)"/> method.
        /// </summary>
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

        /// <summary>
        /// Tests the <see cref="AdbClient.CreateForward(DeviceData, string, string, bool)"/> method.
        /// </summary>
        [Fact]
        public void CreateForwardTest() =>
            RunCreateForwardTest(
                (device) => TestClient.CreateForward(device, "tcp:1", "tcp:2", true),
                "tcp:1;tcp:2");

        /// <summary>
        /// Tests the <see cref="AdbClient.CreateReverseForward(DeviceData, string, string, bool)"/> method.
        /// </summary>
        [Fact]
        public void CreateReverseTest() =>
            RunCreateReverseTest(
                (device) => TestClient.CreateReverseForward(device, "tcp:1", "tcp:2", true),
                "tcp:1;tcp:2");

        /// <summary>
        /// Tests the <see cref="AdbClientExtensions.CreateForward(IAdbClient, DeviceData, int, int)"/> method.
        /// </summary>
        [Fact]
        public void CreateTcpForwardTest() =>
            RunCreateForwardTest(
                (device) => TestClient.CreateForward(device, 3, 4),
                "tcp:3;tcp:4");

        /// <summary>
        /// Tests the <see cref="AdbClientExtensions.CreateForward(IAdbClient, DeviceData, int, string)"/> method.
        /// </summary>
        [Fact]
        public void CreateSocketForwardTest() =>
            RunCreateForwardTest(
                (device) => TestClient.CreateForward(device, 5, "/socket/1"),
                "tcp:5;local:/socket/1");

        /// <summary>
        /// Tests the <see cref="AdbClient.CreateForward(DeviceData, string, string, bool)"/> method.
        /// </summary>
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

        /// <summary>
        /// Tests the <see cref="AdbClient.RemoveForward(DeviceData, int)"/> method.
        /// </summary>
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

        /// <summary>
        /// Tests the <see cref="AdbClient.RemoveReverseForward(DeviceData, string)"/> method.
        /// </summary>
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

        /// <summary>
        /// Tests the <see cref="AdbClient.RemoveAllForwards(DeviceData)"/> method.
        /// </summary>
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

        /// <summary>
        /// Tests the <see cref="AdbClient.RemoveAllReverseForwards(DeviceData)"/> method.
        /// </summary>
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

        /// <summary>
        /// Tests the <see cref="AdbClient.ListForward(DeviceData)"/> method.
        /// </summary>
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

        /// <summary>
        /// Tests the <see cref="AdbClient.ListReverseForward(DeviceData)"/> method.
        /// </summary>
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

        /// <summary>
        /// Tests the <see cref="AdbClient.ExecuteRemoteCommand(string, DeviceData, IShellOutputReceiver)"/> method.
        /// </summary>
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
            using MemoryStream shellStream = new(streamData);

            ConsoleOutputReceiver receiver = new();

            RunTest(
                responses,
                responseMessages,
                requests,
                shellStream,
                () => TestClient.ExecuteRemoteCommand("echo Hello, World", device, receiver));

            Assert.Equal("Hello, World\r\n", receiver.ToString(), ignoreLineEndingDifferences: true);
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.ExecuteRemoteCommand(string, DeviceData, IShellOutputReceiver)"/> method.
        /// </summary>
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
        public void CreateRefreshableFramebufferTest()
        {
            DeviceData device = new()
            {
                Serial = "169.254.109.177:5555",
                State = DeviceState.Online
            };
            Framebuffer framebuffer = TestClient.CreateRefreshableFramebuffer(device);
            Assert.NotNull(framebuffer);
            Assert.Equal(device, framebuffer.Device);
        }

        [Fact]
        public void GetFrameBufferTest()
        {
            DeviceData device = new()
            {
                Serial = "169.254.109.177:5555",
                State = DeviceState.Online
            };

            DummyAdbSocket socket = new();

            socket.Responses.Enqueue(AdbResponse.OK);
            socket.Responses.Enqueue(AdbResponse.OK);

            socket.Requests.Add("host:transport:169.254.109.177:5555");
            socket.Requests.Add("framebuffer:");

            socket.SyncDataReceived.Enqueue(File.ReadAllBytes("Assets/framebufferheader.bin"));
            socket.SyncDataReceived.Enqueue(File.ReadAllBytes("Assets/framebuffer.bin"));

            Framebuffer framebuffer = null;

            lock (FactoriesTests.locker)
            {
                Factories.AdbSocketFactory = (endPoint) => socket;
                framebuffer = TestClient.GetFrameBuffer(device);
            }

            Assert.NotNull(framebuffer);
            Assert.Equal(device, framebuffer.Device);
            Assert.Equal(16, framebuffer.Data.Length);

            FramebufferHeader header = framebuffer.Header;

            Assert.Equal(8u, header.Alpha.Length);
            Assert.Equal(24u, header.Alpha.Offset);
            Assert.Equal(8u, header.Green.Length);
            Assert.Equal(8u, header.Green.Offset);
            Assert.Equal(8u, header.Red.Length);
            Assert.Equal(0u, header.Red.Offset);
            Assert.Equal(8u, header.Blue.Length);
            Assert.Equal(16u, header.Blue.Offset);
            Assert.Equal(32u, header.Bpp);
            Assert.Equal(4u, header.Size);
            Assert.Equal(1u, header.Height);
            Assert.Equal(1u, header.Width);
            Assert.Equal(1u, header.Version);
            Assert.Equal(0u, header.ColorSpace);

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                using Bitmap image = framebuffer.ToImage();
                Assert.NotNull(image);
                Assert.Equal(PixelFormat.Format32bppArgb, image.PixelFormat);

                Assert.Equal(1, image.Width);
                Assert.Equal(1, image.Height);

                Color pixel = image.GetPixel(0, 0);
                Assert.Equal(0x35, pixel.R);
                Assert.Equal(0x4a, pixel.G);
                Assert.Equal(0x4c, pixel.B);
                Assert.Equal(0xff, pixel.A);
            }

            framebuffer.Dispose();
        }

        [Fact]
        public void RunLogServiceTest()
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
                () => TestClient.RunLogService(device, sink, LogId.System));

            Assert.Equal(3, logs.Count);
        }

        /// <summary>
        /// Tests the <see cref="AdbClientExtensions.Reboot(IAdbClient, DeviceData)"/> method.
        /// </summary>
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
                () => TestClient.Reboot(Device));
        }

        /// <summary>
        /// Tests the <see cref="AdbClientExtensions.Pair(IAdbClient, IPAddress, string)"/> method.
        /// </summary>
        [Fact]
        public void PairIPAddressTest() =>
            RunPairTest(
                () => TestClient.Pair(IPAddress.Loopback, "114514"),
                "127.0.0.1:5555",
                "114514");

        /// <summary>
        /// Tests the <see cref="AdbClient.Pair(DnsEndPoint, string)"/> method.
        /// </summary>
        [Fact]
        public void PairDnsEndpointTest() =>
            RunPairTest(
                () => TestClient.Pair(new DnsEndPoint("localhost", 1234), "114514"),
                "localhost:1234",
                "114514");

        /// <summary>
        /// Tests the <see cref="AdbClientExtensions.Pair(IAdbClient, IPEndPoint, string)"/> method.
        /// </summary>
        [Fact]
        public void PairIPEndpointTest() =>
            RunPairTest(
                () => TestClient.Pair(new IPEndPoint(IPAddress.Loopback, 4321), "114514"),
                "127.0.0.1:4321",
                "114514");

        /// <summary>
        /// Tests the <see cref="AdbClientExtensions.Pair(IAdbClient, string, string)"/> method.
        /// </summary>
        [Fact]
        public void PairHostEndpointTest() =>
            RunPairTest(
                () => TestClient.Pair("localhost:9926", "114514"),
                "localhost:9926",
                "114514");

        /// <summary>
        /// Tests the <see cref="AdbClientExtensions.Pair(IAdbClient, IPAddress, string)"/> method.
        /// </summary>
        [Fact]
        public void PairIPAddressNullTest() =>
            _ = Assert.Throws<ArgumentNullException>(() => TestClient.Pair((IPAddress)null, "114514"));

        /// <summary>
        /// Tests the <see cref="AdbClient.Pair(DnsEndPoint, string)"/> method.
        /// </summary>
        [Fact]
        public void PairDnsEndpointNullTest() =>
            _ = Assert.Throws<ArgumentNullException>(() => TestClient.Pair(null, "114514"));

        /// <summary>
        /// Tests the <see cref="AdbClientExtensions.Pair(IAdbClient, IPEndPoint, string)"/> method.
        /// </summary>
        [Fact]
        public void PairIPEndpointNullTest() =>
            _ = Assert.Throws<ArgumentNullException>(() => TestClient.Pair((IPEndPoint)null, "114514"));

        /// <summary>
        /// Tests the <see cref="AdbClientExtensions.Pair(IAdbClient, string, string)"/> method.
        /// </summary>
        [Fact]
        public void PairHostEndpointNullTest() =>
            _ = Assert.Throws<ArgumentNullException>(() => TestClient.Pair((string)null, "114514"));

        /// <summary>
        /// Tests the <see cref="AdbClientExtensions.Connect(IAdbClient, IPAddress)"/> method.
        /// </summary>
        [Fact]
        public void ConnectIPAddressTest() =>
            RunConnectTest(
                () => TestClient.Connect(IPAddress.Loopback),
                "127.0.0.1:5555");

        /// <summary>
        /// Tests the <see cref="AdbClient.Connect(DnsEndPoint)"/> method.
        /// </summary>
        [Fact]
        public void ConnectDnsEndpointTest() =>
            RunConnectTest(
                () => TestClient.Connect(new DnsEndPoint("localhost", 1234)),
                "localhost:1234");

        /// <summary>
        /// Tests the <see cref="AdbClientExtensions.Connect(IAdbClient, IPEndPoint)"/> method.
        /// </summary>
        [Fact]
        public void ConnectIPEndpointTest() =>
            RunConnectTest(
                () => TestClient.Connect(new IPEndPoint(IPAddress.Loopback, 4321)),
                "127.0.0.1:4321");

        /// <summary>
        /// Tests the <see cref="AdbClientExtensions.Connect(IAdbClient, string, int)"/> method.
        /// </summary>
        [Fact]
        public void ConnectHostEndpointTest() =>
            RunConnectTest(
                () => TestClient.Connect("localhost:9926"),
                "localhost:9926");

        /// <summary>
        /// Tests the <see cref="AdbClientExtensions.Connect(IAdbClient, IPAddress)"/> method.
        /// </summary>
        [Fact]
        public void ConnectIPAddressNullTest() =>
            _ = Assert.Throws<ArgumentNullException>(() => TestClient.Connect((IPAddress)null));

        /// <summary>
        /// Tests the <see cref="AdbClient.Connect(DnsEndPoint)"/> method.
        /// </summary>
        [Fact]
        public void ConnectDnsEndpointNullTest() =>
            _ = Assert.Throws<ArgumentNullException>(() => TestClient.Connect(null));

        /// <summary>
        /// Tests the <see cref="AdbClientExtensions.Connect(IAdbClient, IPEndPoint)"/> method.
        /// </summary>
        [Fact]
        public void ConnectIPEndpointNullTest() =>
            _ = Assert.Throws<ArgumentNullException>(() => TestClient.Connect((IPEndPoint)null));

        /// <summary>
        /// Tests the <see cref="AdbClientExtensions.Connect(IAdbClient, string, int)"/> method.
        /// </summary>
        [Fact]
        public void ConnectHostEndpointNullTest() =>
            _ = Assert.Throws<ArgumentNullException>(() => TestClient.Connect((string)null));

        /// <summary>
        /// Tests the <see cref="AdbClient.Disconnect(DnsEndPoint)"/> method.
        /// </summary>
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

        /// <summary>
        /// Tests the <see cref="AdbClient.Root(DeviceData)"/> method.
        /// </summary>
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

        /// <summary>
        /// Tests the <see cref="AdbClient.Unroot(DeviceData)"/> method.
        /// </summary>
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

        /// <summary>
        /// Tests the <see cref="AdbClient.Install(DeviceData, Stream, string[])"/> method.
        /// </summary>
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
                "exec:cmd package 'install' -S 205774"
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

        /// <summary>
        /// Tests the <see cref="AdbClient.InstallCreate(DeviceData, string, string[])"/> method.
        /// </summary>
        [Fact]
        public void InstallCreateTest()
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

            RunTest(
                new AdbResponse[]
                {
                    AdbResponse.OK,
                    AdbResponse.OK,
                },
                NoResponseMessages,
                requests,
                shellStream,
                () => session = TestClient.InstallCreate(device, "com.google.android.gms"));

            Assert.Equal("936013062", session);
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.InstallWrite(DeviceData, Stream, string, string)"/> method.
        /// </summary>
        [Fact]
        public void InstallWriteTest()
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

            byte[] response = Encoding.UTF8.GetBytes("Success: streamed 205774 bytes\n");

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
                    () => TestClient.InstallWrite(device, stream, "base", "936013062"));
            }
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.InstallCommit(DeviceData, string)"/> method.
        /// </summary>
        [Fact]
        public void InstallCommitTest()
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

            RunTest(
                new AdbResponse[]
                {
                    AdbResponse.OK,
                    AdbResponse.OK,
                },
                NoResponseMessages,
                requests,
                shellStream,
                () => TestClient.InstallCommit(device, "936013062"));
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.GetFeatureSet(DeviceData)"/> method.
        /// </summary>
        [Fact]
        public void GetFeatureSetTest()
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

            RunTest(
                new AdbResponse[]
                {
                    AdbResponse.OK,
                },
                responses,
                requests,
                () => features = TestClient.GetFeatureSet(device));

            Assert.Equal(12, features.Count());
            Assert.Equal("sendrecv_v2_brotli", features.First());
            Assert.Equal("stat_v2", features.Last());
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.DumpScreenString(DeviceData)"/> method.
        /// </summary>
        [Fact]
        public void DumpScreenStringTest()
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

            RunTest(
                new AdbResponse[]
                {
                    AdbResponse.OK,
                    AdbResponse.OK,
                },
                NoResponseMessages,
                requests,
                shellStream,
                () => xml = TestClient.DumpScreenString(device));

            Assert.Equal(dump.Replace("Events injected: 1\r\n", "").Replace("UI hierchary dumped to: /dev/tty", "").Trim(), xml);
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.DumpScreen(DeviceData)"/> method.
        /// </summary>
        [Fact]
        public void DumpScreenTest()
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

            RunTest(
                new AdbResponse[]
                {
                    AdbResponse.OK,
                    AdbResponse.OK,
                },
                NoResponseMessages,
                requests,
                shellStream,
                () => xml = TestClient.DumpScreen(device));

            XmlDocument doc = new();
            doc.LoadXml(dump.Replace("Events injected: 1\r\n", "").Replace("UI hierchary dumped to: /dev/tty", "").Trim());

            Assert.Equal(doc, xml);
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.Click(DeviceData, int, int)"/> method.
        /// </summary>
        [Fact]
        public void ClickTest()
        {
            DeviceData device = new()
            {
                Serial = "009d1cd696d5194a",
                State = DeviceState.Online
            };

            string[] requests = new string[]
            {
                "host:transport:009d1cd696d5194a",
                "shell:input tap 100 100"
            };

            byte[] streamData = Encoding.UTF8.GetBytes(@"java.lang.SecurityException: Injecting to another application requires INJECT_EVENTS permission
        at android.os.Parcel.createExceptionOrNull(Parcel.java:2373)
        at android.os.Parcel.createException(Parcel.java:2357)
        at android.os.Parcel.readException(Parcel.java:2340)
        at android.os.Parcel.readException(Parcel.java:2282)
        at android.hardware.input.IInputManager$Stub$Proxy.injectInputEvent(IInputManager.java:946)
        at android.hardware.input.InputManager.injectInputEvent(InputManager.java:907)
        at com.android.commands.input.Input.injectMotionEvent(Input.java:397)
        at com.android.commands.input.Input.access$200(Input.java:41)
        at com.android.commands.input.Input$InputTap.sendTap(Input.java:223)
        at com.android.commands.input.Input$InputTap.run(Input.java:217)
        at com.android.commands.input.Input.onRun(Input.java:107)
        at com.android.internal.os.BaseCommand.run(BaseCommand.java:60)
        at com.android.commands.input.Input.main(Input.java:71)
        at com.android.internal.os.RuntimeInit.nativeFinishInit(Native Method)
        at com.android.internal.os.RuntimeInit.main(RuntimeInit.java:438)
Caused by: android.os.RemoteException: Remote stack trace:
        at com.android.server.input.InputManagerService.injectInputEventInternal(InputManagerService.java:677)
        at com.android.server.input.InputManagerService.injectInputEvent(InputManagerService.java:651)
        at android.hardware.input.IInputManager$Stub.onTransact(IInputManager.java:430)
        at android.os.Binder.execTransactInternal(Binder.java:1165)
        at android.os.Binder.execTransact(Binder.java:1134)");
            using MemoryStream shellStream = new(streamData);

            JavaException exception = Assert.Throws<JavaException>(() =>
            RunTest(
                new AdbResponse[]
                {
                    AdbResponse.OK,
                    AdbResponse.OK,
                },
                NoResponseMessages,
                requests,
                shellStream,
                () => TestClient.Click(device, 100, 100)));

            Assert.Equal("SecurityException", exception.JavaName);
            Assert.Equal("Injecting to another application requires INJECT_EVENTS permission", exception.Message);
            Assert.Equal(@"        at android.os.Parcel.createExceptionOrNull(Parcel.java:2373)
        at android.os.Parcel.createException(Parcel.java:2357)
        at android.os.Parcel.readException(Parcel.java:2340)
        at android.os.Parcel.readException(Parcel.java:2282)
        at android.hardware.input.IInputManager$Stub$Proxy.injectInputEvent(IInputManager.java:946)
        at android.hardware.input.InputManager.injectInputEvent(InputManager.java:907)
        at com.android.commands.input.Input.injectMotionEvent(Input.java:397)
        at com.android.commands.input.Input.access$200(Input.java:41)
        at com.android.commands.input.Input$InputTap.sendTap(Input.java:223)
        at com.android.commands.input.Input$InputTap.run(Input.java:217)
        at com.android.commands.input.Input.onRun(Input.java:107)
        at com.android.internal.os.BaseCommand.run(BaseCommand.java:60)
        at com.android.commands.input.Input.main(Input.java:71)
        at com.android.internal.os.RuntimeInit.nativeFinishInit(Native Method)
        at com.android.internal.os.RuntimeInit.main(RuntimeInit.java:438)
Caused by: android.os.RemoteException: Remote stack trace:
        at com.android.server.input.InputManagerService.injectInputEventInternal(InputManagerService.java:677)
        at com.android.server.input.InputManagerService.injectInputEvent(InputManagerService.java:651)
        at android.hardware.input.IInputManager$Stub.onTransact(IInputManager.java:430)
        at android.os.Binder.execTransactInternal(Binder.java:1165)
        at android.os.Binder.execTransact(Binder.java:1134)", exception.JavaStackTrace);
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.Click(DeviceData, Cords)"/> method.
        /// </summary>
        [Fact]
        public void ClickCordsTest()
        {
            DeviceData device = new()
            {
                Serial = "009d1cd696d5194a",
                State = DeviceState.Online
            };

            string[] requests = new string[]
            {
                "host:transport:009d1cd696d5194a",
                "shell:input tap 100 100"
            };

            byte[] streamData = Encoding.UTF8.GetBytes(@"java.lang.SecurityException: Injecting to another application requires INJECT_EVENTS permission
        at android.os.Parcel.createExceptionOrNull(Parcel.java:2373)
        at android.os.Parcel.createException(Parcel.java:2357)
        at android.os.Parcel.readException(Parcel.java:2340)
        at android.os.Parcel.readException(Parcel.java:2282)
        at android.hardware.input.IInputManager$Stub$Proxy.injectInputEvent(IInputManager.java:946)
        at android.hardware.input.InputManager.injectInputEvent(InputManager.java:907)
        at com.android.commands.input.Input.injectMotionEvent(Input.java:397)
        at com.android.commands.input.Input.access$200(Input.java:41)
        at com.android.commands.input.Input$InputTap.sendTap(Input.java:223)
        at com.android.commands.input.Input$InputTap.run(Input.java:217)
        at com.android.commands.input.Input.onRun(Input.java:107)
        at com.android.internal.os.BaseCommand.run(BaseCommand.java:60)
        at com.android.commands.input.Input.main(Input.java:71)
        at com.android.internal.os.RuntimeInit.nativeFinishInit(Native Method)
        at com.android.internal.os.RuntimeInit.main(RuntimeInit.java:438)
Caused by: android.os.RemoteException: Remote stack trace:
        at com.android.server.input.InputManagerService.injectInputEventInternal(InputManagerService.java:677)
        at com.android.server.input.InputManagerService.injectInputEvent(InputManagerService.java:651)
        at android.hardware.input.IInputManager$Stub.onTransact(IInputManager.java:430)
        at android.os.Binder.execTransactInternal(Binder.java:1165)
        at android.os.Binder.execTransact(Binder.java:1134)");
            using MemoryStream shellStream = new(streamData);

            JavaException exception = Assert.Throws<JavaException>(() =>
            RunTest(
                new AdbResponse[]
                {
                    AdbResponse.OK,
                    AdbResponse.OK,
                },
                NoResponseMessages,
                requests,
                shellStream,
                () => TestClient.Click(device, new Cords(100, 100))));

            Assert.Equal("SecurityException", exception.JavaName);
            Assert.Equal("Injecting to another application requires INJECT_EVENTS permission", exception.Message);
            Assert.Equal(@"        at android.os.Parcel.createExceptionOrNull(Parcel.java:2373)
        at android.os.Parcel.createException(Parcel.java:2357)
        at android.os.Parcel.readException(Parcel.java:2340)
        at android.os.Parcel.readException(Parcel.java:2282)
        at android.hardware.input.IInputManager$Stub$Proxy.injectInputEvent(IInputManager.java:946)
        at android.hardware.input.InputManager.injectInputEvent(InputManager.java:907)
        at com.android.commands.input.Input.injectMotionEvent(Input.java:397)
        at com.android.commands.input.Input.access$200(Input.java:41)
        at com.android.commands.input.Input$InputTap.sendTap(Input.java:223)
        at com.android.commands.input.Input$InputTap.run(Input.java:217)
        at com.android.commands.input.Input.onRun(Input.java:107)
        at com.android.internal.os.BaseCommand.run(BaseCommand.java:60)
        at com.android.commands.input.Input.main(Input.java:71)
        at com.android.internal.os.RuntimeInit.nativeFinishInit(Native Method)
        at com.android.internal.os.RuntimeInit.main(RuntimeInit.java:438)
Caused by: android.os.RemoteException: Remote stack trace:
        at com.android.server.input.InputManagerService.injectInputEventInternal(InputManagerService.java:677)
        at com.android.server.input.InputManagerService.injectInputEvent(InputManagerService.java:651)
        at android.hardware.input.IInputManager$Stub.onTransact(IInputManager.java:430)
        at android.os.Binder.execTransactInternal(Binder.java:1165)
        at android.os.Binder.execTransact(Binder.java:1134)", exception.JavaStackTrace);
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.FindElement(DeviceData, string, TimeSpan)"/> method.
        /// </summary>
        [Fact]
        public void FindElementTest()
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

            RunTest(
                new AdbResponse[]
                {
                    AdbResponse.OK,
                    AdbResponse.OK,
                },
                NoResponseMessages,
                requests,
                shellStream,
                () =>
                {
                    Element element = TestClient.FindElement(device);
                    Assert.Equal(144, element.GetChildCount());
                    element = element[0][0][0][0][0][0][0][0][2][1][0][0];
                    Assert.Equal("where-where", element.Attributes["text"]);
                    Assert.Equal(Area.FromLTRB(45, 889, 427, 973), element.Area);
                });
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.FindElements(DeviceData, string, TimeSpan)"/> method.
        /// </summary>
        [Fact]
        public void FindElementsTest()
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

            RunTest(
                new AdbResponse[]
                {
                    AdbResponse.OK,
                    AdbResponse.OK,
                },
                NoResponseMessages,
                requests,
                shellStream,
                () =>
                {
                    List<Element> elements = TestClient.FindElements(device).ToList();
                    int childCount = elements.Count;
                    elements.ForEach(x => childCount += x.GetChildCount());
                    Assert.Equal(145, childCount);
                    Element element = elements[0][0][0][0][0][0][0][0][0][2][1][0][0];
                    Assert.Equal("where-where", element.Attributes["text"]);
                    Assert.Equal(Area.FromLTRB(45, 889, 427, 973), element.Area);
                });
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
