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

            Assert.Equal("/fstab.donatello", value.Path);
            Assert.Equal(UnixFileStatus.Regular, value.FileMode.GetFileType());
            Assert.Equal((UnixFileStatus)416, value.FileMode.GetPermissions());
            Assert.Equal(597u, value.Size);
            Assert.Equal(DateTimeExtensions.Epoch.ToLocalTime(), value.Time);
            Assert.Equal($"-rw-r-----\t597\t{value.Time}\t/fstab.donatello", value.ToString());
        }

        /// <summary>
        /// Tests the <see cref="SyncService.StatEx(string)"/> method.
        /// </summary>
        [Fact]
        public void StatExTest()
        {
            FileStatisticsEx value = RunTest(
                OkResponses(2),
                NoResponseMessages,
                ["host:transport:169.254.109.177:5555", "sync:"],
                [(SyncCommand.STA2, "/fstab.donatello")],
                [SyncCommand.STA2],
                [
                    [
                        0, 0, 0, 0,
                        167, 0, 0, 0, 0, 0, 0, 0,
                        38, 240, 15, 0, 0, 0, 0, 0,
                        160, 129, 0, 0,
                        1, 0, 0, 0,
                        146, 39, 0, 0,
                        255, 3, 0, 0,
                        85, 2, 0, 0, 0, 0, 0, 0,
                        0, 0, 0, 0, 0, 0, 0, 0,
                        0, 0, 0, 0, 0, 0, 0, 0,
                        0, 0, 0, 0, 0, 0, 0, 0
                    ]
                ],
                null,
                () =>
                {
                    using SyncService service = new(Socket, Device);
                    FileStatisticsEx value = service.StatEx("/fstab.donatello");
                    Assert.False(service.IsProcessing);
                    Assert.False(service.IsOutdate);
                    return value;
                });

            Assert.Equal("/fstab.donatello", value.Path);
            Assert.Equal(UnixErrorCode.Default, value.Error);
            Assert.Equal(167u, value.Device);
            Assert.Equal(1044518u, value.IndexNode);
            Assert.Equal(UnixFileStatus.Regular, value.FileMode.GetFileType());
            Assert.Equal((UnixFileStatus)416, value.FileMode.GetPermissions());
            Assert.Equal(1u, value.LinkCount);
            Assert.Equal(597u, value.Size);
            Assert.Equal(10130u, value.UserId);
            Assert.Equal(1023u, value.GroupId);
            Assert.Equal(DateTimeExtensions.Epoch.ToLocalTime(), value.AccessTime);
            Assert.Equal(DateTimeExtensions.Epoch.ToLocalTime(), value.ModifiedTime);
            Assert.Equal(DateTimeExtensions.Epoch.ToLocalTime(), value.ChangedTime);
            Assert.Equal($"-rw-r-----\t597\t{value.ModifiedTime}\t/fstab.donatello", value.ToString());
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
                    FileStatistics[] value = [.. service.GetDirectoryListing("/storage")];
                    Assert.False(service.IsProcessing);
                    Assert.True(service.IsOutdate);
                    return value;
                });

            Assert.Equal(4, value.Length);

            DateTime time = new(2015, 11, 3, 9, 47, 4, DateTimeKind.Utc);

            FileStatistics dir = value[0];
            Assert.Equal(".", dir.Path);
            Assert.Equal((UnixFileStatus)16873, dir.FileMode);
            Assert.Equal(0u, dir.Size);
            Assert.Equal(time, dir.Time);
            Assert.Equal($"drwxr-x--x\t0\t{dir.Time}\t.", dir.ToString());

            FileStatistics parentDir = value[1];
            Assert.Equal("..", parentDir.Path);
            Assert.Equal((UnixFileStatus)16877, parentDir.FileMode);
            Assert.Equal(0u, parentDir.Size);
            Assert.Equal(time, parentDir.Time);
            Assert.Equal($"drwxr-xr-x\t0\t{parentDir.Time}\t..", parentDir.ToString());

            FileStatistics sdcard0 = value[2];
            Assert.Equal("sdcard0", sdcard0.Path);
            Assert.Equal((UnixFileStatus)41471, sdcard0.FileMode);
            Assert.Equal(24u, sdcard0.Size);
            Assert.Equal(time, sdcard0.Time);
            Assert.Equal($"lrwxrwxrwx\t24\t{sdcard0.Time}\tsdcard0", sdcard0.ToString());

            FileStatistics emulated = value[3];
            Assert.Equal("emulated", emulated.Path);
            Assert.Equal((UnixFileStatus)16749, emulated.FileMode);
            Assert.Equal(0u, emulated.Size);
            Assert.Equal(time, emulated.Time);
            Assert.Equal($"dr-xr-xr-x\t0\t{emulated.Time}\temulated", emulated.ToString());
        }

        /// <summary>
        /// Tests the <see cref="SyncService.GetDirectoryListingEx(string)"/> method.
        /// </summary>
        [Fact]
        public void GetListingExTest()
        {
            FileStatisticsEx[] value = RunTest(
                OkResponses(2),
                [".", "..", "sdcard0", "emulated"],
                ["host:transport:169.254.109.177:5555", "sync:"],
                [(SyncCommand.LIS2, "/storage")],
                [SyncCommand.DNT2, SyncCommand.DNT2, SyncCommand.DNT2, SyncCommand.DNT2, SyncCommand.DONE],
                [
                    [
                        0, 0, 0, 0,
                        19, 0, 0, 0, 0, 0, 0, 0,
                        83, 14, 0, 0, 0, 0, 0, 0,
                        233, 65, 0, 0,
                        4, 0, 0, 0,
                        208, 7, 0, 0,
                        13, 39, 0, 0,
                        0, 0, 0, 0, 0, 0, 0, 0,
                        152, 130, 56, 86, 0, 0, 0, 0,
                        152, 130, 56, 86, 0, 0, 0, 0,
                        152, 130, 56, 86, 0, 0, 0, 0
                    ],
                    [
                        0, 0, 0, 0,
                        5, 3, 1, 0, 0, 0, 0, 0,
                        2, 0, 0, 0, 0, 0, 0, 0,
                        237, 65, 0, 0,
                        27, 0, 0, 0,
                        0, 0, 0, 0,
                        0, 0, 0, 0,
                        0, 0, 0, 0, 0, 0, 0, 0,
                        152, 130, 56, 86, 0, 0, 0, 0,
                        152, 130, 56, 86, 0, 0, 0, 0,
                        152, 130, 56, 86, 0, 0, 0, 0
                    ],
                    [
                        0, 0, 0, 0,
                        153, 0, 0, 0, 0, 0, 0, 0,
                        1, 192, 48, 0, 0, 0, 0, 0,
                        255, 161, 0, 0,
                        5, 0, 0, 0,
                        0, 0, 0, 0,
                        13, 39, 0, 0,
                        24, 0, 0, 0, 0, 0, 0, 0,
                        152, 130, 56, 86, 0, 0, 0, 0,
                        152, 130, 56, 86, 0, 0, 0, 0,
                        152, 130, 56, 86, 0, 0, 0, 0
                    ],
                    [
                        0, 0, 0, 0,
                        19, 0, 0, 0, 0, 0, 0, 0,
                        84, 14, 0, 0, 0, 0, 0, 0,
                        109, 65, 0, 0,
                        2, 0, 0, 0,
                        0, 0, 0, 0,
                        0, 0, 0, 0,
                        0, 0, 0, 0, 0, 0, 0, 0,
                        152, 130, 56, 86, 0, 0, 0, 0,
                        152, 130, 56, 86, 0, 0, 0, 0,
                        152, 130, 56, 86, 0, 0, 0, 0
                    ]
                ],
                null,
                () =>
                {
                    using SyncService service = new(Socket, Device);
                    FileStatisticsEx[] value = [.. service.GetDirectoryListingEx("/storage")];
                    Assert.False(service.IsProcessing);
                    Assert.True(service.IsOutdate);
                    return value;
                });

            Assert.Equal(4, value.Length);

            DateTime time = new(2015, 11, 3, 9, 47, 4, DateTimeKind.Utc);

            FileStatisticsEx dir = value[0];
            Assert.Equal(".", dir.Path);
            Assert.Equal(UnixErrorCode.Default, dir.Error);
            Assert.Equal(19u, dir.Device);
            Assert.Equal(3667u, dir.IndexNode);
            Assert.Equal((UnixFileStatus)16873, dir.FileMode);
            Assert.Equal(4u, dir.LinkCount);
            Assert.Equal(0u, dir.Size);
            Assert.Equal(2000u, dir.UserId);
            Assert.Equal(9997u, dir.GroupId);
            Assert.Equal(time, dir.AccessTime);
            Assert.Equal(time, dir.ModifiedTime);
            Assert.Equal(time, dir.ChangedTime);
            Assert.Equal($"drwxr-x--x\t0\t{dir.ModifiedTime}\t.", dir.ToString());

            FileStatisticsEx parentDir = value[1];
            Assert.Equal("..", parentDir.Path);
            Assert.Equal(UnixErrorCode.Default, parentDir.Error);
            Assert.Equal(66309u, parentDir.Device);
            Assert.Equal(2u, parentDir.IndexNode);
            Assert.Equal((UnixFileStatus)16877, parentDir.FileMode);
            Assert.Equal(27u, parentDir.LinkCount);
            Assert.Equal(0u, parentDir.Size);
            Assert.Equal(0u, parentDir.UserId);
            Assert.Equal(0u, parentDir.GroupId);
            Assert.Equal(time, parentDir.AccessTime);
            Assert.Equal(time, parentDir.ModifiedTime);
            Assert.Equal(time, parentDir.ChangedTime);
            Assert.Equal($"drwxr-xr-x\t0\t{parentDir.ModifiedTime}\t..", parentDir.ToString());

            FileStatisticsEx sdcard0 = value[2];
            Assert.Equal("sdcard0", sdcard0.Path);
            Assert.Equal(UnixErrorCode.Default, sdcard0.Error);
            Assert.Equal(153u, sdcard0.Device);
            Assert.Equal(3194881u, sdcard0.IndexNode);
            Assert.Equal((UnixFileStatus)41471, sdcard0.FileMode);
            Assert.Equal(5u, sdcard0.LinkCount);
            Assert.Equal(24u, sdcard0.Size);
            Assert.Equal(0u, sdcard0.UserId);
            Assert.Equal(9997u, sdcard0.GroupId);
            Assert.Equal(time, sdcard0.AccessTime);
            Assert.Equal(time, sdcard0.ModifiedTime);
            Assert.Equal(time, sdcard0.ChangedTime);
            Assert.Equal($"lrwxrwxrwx\t24\t{sdcard0.ModifiedTime}\tsdcard0", sdcard0.ToString());

            FileStatisticsEx emulated = value[3];
            Assert.Equal("emulated", emulated.Path);
            Assert.Equal(UnixErrorCode.Default, emulated.Error);
            Assert.Equal(19u, emulated.Device);
            Assert.Equal(3668u, emulated.IndexNode);
            Assert.Equal((UnixFileStatus)16749, emulated.FileMode);
            Assert.Equal(2u, emulated.LinkCount);
            Assert.Equal(0u, emulated.Size);
            Assert.Equal(0u, emulated.UserId);
            Assert.Equal(0u, emulated.GroupId);
            Assert.Equal(time, emulated.AccessTime);
            Assert.Equal(time, emulated.ModifiedTime);
            Assert.Equal(time, emulated.ChangedTime);
            Assert.Equal($"dr-xr-xr-x\t0\t{emulated.ModifiedTime}\temulated", emulated.ToString());
        }

        /// <summary>
        /// Tests the <see cref="SyncService.Pull(string, Stream, Action{SyncProgressChangedEventArgs}?, bool, in bool)"/> method.
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
        /// Tests the <see cref="SyncService.Pull(string, Stream, Action{SyncProgressChangedEventArgs}?, bool, in bool)"/> method.
        /// </summary>
        [Fact]
        public void PullExTest()
        {
            using MemoryStream stream = new();
            byte[] content = File.ReadAllBytes("Assets/Fstab.bin");
            byte[] contentLength = BitConverter.GetBytes(content.Length);

            RunTest(
                OkResponses(2),
                NoResponseMessages,
                ["host:transport:169.254.109.177:5555", "sync:"],
                [
                    (SyncCommand.STA2, "/fstab.donatello"),
                    (SyncCommand.RCV2, "/fstab.donatello"),
                    (SyncCommand.RCV2, "0")
                ],
                [SyncCommand.STA2, SyncCommand.DATA, SyncCommand.DONE],
                [
                    [
                        0, 0, 0, 0,
                        167, 0, 0, 0, 0, 0, 0, 0,
                        38, 240, 15, 0, 0, 0, 0, 0,
                        160, 129, 0, 0,
                        1, 0, 0, 0,
                        146, 39, 0, 0,
                        255, 3, 0, 0,
                        85, 2, 0, 0, 0, 0, 0, 0,
                        0, 0, 0, 0, 0, 0, 0, 0,
                        0, 0, 0, 0, 0, 0, 0, 0,
                        0, 0, 0, 0, 0, 0, 0, 0
                    ],
                    contentLength,
                    content
                ],
                null,
                () =>
                {
                    using SyncService service = new(Socket, Device);
                    service.Pull("/fstab.donatello", stream, useV2: true);
                    Assert.False(service.IsProcessing);
                    Assert.True(service.IsOutdate);
                });

            // Make sure the data that has been sent to the stream is the expected data
            Assert.Equal(content, stream.ToArray());
        }

        /// <summary>
        /// Tests the <see cref="SyncService.Push(Stream, string, UnixFileStatus, DateTimeOffset, Action{SyncProgressChangedEventArgs}?, bool, in bool)"/> method.
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
        /// Tests the <see cref="SyncService.Push(Stream, string, UnixFileStatus, DateTimeOffset, Action{SyncProgressChangedEventArgs}?, bool, in bool)"/> method.
        /// </summary>
        [Fact]
        public void PushExTest()
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
                    (SyncCommand.SND2, "/sdcard/test"),
                    (SyncCommand.SND2, "6440"),
                    (SyncCommand.DONE, "1446505200")
                ],
                [SyncCommand.OKAY],
                null,
                [contentMessage],
                () =>
                {
                    using SyncService service = new(Socket, Device);
                    service.Push(stream, "/sdcard/test", (UnixFileStatus)644, new DateTime(2015, 11, 2, 23, 0, 0, DateTimeKind.Utc), useV2: true);
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

        /// <summary>
        /// Tests the <see cref="SyncService.ToString()"/> method.
        /// </summary>
        [Fact]
        public void ToStringTest()
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
            using SyncService service = new(socket, Device);
            Assert.Equal($"{typeof(SyncService)} {{ {nameof(SyncService.Socket)} = {socket}, {nameof(SyncService.Device)} = {Device} }}", service.ToString());
        }
    }
}
