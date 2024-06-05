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
using Xunit;

namespace AdvancedSharpAdbClient.Tests
{
    public partial class AdbClientTests
    {
        /// <summary>
        /// Tests the <see cref="AdbClient.GetAdbVersionAsync(CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async Task GetAdbVersionAsyncTest()
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
        public async Task KillAdbAsyncTest()
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
        public async Task GetDevicesAsyncTest()
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
        public async Task CreateForwardAsyncTest() =>
            await RunCreateForwardAsyncTest(
                device => TestClient.CreateForwardAsync(device, "tcp:1", "tcp:2", true),
                "tcp:1;tcp:2");

        /// <summary>
        /// Tests the <see cref="AdbClient.CreateReverseForwardAsync(DeviceData, string, string, bool, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async Task CreateReverseAsyncTest() =>
            await RunCreateReverseAsyncTest(
                device => TestClient.CreateReverseForwardAsync(device, "tcp:1", "tcp:2", true),
                "tcp:1;tcp:2");

        /// <summary>
        /// Tests the <see cref="AdbClientExtensions.CreateForwardAsync(IAdbClient, DeviceData, int, int, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async Task CreateTcpForwardAsyncTest() =>
            await RunCreateForwardAsyncTest(
                device => TestClient.CreateForwardAsync(device, 3, 4),
                "tcp:3;tcp:4");

        /// <summary>
        /// Tests the <see cref="AdbClientExtensions.CreateForwardAsync(IAdbClient, DeviceData, int, string, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async Task CreateSocketForwardAsyncTest() =>
            await RunCreateForwardAsyncTest(
                device => TestClient.CreateForwardAsync(device, 5, "/socket/1"),
                "tcp:5;local:/socket/1");

        /// <summary>
        /// Tests the <see cref="AdbClient.CreateForwardAsync(DeviceData, string, string, bool, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async Task CreateDuplicateForwardAsyncTest()
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
        public async Task RemoveForwardAsyncTest()
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
        public async Task RemoveReverseForwardAsyncTest()
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
        public async Task RemoveAllForwardsAsyncTest()
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
        public async Task RemoveAllReversesAsyncTest()
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
        public async Task ListForwardAsyncTest()
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
        public async Task ListReverseForwardAsyncTest()
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
        /// Tests the <see cref="AdbClient.ExecuteServerCommandAsync(string, string, IShellOutputReceiver?, Encoding, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async Task ExecuteServerCommandAsyncTest()
        {
            string[] requests = ["host:version"];

            byte[] streamData = "0020"u8.ToArray();
            await using MemoryStream shellStream = new(streamData);

            ConsoleOutputReceiver receiver = new();

            await RunTestAsync(
                OkResponse,
                NoResponseMessages,
                requests,
                [shellStream],
                () => TestClient.ExecuteServerCommandAsync("host", "version", receiver, AdbClient.Encoding));

            string version = receiver.ToString();
            Assert.Equal("0020\r\n", receiver.ToString(), ignoreLineEndingDifferences: true);
            Assert.Equal(32, int.Parse(version, NumberStyles.HexNumber));
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.ExecuteRemoteCommandAsync(string, DeviceData, IShellOutputReceiver?, Encoding, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async Task ExecuteRemoteCommandAsyncTest()
        {
            string[] requests =
            [
                "host:transport:169.254.109.177:5555",
                "shell:echo Hello, World"
            ];

            byte[] streamData = "Hello, World\r\n"u8.ToArray();
            await using MemoryStream shellStream = new(streamData);

            ConsoleOutputReceiver receiver = new();

            await RunTestAsync(
                OkResponses(2),
                NoResponseMessages,
                requests,
                [shellStream],
                () => TestClient.ExecuteRemoteCommandAsync("echo Hello, World", Device, receiver, AdbClient.Encoding));

            Assert.Equal("Hello, World\r\n", receiver.ToString(), ignoreLineEndingDifferences: true);
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.ExecuteRemoteCommandAsync(string, DeviceData, IShellOutputReceiver?, Encoding, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async Task ExecuteRemoteCommandAsyncUnresponsiveTest()
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
                () => TestClient.ExecuteRemoteCommandAsync("echo Hello, World", Device, receiver, AdbClient.Encoding)));
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.GetFrameBufferAsync(DeviceData, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async Task GetFrameBufferAsyncTest()
        {
            string[] requests =
            [
                "host:transport:169.254.109.177:5555",
                "framebuffer:"
            ];

            Framebuffer framebuffer = await RunTestAsync(
                OkResponses(2),
                NoResponseMessages,
                requests,
                NoSyncRequests,
                NoSyncResponses,
                [
                    await File.ReadAllBytesAsync("Assets/FramebufferHeader.bin"),
                    await File.ReadAllBytesAsync("Assets/Framebuffer.bin")
                ],
                null,
                () => TestClient.GetFrameBufferAsync(Device));

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
        /// Tests the <see cref="AdbClient.RunLogServiceAsync(DeviceData, Action{LogEntry}, CancellationToken, LogId[])"/> method.
        /// </summary>
        [Fact]
        public async Task RunLogServiceAsyncTest()
        {
            string[] requests =
            [
                "host:transport:169.254.109.177:5555",
                "shell:logcat -B -b system"
            ];

            ConsoleOutputReceiver receiver = new();

            await using FileStream stream = File.OpenRead("Assets/Logcat.bin");
            await using ShellStream shellStream = new(stream, false);
            List<LogEntry> logs = [];
            Action<LogEntry> sink = logs.Add;

            await RunTestAsync(
                OkResponses(2),
                NoResponseMessages,
                requests,
                [shellStream],
                () => TestClient.RunLogServiceAsync(Device, sink, default, LogId.System));

            Assert.Equal(3, logs.Count);
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.RunLogServiceAsync(DeviceData, CancellationToken, LogId[])"/> method.
        /// </summary>
        [Fact]
        public async Task RunLogServiceEnumerableAsyncTest()
        {
            string[] requests =
            [
                "host:transport:169.254.109.177:5555",
                "shell:logcat -B -b system"
            ];

            ConsoleOutputReceiver receiver = new();

            await using FileStream stream = File.OpenRead("Assets/Logcat.bin");
            await using ShellStream shellStream = new(stream, false);

            List<LogEntry> logs = await RunTestAsync(
                OkResponses(2),
                NoResponseMessages,
                requests,
                [shellStream],
                async () => await TestClient.RunLogServiceAsync(Device, default, LogId.System).ToListAsync());

            Assert.Equal(3, logs.Count);
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.RebootAsync(string, DeviceData, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async Task RebootAsyncTest()
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
        public async Task PairAsyncIPAddressTest() =>
            await RunPairAsyncTest(
                () => TestClient.PairAsync(IPAddress.Loopback, "114514"),
                "127.0.0.1:5555",
                "114514");

        /// <summary>
        /// Tests the <see cref="AdbClientExtensions.PairAsync(IAdbClient, DnsEndPoint, string, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async Task PairAsyncDnsEndpointTest() =>
            await RunPairAsyncTest(
                () => TestClient.PairAsync(new DnsEndPoint("localhost", 1234), "114514"),
                "localhost:1234",
                "114514");

        /// <summary>
        /// Tests the <see cref="AdbClientExtensions.PairAsync(IAdbClient, IPEndPoint, string, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async Task PairAsyncIPEndpointTest() =>
            await RunPairAsyncTest(
                () => TestClient.PairAsync(new IPEndPoint(IPAddress.Loopback, 4321), "114514"),
                "127.0.0.1:4321",
                "114514");

        /// <summary>
        /// Tests the <see cref="AdbClientExtensions.PairAsync(IAdbClient, string, string, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async Task PairAsyncHostEndpointTest() =>
            await RunPairAsyncTest(
                () => TestClient.PairAsync("localhost:9926", "114514"),
                "localhost:9926",
                "114514");

        /// <summary>
        /// Tests the <see cref="AdbClientExtensions.PairAsync(IAdbClient, IPAddress, string, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async Task PairAsyncIPAddressNullTest() =>
            _ = await Assert.ThrowsAsync<ArgumentNullException>(() => TestClient.PairAsync((IPAddress)null, "114514"));

        /// <summary>
        /// Tests the <see cref="AdbClientExtensions.PairAsync(IAdbClient, DnsEndPoint, string, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async Task PairAsyncDnsEndpointNullTest() =>
            _ = await Assert.ThrowsAsync<ArgumentNullException>(() => TestClient.PairAsync((DnsEndPoint)null, "114514"));

        /// <summary>
        /// Tests the <see cref="AdbClientExtensions.PairAsync(IAdbClient, IPEndPoint, string, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async Task PairAsyncIPEndpointNullTest() =>
            _ = await Assert.ThrowsAsync<ArgumentNullException>(() => TestClient.PairAsync((IPEndPoint)null, "114514"));

        /// <summary>
        /// Tests the <see cref="AdbClientExtensions.PairAsync(IAdbClient, string, string, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async Task PairAsyncHostEndpointNullTest() =>
            _ = await Assert.ThrowsAsync<ArgumentNullException>(() => TestClient.PairAsync((string)null, "114514"));

        /// <summary>
        /// Tests the <see cref="AdbClientExtensions.ConnectAsync(IAdbClient, IPAddress, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async Task ConnectAsyncIPAddressTest() =>
            await RunConnectAsyncTest(
                () => TestClient.ConnectAsync(IPAddress.Loopback),
                "127.0.0.1:5555");

        /// <summary>
        /// Tests the <see cref="AdbClientExtensions.ConnectAsync(IAdbClient, DnsEndPoint, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async Task ConnectAsyncDnsEndpointTest() =>
            await RunConnectAsyncTest(
                () => TestClient.ConnectAsync(new DnsEndPoint("localhost", 1234)),
                "localhost:1234");

        /// <summary>
        /// Tests the <see cref="AdbClientExtensions.ConnectAsync(IAdbClient, IPEndPoint, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async Task ConnectAsyncIPEndpointTest() =>
            await RunConnectAsyncTest(
                () => TestClient.ConnectAsync(new IPEndPoint(IPAddress.Loopback, 4321)),
                "127.0.0.1:4321");

        /// <summary>
        /// Tests the <see cref="AdbClient.ConnectAsync(string, int, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async Task ConnectAsyncHostEndpointTest() =>
            await RunConnectAsyncTest(
                () => TestClient.ConnectAsync("localhost:9926"),
                "localhost:9926");

        /// <summary>
        /// Tests the <see cref="AdbClientExtensions.ConnectAsync(IAdbClient, IPAddress, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async Task ConnectAsyncIPAddressNullTest() =>
            _ = await Assert.ThrowsAsync<ArgumentNullException>(() => TestClient.ConnectAsync((IPAddress)null));

        /// <summary>
        /// Tests the <see cref="AdbClientExtensions.ConnectAsync(IAdbClient, DnsEndPoint, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async Task ConnectAsyncDnsEndpointNullTest() =>
            _ = await Assert.ThrowsAsync<ArgumentNullException>(() => TestClient.ConnectAsync((DnsEndPoint)null));

        /// <summary>
        /// Tests the <see cref="AdbClientExtensions.ConnectAsync(IAdbClient, IPEndPoint, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async Task ConnectAsyncIPEndpointNullTest() =>
            _ = await Assert.ThrowsAsync<ArgumentNullException>(() => TestClient.ConnectAsync((IPEndPoint)null));

        /// <summary>
        /// Tests the <see cref="AdbClient.ConnectAsync(string, int, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async Task ConnectAsyncHostEndpointNullTest() =>
            _ = await Assert.ThrowsAsync<ArgumentNullException>(() => TestClient.ConnectAsync(null));

        /// <summary>
        /// Tests the <see cref="AdbClientExtensions.DisconnectAsync(IAdbClient, DnsEndPoint, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async Task DisconnectAsyncTest()
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
        public async Task RootAsyncTest()
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
        public async Task UnrootAsyncTest()
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
        /// Tests the <see cref="AdbClient.InstallAsync(DeviceData, Stream, Action{InstallProgressEventArgs}?, CancellationToken, string[])"/> method.
        /// </summary>
        [Fact]
        public async Task InstallAsyncTest()
        {
            // The app data is sent in chunks of 32 kb
            List<byte[]> applicationDataChunks = [];

            await using (FileStream stream = File.OpenRead("Assets/TestApp/base.apk"))
            {
                byte[] buffer = new byte[32 * 1024];
                int read = 0;

                while ((read = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length))) > 0)
                {
                    byte[] array = buffer.AsSpan(0, read).ToArray();
                    applicationDataChunks.Add(array);
                }
            }

            byte[] response = "Success\n"u8.ToArray();

            await using (FileStream stream = File.OpenRead("Assets/TestApp/base.apk"))
            {
                string[] requests =
                [
                    "host:transport:169.254.109.177:5555",
                    $"exec:cmd package 'install' -S {stream.Length}"
                ];

                await RunTestAsync(
                    OkResponses(2),
                    NoResponseMessages,
                    requests,
                    NoSyncRequests,
                    NoSyncResponses,
                    [response],
                    applicationDataChunks,
                    () => TestClient.InstallAsync(Device, stream,
                        new InstallProgress(
                            PackageInstallProgressState.Preparing,
                            PackageInstallProgressState.Uploading,
                            PackageInstallProgressState.Installing,
                            PackageInstallProgressState.Finished)));
            }
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.InstallMultipleAsync(DeviceData, IEnumerable{Stream}, string, Action{InstallProgressEventArgs}?, CancellationToken, string[])"/> method.
        /// </summary>
        [Fact]
        public async Task InstallMultipleAsyncTest()
        {
            // The app data is sent in chunks of 32 kb
            List<byte[]> applicationDataChunks = [];

            await using (FileStream stream = File.OpenRead("Assets/TestApp/split_config.arm64_v8a.apk"))
            {
                byte[] buffer = new byte[32 * 1024];
                int read = 0;

                while ((read = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length))) > 0)
                {
                    byte[] array = buffer.AsSpan(0, read).ToArray();
                    applicationDataChunks.Add(array);
                }
            }

            await using FileStream abiStream = File.OpenRead("Assets/TestApp/split_config.arm64_v8a.apk");

            string[] requests =
            [
                "host:transport:169.254.109.177:5555",
                "exec:cmd package 'install-create' -p com.google.android.gms",
                "host:transport:169.254.109.177:5555",
                $"exec:cmd package 'install-write' -S {abiStream.Length} 936013062 splitAPK0.apk",
                "host:transport:169.254.109.177:5555",
                "exec:cmd package 'install-commit' 936013062"
            ];

            byte[][] responses =
            [
                AdbClient.Encoding.GetBytes($"Success: streamed {abiStream.Length} bytes\n")
            ];

            await using MemoryStream sessionStream = new("Success: created install session [936013062]\r\n"u8.ToArray());
            await using MemoryStream commitStream = new("Success\n"u8.ToArray());

            await RunTestAsync(
                OkResponses(6),
                NoResponseMessages,
                requests,
                NoSyncRequests,
                NoSyncResponses,
                responses,
                applicationDataChunks,
                [sessionStream, commitStream],
                () => TestClient.InstallMultipleAsync(Device, [abiStream], "com.google.android.gms",
                    new InstallProgress(
                        PackageInstallProgressState.Preparing,
                        PackageInstallProgressState.CreateSession,
                        PackageInstallProgressState.Uploading,
                        PackageInstallProgressState.Installing,
                        PackageInstallProgressState.Finished)));
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.InstallMultipleAsync(DeviceData, Stream, IEnumerable{Stream}, Action{InstallProgressEventArgs}?, CancellationToken, string[])"/> method.
        /// </summary>
        [Fact]
        public async Task InstallMultipleWithBaseAsyncTest()
        {
            // The app data is sent in chunks of 32 kb
            List<byte[]> applicationDataChunks = [];

            await using (FileStream stream = File.OpenRead("Assets/TestApp/base.apk"))
            {
                byte[] buffer = new byte[32 * 1024];
                int read = 0;

                while ((read = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length))) > 0)
                {
                    byte[] array = buffer.AsSpan(0, read).ToArray();
                    applicationDataChunks.Add(array);
                }
            }

            await using (FileStream stream = File.OpenRead("Assets/TestApp/split_config.arm64_v8a.apk"))
            {
                byte[] buffer = new byte[32 * 1024];
                int read = 0;

                while ((read = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length))) > 0)
                {
                    byte[] array = buffer.AsSpan(0, read).ToArray();
                    applicationDataChunks.Add(array);
                }
            }

            await using FileStream baseStream = File.OpenRead("Assets/TestApp/base.apk");
            await using FileStream abiStream = File.OpenRead("Assets/TestApp/split_config.arm64_v8a.apk");

            string[] requests =
            [
                "host:transport:169.254.109.177:5555",
                "exec:cmd package 'install-create'",
                "host:transport:169.254.109.177:5555",
                $"exec:cmd package 'install-write' -S {baseStream.Length} 936013062 baseAPK.apk",
                "host:transport:169.254.109.177:5555",
                $"exec:cmd package 'install-write' -S {abiStream.Length} 936013062 splitAPK0.apk",
                "host:transport:169.254.109.177:5555",
                "exec:cmd package 'install-commit' 936013062"
            ];

            byte[][] responses =
            [
                AdbClient.Encoding.GetBytes($"Success: streamed {baseStream.Length} bytes\n"),
                AdbClient.Encoding.GetBytes($"Success: streamed {abiStream.Length} bytes\n")
            ];

            using MemoryStream sessionStream = new("Success: created install session [936013062]\r\n"u8.ToArray());
            using MemoryStream commitStream = new("Success\n"u8.ToArray());

            await RunTestAsync(
                OkResponses(8),
                NoResponseMessages,
                requests,
                NoSyncRequests,
                NoSyncResponses,
                responses,
                applicationDataChunks,
                [sessionStream, commitStream],
                () => TestClient.InstallMultipleAsync(Device, baseStream, [abiStream],
                    new InstallProgress(
                        PackageInstallProgressState.Preparing,
                        PackageInstallProgressState.CreateSession,
                        PackageInstallProgressState.Uploading,
                        PackageInstallProgressState.Installing,
                        PackageInstallProgressState.Finished)));
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.InstallCreateAsync(DeviceData, string?, CancellationToken, string[])"/> method.
        /// </summary>
        [Fact]
        public async Task InstallCreateAsyncTest()
        {
            string[] requests =
            [
                "host:transport:169.254.109.177:5555",
                "exec:cmd package 'install-create' -p com.google.android.gms"
            ];

            byte[] streamData = "Success: created install session [936013062]\r\n"u8.ToArray();
            await using MemoryStream shellStream = new(streamData);

            string session = await RunTestAsync(
                OkResponses(2),
                NoResponseMessages,
                requests,
                [shellStream],
                () => TestClient.InstallCreateAsync(Device, "com.google.android.gms", default));

            Assert.Equal("936013062", session);
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.InstallWriteAsync(DeviceData, Stream, string, string, Action{double}?, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async Task InstallWriteAsyncTest()
        {
            // The app data is sent in chunks of 32 kb
            List<byte[]> applicationDataChunks = [];

            await using (FileStream stream = File.OpenRead("Assets/TestApp/base.apk"))
            {
                byte[] buffer = new byte[32 * 1024];
                int read = 0;

                while ((read = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length))) > 0)
                {
                    byte[] array = buffer.AsSpan(0, read).ToArray();
                    applicationDataChunks.Add(array);
                }
            }

            await using (FileStream stream = File.OpenRead("Assets/TestApp/base.apk"))
            {
                string[] requests =
                [
                    "host:transport:169.254.109.177:5555",
                    $"exec:cmd package 'install-write' -S {stream.Length} 936013062 base.apk"
                ];

                byte[] response = AdbClient.Encoding.GetBytes($"Success: streamed {stream.Length} bytes\n");

                await RunTestAsync(
                    OkResponses(2),
                    NoResponseMessages,
                    requests,
                    NoSyncRequests,
                    NoSyncResponses,
                    [response],
                    applicationDataChunks,
                    () => TestClient.InstallWriteAsync(Device, stream, "base", "936013062", new InstallProgress()));
            }
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.InstallCommitAsync(DeviceData, string, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async Task InstallCommitAsyncTest()
        {
            string[] requests =
            [
                "host:transport:169.254.109.177:5555",
                "exec:cmd package 'install-commit' 936013062"
            ];

            byte[] streamData = "Success\r\n"u8.ToArray();
            await using MemoryStream shellStream = new(streamData);

            await RunTestAsync(
                OkResponses(2),
                NoResponseMessages,
                requests,
                [shellStream],
                () => TestClient.InstallCommitAsync(Device, "936013062"));
        }

#if WINDOWS10_0_17763_0_OR_GREATER
        /// <summary>
        /// Tests the <see cref="AdbClient.InstallAsync(DeviceData, IRandomAccessStream, Action{InstallProgressEventArgs}?, CancellationToken, string[])"/> method.
        /// </summary>
        [Fact]
        public async Task InstallWinRTAsyncTest()
        {
            if (!OperatingSystem.IsWindowsVersionAtLeast(10)) { return; }

            // The app data is sent in chunks of 32 kb
            List<byte[]> applicationDataChunks = [];

            await using (FileStream stream = File.OpenRead("Assets/TestApp/base.apk"))
            {
                byte[] buffer = new byte[32 * 1024];
                int read = 0;

                while ((read = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length))) > 0)
                {
                    byte[] array = buffer.AsSpan(0, read).ToArray();
                    applicationDataChunks.Add(array);
                }
            }

            byte[] response = "Success\n"u8.ToArray();

            StorageFile storageFile = await StorageFile.GetFileFromPathAsync(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Assets\TestApp\base.apk"));
            using (IRandomAccessStreamWithContentType stream = await storageFile.OpenReadAsync())
            {
                string[] requests =
                [
                    "host:transport:169.254.109.177:5555",
                    $"exec:cmd package 'install' -S {stream.Size}"
                ];

                await RunTestAsync(
                    OkResponses(2),
                    NoResponseMessages,
                    requests,
                    NoSyncRequests,
                    NoSyncResponses,
                    [response],
                    applicationDataChunks,
                    () => TestClient.InstallAsync(Device, stream,
                        new InstallProgress(
                            PackageInstallProgressState.Preparing,
                            PackageInstallProgressState.Uploading,
                            PackageInstallProgressState.Installing,
                            PackageInstallProgressState.Finished)));
            }
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.InstallMultipleAsync(DeviceData, IEnumerable{IRandomAccessStream}, string, Action{InstallProgressEventArgs}?, CancellationToken, string[])"/> method.
        /// </summary>
        [Fact]
        public async Task InstallMultipleWinRTAsyncTest()
        {
            if (!OperatingSystem.IsWindowsVersionAtLeast(10)) { return; }

            // The app data is sent in chunks of 32 kb
            List<byte[]> applicationDataChunks = [];

            await using (FileStream stream = File.OpenRead("Assets/TestApp/split_config.arm64_v8a.apk"))
            {
                byte[] buffer = new byte[32 * 1024];
                int read = 0;

                while ((read = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length))) > 0)
                {
                    byte[] array = buffer.AsSpan(0, read).ToArray();
                    applicationDataChunks.Add(array);
                }
            }

            StorageFile storageFile = await StorageFile.GetFileFromPathAsync(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Assets\TestApp\split_config.arm64_v8a.apk"));
            using IRandomAccessStreamWithContentType abiStream = await storageFile.OpenReadAsync();

            string[] requests =
            [
                "host:transport:169.254.109.177:5555",
                "exec:cmd package 'install-create' -p com.google.android.gms",
                "host:transport:169.254.109.177:5555",
                $"exec:cmd package 'install-write' -S {abiStream.Size} 936013062 splitAPK0.apk",
                "host:transport:169.254.109.177:5555",
                "exec:cmd package 'install-commit' 936013062"
            ];

            byte[][] responses =
            [
                AdbClient.Encoding.GetBytes($"Success: streamed {abiStream.Size} bytes\n")
            ];

            await using MemoryStream sessionStream = new(AdbClient.Encoding.GetBytes("Success: created install session [936013062]\r\n"));
            await using MemoryStream commitStream = new("Success\n"u8.ToArray());

            await RunTestAsync(
                OkResponses(6),
                NoResponseMessages,
                requests,
                NoSyncRequests,
                NoSyncResponses,
                responses,
                applicationDataChunks,
                [sessionStream, commitStream],
                () => TestClient.InstallMultipleAsync(Device, [abiStream], "com.google.android.gms",
                    new InstallProgress(
                        PackageInstallProgressState.Preparing,
                        PackageInstallProgressState.CreateSession,
                        PackageInstallProgressState.Uploading,
                        PackageInstallProgressState.Installing,
                        PackageInstallProgressState.Finished)));
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.InstallMultipleAsync(DeviceData, IRandomAccessStream, IEnumerable{IRandomAccessStream}, Action{InstallProgressEventArgs}?, CancellationToken, string[])"/> method.
        /// </summary>
        [Fact]
        public async Task InstallMultipleWinRTWithBaseAsyncTest()
        {
            if (!OperatingSystem.IsWindowsVersionAtLeast(10)) { return; }

            // The app data is sent in chunks of 32 kb
            List<byte[]> applicationDataChunks = [];

            await using (FileStream stream = File.OpenRead("Assets/TestApp/base.apk"))
            {
                byte[] buffer = new byte[32 * 1024];
                int read = 0;

                while ((read = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length))) > 0)
                {
                    byte[] array = buffer.AsSpan(0, read).ToArray();
                    applicationDataChunks.Add(array);
                }
            }

            await using (FileStream stream = File.OpenRead("Assets/TestApp/split_config.arm64_v8a.apk"))
            {
                byte[] buffer = new byte[32 * 1024];
                int read = 0;

                while ((read = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length))) > 0)
                {
                    byte[] array = buffer.AsSpan(0, read).ToArray();
                    applicationDataChunks.Add(array);
                }
            }

            StorageFile storageFile = await StorageFile.GetFileFromPathAsync(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Assets\TestApp\base.apk"));
            using IRandomAccessStreamWithContentType baseStream = await storageFile.OpenReadAsync();
            storageFile = await StorageFile.GetFileFromPathAsync(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Assets\TestApp\split_config.arm64_v8a.apk"));
            using IRandomAccessStreamWithContentType abiStream = await storageFile.OpenReadAsync();

            string[] requests =
            [
                "host:transport:169.254.109.177:5555",
                "exec:cmd package 'install-create'",
                "host:transport:169.254.109.177:5555",
                $"exec:cmd package 'install-write' -S {baseStream.Size} 936013062 baseAPK.apk",
                "host:transport:169.254.109.177:5555",
                $"exec:cmd package 'install-write' -S {abiStream.Size} 936013062 splitAPK0.apk",
                "host:transport:169.254.109.177:5555",
                "exec:cmd package 'install-commit' 936013062"
            ];

            byte[][] responses =
            [
                AdbClient.Encoding.GetBytes($"Success: streamed {baseStream.Size} bytes\n"),
                AdbClient.Encoding.GetBytes($"Success: streamed {abiStream.Size} bytes\n")
            ];

            using MemoryStream sessionStream = new(AdbClient.Encoding.GetBytes("Success: created install session [936013062]\r\n"));
            using MemoryStream commitStream = new("Success\n"u8.ToArray());

            await RunTestAsync(
                OkResponses(8),
                NoResponseMessages,
                requests,
                NoSyncRequests,
                NoSyncResponses,
                responses,
                applicationDataChunks,
                [sessionStream, commitStream],
                () => TestClient.InstallMultipleAsync(Device, baseStream, [abiStream],
                    new InstallProgress(
                        PackageInstallProgressState.Preparing,
                        PackageInstallProgressState.CreateSession,
                        PackageInstallProgressState.Uploading,
                        PackageInstallProgressState.Installing,
                        PackageInstallProgressState.Finished)));
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.InstallWriteAsync(DeviceData, IRandomAccessStream, string, string, Action{double}?, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async Task InstallWriteWinRTAsyncTest()
        {
            if (!OperatingSystem.IsWindowsVersionAtLeast(10)) { return; }

            // The app data is sent in chunks of 32 kb
            List<byte[]> applicationDataChunks = [];

            await using (FileStream stream = File.OpenRead("Assets/TestApp/base.apk"))
            {
                byte[] buffer = new byte[32 * 1024];
                int read = 0;

                while ((read = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length))) > 0)
                {
                    byte[] array = buffer.AsSpan(0, read).ToArray();
                    applicationDataChunks.Add(array);
                }
            }

            StorageFile storageFile = await StorageFile.GetFileFromPathAsync(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Assets\TestApp\base.apk"));
            using (IRandomAccessStreamWithContentType stream = await storageFile.OpenReadAsync())
            {
                string[] requests =
                [
                    "host:transport:169.254.109.177:5555",
                    $"exec:cmd package 'install-write' -S {stream.Size} 936013062 base.apk"
                ];

                byte[] response = AdbClient.Encoding.GetBytes($"Success: streamed {stream.Size} bytes\n");

                double temp = 0;
                Progress<double> progress = new();
                progress.ProgressChanged += (sender, args) =>
                {
                    Assert.True(temp <= args, $"{nameof(args)}: {args} is less than {temp}.");
                    temp = args;
                };

                await RunTestAsync(
                    OkResponses(2),
                    NoResponseMessages,
                    requests,
                    NoSyncRequests,
                    NoSyncResponses,
                    [response],
                    applicationDataChunks,
                    () => TestClient.InstallWriteAsync(Device, stream, "base", "936013062", progress));
            }
        }
#endif

        /// <summary>
        /// Tests the <see cref="AdbClientExtensions.UninstallAsync(IAdbClient, DeviceData, string, string[])"/> method.
        /// </summary>
        [Fact]
        public async Task UninstallAsyncTest()
        {
            string[] requests =
            [
                "host:transport:169.254.109.177:5555",
                "exec:cmd package 'uninstall' com.android.gallery3d"
            ];

            byte[] streamData = "Success\r\n"u8.ToArray();
            using MemoryStream shellStream = new(streamData);

            await RunTestAsync(
                OkResponses(2),
                NoResponseMessages,
                requests,
                [shellStream],
                () => TestClient.UninstallAsync(Device, "com.android.gallery3d"));
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.GetFeatureSetAsync(DeviceData, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async Task GetFeatureSetAsyncTest()
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
