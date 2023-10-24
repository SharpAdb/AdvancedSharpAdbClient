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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Xunit;

namespace AdvancedSharpAdbClient.Tests
{
    public partial class AdbClientTests
    {
        /// <summary>
        /// Tests the <see cref="AdbClient.GetAdbVersionAsync(CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async void GetAdbVersionAsyncTest()
        {
            string[] responseMessages = ["0020"];
            string[] requests = ["host:version"];

            int version = await RunTestAsync(
                OkResponse,
                responseMessages,
                requests,
                () => TestClient.GetAdbVersionAsync());

            // Make sure and the correct value is returned.
            Assert.Equal(32, version);
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.KillAdbAsync(CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async void KillAdbAsyncTest()
        {
            string[] requests = ["host:kill"];

            await RunTestAsync(
                NoResponses,
                NoResponseMessages,
                requests,
                () => TestClient.KillAdbAsync());
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.GetDevicesAsync(CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async void GetDevicesAsyncTest()
        {
            string[] responseMessages = ["169.254.109.177:5555   device product:VS Emulator 5\" KitKat (4.4) XXHDPI Phone model:5__KitKat__4_4__XXHDPI_Phone device:donatello\n"];
            string[] requests = ["host:devices-l"];

            DeviceData[] devices = await RunTestAsync(
                OkResponse,
                responseMessages,
                requests,
                async () => await TestClient.GetDevicesAsync().ToArrayAsync());

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
        /// Tests the <see cref="AdbClient.CreateForwardAsync(DeviceData, string, string, bool, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async void CreateForwardAsyncTest() =>
            await RunCreateForwardAsyncTest(
                (device) => TestClient.CreateForwardAsync(device, "tcp:1", "tcp:2", true),
                "tcp:1;tcp:2");

        /// <summary>
        /// Tests the <see cref="AdbClient.CreateReverseForwardAsync(DeviceData, string, string, bool, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async void CreateReverseAsyncTest() =>
            await RunCreateReverseAsyncTest(
                (device) => TestClient.CreateReverseForwardAsync(device, "tcp:1", "tcp:2", true),
                "tcp:1;tcp:2");

        /// <summary>
        /// Tests the <see cref="AdbClientExtensions.CreateForwardAsync(IAdbClient, DeviceData, int, int, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async void CreateTcpForwardAsyncTest() =>
            await RunCreateForwardAsyncTest(
                (device) => TestClient.CreateForwardAsync(device, 3, 4),
                "tcp:3;tcp:4");

        /// <summary>
        /// Tests the <see cref="AdbClientExtensions.CreateForwardAsync(IAdbClient, DeviceData, int, string, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async void CreateSocketForwardAsyncTest() =>
            await RunCreateForwardAsyncTest(
                (device) => TestClient.CreateForwardAsync(device, 5, "/socket/1"),
                "tcp:5;local:/socket/1");

        /// <summary>
        /// Tests the <see cref="AdbClient.CreateForwardAsync(DeviceData, string, string, bool, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async void CreateDuplicateForwardAsyncTest()
        {
            AdbResponse[] responses = [AdbResponse.FromError("cannot rebind existing socket")];
            string[] requests = ["host-serial:169.254.109.177:5555:forward:norebind:tcp:1;tcp:2"];

            _ = await Assert.ThrowsAsync<AdbException>(() =>
            RunTestAsync(
                responses,
                NoResponseMessages,
                requests,
                () => TestClient.CreateForwardAsync(Device, "tcp:1", "tcp:2", false)));
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.RemoveForwardAsync(DeviceData, int, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async void RemoveForwardAsyncTest()
        {
            string[] requests = ["host-serial:169.254.109.177:5555:killforward:tcp:1"];

            await RunTestAsync(
                OkResponse,
                NoResponseMessages,
                requests,
                () => TestClient.RemoveForwardAsync(Device, 1));
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.RemoveReverseForwardAsync(DeviceData, string, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async void RemoveReverseForwardAsyncTest()
        {
            string[] requests =
            [
                "host:transport:169.254.109.177:5555",
                "reverse:killforward:localabstract:test"
            ];

            await RunTestAsync(
                OkResponses(2),
                NoResponseMessages,
                requests,
                () => TestClient.RemoveReverseForwardAsync(Device, "localabstract:test"));
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.RemoveAllForwardsAsync(DeviceData, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async void RemoveAllForwardsAsyncTest()
        {
            string[] requests = ["host-serial:169.254.109.177:5555:killforward-all"];

            await RunTestAsync(
                OkResponse,
                NoResponseMessages,
                requests,
                () => TestClient.RemoveAllForwardsAsync(Device));
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.RemoveAllReverseForwardsAsync(DeviceData, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async void RemoveAllReversesAsyncTest()
        {
            string[] requests =
            [
                "host:transport:169.254.109.177:5555",
                "reverse:killforward-all"
            ];

            await RunTestAsync(
                OkResponses(2),
                NoResponseMessages,
                requests,
                () => TestClient.RemoveAllReverseForwardsAsync(Device));
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.ListForwardAsync(DeviceData, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async void ListForwardAsyncTest()
        {
            string[] responseMessages = ["169.254.109.177:5555 tcp:1 tcp:2\n169.254.109.177:5555 tcp:3 tcp:4\n169.254.109.177:5555 tcp:5 local:/socket/1\n"];
            string[] requests = ["host-serial:169.254.109.177:5555:list-forward"];

            ForwardData[] forwards = await RunTestAsync(
                OkResponse,
                responseMessages,
                requests,
                async () => await TestClient.ListForwardAsync(Device).ToArrayAsync());

            Assert.NotNull(forwards);
            Assert.Equal(3, forwards.Length);
            Assert.Equal("169.254.109.177:5555", forwards[0].SerialNumber);
            Assert.Equal("tcp:1", forwards[0].Local);
            Assert.Equal("tcp:2", forwards[0].Remote);
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.ListReverseForwardAsync(DeviceData, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async void ListReverseForwardAsyncTest()
        {
            string[] responseMessages = ["(reverse) localabstract:scrcpy tcp:100\n(reverse) localabstract: scrcpy2 tcp:100\n(reverse) localabstract: scrcpy3 tcp:100\n"];

            string[] requests =
            [
                "host:transport:169.254.109.177:5555",
                "reverse:list-forward"
            ];

            ForwardData[] forwards = await RunTestAsync(
                OkResponses(2),
                responseMessages,
                requests,
                async () => await TestClient.ListReverseForwardAsync(Device).ToArrayAsync());

            Assert.NotNull(forwards);
            Assert.Equal(3, forwards.Length);
            Assert.Equal("(reverse)", forwards[0].SerialNumber);
            Assert.Equal("localabstract:scrcpy", forwards[0].Local);
            Assert.Equal("tcp:100", forwards[0].Remote);
        }

        /// <summary>
        /// Tests the <see cref="AdbClientExtensions.ExecuteServerCommandAsync(IAdbClient, string, string, IShellOutputReceiver, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async void ExecuteServerCommandAsyncTest()
        {
            string[] requests = ["host:version"];

            byte[] streamData = Encoding.ASCII.GetBytes("0020");
            await using MemoryStream shellStream = new(streamData);

            ConsoleOutputReceiver receiver = new();

            await RunTestAsync(
                OkResponse,
                NoResponseMessages,
                requests,
                [shellStream],
                () => TestClient.ExecuteServerCommandAsync("host", "version", receiver));

            string version = receiver.ToString();
            Assert.Equal("0020\r\n", receiver.ToString(), ignoreLineEndingDifferences: true);
            Assert.Equal(32, int.Parse(version, NumberStyles.HexNumber));
        }

        /// <summary>
        /// Tests the <see cref="AdbClientExtensions.ExecuteRemoteCommandAsync(IAdbClient, string, DeviceData, IShellOutputReceiver, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async void ExecuteRemoteCommandAsyncTest()
        {
            string[] requests =
            [
                "host:transport:169.254.109.177:5555",
                "shell:echo Hello, World"
            ];

            byte[] streamData = Encoding.ASCII.GetBytes("Hello, World\r\n");
            await using MemoryStream shellStream = new(streamData);

            ConsoleOutputReceiver receiver = new();

            await RunTestAsync(
                OkResponses(2),
                NoResponseMessages,
                requests,
                [shellStream],
                () => TestClient.ExecuteRemoteCommandAsync("echo Hello, World", Device, receiver));

            Assert.Equal("Hello, World\r\n", receiver.ToString(), ignoreLineEndingDifferences: true);
        }

        /// <summary>
        /// Tests the <see cref="AdbClientExtensions.ExecuteRemoteCommandAsync(IAdbClient, string, DeviceData, IShellOutputReceiver, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async void ExecuteRemoteCommandAsyncUnresponsiveTest()
        {
            string[] requests =
            [
                "host:transport:169.254.109.177:5555",
                "shell:echo Hello, World"
            ];

            ConsoleOutputReceiver receiver = new();

            _ = await Assert.ThrowsAsync<ShellCommandUnresponsiveException>(() =>
            RunTestAsync(
                OkResponses(2),
                NoResponseMessages,
                requests,
                null,
                () => TestClient.ExecuteRemoteCommandAsync("echo Hello, World", Device, receiver)));
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.GetFrameBufferAsync(DeviceData, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async void GetFrameBufferAsyncTest()
        {
            DummyAdbSocket socket = new();

            socket.Responses.Enqueue(AdbResponse.OK);
            socket.Responses.Enqueue(AdbResponse.OK);

            socket.Requests.Add("host:transport:169.254.109.177:5555");
            socket.Requests.Add("framebuffer:");

            socket.SyncDataReceived.Enqueue(File.ReadAllBytes("Assets/framebufferheader.bin"));
            socket.SyncDataReceived.Enqueue(File.ReadAllBytes("Assets/framebuffer.bin"));

            Framebuffer framebuffer = null;

            using (FactoriesLocker locker = await FactoriesLocker.WaitAsync())
            {
                Factories.AdbSocketFactory = (endPoint) => socket;
                framebuffer = await TestClient.GetFrameBufferAsync(Device);
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

#if WINDOWS
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
        /// Tests the <see cref="AdbClientExtensions.RunLogServiceAsync(IAdbClient, DeviceData, Action{LogEntry}, LogId[])"/> method.
        /// </summary>
        [Fact]
        public async void RunLogServiceAsyncTest()
        {
            string[] requests =
            [
                "host:transport:169.254.109.177:5555",
                "shell:logcat -B -b system"
            ];

            ConsoleOutputReceiver receiver = new();

            await using FileStream stream = File.OpenRead("Assets/logcat.bin");
            await using ShellStream shellStream = new(stream, false);
            List<LogEntry> logs = [];
            Action<LogEntry> sink = logs.Add;

            await RunTestAsync(
                OkResponses(2),
                NoResponseMessages,
                requests,
                [shellStream],
                () => TestClient.RunLogServiceAsync(Device, sink, LogId.System));

            Assert.Equal(3, logs.Count);
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.RebootAsync(string, DeviceData, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async void RebootAsyncTest()
        {
            string[] requests =
            [
                "host:transport:169.254.109.177:5555",
                "reboot:"
            ];

            await RunTestAsync(
                OkResponses(2),
                NoResponseMessages,
                requests,
                () => TestClient.RebootAsync(Device));
        }

        /// <summary>
        /// Tests the <see cref="AdbClientExtensions.PairAsync(IAdbClient, IPAddress, string, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async void PairAsyncIPAddressTest() =>
            await RunPairAsyncTest(
                () => TestClient.PairAsync(IPAddress.Loopback, "114514"),
                "127.0.0.1:5555",
                "114514");

        /// <summary>
        /// Tests the <see cref="AdbClient.PairAsync(DnsEndPoint, string, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async void PairAsyncDnsEndpointTest() =>
            await RunPairAsyncTest(
                () => TestClient.PairAsync(new DnsEndPoint("localhost", 1234), "114514"),
                "localhost:1234",
                "114514");

        /// <summary>
        /// Tests the <see cref="AdbClientExtensions.PairAsync(IAdbClient, IPEndPoint, string, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async void PairAsyncIPEndpointTest() =>
            await RunPairAsyncTest(
                () => TestClient.PairAsync(new IPEndPoint(IPAddress.Loopback, 4321), "114514"),
                "127.0.0.1:4321",
                "114514");

        /// <summary>
        /// Tests the <see cref="AdbClientExtensions.PairAsync(IAdbClient, string, string, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async void PairAsyncHostEndpointTest() =>
            await RunPairAsyncTest(
                () => TestClient.PairAsync("localhost:9926", "114514"),
                "localhost:9926",
                "114514");

        /// <summary>
        /// Tests the <see cref="AdbClientExtensions.PairAsync(IAdbClient, IPAddress, string, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async void PairAsyncIPAddressNullTest() =>
            _ = await Assert.ThrowsAsync<ArgumentNullException>(() => TestClient.PairAsync((IPAddress)null, "114514"));

        /// <summary>
        /// Tests the <see cref="AdbClient.PairAsync(DnsEndPoint, string, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async void PairAsyncDnsEndpointNullTest() =>
            _ = await Assert.ThrowsAsync<ArgumentNullException>(() => TestClient.PairAsync(null, "114514"));

        /// <summary>
        /// Tests the <see cref="AdbClientExtensions.PairAsync(IAdbClient, IPEndPoint, string, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async void PairAsyncIPEndpointNullTest() =>
            _ = await Assert.ThrowsAsync<ArgumentNullException>(() => TestClient.PairAsync((IPEndPoint)null, "114514"));

        /// <summary>
        /// Tests the <see cref="AdbClientExtensions.PairAsync(IAdbClient, string, string, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async void PairAsyncHostEndpointNullTest() =>
            _ = await Assert.ThrowsAsync<ArgumentNullException>(() => TestClient.PairAsync((string)null, "114514"));

        /// <summary>
        /// Tests the <see cref="AdbClientExtensions.ConnectAsync(IAdbClient, IPAddress, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async void ConnectAsyncIPAddressTest() =>
            await RunConnectAsyncTest(
                () => TestClient.ConnectAsync(IPAddress.Loopback),
                "127.0.0.1:5555");

        /// <summary>
        /// Tests the <see cref="AdbClient.ConnectAsync(DnsEndPoint, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async void ConnectAsyncDnsEndpointTest() =>
            await RunConnectAsyncTest(
                () => TestClient.ConnectAsync(new DnsEndPoint("localhost", 1234)),
                "localhost:1234");

        /// <summary>
        /// Tests the <see cref="AdbClientExtensions.ConnectAsync(IAdbClient, IPEndPoint, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async void ConnectAsyncIPEndpointTest() =>
            await RunConnectAsyncTest(
                () => TestClient.ConnectAsync(new IPEndPoint(IPAddress.Loopback, 4321)),
                "127.0.0.1:4321");

        /// <summary>
        /// Tests the <see cref="AdbClientExtensions.ConnectAsync(IAdbClient, string, int, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async void ConnectAsyncHostEndpointTest() =>
            await RunConnectAsyncTest(
                () => TestClient.ConnectAsync("localhost:9926"),
                "localhost:9926");

        /// <summary>
        /// Tests the <see cref="AdbClientExtensions.ConnectAsync(IAdbClient, IPAddress, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async void ConnectAsyncIPAddressNullTest() =>
            _ = await Assert.ThrowsAsync<ArgumentNullException>(() => TestClient.ConnectAsync((IPAddress)null));

        /// <summary>
        /// Tests the <see cref="AdbClient.ConnectAsync(DnsEndPoint, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async void ConnectAsyncDnsEndpointNullTest() =>
            _ = await Assert.ThrowsAsync<ArgumentNullException>(() => TestClient.ConnectAsync(null));

        /// <summary>
        /// Tests the <see cref="AdbClientExtensions.ConnectAsync(IAdbClient, IPEndPoint, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async void ConnectAsyncIPEndpointNullTest() =>
            _ = await Assert.ThrowsAsync<ArgumentNullException>(() => TestClient.ConnectAsync((IPEndPoint)null));

        /// <summary>
        /// Tests the <see cref="AdbClientExtensions.ConnectAsync(IAdbClient, string, int, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async void ConnectAsyncHostEndpointNullTest() =>
            _ = await Assert.ThrowsAsync<ArgumentNullException>(() => TestClient.ConnectAsync((string)null));

        /// <summary>
        /// Tests the <see cref="AdbClient.DisconnectAsync(DnsEndPoint, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async void DisconnectAsyncTest()
        {
            string[] requests = ["host:disconnect:localhost:5555"];
            string[] responseMessages = ["disconnected 127.0.0.1:5555"];

            await RunTestAsync(
                OkResponse,
                responseMessages,
                requests,
                () => TestClient.DisconnectAsync(new DnsEndPoint("localhost", 5555)));
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.RootAsync(DeviceData, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async void RootAsyncTest()
        {
            string[] requests =
            [
                "host:transport:169.254.109.177:5555",
                "root:"
            ];

            byte[] expectedData = new byte[1024];
            "adbd cannot run as root in production builds\n"u8.CopyTo(expectedData);

            _ = await Assert.ThrowsAsync<AdbException>(() =>
            RunTestAsync(
                OkResponses(2),
                NoResponseMessages,
                requests,
                NoSyncRequests,
                NoSyncResponses,
                [expectedData],
                null,
                () => TestClient.RootAsync(Device)));
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.UnrootAsync(DeviceData, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async void UnrootAsyncTest()
        {
            string[] requests =
            [
                "host:transport:169.254.109.177:5555",
                "unroot:"
            ];

            byte[] expectedData = new byte[1024];
            "adbd not running as root\n"u8.CopyTo(expectedData);

            _ = await Assert.ThrowsAsync<AdbException>(() =>
            RunTestAsync(
                OkResponses(2),
                NoResponseMessages,
                requests,
                NoSyncRequests,
                NoSyncResponses,
                [expectedData],
                null,
                () => TestClient.UnrootAsync(Device)));
        }

        /// <summary>
        /// Tests the <see cref="AdbClientExtensions.InstallAsync(IAdbClient, DeviceData, Stream, string[])"/> method.
        /// </summary>
        [Fact]
        public async void InstallAsyncTest()
        {
            string[] requests =
            [
                "host:transport:169.254.109.177:5555",
                "exec:cmd package 'install' -S 205774"
            ];

            // The app data is sent in chunks of 32 kb
            List<byte[]> applicationDataChunks = [];

            await using (FileStream stream = File.OpenRead("Assets/testapp.apk"))
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
                        applicationDataChunks.Add(buffer);
                    }
                }
            }

            byte[] response = "Success\n"u8.ToArray();

            await using (FileStream stream = File.OpenRead("Assets/testapp.apk"))
            {
                await RunTestAsync(
                    OkResponses(2),
                    NoResponseMessages,
                    requests,
                    NoSyncRequests,
                    NoSyncResponses,
                    [response],
                    applicationDataChunks.ToArray(),
                    () => TestClient.InstallAsync(Device, stream));
            }
        }

        /// <summary>
        /// Tests the <see cref="AdbClientExtensions.InstallCreateAsync(IAdbClient, DeviceData, string, string[])"/> method.
        /// </summary>
        [Fact]
        public async void InstallCreateAsyncTest()
        {
            string[] requests =
            [
                "host:transport:169.254.109.177:5555",
                "exec:cmd package 'install-create' -p com.google.android.gms"
            ];

            byte[] streamData = Encoding.ASCII.GetBytes("Success: created install session [936013062]\r\n");
            await using MemoryStream shellStream = new(streamData);

            string session = await RunTestAsync(
                OkResponses(2),
                NoResponseMessages,
                requests,
                [shellStream],
                () => TestClient.InstallCreateAsync(Device, "com.google.android.gms"));

            Assert.Equal("936013062", session);
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.InstallWriteAsync(DeviceData, Stream, string, string, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async void InstallWriteAsyncTest()
        {
            string[] requests =
            [
                "host:transport:169.254.109.177:5555",
                "exec:cmd package 'install-write' -S 205774 936013062 base.apk"
            ];

            // The app data is sent in chunks of 32 kb
            List<byte[]> applicationDataChunks = [];

            await using (FileStream stream = File.OpenRead("Assets/testapp.apk"))
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
                        applicationDataChunks.Add(buffer);
                    }
                }
            }

            byte[] response = "Success: streamed 205774 bytes\n"u8.ToArray();

            await using (FileStream stream = File.OpenRead("Assets/testapp.apk"))
            {
                await RunTestAsync(
                    OkResponses(2),
                    NoResponseMessages,
                    requests,
                    NoSyncRequests,
                    NoSyncResponses,
                    [response],
                    applicationDataChunks.ToArray(),
                    () => TestClient.InstallWriteAsync(Device, stream, "base", "936013062"));
            }
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.InstallCommitAsync(DeviceData, string, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async void InstallCommitAsyncTest()
        {
            string[] requests =
            [
                "host:transport:169.254.109.177:5555",
                "exec:cmd package 'install-commit' 936013062"
            ];

            byte[] streamData = Encoding.ASCII.GetBytes("Success\r\n");
            await using MemoryStream shellStream = new(streamData);

            await RunTestAsync(
                OkResponses(2),
                NoResponseMessages,
                requests,
                [shellStream],
                () => TestClient.InstallCommitAsync(Device, "936013062"));
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.GetFeatureSetAsync(DeviceData, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async void GetFeatureSetAsyncTest()
        {
            string[] requests = ["host-serial:169.254.109.177:5555:features"];
            string[] responses = ["sendrecv_v2_brotli,remount_shell,sendrecv_v2,abb_exec,fixed_push_mkdir,fixed_push_symlink_timestamp,abb,shell_v2,cmd,ls_v2,apex,stat_v2\r\n"];

            string[] features = await RunTestAsync(
                OkResponse,
                responses,
                requests,
                async () => await TestClient.GetFeatureSetAsync(Device).ToArrayAsync());

            Assert.Equal(12, features.Length);
            Assert.Equal("sendrecv_v2_brotli", features.FirstOrDefault());
            Assert.Equal("stat_v2", features.LastOrDefault());
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.DumpScreenStringAsync(DeviceData, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async void DumpScreenStringAsyncTest()
        {
            string[] requests =
            [
                "host:transport:169.254.109.177:5555",
                "shell:uiautomator dump /dev/tty"
            ];

            string dump = File.ReadAllText(@"Assets/dumpscreen.txt");
            string cleanDump = File.ReadAllText(@"Assets/dumpscreen_clean.txt");
            byte[] streamData = Encoding.UTF8.GetBytes(dump);
            await using MemoryStream shellStream = new(streamData);

            string xml = await RunTestAsync(
                OkResponses(2),
                NoResponseMessages,
                requests,
                [shellStream],
                () => TestClient.DumpScreenStringAsync(Device));

            Assert.Equal(cleanDump, xml);
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.DumpScreenStringAsync(DeviceData, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async void DumpScreenStringAsyncMIUITest()
        {
            string[] requests =
            [
                "host:transport:169.254.109.177:5555",
                "shell:uiautomator dump /dev/tty"
            ];

            string miuiDump = File.ReadAllText(@"Assets/dumpscreen_miui.txt");
            string cleanMIUIDump = File.ReadAllText(@"Assets/dumpscreen_miui_clean.txt");
            byte[] miuiStreamData = Encoding.UTF8.GetBytes(miuiDump);
            await using MemoryStream miuiStream = new(miuiStreamData);

            string miuiXml = await RunTestAsync(
                OkResponses(2),
                NoResponseMessages,
                requests,
                [miuiStream],
                () => TestClient.DumpScreenStringAsync(Device));

            Assert.Equal(cleanMIUIDump, miuiXml);
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.DumpScreenStringAsync(DeviceData, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async void DumpScreenStringAsyncEmptyTest()
        {
            string[] requests =
            [
                "host:transport:169.254.109.177:5555",
                "shell:uiautomator dump /dev/tty"
            ];

            await using MemoryStream emptyStream = new();

            string emptyXml = await RunTestAsync(
                OkResponses(2),
                NoResponseMessages,
                requests,
                [emptyStream],
                () => TestClient.DumpScreenStringAsync(Device));

            Assert.True(string.IsNullOrEmpty(emptyXml));
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.DumpScreenStringAsync(DeviceData, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async void DumpScreenStringAsyncErrorTest()
        {
            string[] requests =
            [
                "host:transport:169.254.109.177:5555",
                "shell:uiautomator dump /dev/tty"
            ];

            string errorXml = File.ReadAllText(@"Assets/dumpscreen_error.txt");
            byte[] errorStreamData = Encoding.UTF8.GetBytes(errorXml);
            await using MemoryStream errorStream = new(errorStreamData);

            await Assert.ThrowsAsync<XmlException>(() =>
            RunTestAsync(
                OkResponses(2),
                NoResponseMessages,
                requests,
                [errorStream],
                () => TestClient.DumpScreenStringAsync(Device)));
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.DumpScreenAsync(DeviceData, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async void DumpScreenAsyncTest()
        {
            string[] requests =
            [
                "host:transport:169.254.109.177:5555",
                "shell:uiautomator dump /dev/tty"
            ];

            string dump = File.ReadAllText(@"Assets/dumpscreen.txt");
            byte[] streamData = Encoding.UTF8.GetBytes(dump);
            await using MemoryStream shellStream = new(streamData);

            XmlDocument xml = await RunTestAsync(
                OkResponses(2),
                NoResponseMessages,
                requests,
                [shellStream],
                () => TestClient.DumpScreenAsync(Device));

            string cleanDump = File.ReadAllText(@"Assets/dumpscreen_clean.txt");
            XmlDocument doc = new();
            doc.LoadXml(cleanDump);

            Assert.Equal(doc, xml);
        }

#if WINDOWS10_0_17763_0_OR_GREATER
        /// <summary>
        /// Tests the <see cref="AdbClient.DumpScreenWinRTAsync(DeviceData, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async void DumpScreenWinRTAsyncTest()
        {
            string[] requests =
            [
                "host:transport:169.254.109.177:5555",
                "shell:uiautomator dump /dev/tty"
            ];

            string dump = File.ReadAllText(@"Assets/dumpscreen.txt");
            byte[] streamData = Encoding.UTF8.GetBytes(dump);
            await using MemoryStream shellStream = new(streamData);

            Windows.Data.Xml.Dom.XmlDocument xml = await RunTestAsync(
                OkResponses(2),
                NoResponseMessages,
                requests,
                [shellStream],
                () => TestClient.DumpScreenWinRTAsync(Device));

            string cleanDump = File.ReadAllText(@"Assets/dumpscreen_clean.txt");
            Windows.Data.Xml.Dom.XmlDocument doc = new();
            doc.LoadXml(cleanDump);

            Assert.Equal(doc.InnerText, xml.InnerText);
        }

#endif

        /// <summary>
        /// Tests the <see cref="AdbClient.ClickAsync(DeviceData, int, int, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async void ClickAsyncTest()
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
            await using MemoryStream shellStream = new(streamData);

            JavaException exception = await Assert.ThrowsAsync<JavaException>(() =>
            RunTestAsync(
                OkResponses(2),
                NoResponseMessages,
                requests,
                [shellStream],
                () => TestClient.ClickAsync(Device, 100, 100)));

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
        at android.os.Binder.execTransact(Binder.java:1134)", exception.JavaStackTrace, ignoreLineEndingDifferences: true);
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.ClickAsync(DeviceData, Point, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async void ClickCordsAsyncTest()
        {
            string[] requests =
            [
                "host:transport:169.254.109.177:5555",
                "shell:input tap 100 100"
            ];

            byte[] streamData = "Error: Injecting to another application requires INJECT_EVENTS permission\r\n"u8.ToArray();
            await using MemoryStream shellStream = new(streamData);

            _ = await Assert.ThrowsAsync<ElementNotFoundException>(() =>
            RunTestAsync(
                OkResponses(2),
                NoResponseMessages,
                requests,
                [shellStream],
                () => TestClient.ClickAsync(Device, new Point(100, 100))));
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.SwipeAsync(DeviceData, int, int, int, int, long, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async void SwipeAsyncTest()
        {
            string[] requests =
            [
                "host:transport:169.254.109.177:5555",
                "shell:input swipe 100 200 300 400 500"
            ];

            await using MemoryStream shellStream = new();

            await RunTestAsync(
                OkResponses(2),
                NoResponseMessages,
                requests,
                [shellStream],
                () => TestClient.SwipeAsync(Device, 100, 200, 300, 400, 500));
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.SwipeAsync(DeviceData, Element, Element, long, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async void SwipeAsyncElementTest()
        {
            string[] requests =
            [
                "host:transport:169.254.109.177:5555",
                "shell:input swipe 100 200 300 400 500"
            ];

            await using MemoryStream shellStream = new();

            await RunTestAsync(
                OkResponses(2),
                NoResponseMessages,
                requests,
                [shellStream],
                () => TestClient.SwipeAsync(Device, new Element(TestClient, Device, new Rectangle(0, 0, 200, 400)), new Element(TestClient, Device, new Rectangle(0, 0, 600, 800)), 500));
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.IsAppRunningAsync(DeviceData, string, CancellationToken)"/> method.
        /// </summary>
        [Theory]
        [InlineData("21216 27761\r\n", true)]
        [InlineData(" 21216 27761\r\n", true)]
        [InlineData(" \r\n", false)]
        [InlineData("\r\n", false)]
        [InlineData(" ", false)]
        [InlineData("", false)]
        public async void IsAppRunningAsyncTest(string response, bool expected)
        {
            string[] requests =
            [
                "host:transport:169.254.109.177:5555",
                "shell:pidof com.google.android.gms"
            ];

            byte[] streamData = Encoding.UTF8.GetBytes(response);
            await using MemoryStream shellStream = new(streamData);

            bool result = await RunTestAsync(
                OkResponses(2),
                NoResponseMessages,
                requests,
                [shellStream],
                () => TestClient.IsAppRunningAsync(Device, "com.google.android.gms"));

            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.IsAppInForegroundAsync(DeviceData, string, CancellationToken)"/> method.
        /// </summary>
        [Theory]
        [InlineData("app.lawnchair", true)]
        [InlineData("com.android.settings", true)]
        [InlineData("com.google.android.gms", false)]
        public async void IsAppInForegroundAsyncTest(string packageName, bool expected)
        {
            string[] requests =
            [
                "host:transport:169.254.109.177:5555",
                "shell:dumpsys activity activities | grep mResumedActivity"
            ];

            byte[] streamData = @"    mResumedActivity: ActivityRecord{1f5309a u0 com.android.settings/.homepage.SettingsHomepageActivity t61029}
    mResumedActivity: ActivityRecord{896cc3 u0 app.lawnchair/.LawnchairLauncher t5}"u8.ToArray();
            await using MemoryStream shellStream = new(streamData);

            bool result = await RunTestAsync(
                OkResponses(2),
                NoResponseMessages,
                requests,
                [shellStream],
                () => TestClient.IsAppInForegroundAsync(Device, packageName));

            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.GetAppStatusAsync(DeviceData, string, CancellationToken)"/> method.
        /// </summary>
        [Theory]
        [InlineData("com.google.android.gms", "21216 27761\r\n", AppStatus.Background)]
        [InlineData("com.android.gallery3d", "\r\n", AppStatus.Stopped)]
        public async void GetAppStatusAsyncTest(string packageName, string response, AppStatus expected)
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
            await using MemoryStream activityStream = new(activityData);
            byte[] pidData = Encoding.UTF8.GetBytes(response);
            await using MemoryStream pidStream = new(pidData);

            AppStatus result = await RunTestAsync(
                OkResponses(4),
                NoResponseMessages,
                requests,
                [activityStream, pidStream],
                () => TestClient.GetAppStatusAsync(Device, packageName));

            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.GetAppStatusAsync(DeviceData, string, CancellationToken)"/> method.
        /// </summary>
        [Theory]
        [InlineData("app.lawnchair", AppStatus.Foreground)]
        [InlineData("com.android.settings", AppStatus.Foreground)]
        public async void GetAppStatusAsyncForegroundTest(string packageName, AppStatus expected)
        {
            string[] requests =
            [
                "host:transport:169.254.109.177:5555",
                "shell:dumpsys activity activities | grep mResumedActivity"
            ];

            byte[] streamData = @"    mResumedActivity: ActivityRecord{1f5309a u0 com.android.settings/.homepage.SettingsHomepageActivity t61029}
    mResumedActivity: ActivityRecord{896cc3 u0 app.lawnchair/.LawnchairLauncher t5}"u8.ToArray();
            await using MemoryStream shellStream = new(streamData);

            AppStatus result = await RunTestAsync(
                OkResponses(2),
                NoResponseMessages,
                requests,
                [shellStream],
                () => TestClient.GetAppStatusAsync(Device, packageName));

            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.FindElementAsync(DeviceData, string, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async void FindElementAsyncTest()
        {
            string[] requests =
            [
                "host:transport:169.254.109.177:5555",
                "shell:uiautomator dump /dev/tty"
            ];

            string dump = File.ReadAllText(@"Assets/dumpscreen.txt");
            byte[] streamData = Encoding.UTF8.GetBytes(dump);
            await using MemoryStream shellStream = new(streamData);

            Element element = await RunTestAsync(
                OkResponses(2),
                NoResponseMessages,
                requests,
                [shellStream],
                () => TestClient.FindElementAsync(Device));

            Assert.Equal(144, element.GetChildCount());
            Element child = element[0][0][0][0][0][0][0][0][2][1][0][0];
            Assert.Equal("where-where", child.Text);
            Assert.Equal(Rectangle.FromLTRB(45, 889, 427, 973), child.Bounds);
            Assert.Equal(child, element.FindDescendantOrSelf(x => x.Text == "where-where"));
            Assert.Equal(2, element.FindDescendants().Where(x => x.Text == "where-where").Count());
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.FindElementsAsync(DeviceData, string, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async void FindElementsAsyncTest()
        {
            string[] requests =
            [
                "host:transport:169.254.109.177:5555",
                "shell:uiautomator dump /dev/tty"
            ];

            string dump = File.ReadAllText(@"Assets/dumpscreen.txt");
            byte[] streamData = Encoding.UTF8.GetBytes(dump);
            await using MemoryStream shellStream = new(streamData);

            Element[] elements = await RunTestAsync(
                OkResponses(2),
                NoResponseMessages,
                requests,
                [shellStream],
                async () => await TestClient.FindElementsAsync(Device).ToArrayAsync());

            int childCount = elements.Length;
            Array.ForEach(elements, x => childCount += x.GetChildCount());
            Assert.Equal(145, childCount);
            Element element = elements[0][0][0][0][0][0][0][0][0][2][1][0][0];
            Assert.Equal("where-where", element.Attributes["text"]);
            Assert.Equal(Rectangle.FromLTRB(45, 889, 427, 973), element.Bounds);
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.FindAsyncElements(DeviceData, string, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async void FindAsyncElementsTest()
        {
            string[] requests =
            [
                "host:transport:169.254.109.177:5555",
                "shell:uiautomator dump /dev/tty"
            ];

            string dump = File.ReadAllText(@"Assets/dumpscreen.txt");
            byte[] streamData = Encoding.UTF8.GetBytes(dump);
            await using MemoryStream shellStream = new(streamData);

            List<Element> elements = await RunTestAsync(
                OkResponses(2),
                NoResponseMessages,
                requests,
                [shellStream],
                async () =>
                {
                    List<Element> elements = [];
                    await foreach (Element element in TestClient.FindAsyncElements(Device))
                    {
                        elements.Add(element);
                    }
                    return elements;
                });

            int childCount = elements.Count;
            elements.ForEach(x => childCount += x.GetChildCount());
            Assert.Equal(145, childCount);
            Element element = elements[0][0][0][0][0][0][0][0][0][2][1][0][0];
            Assert.Equal("where-where", element.Attributes["text"]);
            Assert.Equal(Rectangle.FromLTRB(45, 889, 427, 973), element.Bounds);
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.SendKeyEventAsync(DeviceData, string, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async void SendKeyEventAsyncTest()
        {
            string[] requests =
            [
                "host:transport:169.254.109.177:5555",
                "shell:input keyevent KEYCODE_MOVE_END"
            ];

            await using MemoryStream shellStream = new();

            await RunTestAsync(
                OkResponses(2),
                NoResponseMessages,
                requests,
                [shellStream],
                () => TestClient.SendKeyEventAsync(Device, "KEYCODE_MOVE_END"));
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.SendTextAsync(DeviceData, string, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async void SendTextAsyncTest()
        {
            string[] requests =
            [
                "host:transport:169.254.109.177:5555",
                "shell:input text Hello, World",
            ];

            await using MemoryStream shellStream = new();

            await RunTestAsync(
                OkResponses(2),
                NoResponseMessages,
                requests,
                [shellStream],
                () => TestClient.SendTextAsync(Device, "Hello, World"));
        }

        /// <summary>
        /// Tests the <see cref="AdbClientExtensions.ClearInputAsync(IAdbClient, DeviceData, int, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async void ClearInputAsyncTest()
        {
            string[] requests =
            [
                "host:transport:169.254.109.177:5555",
                "shell:input keyevent KEYCODE_MOVE_END",
                "host:transport:169.254.109.177:5555",
                "shell:input keyevent KEYCODE_DEL KEYCODE_DEL KEYCODE_DEL"
            ];

            await using MemoryStream firstShellStream = new();
            await using MemoryStream secondShellStream = new();

            await RunTestAsync(
                OkResponses(4),
                NoResponseMessages,
                requests,
                [firstShellStream, secondShellStream],
                () => TestClient.ClearInputAsync(Device, 3));
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.StartAppAsync(DeviceData, string, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async void StartAppAsyncTest()
        {
            string[] requests =
            [
                "host:transport:169.254.109.177:5555",
                "shell:monkey -p com.android.settings 1",
            ];

            await RunTestAsync(
                OkResponses(2),
                NoResponseMessages,
                requests,
                () => TestClient.StartAppAsync(Device, "com.android.settings"));
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.StopAppAsync(DeviceData, string, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async void StopAppAsyncTest()
        {
            string[] requests =
            [
                "host:transport:169.254.109.177:5555",
                "shell:am force-stop com.android.settings",
            ];

            await RunTestAsync(
                OkResponses(2),
                NoResponseMessages,
                requests,
                () => TestClient.StopAppAsync(Device, "com.android.settings"));
        }

        /// <summary>
        /// Tests the <see cref="AdbClientExtensions.ClickBackButtonAsync(IAdbClient, DeviceData, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async void ClickBackButtonAsyncTest()
        {
            string[] requests =
            [
                "host:transport:169.254.109.177:5555",
                "shell:input keyevent KEYCODE_BACK"
            ];

            await using MemoryStream shellStream = new();

            await RunTestAsync(
                OkResponses(2),
                NoResponseMessages,
                requests,
                [shellStream],
                () => TestClient.ClickBackButtonAsync(Device));
        }

        /// <summary>
        /// Tests the <see cref="AdbClientExtensions.ClickHomeButtonAsync(IAdbClient, DeviceData, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async void ClickHomeButtonAsyncTest()
        {
            string[] requests =
            [
                "host:transport:169.254.109.177:5555",
                "shell:input keyevent KEYCODE_HOME"
            ];

            await using MemoryStream shellStream = new();

            await RunTestAsync(
                OkResponses(2),
                NoResponseMessages,
                requests,
                [shellStream],
                () => TestClient.ClickHomeButtonAsync(Device));
        }

        private Task RunConnectAsyncTest(Func<Task> test, string connectString)
        {
            string[] requests = [$"host:connect:{connectString}"];
            string[] responseMessages = [$"connected to {connectString}"];

            return RunTestAsync(
                OkResponse,
                responseMessages,
                requests,
                test);
        }

        private Task RunPairAsyncTest(Func<Task> test, string connectString, string code)
        {
            string[] requests = [$"host:pair:{code}:{connectString}"];
            string[] responseMessages = [$"Successfully paired to {connectString} [guid=adb-996198a3-xPRwsQ]"];

            return RunTestAsync(
                OkResponse,
                responseMessages,
                requests,
                test);
        }

        private Task RunCreateReverseAsyncTest(Func<DeviceData, Task> test, string reverseString)
        {
            string[] requests =
            [
                "host:transport:169.254.109.177:5555",
                $"reverse:forward:{reverseString}",
            ];

            return RunTestAsync(
                OkResponses(3),
                [null],
                requests,
                () => test(Device));
        }

        private Task RunCreateForwardAsyncTest(Func<DeviceData, Task> test, string forwardString)
        {
            string[] requests = [$"host-serial:169.254.109.177:5555:forward:{forwardString}"];

            return RunTestAsync(
                OkResponses(2),
                [null],
                requests,
                () => test(Device));
        }
    }
}
