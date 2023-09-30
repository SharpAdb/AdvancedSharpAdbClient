using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using Xunit;

namespace AdvancedSharpAdbClient.Tests
{
    /// <summary>
    /// Tests the <see cref="TcpSocket"/> class.
    /// </summary>
    public partial class TcpSocketTests
    {
        [Fact]
        public void LifecycleTest()
        {
            using TcpSocket socket = new();
            Assert.False(socket.Connected);

            socket.Connect(new DnsEndPoint("www.bing.com", 80));
            Assert.True(socket.Connected);

            byte[] data = Encoding.ASCII.GetBytes("GET / HTTP/1.1\n\n");
            socket.Send(data, data.Length, SocketFlags.None);

            byte[] responseData = new byte[128];
            socket.Receive(responseData, responseData.Length, SocketFlags.None);

            _ = Encoding.ASCII.GetString(responseData);
        }

        [Fact]
        public void LifecycleSpanTest()
        {
            using TcpSocket socket = new();
            Assert.False(socket.Connected);

            socket.Connect(new DnsEndPoint("www.bing.com", 80));
            Assert.True(socket.Connected);

            ReadOnlySpan<byte> data = Encoding.ASCII.GetBytes("GET / HTTP/1.1\n\n");
            socket.Send(data, SocketFlags.None);

            byte[] responseData = new byte[128];
            socket.Receive([.. responseData], SocketFlags.None);

            _ = Encoding.ASCII.GetString(responseData);
        }

        [Fact]
        public void ReconnectTest()
        {
            using TcpSocket socket = new();
            Assert.False(socket.Connected);

            socket.Connect(new DnsEndPoint("www.bing.com", 80));
            Assert.True(socket.Connected);

            socket.Dispose();
            Assert.False(socket.Connected);

            socket.Reconnect();
            Assert.True(socket.Connected);
        }

        [Fact]
        public void BufferSizeTest()
        {
            using TcpSocket socket = new()
            {
                ReceiveBufferSize = 1024
            };
            // https://stackoverflow.com/questions/29356626/is-there-a-way-to-reduce-the-minimum-lower-limit-of-the-socket-send-buffer-size
            Assert.Equal(RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? 2304 : 1024, socket.ReceiveBufferSize);
        }

        [Fact]
        public void CreateUnsupportedSocketTest()
        {
            using TcpSocket socket = new();
            _ = Assert.Throws<NotSupportedException>(() => socket.Connect(new CustomEndPoint()));
        }
    }
}
