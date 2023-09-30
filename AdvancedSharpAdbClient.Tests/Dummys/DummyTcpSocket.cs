using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace AdvancedSharpAdbClient.Tests
{
    internal class DummyTcpSocket : ITcpSocket
    {
        /// <summary>
        /// The stream from which the <see cref="DummyTcpSocket"/> reads.
        /// </summary>
        public MemoryStream InputStream { get; set; } = new MemoryStream();

        /// <summary>
        /// The stream to which the <see cref="DummyTcpSocket"/> writes.
        /// </summary>
        public MemoryStream OutputStream { get; set; } = new MemoryStream();

        public bool Connected { get; set; } = true;

        public int ReceiveBufferSize { get; set; } = 1024;

        public void Close() => Connected = false;

        public void Connect(EndPoint endPoint) => Connected = true;

        public ValueTask ConnectAsync(EndPoint endPoint, CancellationToken cancellationToken)
        {
            Connected = true;
            return ValueTask.CompletedTask;
        }

        public void Reconnect() => Connected = true;

        public ValueTask ReconnectAsync(CancellationToken cancellationToken)
        {
            Connected = true;
            return ValueTask.CompletedTask;
        }

        public void Dispose() => Connected = false;

        public Stream GetStream() => OutputStream;

        public int Receive(byte[] buffer, SocketFlags socketFlags) => InputStream.Read(buffer);

        public int Receive(byte[] buffer, int size, SocketFlags socketFlags) => InputStream.Read(buffer, 0, size);

        public int Receive(byte[] buffer, int offset, int size, SocketFlags socketFlags) => InputStream.Read(buffer, offset, size);

        public int Receive(Span<byte> buffer, SocketFlags socketFlags) => InputStream.Read(buffer);

        public Task<int> ReceiveAsync(byte[] buffer, SocketFlags socketFlags, CancellationToken cancellationToken) => InputStream.ReadAsync(buffer, cancellationToken).AsTask();

        public Task<int> ReceiveAsync(byte[] buffer, int size, SocketFlags socketFlags, CancellationToken cancellationToken) => InputStream.ReadAsync(buffer, 0, size, cancellationToken);

        public Task<int> ReceiveAsync(byte[] buffer, int offset, int size, SocketFlags socketFlags, CancellationToken cancellationToken) => InputStream.ReadAsync(buffer, offset, size, cancellationToken);

        public ValueTask<int> ReceiveAsync(Memory<byte> buffer, SocketFlags socketFlags, CancellationToken cancellationToken = default) => InputStream.ReadAsync(buffer, cancellationToken);

        public int Send(byte[] buffer, SocketFlags socketFlags)
        {
            OutputStream.Write(buffer);
            return buffer.Length;
        }

        public int Send(byte[] buffer, int size, SocketFlags socketFlags)
        {
            OutputStream.Write(buffer, 0, size);
            return size;
        }

        public int Send(byte[] buffer, int offset, int size, SocketFlags socketFlags)
        {
            OutputStream.Write(buffer, offset, size);
            return size;
        }

        public int Send(ReadOnlySpan<byte> buffer, SocketFlags socketFlags)
        {
            OutputStream.Write(buffer);
            return buffer.Length;
        }

        public async Task<int> SendAsync(byte[] buffer, SocketFlags socketFlags, CancellationToken cancellationToken)
        {
            await OutputStream.WriteAsync(buffer, cancellationToken);
            return buffer.Length;
        }

        public async Task<int> SendAsync(byte[] buffer, int size, SocketFlags socketFlags, CancellationToken cancellationToken)
        {
            await OutputStream.WriteAsync(buffer.AsMemory(0, size), cancellationToken);
            return size;
        }

        public async Task<int> SendAsync(byte[] buffer, int offset, int size, SocketFlags socketFlags, CancellationToken cancellationToken = default)
        {
            await OutputStream.WriteAsync(buffer.AsMemory(offset, size), cancellationToken);
            return size;
        }

        public async ValueTask<int> SendAsync(ReadOnlyMemory<byte> buffer, SocketFlags socketFlags, CancellationToken cancellationToken = default)
        {
            await OutputStream.WriteAsync(buffer, cancellationToken);
            return buffer.Length;
        }

        public byte[] GetBytesSent() => OutputStream.ToArray();
    }
}
