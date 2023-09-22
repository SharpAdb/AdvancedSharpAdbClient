using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Xunit;

namespace AdvancedSharpAdbClient.Tests
{
    /// <summary>
    /// Tests the <see cref="SyncService"/> class.
    /// </summary>
    public partial class SyncServiceTests : SocketBasedTests
    {
        // Toggle the integration test flag to true to run on an actual adb server
        // (and to build/validate the test cases), set to false to use the mocked
        // adb sockets.
        // In release mode, this flag is ignored and the mocked adb sockets are always used.
        public SyncServiceTests() : base(integrationTest: false, doDispose: false)
        {
        }

        [Fact]
        public void StatTest()
        {
            DeviceData device = new()
            {
                Serial = "169.254.109.177:5555",
                State = DeviceState.Online
            };

            FileStatistics value = null;

            RunTest(
                OkResponses(2),
                NoResponseMessages,
                Requests("host:transport:169.254.109.177:5555", "sync:"),
                SyncRequests(SyncCommand.STAT, "/fstab.donatello"),
                new[] { SyncCommand.STAT },
                new byte[][] { [160, 129, 0, 0, 85, 2, 0, 0, 0, 0, 0, 0] },
                null,
                () =>
                {
                    using SyncService service = new(Socket, device);
                    value = service.Stat("/fstab.donatello");
                });

            Assert.NotNull(value);
            Assert.Equal(UnixFileMode.Regular, value.FileMode & UnixFileMode.TypeMask);
            Assert.Equal(597, value.Size);
            Assert.Equal(DateTimeHelper.Epoch.ToLocalTime(), value.Time);
        }

        [Fact]
        public void GetListingTest()
        {
            DeviceData device = new()
            {
                Serial = "169.254.109.177:5555",
                State = DeviceState.Online
            };

            List<FileStatistics> value = null;

            RunTest(
                OkResponses(2),
                ResponseMessages(".", "..", "sdcard0", "emulated"),
                Requests("host:transport:169.254.109.177:5555", "sync:"),
                SyncRequests(SyncCommand.LIST, "/storage"),
                new[] { SyncCommand.DENT, SyncCommand.DENT, SyncCommand.DENT, SyncCommand.DENT, SyncCommand.DONE },
                new byte[][]
                {
                    [233, 65, 0, 0, 0, 0, 0, 0, 152, 130, 56, 86],
                    [237, 65, 0, 0, 0, 0, 0, 0, 152, 130, 56, 86],
                    [255, 161, 0, 0, 24, 0, 0, 0, 152, 130, 56, 86],
                    [109, 65, 0, 0, 0, 0, 0, 0, 152, 130, 56, 86]
                },
                null,
                () =>
                {
                    using SyncService service = new(Socket, device);
                    value = service.GetDirectoryListing("/storage").ToList();
                });

            Assert.Equal(4, value.Count);

            DateTime time = new DateTime(2015, 11, 3, 9, 47, 4, DateTimeKind.Utc).ToLocalTime();

            FileStatistics dir = value[0];
            Assert.Equal(".", dir.Path);
            Assert.Equal((UnixFileMode)16873, dir.FileMode);
            Assert.Equal(0, dir.Size);
            Assert.Equal(time, dir.Time);

            FileStatistics parentDir = value[1];
            Assert.Equal("..", parentDir.Path);
            Assert.Equal((UnixFileMode)16877, parentDir.FileMode);
            Assert.Equal(0, parentDir.Size);
            Assert.Equal(time, parentDir.Time);

            FileStatistics sdcard0 = value[2];
            Assert.Equal("sdcard0", sdcard0.Path);
            Assert.Equal((UnixFileMode)41471, sdcard0.FileMode);
            Assert.Equal(24, sdcard0.Size);
            Assert.Equal(time, sdcard0.Time);

            FileStatistics emulated = value[3];
            Assert.Equal("emulated", emulated.Path);
            Assert.Equal((UnixFileMode)16749, emulated.FileMode);
            Assert.Equal(0, emulated.Size);
            Assert.Equal(time, emulated.Time);
        }

        [Fact]
        public void PullTest()
        {
            DeviceData device = new()
            {
                Serial = "169.254.109.177:5555",
                State = DeviceState.Online
            };

            using MemoryStream stream = new();
            byte[] content = File.ReadAllBytes("Assets/fstab.bin");
            byte[] contentLength = BitConverter.GetBytes(content.Length);

            RunTest(
                OkResponses(2),
                ResponseMessages(),
                Requests("host:transport:169.254.109.177:5555", "sync:"),
                SyncRequests(SyncCommand.STAT, "/fstab.donatello").Union(SyncRequests(SyncCommand.RECV, "/fstab.donatello")),
                [SyncCommand.STAT, SyncCommand.DATA, SyncCommand.DONE],
                [
                    [160, 129, 0, 0, 85, 2, 0, 0, 0, 0, 0, 0],
                    contentLength,
                    content
                ],
                null,
                () =>
                {
                    using SyncService service = new(Socket, device);
                    service.Pull("/fstab.donatello", stream, null, CancellationToken.None);
                });

            // Make sure the data that has been sent to the stream is the expected data
            Assert.Equal(content, stream.ToArray());
        }

        [Fact]
        public void PushTest()
        {
            DeviceData device = new()
            {
                Serial = "169.254.109.177:5555",
                State = DeviceState.Online
            };

            Stream stream = File.OpenRead("Assets/fstab.bin");
            byte[] content = File.ReadAllBytes("Assets/fstab.bin");
            List<byte> contentMessage =
            [
                .. SyncCommandConverter.GetBytes(SyncCommand.DATA),
                .. BitConverter.GetBytes(content.Length),
                .. content,
            ];

            RunTest(
                OkResponses(2),
                ResponseMessages(),
                Requests("host:transport:169.254.109.177:5555", "sync:"),
                SyncRequests(
                    SyncCommand.SEND, "/sdcard/test,644",
                    SyncCommand.DONE, "1446505200"),
                [SyncCommand.OKAY],
                null,
                [[.. contentMessage]],
                () =>
                {
                    using SyncService service = new(Socket, device);
                    service.Push(stream, "/sdcard/test", 0644, new DateTime(2015, 11, 2, 23, 0, 0, DateTimeKind.Utc), null, CancellationToken.None);
                });
        }
    }
}
