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
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Xunit;
using System.Data.Common;

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

        /// <summary>
        /// Tests the <see cref="AdbClient.KillAdbAsync(CancellationToken)"/> method.
        /// </summary>
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

        /// <summary>
        /// Tests the <see cref="AdbClient.GetDevicesAsync(CancellationToken)"/> method.
        /// </summary>
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

        /// <summary>
        /// Tests the <see cref="AdbClient.RemoveForwardAsync(DeviceData, int, CancellationToken)"/> method.
        /// </summary>
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

        /// <summary>
        /// Tests the <see cref="AdbClient.RemoveReverseForwardAsync(DeviceData, string, CancellationToken)"/> method.
        /// </summary>
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

        /// <summary>
        /// Tests the <see cref="AdbClient.RemoveAllForwardsAsync(DeviceData, CancellationToken)"/> method.
        /// </summary>
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

        /// <summary>
        /// Tests the <see cref="AdbClient.RemoveAllReverseForwardsAsync(DeviceData, CancellationToken)"/> method.
        /// </summary>
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

        /// <summary>
        /// Tests the <see cref="AdbClient.ListForwardAsync(DeviceData, CancellationToken)"/> method.
        /// </summary>
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

        /// <summary>
        /// Tests the <see cref="AdbClient.ListReverseForwardAsync(DeviceData, CancellationToken)"/> method.
        /// </summary>
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

        /// <summary>
        /// Tests the <see cref="AdbClient.ExecuteRemoteCommandAsync(string, DeviceData, IShellOutputReceiver, CancellationToken)"/> method.
        /// </summary>
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
            await using MemoryStream shellStream = new(streamData);

            ConsoleOutputReceiver receiver = new();

            await RunTestAsync(
                responses,
                responseMessages,
                requests,
                shellStream,
                () => TestClient.ExecuteRemoteCommandAsync("echo Hello, World", device, receiver));

            Assert.Equal("Hello, World\r\n", receiver.ToString(), ignoreLineEndingDifferences: true);
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.ExecuteRemoteCommandAsync(string, DeviceData, IShellOutputReceiver, CancellationToken)"/> method.
        /// </summary>
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

        /// <summary>
        /// Tests the <see cref="AdbClient.GetFrameBufferAsync(DeviceData, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async void GetFrameBufferAsyncTest()
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

            Factories.AdbSocketFactory = (endPoint) => socket;
            framebuffer = await TestClient.GetFrameBufferAsync(device);
            
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

        /// <summary>
        /// Tests the <see cref="AdbClient.RunLogServiceAsync(DeviceData, Action{LogEntry}, CancellationToken, LogId[])"/> method.
        /// </summary>
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

            await using Stream stream = File.OpenRead("Assets/logcat.bin");
            await using ShellStream shellStream = new(stream, false);
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

        /// <summary>
        /// Tests the <see cref="AdbClient.RebootAsync(string, DeviceData, CancellationToken)"/> method.
        /// </summary>
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
            string[] requests = new string[] { "host:disconnect:localhost:5555" };
            string[] responseMessages = new string[] { "disconnected 127.0.0.1:5555" };

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

        /// <summary>
        /// Tests the <see cref="AdbClient.UnrootAsync(DeviceData, CancellationToken)"/> method.
        /// </summary>
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

        /// <summary>
        /// Tests the <see cref="AdbClient.InstallAsync(DeviceData, Stream, string[])"/> method.
        /// </summary>
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

            await using (Stream stream = File.OpenRead("Assets/testapp.apk"))
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

            await using (Stream stream = File.OpenRead("Assets/testapp.apk"))
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

        /// <summary>
        /// Tests the <see cref="AdbClient.InstallCreateAsync(DeviceData, string, string[])"/> method.
        /// </summary>
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
            await using MemoryStream shellStream = new(streamData);

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

        /// <summary>
        /// Tests the <see cref="AdbClient.InstallWriteAsync(DeviceData, Stream, string, string, CancellationToken)"/> method.
        /// </summary>
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

            await using (Stream stream = File.OpenRead("Assets/testapp.apk"))
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

            await using (Stream stream = File.OpenRead("Assets/testapp.apk"))
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

        /// <summary>
        /// Tests the <see cref="AdbClient.InstallCommitAsync(DeviceData, string, CancellationToken)"/> method.
        /// </summary>
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
            await using MemoryStream shellStream = new(streamData);

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

        /// <summary>
        /// Tests the <see cref="AdbClient.GetFeatureSetAsync(DeviceData, CancellationToken)"/> method.
        /// </summary>
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

        /// <summary>
        /// Tests the <see cref="AdbClient.DumpScreenStringAsync(DeviceData, CancellationToken)"/> method.
        /// </summary>
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
            string cleanDump = File.ReadAllText(@"Assets/dumpscreen_clean.txt");
            byte[] streamData = Encoding.UTF8.GetBytes(dump);
            await using MemoryStream shellStream = new(streamData);

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

            Assert.Equal(cleanDump, xml);
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.DumpScreenStringAsync(DeviceData, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async void DumpScreenStringAsyncMIUITest()
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

            string miuiDump = File.ReadAllText(@"Assets/dumpscreen_miui.txt");
            string cleanMIUIDump = File.ReadAllText(@"Assets/dumpscreen_miui_clean.txt");
            byte[] miuiStreamData = Encoding.UTF8.GetBytes(miuiDump);
            await using MemoryStream miuiStream = new(miuiStreamData);

            string miuiXml = string.Empty;

            await RunTestAsync(
                new AdbResponse[]
                {
                    AdbResponse.OK,
                    AdbResponse.OK,
                },
                NoResponseMessages,
                requests,
                miuiStream,
                async () => miuiXml = await TestClient.DumpScreenStringAsync(device));

            Assert.Equal(cleanMIUIDump, miuiXml);
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.DumpScreenStringAsync(DeviceData, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async void DumpScreenStringAsyncEmptyTest()
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

            byte[] emptyStreamData = Encoding.UTF8.GetBytes(string.Empty);
            await using MemoryStream emptyStream = new(emptyStreamData);
            string emptyXml = string.Empty;

            await RunTestAsync(
               new AdbResponse[]
               {
                    AdbResponse.OK,
                    AdbResponse.OK,
               },
               NoResponseMessages,
               requests,
               emptyStream,
               async () => emptyXml = await TestClient.DumpScreenStringAsync(device));

            Assert.True(string.IsNullOrEmpty(emptyXml));
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.DumpScreenStringAsync(DeviceData, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async void DumpScreenStringAsyncErrorTest()
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

            string errorXml = File.ReadAllText(@"Assets/dumpscreen_error.txt");
            byte[] errorStreamData = Encoding.UTF8.GetBytes(errorXml);
            await using MemoryStream errorStream = new(errorStreamData);

            await Assert.ThrowsAsync<XmlException>(() =>
            RunTestAsync(
                new AdbResponse[]
                {
                    AdbResponse.OK,
                    AdbResponse.OK,
                },
                NoResponseMessages,
                requests,
                errorStream,
                () => TestClient.DumpScreenStringAsync(device)));
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.DumpScreenAsync(DeviceData, CancellationToken)"/> method.
        /// </summary>
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
            await using MemoryStream shellStream = new(streamData);

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

            string cleanDump = File.ReadAllText(@"Assets/dumpscreen_clean.txt");
            XmlDocument doc = new();
            doc.LoadXml(cleanDump);

            Assert.Equal(doc, xml);
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.ClickAsync(DeviceData, int, int, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async void ClickAsyncTest()
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
            await using MemoryStream shellStream = new(streamData);

            JavaException exception = await Assert.ThrowsAsync<JavaException>(() =>
            RunTestAsync(
                new AdbResponse[]
                {
                    AdbResponse.OK,
                    AdbResponse.OK,
                },
                NoResponseMessages,
                requests,
                shellStream,
                () => TestClient.ClickAsync(device, 100, 100)));

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
        /// Tests the <see cref="AdbClient.ClickAsync(DeviceData, Cords, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async void ClickCordsAsyncTest()
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
            
            byte[] streamData = Encoding.UTF8.GetBytes(@"Error: Injecting to another application requires INJECT_EVENTS permission");
            await using MemoryStream shellStream = new(streamData);

            _ = await Assert.ThrowsAsync<ElementNotFoundException>(() =>
            RunTestAsync(
                new AdbResponse[]
                {
                    AdbResponse.OK,
                    AdbResponse.OK,
                },
                NoResponseMessages,
                requests,
                shellStream,
                () => TestClient.ClickAsync(device, new Cords(100, 100))));
        }

        /// <summary>
        /// Tests the <see cref="AdbClient.FindElementAsync(DeviceData, string, CancellationToken)"/> method.
        /// </summary>
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
            await using MemoryStream shellStream = new(streamData);

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

        /// <summary>
        /// Tests the <see cref="AdbClient.FindElementsAsync(DeviceData, string, CancellationToken)"/> method.
        /// </summary>
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
            await using MemoryStream shellStream = new(streamData);

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
