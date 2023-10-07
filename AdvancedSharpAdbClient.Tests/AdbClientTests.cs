using AdvancedSharpAdbClient.Exceptions;
using AdvancedSharpAdbClient.Logs;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;
using Xunit;

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
            string[] responseMessages = ["0020"];
            string[] requests = ["host:version"];

            int version = RunTest(
                OkResponse,
                responseMessages,
                requests,
                TestClient.GetAdbVersion);

            // Make sure and the correct value is returned.
            Assert.Equal(32, version);
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.KillAdb"/> method.
        /// </summary>
        [Fact]
        public void KillAdbTest()
        {
            string[] requests = ["host:kill"];

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
            string[] responseMessages = ["169.254.109.177:5555   device product:VS Emulator 5\" KitKat (4.4) XXHDPI Phone model:5__KitKat__4_4__XXHDPI_Phone device:donatello\n"];
            string[] requests = ["host:devices-l"];

            DeviceData[] devices = RunTest(
                OkResponse,
                responseMessages,
                requests,
                () => TestClient.GetDevices().ToArray());

            // Make sure and the correct value is returned.
            Assert.NotNull(devices);
            Assert.Single(devices);

            DeviceData device = devices.SingleOrDefault();

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
            string[] requests = ["host:transport:169.254.109.177:5555"];

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
            string[] requests = ["host:transport:169.254.109.177:5555"];

            _ = Assert.Throws<DeviceNotFoundException>(() =>
            RunTest(
                [AdbResponse.FromError("device not found")],
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
            string[] requests = ["host:transport:169.254.109.177:5555"];

            _ = Assert.Throws<AdbException>(() =>
            RunTest(
                [AdbResponse.FromError("Too many cats.")],
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
            AdbResponse[] responses = [AdbResponse.FromError("cannot rebind existing socket")];
            string[] requests = ["host-serial:169.254.109.177:5555:forward:norebind:tcp:1;tcp:2"];

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
            string[] requests = ["host-serial:169.254.109.177:5555:killforward:tcp:1"];

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
            string[] requests =
            [
                "host:transport:169.254.109.177:5555",
                "reverse:killforward:localabstract:test"
            ];

            RunTest(
                OkResponses(2),
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
            string[] requests = ["host-serial:169.254.109.177:5555:killforward-all"];

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
            string[] requests =
            [
                "host:transport:169.254.109.177:5555",
                "reverse:killforward-all"
            ];

            RunTest(
                OkResponses(2),
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
            string[] responseMessages = ["169.254.109.177:5555 tcp:1 tcp:2\n169.254.109.177:5555 tcp:3 tcp:4\n169.254.109.177:5555 tcp:5 local:/socket/1\n"];
            string[] requests = ["host-serial:169.254.109.177:5555:list-forward"];

            ForwardData[] forwards = RunTest(
                OkResponse,
                responseMessages,
                requests,
                () => TestClient.ListForward(Device).ToArray());

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
            string[] responseMessages = ["(reverse) localabstract:scrcpy tcp:100\n(reverse) localabstract: scrcpy2 tcp:100\n(reverse) localabstract: scrcpy3 tcp:100\n"];

            string[] requests =
            [
                "host:transport:169.254.109.177:5555",
                "reverse:list-forward"
            ];

            ForwardData[] forwards = RunTest(
                OkResponses(2),
                responseMessages,
                requests,
                () => TestClient.ListReverseForward(Device).ToArray());

            Assert.NotNull(forwards);
            Assert.Equal(3, forwards.Length);
            Assert.Equal("(reverse)", forwards[0].SerialNumber);
            Assert.Equal("localabstract:scrcpy", forwards[0].Local);
            Assert.Equal("tcp:100", forwards[0].Remote);
        }

        /// <summary>
        /// Tests the <see cref="AdbClientExtensions.ExecuteServerCommand(IAdbClient, string, string, IShellOutputReceiver)"/> method.
        /// </summary>
        [Fact]
        public void ExecuteServerCommandTest()
        {
            string[] requests = ["host:version"];

            byte[] streamData = Encoding.ASCII.GetBytes("0020");
            using MemoryStream shellStream = new(streamData);

            ConsoleOutputReceiver receiver = new();

            RunTest(
                OkResponse,
                NoResponseMessages,
                requests,
                [shellStream],
                () => TestClient.ExecuteServerCommand("host", "version", receiver));

            string version = receiver.ToString();
            Assert.Equal("0020\r\n", receiver.ToString(), ignoreLineEndingDifferences: true);
            Assert.Equal(32, int.Parse(version, NumberStyles.HexNumber));
        }

        /// <summary>
        /// Tests the <see cref="AdbClientExtensions.ExecuteRemoteCommand(IAdbClient, string, DeviceData, IShellOutputReceiver)"/> method.
        /// </summary>
        [Fact]
        public void ExecuteRemoteCommandTest()
        {
            string[] requests =
            [
                "host:transport:169.254.109.177:5555",
                "shell:echo Hello, World"
            ];

            byte[] streamData = Encoding.ASCII.GetBytes("Hello, World\r\n");
            using MemoryStream shellStream = new(streamData);

            ConsoleOutputReceiver receiver = new();

            RunTest(
                OkResponses(2),
                NoResponseMessages,
                requests,
                [shellStream],
                () => TestClient.ExecuteRemoteCommand("echo Hello, World", Device, receiver));

            Assert.Equal("Hello, World\r\n", receiver.ToString(), ignoreLineEndingDifferences: true);
        }

        /// <summary>
        /// Tests the <see cref="AdbClientExtensions.ExecuteRemoteCommand(IAdbClient, string, DeviceData, IShellOutputReceiver)"/> method.
        /// </summary>
        [Fact]
        public void ExecuteRemoteCommandUnresponsiveTest()
        {
            string[] requests =
            [
                "host:transport:169.254.109.177:5555",
                "shell:echo Hello, World"
            ];

            ConsoleOutputReceiver receiver = new();

            _ = Assert.Throws<ShellCommandUnresponsiveException>(() =>
            RunTest(
                OkResponses(2),
                NoResponseMessages,
                requests,
                null,
                () => TestClient.ExecuteRemoteCommand("echo Hello, World", Device, receiver)));
        }

        [Fact]
        public void CreateRefreshableFramebufferTest()
        {
            Framebuffer framebuffer = TestClient.CreateRefreshableFramebuffer(Device);
            Assert.NotNull(framebuffer);
            Assert.Equal(Device, framebuffer.Device);
        }

        [Fact]
        public void GetFrameBufferTest()
        {
            DummyAdbSocket socket = new();

            socket.Responses.Enqueue(AdbResponse.OK);
            socket.Responses.Enqueue(AdbResponse.OK);

            socket.Requests.Add("host:transport:169.254.109.177:5555");
            socket.Requests.Add("framebuffer:");

            socket.SyncDataReceived.Enqueue(File.ReadAllBytes("Assets/framebufferheader.bin"));
            socket.SyncDataReceived.Enqueue(File.ReadAllBytes("Assets/framebuffer.bin"));

            Framebuffer framebuffer = null;

            using (FactoriesLocker locker = FactoriesLocker.Wait())
            {
                Factories.AdbSocketFactory = (endPoint) => socket;
                framebuffer = TestClient.GetFrameBuffer(Device);
                Factories.Reset();
            }

            Assert.NotNull(framebuffer);
            Assert.Equal(Device, framebuffer.Device);
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
            string[] requests =
            [
                "host:transport:169.254.109.177:5555",
                "shell:logcat -B -b system"
            ];

            ConsoleOutputReceiver receiver = new();

            using FileStream stream = File.OpenRead("Assets/logcat.bin");
            using ShellStream shellStream = new(stream, false);
            List<LogEntry> logs = [];
            Action<LogEntry> sink = logs.Add;

            RunTest(
                OkResponses(2),
                NoResponseMessages,
                requests,
                [shellStream],
                () => TestClient.RunLogService(Device, sink, LogId.System));

            Assert.Equal(3, logs.Count);
        }

        /// <summary>
        /// Tests the <see cref="AdbClientExtensions.Reboot(IAdbClient, DeviceData)"/> method.
        /// </summary>
        [Fact]
        public void RebootTest()
        {
            string[] requests =
            [
                "host:transport:169.254.109.177:5555",
                "reboot:"
            ];

            RunTest(
                OkResponses(2),
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
            string[] requests = ["host:disconnect:localhost:5555"];
            string[] responseMessages = ["disconnected 127.0.0.1:5555"];

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
            string[] requests =
            [
                "host:transport:169.254.109.177:5555",
                "root:"
            ];

            byte[] expectedData = new byte[1024];
            "adbd cannot run as root in production builds\n"u8.CopyTo(expectedData);

            _ = Assert.Throws<AdbException>(() =>
            RunTest(
                OkResponses(2),
                NoResponseMessages,
                requests,
                NoSyncRequests,
                NoSyncResponses,
                [expectedData],
                null,
                () => TestClient.Root(Device)));
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.Unroot(DeviceData)"/> method.
        /// </summary>
        [Fact]
        public void UnrootTest()
        {
            string[] requests =
            [
                "host:transport:169.254.109.177:5555",
                "unroot:"
            ];

            byte[] expectedData = new byte[1024];
            "adbd not running as root\n"u8.CopyTo(expectedData);

            _ = Assert.Throws<AdbException>(() =>
            RunTest(
                OkResponses(2),
                NoResponseMessages,
                requests,
                NoSyncRequests,
                NoSyncResponses,
                [expectedData],
                null,
                () => TestClient.Unroot(Device)));
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.Install(DeviceData, Stream, string[])"/> method.
        /// </summary>
        [Fact]
        public void InstallTest()
        {
            string[] requests =
            [
                "host:transport:169.254.109.177:5555",
                "exec:cmd package 'install' -S 205774"
            ];

            // The app data is sent in chunks of 32 kb
            List<byte[]> applicationDataChunks = [];

            using (FileStream stream = File.OpenRead("Assets/testapp.apk"))
            {
                while (true)
                {
                    byte[] buffer = new byte[32 * 1024];
                    int read = stream.Read(buffer.AsSpan(0, buffer.Length));

                    if (read == 0)
                    {
                        break;
                    }
                    else
                    {
                        buffer = buffer.Take(read).ToArray();
                        applicationDataChunks.Add(buffer);
                    }
                }
            }

            byte[] response = "Success\n"u8.ToArray();

            using (FileStream stream = File.OpenRead("Assets/testapp.apk"))
            {
                RunTest(
                    OkResponses(2),
                    NoResponseMessages,
                    requests,
                    NoSyncRequests,
                    NoSyncResponses,
                    [response],
                    applicationDataChunks.ToArray(),
                    () => TestClient.Install(Device, stream));
            }
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.InstallCreate(DeviceData, string, string[])"/> method.
        /// </summary>
        [Fact]
        public void InstallCreateTest()
        {
            string[] requests =
            [
                "host:transport:169.254.109.177:5555",
                "exec:cmd package 'install-create' -p com.google.android.gms"
            ];

            byte[] streamData = Encoding.ASCII.GetBytes("Success: created install session [936013062]\r\n");
            using MemoryStream shellStream = new(streamData);

            string session = RunTest(
                OkResponses(2),
                NoResponseMessages,
                requests,
                [shellStream],
                () => TestClient.InstallCreate(Device, "com.google.android.gms"));

            Assert.Equal("936013062", session);
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.InstallWrite(DeviceData, Stream, string, string)"/> method.
        /// </summary>
        [Fact]
        public void InstallWriteTest()
        {
            string[] requests =
            [
                "host:transport:169.254.109.177:5555",
                "exec:cmd package 'install-write' -S 205774 936013062 base.apk"
            ];

            // The app data is sent in chunks of 32 kb
            List<byte[]> applicationDataChunks = [];

            using (FileStream stream = File.OpenRead("Assets/testapp.apk"))
            {
                while (true)
                {
                    byte[] buffer = new byte[32 * 1024];
                    int read = stream.Read(buffer.AsSpan(0, buffer.Length));

                    if (read == 0)
                    {
                        break;
                    }
                    else
                    {
                        buffer = buffer.Take(read).ToArray();
                        applicationDataChunks.Add(buffer);
                    }
                }
            }

            byte[] response = "Success: streamed 205774 bytes\n"u8.ToArray();

            using (FileStream stream = File.OpenRead("Assets/testapp.apk"))
            {
                RunTest(
                    OkResponses(2),
                    NoResponseMessages,
                    requests,
                    NoSyncRequests,
                    NoSyncResponses,
                    [response],
                    applicationDataChunks.ToArray(),
                    () => TestClient.InstallWrite(Device, stream, "base", "936013062"));
            }
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.InstallCommit(DeviceData, string)"/> method.
        /// </summary>
        [Fact]
        public void InstallCommitTest()
        {
            string[] requests =
            [
                "host:transport:169.254.109.177:5555",
                "exec:cmd package 'install-commit' 936013062"
            ];

            byte[] streamData = Encoding.ASCII.GetBytes("Success\r\n");
            using MemoryStream shellStream = new(streamData);

            RunTest(
                OkResponses(2),
                NoResponseMessages,
                requests,
                [shellStream],
                () => TestClient.InstallCommit(Device, "936013062"));
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.GetFeatureSet(DeviceData)"/> method.
        /// </summary>
        [Fact]
        public void GetFeatureSetTest()
        {
            string[] requests = ["host-serial:169.254.109.177:5555:features"];
            string[] responses = ["sendrecv_v2_brotli,remount_shell,sendrecv_v2,abb_exec,fixed_push_mkdir,fixed_push_symlink_timestamp,abb,shell_v2,cmd,ls_v2,apex,stat_v2\r\n"];

            string[] features = RunTest(
                OkResponse,
                responses,
                requests,
                () => TestClient.GetFeatureSet(Device).ToArray());

            Assert.Equal(12, features.Length);
            Assert.Equal("sendrecv_v2_brotli", features.FirstOrDefault());
            Assert.Equal("stat_v2", features.LastOrDefault());
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.DumpScreenString(DeviceData)"/> method.
        /// </summary>
        [Fact]
        public void DumpScreenStringTest()
        {
            string[] requests =
            [
                "host:transport:169.254.109.177:5555",
                "shell:uiautomator dump /dev/tty"
            ];

            string dump = File.ReadAllText(@"Assets/dumpscreen.txt");
            string cleanDump = File.ReadAllText(@"Assets/dumpscreen_clean.txt");
            byte[] streamData = Encoding.UTF8.GetBytes(dump);
            using MemoryStream shellStream = new(streamData);

            string xml = RunTest(
                OkResponses(2),
                NoResponseMessages,
                requests,
                [shellStream],
                () => TestClient.DumpScreenString(Device));

            Assert.Equal(cleanDump, xml);
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.DumpScreenString(DeviceData)"/> method.
        /// </summary>
        [Fact]
        public void DumpScreenStringMIUITest()
        {
            string[] requests =
            [
                "host:transport:169.254.109.177:5555",
                "shell:uiautomator dump /dev/tty"
            ];

            string miuidump = File.ReadAllText(@"Assets/dumpscreen_miui.txt");
            string cleanMIUIDump = File.ReadAllText(@"Assets/dumpscreen_miui_clean.txt");
            byte[] miuiStreamData = Encoding.UTF8.GetBytes(miuidump);
            using MemoryStream miuiStream = new(miuiStreamData);

            string miuiXml = RunTest(
                OkResponses(2),
                NoResponseMessages,
                requests,
                [miuiStream],
                () => TestClient.DumpScreenString(Device));

            Assert.Equal(cleanMIUIDump, miuiXml);
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.DumpScreenString(DeviceData)"/> method.
        /// </summary>
        [Fact]
        public void DumpScreenStringEmptyTest()
        {
            string[] requests =
            [
                "host:transport:169.254.109.177:5555",
                "shell:uiautomator dump /dev/tty"
            ];

            byte[] emptyStreamData = Encoding.UTF8.GetBytes(string.Empty);
            using MemoryStream emptyStream = new(emptyStreamData);

            string emptyXml = RunTest(
                OkResponses(2),
                NoResponseMessages,
                requests,
                [emptyStream],
                () => TestClient.DumpScreenString(Device));

            Assert.True(string.IsNullOrEmpty(emptyXml));
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.DumpScreenString(DeviceData)"/> method.
        /// </summary>
        [Fact]
        public void DumpScreenStringErrorTest()
        {
            string[] requests =
            [
                "host:transport:169.254.109.177:5555",
                "shell:uiautomator dump /dev/tty"
            ];

            string errorXml = File.ReadAllText(@"Assets/dumpscreen_error.txt");
            byte[] errorStreamData = Encoding.UTF8.GetBytes(errorXml);
            using MemoryStream errorStream = new(errorStreamData);

            Assert.Throws<XmlException>(() =>
            RunTest(
                OkResponses(2),
                NoResponseMessages,
                requests,
                [errorStream],
                () => TestClient.DumpScreenString(Device)));
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.DumpScreen(DeviceData)"/> method.
        /// </summary>
        [Fact]
        public void DumpScreenTest()
        {
            string[] requests =
            [
                "host:transport:169.254.109.177:5555",
                "shell:uiautomator dump /dev/tty"
            ];

            string dump = File.ReadAllText(@"Assets/dumpscreen.txt");
            byte[] streamData = Encoding.UTF8.GetBytes(dump);
            using MemoryStream shellStream = new(streamData);

            XmlDocument xml = RunTest(
                OkResponses(2),
                NoResponseMessages,
                requests,
                [shellStream],
                () => TestClient.DumpScreen(Device));

            string cleanDump = File.ReadAllText(@"Assets/dumpscreen_clean.txt");
            XmlDocument doc = new();
            doc.LoadXml(cleanDump);

            Assert.Equal(doc, xml);
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.Click(DeviceData, int, int)"/> method.
        /// </summary>
        [Fact]
        public void ClickTest()
        {
            string[] requests =
            [
                "host:transport:169.254.109.177:5555",
                "shell:input tap 100 100"
            ];

            byte[] streamData = @"java.lang.SecurityException: Injecting to another application requires INJECT_EVENTS permission
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
        at android.os.Binder.execTransact(Binder.java:1134)"u8.ToArray();
            using MemoryStream shellStream = new(streamData);

            JavaException exception = Assert.Throws<JavaException>(() =>
            RunTest(
                OkResponses(2),
                NoResponseMessages,
                requests,
                [shellStream],
                () => TestClient.Click(Device, 100, 100)));

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
            string[] requests =
            [
                "host:transport:169.254.109.177:5555",
                "shell:input tap 100 100"
            ];

            byte[] streamData = "Error: Injecting to another application requires INJECT_EVENTS permission\r\n"u8.ToArray();
            using MemoryStream shellStream = new(streamData);

            _ = Assert.Throws<ElementNotFoundException>(() =>
            RunTest(
                OkResponses(2),
                NoResponseMessages,
                requests,
                [shellStream],
                () => TestClient.Click(Device, new Cords(100, 100))));
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.Swipe(DeviceData, int, int, int, int, long)"/> method.
        /// </summary>
        [Fact]
        public void SwipeTest()
        {
            string[] requests =
            [
                "host:transport:169.254.109.177:5555",
                "shell:input swipe 100 200 300 400 500"
            ];

            using MemoryStream shellStream = new();

            RunTest(
                OkResponses(2),
                NoResponseMessages,
                requests,
                [shellStream],
                () => TestClient.Swipe(Device, 100, 200, 300, 400, 500));
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.Swipe(DeviceData, Element, Element, long)"/> method.
        /// </summary>
        [Fact]
        public void SwipeElementTest()
        {
            string[] requests =
            [
                "host:transport:169.254.109.177:5555",
                "shell:input swipe 100 200 300 400 500"
            ];

            using MemoryStream shellStream = new();

            RunTest(
                OkResponses(2),
                NoResponseMessages,
                requests,
                [shellStream],
                () => TestClient.Swipe(Device, new Element(TestClient, Device, new Area(0, 0, 200, 400)), new Element(TestClient, Device, new Area(0, 0, 600, 800)), 500));
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.IsAppRunning(DeviceData, string)"/> method.
        /// </summary>
        [Theory]
        [InlineData("21216 27761\r\n", true)]
        [InlineData(" 21216 27761\r\n", true)]
        [InlineData("12836\r\n", true)]
        [InlineData(" \r\n", false)]
        [InlineData("\r\n", false)]
        [InlineData(" ", false)]
        [InlineData("", false)]
        public void IsAppRunningTest(string response, bool expected)
        {
            string[] requests =
            [
                "host:transport:169.254.109.177:5555",
                "shell:pidof com.google.android.gms"
            ];

            byte[] streamData = Encoding.UTF8.GetBytes(response);
            using MemoryStream shellStream = new(streamData);

            bool result = RunTest(
                OkResponses(2),
                NoResponseMessages,
                requests,
                [shellStream],
                () => TestClient.IsAppRunning(Device, "com.google.android.gms"));

            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.IsAppInForeground(DeviceData, string)"/> method.
        /// </summary>
        [Theory]
        [InlineData("app.lawnchair", true)]
        [InlineData("com.android.settings", true)]
        [InlineData("com.google.android.gms", false)]
        public void IsAppInForegroundTest(string packageName, bool expected)
        {
            string[] requests =
            [
                "host:transport:169.254.109.177:5555",
                "shell:dumpsys activity activities | grep mResumedActivity"
            ];

            byte[] streamData = @"    mResumedActivity: ActivityRecord{1f5309a u0 com.android.settings/.homepage.SettingsHomepageActivity t61029}
    mResumedActivity: ActivityRecord{896cc3 u0 app.lawnchair/.LawnchairLauncher t5}"u8.ToArray();
            using MemoryStream shellStream = new(streamData);

            bool result = RunTest(
                OkResponses(2),
                NoResponseMessages,
                requests,
                [shellStream],
                () => TestClient.IsAppInForeground(Device, packageName));

            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.GetAppStatus(DeviceData, string)"/> method.
        /// </summary>
        [Theory]
        [InlineData("com.google.android.gms", "21216 27761\r\n", AppStatus.Background)]
        [InlineData("com.android.gallery3d", "\r\n", AppStatus.Stopped)]
        public void GetAppStatusTest(string packageName, string response, AppStatus expected)
        {
            string[] requests =
            [
                "host:transport:169.254.109.177:5555",
                "shell:dumpsys activity activities | grep mResumedActivity",
                "host:transport:169.254.109.177:5555",
                $"shell:pidof {packageName}"
            ];

            byte[] activityData = @"    mResumedActivity: ActivityRecord{1f5309a u0 com.android.settings/.homepage.SettingsHomepageActivity t61029}
    mResumedActivity: ActivityRecord{896cc3 u0 app.lawnchair/.LawnchairLauncher t5}"u8.ToArray();
            using MemoryStream activityStream = new(activityData);
            byte[] pidData = Encoding.UTF8.GetBytes(response);
            using MemoryStream pidStream = new(pidData);

            AppStatus result = RunTest(
                OkResponses(4),
                NoResponseMessages,
                requests,
                [activityStream, pidStream],
                () => TestClient.GetAppStatus(Device, packageName));

            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.GetAppStatus(DeviceData, string)"/> method.
        /// </summary>
        [Theory]
        [InlineData("app.lawnchair", AppStatus.Foreground)]
        [InlineData("com.android.settings", AppStatus.Foreground)]
        public void GetAppStatusForegroundTest(string packageName, AppStatus expected)
        {
            string[] requests =
            [
                "host:transport:169.254.109.177:5555",
                "shell:dumpsys activity activities | grep mResumedActivity"
            ];

            byte[] streamData = @"    mResumedActivity: ActivityRecord{1f5309a u0 com.android.settings/.homepage.SettingsHomepageActivity t61029}
    mResumedActivity: ActivityRecord{896cc3 u0 app.lawnchair/.LawnchairLauncher t5}"u8.ToArray();
            using MemoryStream shellStream = new(streamData);

            AppStatus result = RunTest(
                OkResponses(2),
                NoResponseMessages,
                requests,
                [shellStream],
                () => TestClient.GetAppStatus(Device, packageName));

            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.FindElement(DeviceData, string, TimeSpan)"/> method.
        /// </summary>
        [Fact]
        public void FindElementTest()
        {
            string[] requests =
            [
                "host:transport:169.254.109.177:5555",
                "shell:uiautomator dump /dev/tty"
            ];

            string dump = File.ReadAllText(@"Assets/dumpscreen.txt");
            byte[] streamData = Encoding.UTF8.GetBytes(dump);
            using MemoryStream shellStream = new(streamData);

            Element element = RunTest(
                OkResponses(2),
                NoResponseMessages,
                requests,
                [shellStream],
                () => TestClient.FindElement(Device));

            Assert.Equal(144, element.GetChildCount());
            element = element[0][0][0][0][0][0][0][0][2][1][0][0];
            Assert.Equal("where-where", element.Attributes["text"]);
            Assert.Equal(Area.FromLTRB(45, 889, 427, 973), element.Area);
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.FindElements(DeviceData, string, TimeSpan)"/> method.
        /// </summary>
        [Fact]
        public void FindElementsTest()
        {
            string[] requests =
            [
                "host:transport:169.254.109.177:5555",
                "shell:uiautomator dump /dev/tty"
            ];

            string dump = File.ReadAllText(@"Assets/dumpscreen.txt");
            byte[] streamData = Encoding.UTF8.GetBytes(dump);
            using MemoryStream shellStream = new(streamData);

            Element[] elements = RunTest(
                OkResponses(2),
                NoResponseMessages,
                requests,
                [shellStream],
                () => TestClient.FindElements(Device).ToArray());

            int childCount = elements.Length;
            Array.ForEach(elements, x => childCount += x.GetChildCount());
            Assert.Equal(145, childCount);
            Element element = elements[0][0][0][0][0][0][0][0][0][2][1][0][0];
            Assert.Equal("where-where", element.Attributes["text"]);
            Assert.Equal(Area.FromLTRB(45, 889, 427, 973), element.Area);
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.SendKeyEvent(DeviceData, string)"/> method.
        /// </summary>
        [Fact]
        public void SendKeyEventTest()
        {
            string[] requests =
            [
                "host:transport:169.254.109.177:5555",
                "shell:input keyevent KEYCODE_MOVE_END"
            ];

            using MemoryStream shellStream = new();

            RunTest(
                OkResponses(2),
                NoResponseMessages,
                requests,
                [shellStream],
                () => TestClient.SendKeyEvent(Device, "KEYCODE_MOVE_END"));
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.SendText(DeviceData, string)"/> method.
        /// </summary>
        [Fact]
        public void SendTextTest()
        {
            string[] requests =
            [
                "host:transport:169.254.109.177:5555",
                "shell:input text Hello, World",
            ];

            using MemoryStream shellStream = new();

            RunTest(
                OkResponses(2),
                NoResponseMessages,
                requests,
                [shellStream],
                () => TestClient.SendText(Device, "Hello, World"));
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.ClearInput(DeviceData, int)"/> method.
        /// </summary>
        [Fact]
        public void ClearInputTest()
        {
            string[] requests =
            [
                "host:transport:169.254.109.177:5555",
                "shell:input keyevent KEYCODE_MOVE_END",
                "host:transport:169.254.109.177:5555",
                "shell:input keyevent KEYCODE_DEL KEYCODE_DEL KEYCODE_DEL"
            ];

            using MemoryStream firstShellStream = new();
            using MemoryStream secondShellStream = new();

            RunTest(
                OkResponses(4),
                NoResponseMessages,
                requests,
                [firstShellStream, secondShellStream],
                () => TestClient.ClearInput(Device, 3));
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.StartApp(DeviceData, string)"/> method.
        /// </summary>
        [Fact]
        public void StartAppTest()
        {
            string[] requests =
            [
                "host:transport:169.254.109.177:5555",
                "shell:monkey -p com.android.settings 1",
            ];

            RunTest(
                OkResponses(2),
                NoResponseMessages,
                requests,
                () => TestClient.StartApp(Device, "com.android.settings"));
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.StopApp(DeviceData, string)"/> method.
        /// </summary>
        [Fact]
        public void StopAppTest()
        {
            string[] requests =
            [
                "host:transport:169.254.109.177:5555",
                "shell:am force-stop com.android.settings",
            ];

            RunTest(
                OkResponses(2),
                NoResponseMessages,
                requests,
                () => TestClient.StopApp(Device, "com.android.settings"));
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.BackBtn(DeviceData)"/> method.
        /// </summary>
        [Fact]
        public void BackBtnTest()
        {
            string[] requests =
            [
                "host:transport:169.254.109.177:5555",
                "shell:input keyevent KEYCODE_BACK"
            ];

            using MemoryStream shellStream = new();

            RunTest(
                OkResponses(2),
                NoResponseMessages,
                requests,
                [shellStream],
                () => TestClient.BackBtn(Device));
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.HomeBtn(DeviceData)"/> method.
        /// </summary>
        [Fact]
        public void HomeBtnTest()
        {
            string[] requests =
            [
                "host:transport:169.254.109.177:5555",
                "shell:input keyevent KEYCODE_HOME"
            ];

            using MemoryStream shellStream = new();

            RunTest(
                OkResponses(2),
                NoResponseMessages,
                requests,
                [shellStream],
                () => TestClient.HomeBtn(Device));
        }

        private void RunConnectTest(Action test, string connectString)
        {
            string[] requests = [$"host:connect:{connectString}"];
            string[] responseMessages = [$"connected to {connectString}"];

            RunTest(
                OkResponse,
                responseMessages,
                requests,
                test);
        }

        private void RunPairTest(Action test, string connectString, string code)
        {
            string[] requests = [$"host:pair:{code}:{connectString}"];
            string[] responseMessages = [$"Successfully paired to {connectString} [guid=adb-996198a3-xPRwsQ]"];

            RunTest(
                OkResponse,
                responseMessages,
                requests,
                test);
        }

        private void RunCreateReverseTest(Action<DeviceData> test, string reverseString)
        {
            string[] requests =
            [
                "host:transport:169.254.109.177:5555",
                $"reverse:forward:{reverseString}",
            ];

            RunTest(
                OkResponses(3),
                [null],
                requests,
                () => test(Device));
        }

        private void RunCreateForwardTest(Action<DeviceData> test, string forwardString)
        {
            string[] requests = [$"host-serial:169.254.109.177:5555:forward:{forwardString}"];

            RunTest(
                OkResponses(2),
                [null],
                requests,
                () => test(Device));
        }
    }
}
