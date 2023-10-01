using AdvancedSharpAdbClient.Exceptions;
using System;
using System.IO;
using System.Text;
using Xunit;

namespace AdvancedSharpAdbClient.Tests
{
    /// <summary>
    /// Tests the <see cref="AdbSocket"/> class.
    /// </summary>
    public partial class AdbSocketTests
    {
        [Fact]
        public void CloseTest()
        {
            using DummyTcpSocket tcpSocket = new();
            using AdbSocket socket = new(tcpSocket);

            Assert.True(socket.Connected);

            socket.Close();
            Assert.False(socket.Connected);
        }

        [Fact]
        public void DisposeTest()
        {
            using DummyTcpSocket tcpSocket = new();
            using AdbSocket socket = new(tcpSocket);

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
        public void SendSyncDATARequestTest() =>
            RunTest(
                (socket) => socket.SendSyncRequest(SyncCommand.DATA, 2),
                [(byte)'D', (byte)'A', (byte)'T', (byte)'A', 2, 0, 0, 0]);

        [Fact]
        public void SendSyncSENDRequestTest() =>
            RunTest(
                (socket) => socket.SendSyncRequest(SyncCommand.SEND, "/test"),
                [(byte)'S', (byte)'E', (byte)'N', (byte)'D', 5, 0, 0, 0, (byte)'/', (byte)'t', (byte)'e', (byte)'s', (byte)'t']);

        [Fact]
        public void SendSyncDENTRequestTest() =>
            RunTest(
                (socket) => socket.SendSyncRequest(SyncCommand.DENT, "/data", 633),
                [(byte)'D', (byte)'E', (byte)'N', (byte)'T', 9, 0, 0, 0, (byte)'/', (byte)'d', (byte)'a', (byte)'t', (byte)'a', (byte)',', (byte)'6', (byte)'3', (byte)'3']);

        [Fact]
        public void SendSyncNullRequestTest() =>
            _ = Assert.Throws<ArgumentNullException>(() => RunTest((socket) => socket.SendSyncRequest(SyncCommand.DATA, null), []));

        [Fact]
        public void ReadSyncResponse()
        {
            using DummyTcpSocket tcpSocket = new();
            using AdbSocket socket = new(tcpSocket);

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
            using DummyTcpSocket tcpSocket = new();
            using AdbSocket socket = new(tcpSocket);

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
        public void ReadAdbOkayResponseTest()
        {
            using DummyTcpSocket tcpSocket = new();
            using AdbSocket socket = new(tcpSocket);

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
            using DummyTcpSocket tcpSocket = new();
            using AdbSocket socket = new(tcpSocket);

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
            using DummyTcpSocket tcpSocket = new();
            using AdbSocket socket = new(tcpSocket);

            // Read 100 bytes from a stream which has 101 bytes available
            byte[] data = new byte[101];
            for (int i = 0; i < 101; i++)
            {
                data[i] = (byte)i;
            }

            tcpSocket.InputStream.Write(data);
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
        public void ReadSpanTest()
        {
            using DummyTcpSocket tcpSocket = new();
            using AdbSocket socket = new(tcpSocket);

            // Read 100 bytes from a stream which has 101 bytes available
            byte[] data = new byte[101];
            for (int i = 0; i < 101; i++)
            {
                data[i] = (byte)i;
            }

            tcpSocket.InputStream.Write(data);
            tcpSocket.InputStream.Position = 0;

            // Buffer has a capacity of 101, but we'll only want to read 100 bytes
            byte[] received = new byte[101];

            _ = socket.Read(received.AsSpan(0, 100));

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
            using DummyTcpSocket tcpSocket = new();
            using AdbSocket socket = new(tcpSocket);

            using Stream stream = socket.GetShellStream();
            Assert.IsType<ShellStream>(stream);

            using ShellStream shellStream = (ShellStream)stream;
            Assert.Equal(tcpSocket.OutputStream, shellStream.Inner);
        }

        private static void RunTest(Action<IAdbSocket> test, byte[] expectedDataSent)
        {
            using DummyTcpSocket tcpSocket = new();
            using AdbSocket socket = new(tcpSocket);

            // Run the test.
            test(socket);

            // Validate the data that was sent over the wire.
            Assert.Equal(expectedDataSent, tcpSocket.GetBytesSent());
        }
    }
}
