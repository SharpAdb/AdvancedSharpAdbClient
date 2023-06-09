using AdvancedSharpAdbClient.Exceptions;
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
        [Fact]
        public async void SendSyncDATARequestAsyncTest() =>
            await RunTestAsync(
                (socket) => socket.SendSyncRequestAsync(SyncCommand.DATA, 2, CancellationToken.None),
                new byte[] { (byte)'D', (byte)'A', (byte)'T', (byte)'A', 2, 0, 0, 0 });

        [Fact]
        public async void SendSyncSENDRequestAsyncTest() =>
            await RunTestAsync(
                (socket) => socket.SendSyncRequestAsync(SyncCommand.SEND, "/test", CancellationToken.None),
                new byte[] { (byte)'S', (byte)'E', (byte)'N', (byte)'D', 5, 0, 0, 0, (byte)'/', (byte)'t', (byte)'e', (byte)'s', (byte)'t' });

        [Fact]
        public async void SendSyncDENTRequestAsyncTest() =>
            await RunTestAsync(
                (socket) => socket.SendSyncRequestAsync(SyncCommand.DENT, "/data", 633, CancellationToken.None),
                new byte[] { (byte)'D', (byte)'E', (byte)'N', (byte)'T', 9, 0, 0, 0, (byte)'/', (byte)'d', (byte)'a', (byte)'t', (byte)'a', (byte)',', (byte)'6', (byte)'3', (byte)'3' });

        [Fact]
        public async void SendSyncNullRequestAsyncTest() =>
            _ = await Assert.ThrowsAsync<ArgumentNullException>(() => RunTestAsync((socket) => socket.SendSyncRequestAsync(SyncCommand.DATA, null, CancellationToken.None), Array.Empty<byte>()));

        [Fact]
        public async void ReadSyncResponseAsync()
        {
            DummyTcpSocket tcpSocket = new();
            AdbSocket socket = new(tcpSocket);

            using (StreamWriter writer = new(tcpSocket.InputStream, Encoding.ASCII, 4, true))
            {
                await writer.WriteAsync("DENT");
            }

            tcpSocket.InputStream.Position = 0;

            Assert.Equal(SyncCommand.DENT, await socket.ReadSyncResponseAsync());
        }

        [Fact]
        public async void ReadStringAsyncTest()
        {
            DummyTcpSocket tcpSocket = new();
            AdbSocket socket = new(tcpSocket);

            using (BinaryWriter writer = new(tcpSocket.InputStream, Encoding.ASCII, true))
            {
                writer.Write(Encoding.ASCII.GetBytes(5.ToString("X4")));
                writer.Write(Encoding.ASCII.GetBytes("Hello"));
                writer.Flush();
            }

            tcpSocket.InputStream.Position = 0;

            Assert.Equal("Hello", await socket.ReadStringAsync());
        }

        [Fact]
        public async void ReadAdbOkayResponseAsyncTest()
        {
            DummyTcpSocket tcpSocket = new();
            AdbSocket socket = new(tcpSocket);

            using (StreamWriter writer = new(tcpSocket.InputStream, Encoding.ASCII, 4, true))
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

        [Fact]
        public async void ReadAdbFailResponseAsyncTest()
        {
            DummyTcpSocket tcpSocket = new();
            AdbSocket socket = new(tcpSocket);

            using (StreamWriter writer = new(tcpSocket.InputStream, Encoding.ASCII, 4, true))
            {
                await writer.WriteAsync("FAIL");
                await writer.WriteAsync(17.ToString("X4"));
                await writer.WriteAsync("This did not work");
            }

            tcpSocket.InputStream.Position = 0;

            _ = await Assert.ThrowsAsync<AdbException>(() => socket.ReadAdbResponseAsync());
        }

        [Fact]
        public async void ReadAsyncTest()
        {
            DummyTcpSocket tcpSocket = new();
            AdbSocket socket = new(tcpSocket);

            // Read 100 bytes from a stream which has 101 bytes available
            byte[] data = new byte[101];
            for (int i = 0; i < 101; i++)
            {
                data[i] = (byte)i;
            }

            await tcpSocket.InputStream.WriteAsync(data, 0, 101);
            tcpSocket.InputStream.Position = 0;

            // Buffer has a capacity of 101, but we'll only want to read 100 bytes
            byte[] received = new byte[101];

            await socket.ReadAsync(received, 100, CancellationToken.None);

            for (int i = 0; i < 100; i++)
            {
                Assert.Equal(received[i], (byte)i);
            }

            Assert.Equal(0, received[100]);
        }

        [Fact]
        public async void SendAdbRequestAsyncTest() =>
            await RunTestAsync(
                (socket) => socket.SendAdbRequestAsync("Test", CancellationToken.None),
                Encoding.ASCII.GetBytes("0004Test"));

        private static async Task RunTestAsync(Func<IAdbSocket, Task> test, byte[] expectedDataSent)
        {
            DummyTcpSocket tcpSocket = new();
            AdbSocket socket = new(tcpSocket);

            // Run the test.
            await test(socket);

            // Validate the data that was sent over the wire.
            Assert.Equal(expectedDataSent, tcpSocket.GetBytesSent());
        }
    }
}
