using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AdvancedSharpAdbClient.Tests
{
    public partial class SyncServiceTests
    {
        /// <summary>
        /// Tests the <see cref="SyncService.StatAsync(string, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async Task StatAsyncTest()
        {
            FileStatistics value = await RunTestAsync(
                OkResponses(2),
                NoResponseMessages,
                ["host:transport:169.254.109.177:5555", "sync:"],
                [(SyncCommand.STAT, "/fstab.donatello")],
                [SyncCommand.STAT],
                [[160, 129, 0, 0, 85, 2, 0, 0, 0, 0, 0, 0]],
                null,
                async () =>
                {
                    using SyncService service = new(Socket, Device);
                    FileStatistics value = await service.StatAsync("/fstab.donatello");
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
        /// Tests the <see cref="SyncService.GetDirectoryListingAsync(string, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async Task GetListingAsyncTest()
        {
            List<FileStatistics> value = await RunTestAsync(
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
                async () =>
                {
                    using SyncService service = new(Socket, Device);
                    List<FileStatistics> value = await service.GetDirectoryListingAsync("/storage");
                    Assert.False(service.IsProcessing);
                    Assert.True(service.IsOutdate);
                    return value;
                });

            Assert.Equal(4, value.Count);

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
        /// Tests the <see cref="SyncService.GetDirectoryAsyncListing(string, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async Task GetAsyncListingTest()
        {
            List<FileStatistics> value = await RunTestAsync(
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
                async () =>
                {
                    using SyncService service = new(Socket, Device);
                    List<FileStatistics> value = await service.GetDirectoryAsyncListing("/storage").ToListAsync();
                    Assert.False(service.IsProcessing);
                    Assert.True(service.IsOutdate);
                    return value;
                });

            Assert.Equal(4, value.Count);

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
        /// Tests the <see cref="SyncService.PullAsync(string, Stream, Action{SyncProgressChangedEventArgs}?, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async Task PullAsyncTest()
        {
            await using MemoryStream stream = new();
            byte[] content = await File.ReadAllBytesAsync("Assets/Fstab.bin");
            byte[] contentLength = BitConverter.GetBytes(content.Length);

            await RunTestAsync(
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
                async () =>
                {
                    using SyncService service = new(Socket, Device);
                    await service.PullAsync("/fstab.donatello", stream, null);
                    Assert.False(service.IsProcessing);
                    Assert.True(service.IsOutdate);
                });

            // Make sure the data that has been sent to the stream is the expected data
            Assert.Equal(content, stream.ToArray());
        }

        /// <summary>
        /// Tests the <see cref="SyncService.PushAsync(Stream, string, UnixFileStatus, DateTimeOffset, Action{SyncProgressChangedEventArgs}?, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async Task PushAsyncTest()
        {
            FileStream stream = File.OpenRead("Assets/Fstab.bin");
            byte[] content = await File.ReadAllBytesAsync("Assets/Fstab.bin");
            byte[] contentMessage =
            [
                .. SyncCommand.DATA.GetBytes(),
                .. BitConverter.GetBytes(content.Length),
                .. content,
            ];

            await RunTestAsync(
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
                async () =>
                {
                    using SyncService service = new(Socket, Device);
                    await service.PushAsync(stream, "/sdcard/test", UnixFileStatus.StickyBit | UnixFileStatus.UserWrite | UnixFileStatus.OtherRead, new DateTime(2015, 11, 2, 23, 0, 0, DateTimeKind.Utc), null);
                    Assert.False(service.IsProcessing);
                    Assert.True(service.IsOutdate);
                });
        }

        /// <summary>
        /// Tests the <see cref="SyncService.IsProcessing"/> field.
        /// </summary>
        [Fact]
        public async Task IsProcessingAsyncTest()
        {
            await RunTestAsync(
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
                async () =>
                {
                    using SyncService service = new(Socket, Device);
                    await foreach (FileStatistics stat in service.GetDirectoryAsyncListing("/storage"))
                    {
                        Assert.False(service.IsOutdate);
                        Assert.True(service.IsProcessing);
                        _ = await Assert.ThrowsAsync<InvalidOperationException>(() => service.PushAsync((Stream)null, null, default, default));
                        _ = await Assert.ThrowsAsync<InvalidOperationException>(() => service.PullAsync(null, (Stream)null));
#if WINDOWS10_0_17763_0_OR_GREATER
                        _ = await Assert.ThrowsAsync<InvalidOperationException>(() => service.PushAsync((IInputStream)null, null, default, default));
                        _ = await Assert.ThrowsAsync<InvalidOperationException>(() => service.PullAsync(null, (IOutputStream)null));
#endif
                        _ = await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetDirectoryListingAsync(null));
                        _ = await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetDirectoryAsyncListing(null).ToListAsync().AsTask());
                    }
                    Assert.False(service.IsProcessing);
                    Assert.True(service.IsOutdate);
                });
        }

#if WINDOWS10_0_17763_0_OR_GREATER
        /// <summary>
        /// Tests the <see cref="SyncService.PullAsync(string, IOutputStream, Action{SyncProgressChangedEventArgs}?, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async Task PullWinRTAsyncTest()
        {
            if (!OperatingSystem.IsWindowsVersionAtLeast(10)) { return; }

            using InMemoryRandomAccessStream stream = new();
            byte[] content = await File.ReadAllBytesAsync("Assets/Fstab.bin");
            byte[] contentLength = BitConverter.GetBytes(content.Length);

            await RunTestAsync(
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
                async () =>
                {
                    using SyncService service = new(Socket, Device);
                    await service.PullAsync("/fstab.donatello", stream, null);
                    Assert.False(service.IsProcessing);
                    Assert.True(service.IsOutdate);
                });

            IBuffer buffer = await stream.GetInputStreamAt(0).ReadAsync(new byte[(int)stream.Size].AsBuffer(), (uint)stream.Size, InputStreamOptions.None);
            // Make sure the data that has been sent to the stream is the expected data
            Assert.Equal(content, buffer.ToArray());
        }

        /// <summary>
        /// Tests the <see cref="SyncService.PushAsync(IInputStream, string, UnixFileStatus, DateTimeOffset, Action{SyncProgressChangedEventArgs}?, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async Task PushWinRTAsyncTest()
        {
            if (!OperatingSystem.IsWindowsVersionAtLeast(10)) { return; }

            StorageFile storageFile = await StorageFile.GetFileFromPathAsync(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Assets\Fstab.bin"));
            using IRandomAccessStreamWithContentType stream = await storageFile.OpenReadAsync();
            byte[] content = await File.ReadAllBytesAsync("Assets/Fstab.bin");
            byte[] contentMessage =
            [
                .. SyncCommand.DATA.GetBytes(),
                .. BitConverter.GetBytes(content.Length),
                .. content,
            ];

            await RunTestAsync(
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
                async () =>
                {
                    using SyncService service = new(Socket, Device);
                    await service.PushAsync(stream, "/sdcard/test", (UnixFileStatus)644, new DateTime(2015, 11, 2, 23, 0, 0, DateTimeKind.Utc), null);
                    Assert.False(service.IsProcessing);
                    Assert.True(service.IsOutdate);
                });
        }
#endif
    }
}
