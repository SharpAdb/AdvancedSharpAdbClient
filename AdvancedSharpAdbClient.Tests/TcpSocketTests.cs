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

            byte[] data = "GET / HTTP/1.1\n\n"u8.ToArray();
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

            ReadOnlySpan<byte> data = "GET / HTTP/1.1\n\n"u8;
            socket.Send(data, SocketFlags.None);

            byte[] responseData = new byte[128];
            socket.Receive([.. responseData], SocketFlags.None);

            _ = Encoding.ASCII.GetString(responseData);
        }

        /// <summary>
        /// Tests the <see cref="TcpSocket.Reconnect"/> method.
        /// </summary>
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

        /// <summary>
        /// Tests the <see cref="TcpSocket.ReceiveBufferSize"/> property.
        /// </summary>
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

        /// <summary>
        /// Tests the <see cref="TcpSocket.Connect(EndPoint)"/> method.
        /// </summary>
        [Fact]
        public void CreateUnsupportedSocketTest()
        {
            using TcpSocket socket = new();
            _ = Assert.Throws<NotSupportedException>(() => socket.Connect(new CustomEndPoint()));
        }

        /// <summary>
        /// Tests the <see cref="TcpSocket.Clone()"/> method.
        /// </summary>
        [Fact]
        public void CloneTest()
        {
            using TcpSocket tcpSocket = new();
            Assert.True(tcpSocket is ICloneable<ITcpSocket>);
            Assert.Throws<ArgumentNullException>(tcpSocket.Clone);
            tcpSocket.Connect(new DnsEndPoint("www.bing.com", 80));
            using TcpSocket socket = tcpSocket.Clone();
            Assert.Equal(tcpSocket.EndPoint, socket.EndPoint);
            Assert.True(socket.Connected);
        }
    }
}
