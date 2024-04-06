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
        /// <summary>
        /// Tests the <see cref="AdbSocket.Close"/> method.
        /// </summary>
        [Fact]
        public void CloseTest()
        {
            using DummyTcpSocket tcpSocket = new();
            using AdbSocket socket = new(tcpSocket);

            Assert.True(socket.Connected);

            socket.Close();
            Assert.False(socket.Connected);
        }

        /// <summary>
        /// Tests the <see cref="AdbSocket.Dispose()"/> method.
        /// </summary>
        [Fact]
        public void DisposeTest()
        {
            using DummyTcpSocket tcpSocket = new();
            using AdbSocket socket = new(tcpSocket);

            Assert.True(socket.Connected);

            socket.Dispose();
            Assert.False(socket.Connected);
        }

        /// <summary>
        /// Tests the <see cref="AdbSocket.IsOkay(ReadOnlySpan{byte})"/> method.
        /// </summary>
        [Fact]
        public void IsOkayTest()
        {
            byte[] okay = "OKAY"u8.ToArray();
            byte[] fail = "FAIL"u8.ToArray();

            Assert.True(AdbSocket.IsOkay(okay));
            Assert.False(AdbSocket.IsOkay(fail));
        }

        /// <summary>
        /// Tests the <see cref="AdbSocket.SendSyncRequest(SyncCommand, int)"/> method.
        /// </summary>
        [Fact]
        public void SendSyncDATARequestTest() =>
            RunTest(
                socket => socket.SendSyncRequest(SyncCommand.DATA, 2),
                [(byte)'D', (byte)'A', (byte)'T', (byte)'A', 2, 0, 0, 0]);

        /// <summary>
        /// Tests the <see cref="AdbSocket.SendSyncRequest(SyncCommand, string, UnixFileStatus)"/> method.
        /// </summary>
        [Fact]
        public void SendSyncSENDRequestTest() =>
            RunTest(
                socket => socket.SendSyncRequest(SyncCommand.SEND, "/test", UnixFileStatus.GroupMask | UnixFileStatus.StickyBit | UnixFileStatus.UserExecute | UnixFileStatus.OtherExecute),
                [(byte)'S', (byte)'E', (byte)'N', (byte)'D', 9, 0, 0, 0, (byte)'/', (byte)'t', (byte)'e', (byte)'s', (byte)'t', (byte)',', (byte)'6', (byte)'3', (byte)'3']);

        /// <summary>
        /// Tests the <see cref="AdbSocket.SendSyncRequest(SyncCommand, string)"/> method.
        /// </summary>
        [Fact]
        public void SendSyncDENTRequestTest() =>
            RunTest(
                socket => socket.SendSyncRequest(SyncCommand.DENT, "/data"),
                [(byte)'D', (byte)'E', (byte)'N', (byte)'T', 5, 0, 0, 0, (byte)'/', (byte)'d', (byte)'a', (byte)'t', (byte)'a']);

        /// <summary>
        /// Tests the <see cref="AdbSocket.SendSyncRequest(SyncCommand, string)"/> method.
        /// </summary>
        [Fact]
        public void SendSyncNullRequestTest() =>
            _ = Assert.Throws<ArgumentNullException>(() =>
                RunTest(socket => socket.SendSyncRequest(SyncCommand.DATA, null), []));

        /// <summary>
        /// Tests the <see cref="AdbSocket.ReadSyncResponse"/> method.
        /// </summary>
        [Fact]
        public void ReadSyncResponseTest()
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

        /// <summary>
        /// Tests the <see cref="AdbSocket.ReadString"/> method.
        /// </summary>
        [Fact]
        public void ReadStringTest()
        {
            using DummyTcpSocket tcpSocket = new();
            using AdbSocket socket = new(tcpSocket);

            using (BinaryWriter writer = new(tcpSocket.InputStream, Encoding.UTF8, true))
            {
                writer.Write(Encoding.UTF8.GetBytes(5.ToString("X4")));
                writer.Write("Hello"u8);
                writer.Flush();
            }

            tcpSocket.InputStream.Position = 0;

            Assert.Equal("Hello", socket.ReadString());
        }

        /// <summary>
        /// Tests the <see cref="AdbSocket.ReadSyncString"/> method.
        /// </summary>
        [Fact]
        public void ReadSyncStringTest()
        {
            using DummyTcpSocket tcpSocket = new();
            using AdbSocket socket = new(tcpSocket);

            using (BinaryWriter writer = new(tcpSocket.InputStream, Encoding.UTF8, true))
            {
                writer.Write(5);
                writer.Write("Hello"u8);
                writer.Flush();
            }

            tcpSocket.InputStream.Position = 0;

            Assert.Equal("Hello", socket.ReadSyncString());
        }

        /// <summary>
        /// Tests the <see cref="AdbSocket.ReadAdbResponse"/> method.
        /// </summary>
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

        /// <summary>
        /// Tests the <see cref="AdbSocket.ReadAdbResponse"/> method.
        /// </summary>
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

            _ = Assert.Throws<AdbException>(() => _ = socket.ReadAdbResponse());
        }

        /// <summary>
        /// Tests the <see cref="AdbSocket.Read(byte[], int)"/> method.
        /// </summary>
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

        /// <summary>
        /// Tests the <see cref="AdbSocket.Read(Span{byte})"/> method.
        /// </summary>
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

        /// <summary>
        /// Tests the <see cref="AdbSocket.SendAdbRequest(string)"/> method.
        /// </summary>
        [Fact]
        public void SendAdbRequestTest() =>
            RunTest(
                socket => socket.SendAdbRequest("Test"),
                "0004Test"u8.ToArray());

        /// <summary>
        /// Tests the <see cref="AdbSocket.GetShellStream"/> method.
        /// </summary>
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


        /// <summary>
        /// Tests the <see cref="AdbSocket.Clone()"/> method.
        /// </summary>
        [Fact]
        public void CloneTest()
        {
            using DummyTcpSocket tcpSocket = new();
            using AdbSocket adbSocket = new(tcpSocket);
            Assert.True(adbSocket is ICloneable<IAdbSocket>);
            using AdbSocket socket = adbSocket.Clone();
            Assert.NotEqual(adbSocket.Socket, socket.Socket);
            Assert.Equal(adbSocket.Connected, socket.Connected);
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
