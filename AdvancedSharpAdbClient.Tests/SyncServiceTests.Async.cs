using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Xunit;

namespace AdvancedSharpAdbClient.Tests
{
    public partial class SyncServiceTests
    {
        [Fact]
        public async void StatAsyncTest()
        {
            DeviceData device = new()
            {
                Serial = "169.254.109.177:5555",
                State = DeviceState.Online
            };

            FileStatistics value = null;

            await RunTestAsync(
                OkResponses(2),
                NoResponseMessages,
                Requests("host:transport:169.254.109.177:5555", "sync:"),
                SyncRequests(SyncCommand.STAT, "/fstab.donatello"),
                [SyncCommand.STAT],
                [[160, 129, 0, 0, 85, 2, 0, 0, 0, 0, 0, 0]],
                null,
                async () =>
                {
                    using SyncService service = new(Socket, device);
                    value = await service.StatAsync("/fstab.donatello");
                });

            Assert.NotNull(value);
            Assert.Equal(UnixFileType.Regular, value.FileType & UnixFileType.TypeMask);
            Assert.Equal(597, value.Size);
            Assert.Equal(DateTimeExtension.Epoch.ToLocalTime(), value.Time);
        }

        [Fact]
        public async void GetListingAsyncTest()
        {
            DeviceData device = new()
            {
                Serial = "169.254.109.177:5555",
                State = DeviceState.Online
            };

            List<FileStatistics> value = null;

            await RunTestAsync(
                OkResponses(2),
                ResponseMessages(".", "..", "sdcard0", "emulated"),
                Requests("host:transport:169.254.109.177:5555", "sync:"),
                SyncRequests(SyncCommand.LIST, "/storage"),
                [SyncCommand.DENT, SyncCommand.DENT, SyncCommand.DENT, SyncCommand.DENT, SyncCommand.DONE],
                [
                    [233, 65, 0, 0, 0, 0, 0, 0, 152, 130, 56, 86],
                    [237, 65, 0, 0, 0, 0, 0, 0, 152, 130, 56, 86],
                    [255, 161, 0, 0, 24, 0, 0, 0, 152, 130, 56, 86],
                    [109, 65, 0, 0, 0, 0, 0, 0, 152, 130, 56, 86]
                ],
                null,
                async () =>
                {
                    using SyncService service = new(Socket, device);
                    value = (await service.GetDirectoryListingAsync("/storage")).ToList();
                });

            Assert.Equal(4, value.Count);

            DateTime time = new DateTime(2015, 11, 3, 9, 47, 4, DateTimeKind.Utc).ToLocalTime();

            FileStatistics dir = value[0];
            Assert.Equal(".", dir.Path);
            Assert.Equal((UnixFileType)16873, dir.FileType);
            Assert.Equal(0, dir.Size);
            Assert.Equal(time, dir.Time);

            FileStatistics parentDir = value[1];
            Assert.Equal("..", parentDir.Path);
            Assert.Equal((UnixFileType)16877, parentDir.FileType);
            Assert.Equal(0, parentDir.Size);
            Assert.Equal(time, parentDir.Time);

            FileStatistics sdcard0 = value[2];
            Assert.Equal("sdcard0", sdcard0.Path);
            Assert.Equal((UnixFileType)41471, sdcard0.FileType);
            Assert.Equal(24, sdcard0.Size);
            Assert.Equal(time, sdcard0.Time);

            FileStatistics emulated = value[3];
            Assert.Equal("emulated", emulated.Path);
            Assert.Equal((UnixFileType)16749, emulated.FileType);
            Assert.Equal(0, emulated.Size);
            Assert.Equal(time, emulated.Time);
        }

        [Fact]
        public async void PullAsyncTest()
        {
            DeviceData device = new()
            {
                Serial = "169.254.109.177:5555",
                State = DeviceState.Online
            };

            await using MemoryStream stream = new();
            byte[] content = File.ReadAllBytes("Assets/fstab.bin");
            byte[] contentLength = BitConverter.GetBytes(content.Length);

            await RunTestAsync(
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
                async () =>
                {
                    using SyncService service = new(Socket, device);
                    await service.PullAsync("/fstab.donatello", stream, null, CancellationToken.None);
                });

            // Make sure the data that has been sent to the stream is the expected data
            Assert.Equal(content, stream.ToArray());
        }

        [Fact]
        public async void PushAsyncTest()
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

            await RunTestAsync(
                OkResponses(2),
                ResponseMessages(),
                Requests("host:transport:169.254.109.177:5555", "sync:"),
                SyncRequests(
                    SyncCommand.SEND, "/sdcard/test,644",
                    SyncCommand.DONE, "1446505200"),
                [SyncCommand.OKAY],
                null,
                [[.. contentMessage]],
                async () =>
                {
                    using SyncService service = new(Socket, device);
                    await service.PushAsync(stream, "/sdcard/test", 0644, new DateTime(2015, 11, 2, 23, 0, 0, DateTimeKind.Utc), null, CancellationToken.None);
                });
        }
    }
}
