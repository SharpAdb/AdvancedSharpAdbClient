using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
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

        /// <summary>
        /// Tests the <see cref="AdbClient()"/> method.
        /// </summary>
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
            Assert.Equal("0009host:kill"u8, AdbClient.FormAdbRequest("host:kill"));
            Assert.Equal("000Chost:version"u8, AdbClient.FormAdbRequest("host:version"));
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.CreateAdbForwardRequest(string, int)"/> method.
        /// </summary>
        [Fact]
        public void CreateAdbForwardRequestTest()
        {
            Assert.Equal("0008tcp:1984"u8, AdbClient.CreateAdbForwardRequest(null, 1984));
            Assert.Equal("0012tcp:1981:127.0.0.1"u8, AdbClient.CreateAdbForwardRequest("127.0.0.1", 1981));
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
                device => TestClient.CreateForward(device, "tcp:1", "tcp:2", true),
                "tcp:1;tcp:2");

        /// <summary>
        /// Tests the <see cref="AdbClient.CreateReverseForward(DeviceData, string, string, bool)"/> method.
        /// </summary>
        [Fact]
        public void CreateReverseTest() =>
            RunCreateReverseTest(
                device => TestClient.CreateReverseForward(device, "tcp:1", "tcp:2", true),
                "tcp:1;tcp:2");

        /// <summary>
        /// Tests the <see cref="AdbClientExtensions.CreateForward(IAdbClient, DeviceData, int, int)"/> method.
        /// </summary>
        [Fact]
        public void CreateTcpForwardTest() =>
            RunCreateForwardTest(
                device => TestClient.CreateForward(device, 3, 4),
                "tcp:3;tcp:4");

        /// <summary>
        /// Tests the <see cref="AdbClientExtensions.CreateForward(IAdbClient, DeviceData, int, string)"/> method.
        /// </summary>
        [Fact]
        public void CreateSocketForwardTest() =>
            RunCreateForwardTest(
                device => TestClient.CreateForward(device, 5, "/socket/1"),
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
        /// Tests the <see cref="AdbClient.ExecuteServerCommand(string, string, IAdbSocket, IShellOutputReceiver?, Encoding)"/> method.
        /// </summary>
        [Fact]
        public void ExecuteServerCommandTest()
        {
            string[] requests = ["host:version"];

            byte[] streamData = "0020"u8.ToArray();
            using MemoryStream shellStream = new(streamData);

            ConsoleOutputReceiver receiver = new();

            RunTest(
                OkResponse,
                NoResponseMessages,
                requests,
                [shellStream],
                () => TestClient.ExecuteServerCommand("host", "version", receiver, AdbClient.Encoding));

            string version = receiver.ToString();
            Assert.Equal("0020\r\n", receiver.ToString(), ignoreLineEndingDifferences: true);
            Assert.Equal(32, int.Parse(version, NumberStyles.HexNumber));
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.ExecuteRemoteCommand(string, DeviceData, IShellOutputReceiver?, Encoding)"/> method.
        /// </summary>
        [Fact]
        public void ExecuteRemoteCommandTest()
        {
            string[] requests =
            [
                "host:transport:169.254.109.177:5555",
                "shell:echo Hello, World"
            ];

            byte[] streamData = "Hello, World\r\n"u8.ToArray();
            using MemoryStream shellStream = new(streamData);

            ConsoleOutputReceiver receiver = new();

            RunTest(
                OkResponses(2),
                NoResponseMessages,
                requests,
                [shellStream],
                () => TestClient.ExecuteRemoteCommand("echo Hello, World", Device, receiver, AdbClient.Encoding));

            Assert.Equal("Hello, World\r\n", receiver.ToString(), ignoreLineEndingDifferences: true);
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.ExecuteRemoteCommand(string, DeviceData, IShellOutputReceiver?, Encoding)"/> method.
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
                () => TestClient.ExecuteRemoteCommand("echo Hello, World", Device, receiver, AdbClient.Encoding)));
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.CreateFramebuffer(DeviceData)"/> method.
        /// </summary>
        [Fact]
        public void CreateFramebufferTest()
        {
            Framebuffer framebuffer = TestClient.CreateFramebuffer(Device);
            Assert.NotNull(framebuffer);
            Assert.Equal(Device, framebuffer.Device);
            Assert.Equal(default, framebuffer.Header);
            Assert.Null(framebuffer.Data);
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.GetFrameBuffer(DeviceData)"/> method.
        /// </summary>
        [Fact]
        public void GetFrameBufferTest()
        {
            string[] requests =
            [
                "host:transport:169.254.109.177:5555",
                "framebuffer:"
            ];

            Framebuffer framebuffer = RunTest(
                OkResponses(2),
                NoResponseMessages,
                requests,
                NoSyncRequests,
                NoSyncResponses,
                [
                    File.ReadAllBytes("Assets/FramebufferHeader.bin"),
                    File.ReadAllBytes("Assets/Framebuffer.bin")
                ],
                null,
                () => TestClient.GetFrameBuffer(Device));

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

#if WINDOWS
            if (!OperatingSystem.IsWindows()) { return; }

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
#endif

            framebuffer.Dispose();
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.RunLogService(DeviceData, Action{LogEntry}, in bool, LogId[])"/> method.
        /// </summary>
        [Fact]
        public void RunLogServiceTest()
        {
            string[] requests =
            [
                "host:transport:169.254.109.177:5555",
                "shell:logcat -B -b system"
            ];

            ConsoleOutputReceiver receiver = new();

            using FileStream stream = File.OpenRead("Assets/Logcat.bin");
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
        /// Tests the <see cref="AdbClient.RunLogService(DeviceData, LogId[])"/> method.
        /// </summary>
        [Fact]
        public void RunLogServiceEnumerableTest()
        {
            string[] requests =
            [
                "host:transport:169.254.109.177:5555",
                "shell:logcat -B -b system"
            ];

            ConsoleOutputReceiver receiver = new();

            using FileStream stream = File.OpenRead("Assets/Logcat.bin");
            using ShellStream shellStream = new(stream, false);

            LogEntry[] logs = RunTest(
                OkResponses(2),
                NoResponseMessages,
                requests,
                [shellStream],
                () => TestClient.RunLogService(Device, LogId.System).ToArray());

            Assert.Equal(3, logs.Length);
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
        /// Tests the <see cref="AdbClientExtensions.Pair(IAdbClient, DnsEndPoint, string)"/> method.
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
        /// Tests the <see cref="AdbClientExtensions.Pair(IAdbClient, DnsEndPoint, string)"/> method.
        /// </summary>
        [Fact]
        public void PairDnsEndpointNullTest() =>
            _ = Assert.Throws<ArgumentNullException>(() => TestClient.Pair((DnsEndPoint)null, "114514"));

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
        /// Tests the <see cref="AdbClientExtensions.Connect(IAdbClient, DnsEndPoint)"/> method.
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
        /// Tests the <see cref="AdbClient.Connect(string, int)"/> method.
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
        /// Tests the <see cref="AdbClientExtensions.Connect(IAdbClient, DnsEndPoint)"/> method.
        /// </summary>
        [Fact]
        public void ConnectDnsEndpointNullTest() =>
            _ = Assert.Throws<ArgumentNullException>(() => TestClient.Connect((DnsEndPoint)null));

        /// <summary>
        /// Tests the <see cref="AdbClientExtensions.Connect(IAdbClient, IPEndPoint)"/> method.
        /// </summary>
        [Fact]
        public void ConnectIPEndpointNullTest() =>
            _ = Assert.Throws<ArgumentNullException>(() => TestClient.Connect((IPEndPoint)null));

        /// <summary>
        /// Tests the <see cref="AdbClient.Connect(string, int)"/> method.
        /// </summary>
        [Fact]
        public void ConnectHostEndpointNullTest() =>
            _ = Assert.Throws<ArgumentNullException>(() => TestClient.Connect(null));

        /// <summary>
        /// Tests the <see cref="AdbClientExtensions.Disconnect(IAdbClient, DnsEndPoint)"/> method.
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
        /// Tests the <see cref="AdbClient.Install(DeviceData, Stream, Action{InstallProgressEventArgs}?, string[])"/> method.
        /// </summary>
        [Fact]
        public void InstallTest()
        {
            // The app data is sent in chunks of 32 kb
            List<byte[]> applicationDataChunks = [];

            using (FileStream stream = File.OpenRead("Assets/TestApp/base.apk"))
            {
                byte[] buffer = new byte[32 * 1024];
                int read = 0;

                while ((read = stream.Read(buffer.AsSpan(0, buffer.Length))) > 0)
                {
                    byte[] array = buffer.AsSpan(0, read).ToArray();
                    applicationDataChunks.Add(array);
                }
            }

            byte[] response = "Success\n"u8.ToArray();

            using (FileStream stream = File.OpenRead("Assets/TestApp/base.apk"))
            {
                string[] requests =
                [
                    "host:transport:169.254.109.177:5555",
                    $"exec:cmd package 'install' -S {stream.Length}"
                ];

                RunTest(
                    OkResponses(2),
                    NoResponseMessages,
                    requests,
                    NoSyncRequests,
                    NoSyncResponses,
                    [response],
                    applicationDataChunks,
                    () => TestClient.Install(Device, stream,
                        new InstallProgress(
                            PackageInstallProgressState.Preparing,
                            PackageInstallProgressState.Uploading,
                            PackageInstallProgressState.Installing,
                            PackageInstallProgressState.Finished)));
            }
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.InstallMultiple(DeviceData, IEnumerable{Stream}, string, Action{InstallProgressEventArgs}?, string[])"/> method.
        /// </summary>
        [Fact]
        public void InstallMultipleTest()
        {
            // The app data is sent in chunks of 32 kb
            List<byte[]> applicationDataChunks = [];

            using (FileStream stream = File.OpenRead("Assets/TestApp/split_config.arm64_v8a.apk"))
            {
                byte[] buffer = new byte[32 * 1024];
                int read = 0;

                while ((read = stream.Read(buffer.AsSpan(0, buffer.Length))) > 0)
                {
                    byte[] array = buffer.AsSpan(0, read).ToArray();
                    applicationDataChunks.Add(array);
                }
            }

            using (FileStream stream = File.OpenRead("Assets/TestApp/split_config.xxhdpi.apk"))
            {
                byte[] buffer = new byte[32 * 1024];
                int read = 0;

                while ((read = stream.Read(buffer.AsSpan(0, buffer.Length))) > 0)
                {
                    byte[] array = buffer.AsSpan(0, read).ToArray();
                    applicationDataChunks.Add(array);
                }
            }

            using FileStream abiStream = File.OpenRead("Assets/TestApp/split_config.arm64_v8a.apk");
            using FileStream dpiStream = File.OpenRead("Assets/TestApp/split_config.xxhdpi.apk");

            string[] requests =
            [
                "host:transport:169.254.109.177:5555",
                "exec:cmd package 'install-create' -p com.google.android.gms",
                "host:transport:169.254.109.177:5555",
                $"exec:cmd package 'install-write' -S {abiStream.Length} 936013062 splitAPK0.apk",
                "host:transport:169.254.109.177:5555",
                $"exec:cmd package 'install-write' -S {dpiStream.Length} 936013062 splitAPK1.apk",
                "host:transport:169.254.109.177:5555",
                "exec:cmd package 'install-commit' 936013062"
            ];

            byte[][] responses =
            [
                AdbClient.Encoding.GetBytes($"Success: streamed {abiStream.Length} bytes\n"),
                AdbClient.Encoding.GetBytes($"Success: streamed {dpiStream.Length} bytes\n")
            ];

            using MemoryStream sessionStream = new("Success: created install session [936013062]\r\n"u8.ToArray());
            using MemoryStream commitStream = new("Success\n"u8.ToArray());

            RunTest(
                OkResponses(8),
                NoResponseMessages,
                requests,
                NoSyncRequests,
                NoSyncResponses,
                responses,
                applicationDataChunks,
                [sessionStream, commitStream],
                () => TestClient.InstallMultiple(Device, [abiStream, dpiStream], "com.google.android.gms",
                    new InstallProgress(
                        PackageInstallProgressState.Preparing,
                        PackageInstallProgressState.CreateSession,
                        PackageInstallProgressState.Uploading,
                        PackageInstallProgressState.Installing,
                        PackageInstallProgressState.Finished)));
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.InstallMultiple(DeviceData, Stream, IEnumerable{Stream}, Action{InstallProgressEventArgs}?, string[])"/> method.
        /// </summary>
        [Fact]
        public void InstallMultipleWithBaseTest()
        {
            // The app data is sent in chunks of 32 kb
            List<byte[]> applicationDataChunks = [];

            using (FileStream stream = File.OpenRead("Assets/TestApp/base.apk"))
            {
                byte[] buffer = new byte[32 * 1024];
                int read = 0;

                while ((read = stream.Read(buffer.AsSpan(0, buffer.Length))) > 0)
                {
                    byte[] array = buffer.AsSpan(0, read).ToArray();
                    applicationDataChunks.Add(array);
                }
            }

            using (FileStream stream = File.OpenRead("Assets/TestApp/split_config.arm64_v8a.apk"))
            {
                byte[] buffer = new byte[32 * 1024];
                int read = 0;

                while ((read = stream.Read(buffer.AsSpan(0, buffer.Length))) > 0)
                {
                    byte[] array = buffer.AsSpan(0, read).ToArray();
                    applicationDataChunks.Add(array);
                }
            }

            using (FileStream stream = File.OpenRead("Assets/TestApp/split_config.xxhdpi.apk"))
            {
                byte[] buffer = new byte[32 * 1024];
                int read = 0;

                while ((read = stream.Read(buffer.AsSpan(0, buffer.Length))) > 0)
                {
                    byte[] array = buffer.AsSpan(0, read).ToArray();
                    applicationDataChunks.Add(array);
                }
            }

            using FileStream baseStream = File.OpenRead("Assets/TestApp/base.apk");
            using FileStream abiStream = File.OpenRead("Assets/TestApp/split_config.arm64_v8a.apk");
            using FileStream dpiStream = File.OpenRead("Assets/TestApp/split_config.xxhdpi.apk");

            string[] requests =
            [
                "host:transport:169.254.109.177:5555",
                "exec:cmd package 'install-create'",
                "host:transport:169.254.109.177:5555",
                $"exec:cmd package 'install-write' -S {baseStream.Length} 936013062 baseAPK.apk",
                "host:transport:169.254.109.177:5555",
                $"exec:cmd package 'install-write' -S {abiStream.Length} 936013062 splitAPK0.apk",
                "host:transport:169.254.109.177:5555",
                $"exec:cmd package 'install-write' -S {dpiStream.Length} 936013062 splitAPK1.apk",
                "host:transport:169.254.109.177:5555",
                "exec:cmd package 'install-commit' 936013062"
            ];

            byte[][] responses =
            [
                AdbClient.Encoding.GetBytes($"Success: streamed {baseStream.Length} bytes\n"),
                AdbClient.Encoding.GetBytes($"Success: streamed {abiStream.Length} bytes\n"),
                AdbClient.Encoding.GetBytes($"Success: streamed {dpiStream.Length} bytes\n")
            ];

            using MemoryStream sessionStream = new("Success: created install session [936013062]\r\n"u8.ToArray());
            using MemoryStream commitStream = new("Success\n"u8.ToArray());

            RunTest(
                OkResponses(10),
                NoResponseMessages,
                requests,
                NoSyncRequests,
                NoSyncResponses,
                responses,
                applicationDataChunks,
                [sessionStream, commitStream],
                () => TestClient.InstallMultiple(Device, baseStream, [abiStream, dpiStream],
                    new InstallProgress(
                        PackageInstallProgressState.Preparing,
                        PackageInstallProgressState.CreateSession,
                        PackageInstallProgressState.Uploading,
                        PackageInstallProgressState.Installing,
                        PackageInstallProgressState.Finished)));
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

            byte[] streamData = "Success: created install session [936013062]\r\n"u8.ToArray();
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
        /// Tests the <see cref="AdbClient.InstallWrite(DeviceData, Stream, string, string, Action{double}?)"/> method.
        /// </summary>
        [Fact]
        public void InstallWriteTest()
        {
            // The app data is sent in chunks of 32 kb
            List<byte[]> applicationDataChunks = [];

            using (FileStream stream = File.OpenRead("Assets/TestApp/base.apk"))
            {
                byte[] buffer = new byte[32 * 1024];
                int read = 0;

                while ((read = stream.Read(buffer.AsSpan(0, buffer.Length))) > 0)
                {
                    byte[] array = buffer.AsSpan(0, read).ToArray();
                    applicationDataChunks.Add(array);
                }
            }

            using (FileStream stream = File.OpenRead("Assets/TestApp/base.apk"))
            {
                string[] requests =
                [
                    "host:transport:169.254.109.177:5555",
                    $"exec:cmd package 'install-write' -S {stream.Length} 936013062 base.apk"
                ];

                byte[] response = AdbClient.Encoding.GetBytes($"Success: streamed {stream.Length} bytes\n");

                RunTest(
                    OkResponses(2),
                    NoResponseMessages,
                    requests,
                    NoSyncRequests,
                    NoSyncResponses,
                    [response],
                    applicationDataChunks,
                    () => TestClient.InstallWrite(Device, stream, "base", "936013062", new InstallProgress()));
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

            byte[] streamData = "Success\r\n"u8.ToArray();
            using MemoryStream shellStream = new(streamData);

            RunTest(
                OkResponses(2),
                NoResponseMessages,
                requests,
                [shellStream],
                () => TestClient.InstallCommit(Device, "936013062"));
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.Uninstall(DeviceData, string, string[])"/> method.
        /// </summary>
        [Fact]
        public void UninstallTest()
        {
            string[] requests =
            [
                "host:transport:169.254.109.177:5555",
                "exec:cmd package 'uninstall' com.android.gallery3d"
            ];

            byte[] streamData = "Success\r\n"u8.ToArray();
            using MemoryStream shellStream = new(streamData);

            RunTest(
                OkResponses(2),
                NoResponseMessages,
                requests,
                [shellStream],
                () => TestClient.Uninstall(Device, "com.android.gallery3d"));
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
        /// Tests the <see cref="AdbClient.Clone()"/> method.
        /// </summary>
        [Fact]
        public void CloneTest()
        {
            Assert.True(TestClient is ICloneable<IAdbClient>);
#if WINDOWS10_0_17763_0_OR_GREATER
            Assert.True(TestClient is ICloneable<IAdbClient.IWinRT>);
#endif
            AdbClient client = TestClient.Clone();
            Assert.Equal(TestClient.EndPoint, client.EndPoint);
            DnsEndPoint endPoint = new("localhost", 5555);
            client = TestClient.Clone(endPoint);
            Assert.Equal(endPoint, client.EndPoint);
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

        private struct InstallProgress(params PackageInstallProgressState[] states) : IProgress<InstallProgressEventArgs>, IProgress<double>
        {
            private PackageInstallProgressState? state;
            private int packageFinished;
            private int packageRequired;
            private double uploadProgress;

            private int step = 0;

            public void Report(InstallProgressEventArgs value)
            {
                if (value.State == state)
                {
                    Assert.True(uploadProgress <= value.UploadProgress, $"{nameof(value.UploadProgress)}: {value.UploadProgress} is less than {uploadProgress}.");
                    Assert.True(packageFinished <= value.PackageFinished, $"{nameof(value.PackageFinished)}: {value.PackageFinished} is less than {packageFinished}.");
                }
                else
                {
                    Assert.Equal(states[step++], value.State);
                }

                if (value.State is
                    PackageInstallProgressState.CreateSession
                    or PackageInstallProgressState.Installing
                    or PackageInstallProgressState.Finished)
                {
                    Assert.Equal(0, value.UploadProgress);
                    Assert.Equal(0, value.PackageRequired);
                    Assert.Equal(0, value.PackageFinished);
                }
                else
                {
                    if (packageRequired == 0)
                    {
                        packageRequired = value.PackageRequired;
                    }
                    else
                    {
                        Assert.Equal(packageRequired, value.PackageRequired);
                    }
                }

                state = value.State;
                packageFinished = value.PackageFinished;
                uploadProgress = value.UploadProgress;
            }

            public void Report(double value)
            {
                Assert.True(uploadProgress <= value, $"{nameof(value)}: {value} is less than {uploadProgress}.");
                uploadProgress = value;
            }
        }
    }
}
