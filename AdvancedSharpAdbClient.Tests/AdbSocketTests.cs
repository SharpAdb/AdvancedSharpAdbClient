using AdvancedSharpAdbClient.Exceptions;
using AdvancedSharpAdbClient.Logs;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AdvancedSharpAdbClient.Tests
{
    /// <summary>
    /// Tests the <see cref="AdbSocket"/> class.
    /// </summary>
    public class AdbSocketTests
    {
        [Fact]
        public void CloseTest()
        {
            DummyTcpSocket tcpSocket = new();
            AdbSocket socket = new(tcpSocket);

            Assert.True(socket.Connected);

            socket.Dispose();
            Assert.False(socket.Connected);
        }

        [Fact]
        public void DisposeTest()
        {
            DummyTcpSocket tcpSocket = new();
            AdbSocket socket = new(tcpSocket);

            Assert.True(socket.Connected);

            socket.Dispose();
            Assert.False(socket.Connected);
        }

        [Fact]
        public void IsOkayTest()
        {
            byte[] okay = Encoding.ASCII.GetBytes("OKAY");
            byte[] fail = Encoding.ASCII.GetBytes("FAIL");

            Assert.True(AdbSocket.IsOkay(okay));
            Assert.False(AdbSocket.IsOkay(fail));
        }

        [Fact]
        public void SendSyncRequestTest() =>
            RunTest(
                (socket) => socket.SendSyncRequest(SyncCommand.DATA, 2),
                new byte[] { (byte)'D', (byte)'A', (byte)'T', (byte)'A', 2, 0, 0, 0 });

        [Fact]
        public void SendSyncRequestTest2() =>
            RunTest(
                (socket) => socket.SendSyncRequest(SyncCommand.SEND, "/test"),
                new byte[] { (byte)'S', (byte)'E', (byte)'N', (byte)'D', 5, 0, 0, 0, (byte)'/', (byte)'t', (byte)'e', (byte)'s', (byte)'t' });

        [Fact]
        public void SendSyncRequestTest3() =>
            RunTest(
                (socket) => socket.SendSyncRequest(SyncCommand.DENT, "/data", 633),
                new byte[] { (byte)'D', (byte)'E', (byte)'N', (byte)'T', 9, 0, 0, 0, (byte)'/', (byte)'d', (byte)'a', (byte)'t', (byte)'a', (byte)',', (byte)'6', (byte)'3', (byte)'3' });

        [Fact]
        public void SendSyncNullRequestTest() =>
            _ = Assert.Throws<ArgumentNullException>(() => RunTest((socket) => socket.SendSyncRequest(SyncCommand.DATA, null), Array.Empty<byte>()));

        [Fact]
        public void ReadSyncResponse()
        {
            DummyTcpSocket tcpSocket = new();
            AdbSocket socket = new(tcpSocket);

            using (StreamWriter writer = new(tcpSocket.InputStream, Encoding.ASCII, 4, true))
            {
                writer.Write("DENT");
            }

            tcpSocket.InputStream.Position = 0;

            Assert.Equal(SyncCommand.DENT, socket.ReadSyncResponse());
        }

        [Fact]
        public void ReadSyncString()
        {
            DummyTcpSocket tcpSocket = new();
            AdbSocket socket = new(tcpSocket);

            using (BinaryWriter writer = new(tcpSocket.InputStream, Encoding.ASCII, true))
            {
                writer.Write(5);
                writer.Write(Encoding.ASCII.GetBytes("Hello"));
                writer.Flush();
            }

            tcpSocket.InputStream.Position = 0;

            Assert.Equal("Hello", socket.ReadSyncString());
        }

        [Fact]
        public async Task ReadStringAsyncTest()
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

            Assert.Equal("Hello", await socket.ReadStringAsync(CancellationToken.None));
        }

        [Fact]
        public void ReadAdbOkayResponseTest()
        {
            DummyTcpSocket tcpSocket = new();
            AdbSocket socket = new(tcpSocket);

            using (StreamWriter writer = new(tcpSocket.InputStream, Encoding.ASCII, 4, true))
            {
                writer.Write("OKAY");
            }

            tcpSocket.InputStream.Position = 0;

            AdbResponse response = socket.ReadAdbResponse();
            Assert.True(response.IOSuccess);
            Assert.Equal(string.Empty, response.Message);
            Assert.True(response.Okay);
            Assert.False(response.Timeout);
        }

        [Fact]
        public void ReadAdbFailResponseTest()
        {
            DummyTcpSocket tcpSocket = new();
            AdbSocket socket = new(tcpSocket);

            using (StreamWriter writer = new(tcpSocket.InputStream, Encoding.ASCII, 4, true))
            {
                writer.Write("FAIL");
                writer.Write(17.ToString("X4"));
                writer.Write("This did not work");
            }

            tcpSocket.InputStream.Position = 0;

            _ = Assert.Throws<AdbException>(socket.ReadAdbResponse);
        }

        [Fact]
        public void ReadTest()
        {
            DummyTcpSocket tcpSocket = new();
            AdbSocket socket = new(tcpSocket);

            // Read 100 bytes from a stream which has 101 bytes available
            byte[] data = new byte[101];
            for (int i = 0; i < 101; i++)
            {
                data[i] = (byte)i;
            }

            tcpSocket.InputStream.Write(data, 0, 101);
            tcpSocket.InputStream.Position = 0;

            // Buffer has a capacity of 101, but we'll only want to read 100 bytes
            byte[] received = new byte[101];

            socket.Read(received, 100);

            for (int i = 0; i < 100; i++)
            {
                Assert.Equal(received[i], (byte)i);
            }

            Assert.Equal(0, received[100]);
        }

        [Fact]
        public async Task ReadAsyncTest()
        {
            DummyTcpSocket tcpSocket = new();
            AdbSocket socket = new(tcpSocket);

            // Read 100 bytes from a stream which has 101 bytes available
            byte[] data = new byte[101];
            for (int i = 0; i < 101; i++)
            {
                data[i] = (byte)i;
            }

            tcpSocket.InputStream.Write(data, 0, 101);
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
        public void SendAdbRequestTest() =>
            RunTest(
                (socket) => socket.SendAdbRequest("Test"),
                Encoding.ASCII.GetBytes("0004Test"));

        [Fact]
        public void GetShellStreamTest()
        {
            DummyTcpSocket tcpSocket = new();
            AdbSocket socket = new(tcpSocket);

            Stream stream = socket.GetShellStream();
            Assert.IsType<ShellStream>(stream);

            ShellStream shellStream = (ShellStream)stream;
            Assert.Equal(tcpSocket.OutputStream, shellStream.Inner);
        }

        private static void RunTest(Action<IAdbSocket> test, byte[] expectedDataSent)
        {
            DummyTcpSocket tcpSocket = new();
            AdbSocket socket = new(tcpSocket);

            // Run the test.
            test(socket);

            // Validate the data that was sent over the wire.
            Assert.Equal(expectedDataSent, tcpSocket.GetBytesSent());
        }
    }
}
