using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Xunit;

namespace AdvancedSharpAdbClient.Tests
{
    /// <summary>
    /// Tests the <see cref="TcpSocket"/> class.
    /// </summary>
    public class TcpSocketTests
    {
        [Fact]
        public void LifecycleTest()
        {
            TcpSocket socket = new();
            Assert.False(socket.Connected);

            socket.Connect(new DnsEndPoint("www.bing.com", 80));
            Assert.True(socket.Connected);

            byte[] data = Encoding.ASCII.GetBytes("GET / HTTP/1.1\n\n");
            socket.Send(data, 0, data.Length, SocketFlags.None);

            byte[] responseData = new byte[128];
            socket.Receive(responseData, 0, SocketFlags.None);

            _ = Encoding.ASCII.GetString(responseData);
            socket.Dispose();
        }

        [Fact]
        public void ReconnectTest()
        {
            TcpSocket socket = new();
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
            TcpSocket socket = new()
            {
                ReceiveBufferSize = 1024
            };
            Assert.Equal(1024, socket.ReceiveBufferSize);
            socket.Dispose();
        }

        [Fact]
        public void CreateUnsupportedSocketTest()
        {
            TcpSocket socket = new();
            _ = Assert.Throws<NotSupportedException>(() => socket.Connect(new CustomEndPoint()));
        }
    }
}
