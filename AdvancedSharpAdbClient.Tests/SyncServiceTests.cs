using System;
using System.IO;
using System.Linq;
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

        /// <summary>
        /// Tests the <see cref="SyncService.Stat(string)"/> method.
        /// </summary>
        [Fact]
        public void StatTest()
        {
            FileStatistics value = RunTest(
                OkResponses(2),
                NoResponseMessages,
                ["host:transport:169.254.109.177:5555", "sync:"],
                [(SyncCommand.STAT, "/fstab.donatello")],
                [SyncCommand.STAT],
                [[160, 129, 0, 0, 85, 2, 0, 0, 0, 0, 0, 0]],
                null,
                () =>
                {
                    using SyncService service = new(Socket, Device);
                    FileStatistics value = service.Stat("/fstab.donatello");
                    Assert.False(service.IsProcessing);
                    Assert.False(service.IsOutdate);
                    return value;
                });

            Assert.Equal(UnixFileStatus.Regular, value.FileMode.GetFileType());
            Assert.Equal((UnixFileStatus)416, value.FileMode.GetPermissions());
            Assert.Equal(597, value.Size);
            Assert.Equal(DateTimeExtensions.Epoch.ToLocalTime(), value.Time);
        }

        /// <summary>
        /// Tests the <see cref="SyncService.GetDirectoryListing(string)"/> method.
        /// </summary>
        [Fact]
        public void GetListingTest()
        {
            FileStatistics[] value = RunTest(
                OkResponses(2),
                [".", "..", "sdcard0", "emulated"],
                ["host:transport:169.254.109.177:5555", "sync:"],
                [(SyncCommand.LIST, "/storage")],
                [SyncCommand.DENT, SyncCommand.DENT, SyncCommand.DENT, SyncCommand.DENT, SyncCommand.DONE],
                [
                    [233, 65, 0, 0, 0, 0, 0, 0, 152, 130, 56, 86],
                    [237, 65, 0, 0, 0, 0, 0, 0, 152, 130, 56, 86],
                    [255, 161, 0, 0, 24, 0, 0, 0, 152, 130, 56, 86],
                    [109, 65, 0, 0, 0, 0, 0, 0, 152, 130, 56, 86]
                ],
                null,
                () =>
                {
                    using SyncService service = new(Socket, Device);
                    FileStatistics[] value = service.GetDirectoryListing("/storage").ToArray();
                    Assert.False(service.IsProcessing);
                    Assert.True(service.IsOutdate);
                    return value;
                });

            Assert.Equal(4, value.Length);

            DateTime time = new(2015, 11, 3, 9, 47, 4, DateTimeKind.Utc);

            FileStatistics dir = value[0];
            Assert.Equal(".", dir.Path);
            Assert.Equal((UnixFileStatus)16873, dir.FileMode);
            Assert.Equal(0, dir.Size);
            Assert.Equal(time, dir.Time);

            FileStatistics parentDir = value[1];
            Assert.Equal("..", parentDir.Path);
            Assert.Equal((UnixFileStatus)16877, parentDir.FileMode);
            Assert.Equal(0, parentDir.Size);
            Assert.Equal(time, parentDir.Time);

            FileStatistics sdcard0 = value[2];
            Assert.Equal("sdcard0", sdcard0.Path);
            Assert.Equal((UnixFileStatus)41471, sdcard0.FileMode);
            Assert.Equal(24, sdcard0.Size);
            Assert.Equal(time, sdcard0.Time);

            FileStatistics emulated = value[3];
            Assert.Equal("emulated", emulated.Path);
            Assert.Equal((UnixFileStatus)16749, emulated.FileMode);
            Assert.Equal(0, emulated.Size);
            Assert.Equal(time, emulated.Time);
        }

        /// <summary>
        /// Tests the <see cref="SyncService.Pull(string, Stream, Action{SyncProgressChangedEventArgs}?, in bool)"/> method.
        /// </summary>
        [Fact]
        public void PullTest()
        {
            using MemoryStream stream = new();
            byte[] content = File.ReadAllBytes("Assets/Fstab.bin");
            byte[] contentLength = BitConverter.GetBytes(content.Length);

            RunTest(
                OkResponses(2),
                NoResponseMessages,
                ["host:transport:169.254.109.177:5555", "sync:"],
                [
                    (SyncCommand.STAT, "/fstab.donatello"),
                    (SyncCommand.RECV, "/fstab.donatello")
                ],
                [SyncCommand.STAT, SyncCommand.DATA, SyncCommand.DONE],
                [
                    [160, 129, 0, 0, 85, 2, 0, 0, 0, 0, 0, 0],
                    contentLength,
                    content
                ],
                null,
                () =>
                {
                    using SyncService service = new(Socket, Device);
                    service.Pull("/fstab.donatello", stream);
                    Assert.False(service.IsProcessing);
                    Assert.True(service.IsOutdate);
                });

            // Make sure the data that has been sent to the stream is the expected data
            Assert.Equal(content, stream.ToArray());
        }

        /// <summary>
        /// Tests the <see cref="SyncService.Push(Stream, string, UnixFileStatus, DateTimeOffset, Action{SyncProgressChangedEventArgs}?, in bool)"/> method.
        /// </summary>
        [Fact]
        public void PushTest()
        {
            FileStream stream = File.OpenRead("Assets/Fstab.bin");
            byte[] content = File.ReadAllBytes("Assets/Fstab.bin");
            byte[] contentMessage =
            [
                .. SyncCommand.DATA.GetBytes(),
                .. BitConverter.GetBytes(content.Length),
                .. content,
            ];

            RunTest(
                OkResponses(2),
                NoResponseMessages,
                ["host:transport:169.254.109.177:5555", "sync:"],
                [
                    (SyncCommand.SEND, "/sdcard/test,644"),
                    (SyncCommand.DONE, "1446505200")
                ],
                [SyncCommand.OKAY],
                null,
                [contentMessage],
                () =>
                {
                    using SyncService service = new(Socket, Device);
                    service.Push(stream, "/sdcard/test", (UnixFileStatus)644, new DateTime(2015, 11, 2, 23, 0, 0, DateTimeKind.Utc));
                    Assert.False(service.IsProcessing);
                    Assert.True(service.IsOutdate);
                });
        }

        /// <summary>
        /// Tests the <see cref="SyncService.IsProcessing"/> field.
        /// </summary>
        [Fact]
        public void IsProcessingTest()
        {
            RunTest(
                OkResponses(2),
                [".", "..", "sdcard0", "emulated"],
                ["host:transport:169.254.109.177:5555", "sync:"],
                [(SyncCommand.LIST, "/storage")],
                [SyncCommand.DENT, SyncCommand.DENT, SyncCommand.DENT, SyncCommand.DENT, SyncCommand.DONE],
                [
                    [233, 65, 0, 0, 0, 0, 0, 0, 152, 130, 56, 86],
                    [237, 65, 0, 0, 0, 0, 0, 0, 152, 130, 56, 86],
                    [255, 161, 0, 0, 24, 0, 0, 0, 152, 130, 56, 86],
                    [109, 65, 0, 0, 0, 0, 0, 0, 152, 130, 56, 86]
                ],
                null,
                () =>
                {
                    using SyncService service = new(Socket, Device);
                    foreach (FileStatistics stat in service.GetDirectoryListing("/storage"))
                    {
                        Assert.False(service.IsOutdate);
                        Assert.True(service.IsProcessing);
                        _ = Assert.Throws<InvalidOperationException>(() => service.Push(null, null, default, default));
                        _ = Assert.Throws<InvalidOperationException>(() => service.Pull(null, null));
                        _ = Assert.Throws<InvalidOperationException>(() => service.GetDirectoryListing(null).FirstOrDefault());
                    }
                    Assert.False(service.IsProcessing);
                    Assert.True(service.IsOutdate);
                });
        }

        /// <summary>
        /// Tests the <see cref="SyncService.Clone()"/> method.
        /// </summary>
        [Fact]
        public void CloneTest()
        {
            DummyAdbSocket socket = new()
            {
                Requests =
                {
                    "host:transport:169.254.109.177:5555",
                    "sync:",
                    "host:transport:169.254.109.177:5555",
                    "sync:"
                }
            };
            socket.Responses.Enqueue(AdbResponse.OK);
            socket.Responses.Enqueue(AdbResponse.OK);
            socket.Responses.Enqueue(AdbResponse.OK);
            socket.Responses.Enqueue(AdbResponse.OK);
            using SyncService syncService = new(socket, Device);
            Assert.True(syncService is ICloneable<ISyncService>);
#if WINDOWS10_0_17763_0_OR_GREATER
            Assert.True(syncService is ICloneable<ISyncService.IWinRT>);
#endif
            using SyncService service = syncService.Clone();
            Assert.NotEqual(syncService.Socket, service.Socket);
            Assert.Equal(syncService.Device, service.Device);
        }
    }
}
