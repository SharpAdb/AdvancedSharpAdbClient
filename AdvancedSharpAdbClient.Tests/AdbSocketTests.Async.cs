using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AdvancedSharpAdbClient.Tests
{
    public partial class AdbSocketTests
    {
        /// <summary>
        /// Tests the <see cref="AdbSocket.SendSyncRequestAsync(SyncCommand, int, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async Task SendSyncDATARequestAsyncTest() =>
            await RunTestAsync(
                socket => socket.SendSyncRequestAsync(SyncCommand.DATA, 2, default),
                [(byte)'D', (byte)'A', (byte)'T', (byte)'A', 2, 0, 0, 0]);

        /// <summary>
        /// Tests the <see cref="AdbSocket.SendSyncRequestAsync(SyncCommand, string, UnixFileStatus, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async Task SendSyncSENDRequestAsyncTest() =>
            await RunTestAsync(
                socket => socket.SendSyncRequestAsync(SyncCommand.SEND, "/test", UnixFileStatus.GroupMask | UnixFileStatus.StickyBit | UnixFileStatus.UserExecute | UnixFileStatus.OtherExecute, default),
                [(byte)'S', (byte)'E', (byte)'N', (byte)'D', 9, 0, 0, 0, (byte)'/', (byte)'t', (byte)'e', (byte)'s', (byte)'t', (byte)',', (byte)'6', (byte)'3', (byte)'3']);

        /// <summary>
        /// Tests the <see cref="AdbSocket.SendSyncRequestAsync(SyncCommand, string, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async Task SendSyncDENTRequestAsyncTest() =>
            await RunTestAsync(
                socket => socket.SendSyncRequestAsync(SyncCommand.DENT, "/data", default),
                [(byte)'D', (byte)'E', (byte)'N', (byte)'T', 5, 0, 0, 0, (byte)'/', (byte)'d', (byte)'a', (byte)'t', (byte)'a']);

        /// <summary>
        /// Tests the <see cref="AdbSocket.SendSyncRequestAsync(SyncCommand, string, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async Task SendSyncNullRequestAsyncTest() =>
            _ = await Assert.ThrowsAsync<ArgumentNullException>(() =>
                RunTestAsync(socket => socket.SendSyncRequestAsync(SyncCommand.DATA, null, default), []));

        /// <summary>
        /// Tests the <see cref="AdbSocket.ReadSyncResponseAsync(CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async Task ReadSyncResponseAsync()
        {
            using DummyTcpSocket tcpSocket = new();
            using AdbSocket socket = new(tcpSocket);

            await using (StreamWriter writer = new(tcpSocket.InputStream, Encoding.ASCII, 4, true))
            {
                await writer.WriteAsync("DENT");
            }

            tcpSocket.InputStream.Position = 0;

            Assert.Equal(SyncCommand.DENT, await socket.ReadSyncResponseAsync());
        }

        /// <summary>
        /// Tests the <see cref="AdbSocket.ReadStringAsync(CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async Task ReadStringAsyncTest()
        {
            using DummyTcpSocket tcpSocket = new();
            using AdbSocket socket = new(tcpSocket);

            await using (BinaryWriter writer = new(tcpSocket.InputStream, Encoding.UTF8, true))
            {
                writer.Write(Encoding.UTF8.GetBytes(5.ToString("X4")));
                writer.Write("Hello"u8);
                writer.Flush();
            }

            tcpSocket.InputStream.Position = 0;

            Assert.Equal("Hello", await socket.ReadStringAsync());
        }

        /// <summary>
        /// Tests the <see cref="AdbSocket.ReadSyncStringAsync(CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async Task ReadSyncStringAsyncTest()
        {
            using DummyTcpSocket tcpSocket = new();
            using AdbSocket socket = new(tcpSocket);

            await using (BinaryWriter writer = new(tcpSocket.InputStream, Encoding.UTF8, true))
            {
                writer.Write(5);
                writer.Write("Hello"u8);
                writer.Flush();
            }

            tcpSocket.InputStream.Position = 0;

            Assert.Equal("Hello", await socket.ReadSyncStringAsync());
        }

        /// <summary>
        /// Tests the <see cref="AdbSocket.ReadAdbResponseAsync(CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async Task ReadAdbOkayResponseAsyncTest()
        {
            using DummyTcpSocket tcpSocket = new();
            using AdbSocket socket = new(tcpSocket);

            await using (StreamWriter writer = new(tcpSocket.InputStream, Encoding.ASCII, 4, true))
            {
                await writer.WriteAsync("OKAY");
            }

            tcpSocket.InputStream.Position = 0;

            AdbResponse response = await socket.ReadAdbResponseAsync();
            Assert.True(response.IOSuccess);
            Assert.Equal(string.Empty, response.Message);
            Assert.True(response.Okay);
            Assert.False(response.Timeout);
        }

        /// <summary>
        /// Tests the <see cref="AdbSocket.ReadAdbResponseAsync(CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async Task ReadAdbFailResponseAsyncTest()
        {
            using DummyTcpSocket tcpSocket = new();
            using AdbSocket socket = new(tcpSocket);

            await using (StreamWriter writer = new(tcpSocket.InputStream, Encoding.ASCII, 4, true))
            {
                await writer.WriteAsync("FAIL");
                await writer.WriteAsync(17.ToString("X4"));
                await writer.WriteAsync("This did not work");
            }

            tcpSocket.InputStream.Position = 0;

            _ = await Assert.ThrowsAsync<AdbException>(() => socket.ReadAdbResponseAsync());
        }

        /// <summary>
        /// Tests the <see cref="AdbSocket.ReadAsync(byte[], int, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async Task ReadAsyncTest()
        {
            using DummyTcpSocket tcpSocket = new();
            using AdbSocket socket = new(tcpSocket);

            // Read 100 bytes from a stream which has 101 bytes available
            byte[] data = new byte[101];
            for (int i = 0; i < 101; i++)
            {
                data[i] = (byte)i;
            }

            await tcpSocket.InputStream.WriteAsync(data);
            tcpSocket.InputStream.Position = 0;

            // Buffer has a capacity of 101, but we'll only want to read 100 bytes
            byte[] received = new byte[101];

            await socket.ReadAsync(received, 100);

            for (int i = 0; i < 100; i++)
            {
                Assert.Equal(received[i], (byte)i);
            }

            Assert.Equal(0, received[100]);
        }

        /// <summary>
        /// Tests the <see cref="AdbSocket.ReadAsync(Memory{byte}, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async Task ReadAsyncMemoryTest()
        {
            using DummyTcpSocket tcpSocket = new();
            using AdbSocket socket = new(tcpSocket);

            // Read 100 bytes from a stream which has 101 bytes available
            byte[] data = new byte[101];
            for (int i = 0; i < 101; i++)
            {
                data[i] = (byte)i;
            }

            await tcpSocket.InputStream.WriteAsync(data);
            tcpSocket.InputStream.Position = 0;

            // Buffer has a capacity of 101, but we'll only want to read 100 bytes
            byte[] received = new byte[101];

            await socket.ReadAsync(received.AsMemory(0, 100));

            for (int i = 0; i < 100; i++)
            {
                Assert.Equal(received[i], (byte)i);
            }

            Assert.Equal(0, received[100]);
        }

        /// <summary>
        /// Tests the <see cref="AdbSocket.SendAdbRequestAsync(string, CancellationToken)"/> method.
        /// </summary>
        [Fact]
        public async Task SendAdbRequestAsyncTest() =>
            await RunTestAsync(
                socket => socket.SendAdbRequestAsync("Test", default),
                "0004Test"u8.ToArray());

        private static async Task RunTestAsync(Func<IAdbSocket, Task> test, byte[] expectedDataSent)
        {
            using DummyTcpSocket tcpSocket = new();
            using AdbSocket socket = new(tcpSocket);

            // Run the test.
            await test(socket);

            // Validate the data that was sent over the wire.
            Assert.Equal(expectedDataSent, tcpSocket.GetBytesSent());
        }
    }
}
