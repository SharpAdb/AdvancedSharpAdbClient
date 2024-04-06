using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace AdvancedSharpAdbClient.Tests
{
    /// <summary>
    /// A mock implementation of the <see cref="ITcpSocket"/> class.
    /// </summary>
    internal class DummyTcpSocket : ITcpSocket, ICloneable<DummyTcpSocket>
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

        public async Task ConnectAsync(EndPoint endPoint, CancellationToken cancellationToken = default)
        {
            await Task.Yield();
            Connected = true;
        }

        public void Reconnect(bool isForce = false) => Connected = true;

        public async Task ReconnectAsync(bool isForce, CancellationToken cancellationToken = default)
        {
            await Task.Yield();
            Connected = true;
        }

        public void Dispose() => Connected = false;

        public Stream GetStream() => OutputStream;

        public int Receive(byte[] buffer, SocketFlags socketFlags) => InputStream.Read(buffer);

        public int Receive(byte[] buffer, int size, SocketFlags socketFlags) => InputStream.Read(buffer, 0, size);

        public int Receive(byte[] buffer, int offset, int size, SocketFlags socketFlags) => InputStream.Read(buffer, offset, size);

        public int Receive(Span<byte> buffer, SocketFlags socketFlags) => InputStream.Read(buffer);

        public Task<int> ReceiveAsync(byte[] buffer, SocketFlags socketFlags, CancellationToken cancellationToken) => InputStream.ReadAsync(buffer, cancellationToken).AsTask();

        public Task<int> ReceiveAsync(byte[] buffer, int size, SocketFlags socketFlags, CancellationToken cancellationToken = default) => InputStream.ReadAsync(buffer, 0, size, cancellationToken);

        public Task<int> ReceiveAsync(byte[] buffer, int offset, int size, SocketFlags socketFlags, CancellationToken cancellationToken = default) => InputStream.ReadAsync(buffer, offset, size, cancellationToken);

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
            await OutputStream.WriteAsync(buffer, cancellationToken).ConfigureAwait(false);
            return buffer.Length;
        }

        public async Task<int> SendAsync(byte[] buffer, int size, SocketFlags socketFlags, CancellationToken cancellationToken = default)
        {
            await OutputStream.WriteAsync(buffer.AsMemory(0, size), cancellationToken).ConfigureAwait(false);
            return size;
        }

        public async Task<int> SendAsync(byte[] buffer, int offset, int size, SocketFlags socketFlags, CancellationToken cancellationToken = default)
        {
            await OutputStream.WriteAsync(buffer.AsMemory(offset, size), cancellationToken).ConfigureAwait(false);
            return size;
        }

        public async ValueTask<int> SendAsync(ReadOnlyMemory<byte> buffer, SocketFlags socketFlags, CancellationToken cancellationToken = default)
        {
            await OutputStream.WriteAsync(buffer, cancellationToken).ConfigureAwait(false);
            return buffer.Length;
        }

        public byte[] GetBytesSent() => OutputStream.ToArray();

        public DummyTcpSocket Clone()
        {
            DummyTcpSocket socket = new()
            {
                Connected = true,
                ReceiveBufferSize = ReceiveBufferSize
            };
            return socket;
        }

        object ICloneable.Clone() => Clone();
    }
}
