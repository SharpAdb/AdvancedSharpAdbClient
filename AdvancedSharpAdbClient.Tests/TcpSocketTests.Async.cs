using System.Net;
using System.Net.Sockets;
using System.Text;
using Xunit;

namespace AdvancedSharpAdbClient.Tests
{
    public partial class TcpSocketTests
    {
        [Fact]
        public async void LifecycleAsyncTest()
        {
            TcpSocket socket = new();
            Assert.False(socket.Connected);

            socket.Connect(new DnsEndPoint("www.bing.com", 80));
            Assert.True(socket.Connected);

            byte[] data = Encoding.ASCII.GetBytes("GET / HTTP/1.1\n\n");
            await socket.SendAsync(data, 0, data.Length, SocketFlags.None);

            byte[] responseData = new byte[128];
            await socket.ReceiveAsync(responseData, 0, responseData.Length, SocketFlags.None);

            _ = Encoding.ASCII.GetString(responseData);
            socket.Dispose();
        }
    }
}
