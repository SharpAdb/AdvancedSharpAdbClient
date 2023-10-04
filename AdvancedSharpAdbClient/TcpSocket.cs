// <copyright file="TcpSocket.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace AdvancedSharpAdbClient
{
    /// <summary>
    /// Implements the <see cref="ITcpSocket"/> interface using the standard <see cref="Socket"/> class.
    /// </summary>
    public partial class TcpSocket : ITcpSocket
    {
        /// <summary>
        /// The underlying socket that manages the connection.
        /// </summary>
        protected Socket socket;

        /// <summary>
        /// The <see cref="EndPoint"/> at which the socket is listening.
        /// </summary>
        protected EndPoint endPoint;

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpSocket"/> class.
        /// </summary>
        public TcpSocket() => socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        /// <inheritdoc/>
        public bool Connected => socket.Connected;

        /// <inheritdoc/>
        public int ReceiveBufferSize
        {
            get => socket.ReceiveBufferSize;
            set => socket.ReceiveBufferSize = value;
        }

        /// <inheritdoc/>
        public virtual void Connect(EndPoint endPoint)
        {
            if (endPoint is not (IPEndPoint or DnsEndPoint))
            {
                throw new NotSupportedException("Only TCP endpoints are supported");
            }

            socket.Connect(endPoint);
            socket.Blocking = true;
            this.endPoint = endPoint;
        }

        /// <inheritdoc/>
        public virtual void Reconnect()
        {
            if (socket.Connected)
            {
                // Already connected - nothing to do.
                return;
            }
            else
            {
                socket.Dispose();
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                Connect(endPoint);
            }
        }

        /// <inheritdoc/>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                socket.Dispose();
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        public virtual void Close() => socket.Close();

        /// <inheritdoc/>
        public virtual int Send(byte[] buffer, int size, SocketFlags socketFlags) =>
#if HAS_BUFFERS
            socket.Send(buffer.AsSpan(0, size), socketFlags);
#else
            socket.Send(buffer, size, socketFlags);
#endif

        /// <inheritdoc/>
        public virtual int Send(byte[] buffer, int offset, int size, SocketFlags socketFlags) =>
#if HAS_BUFFERS
            socket.Send(buffer.AsSpan(offset, size), socketFlags);
#else
            socket.Send(buffer, offset, size, socketFlags);
#endif

        /// <inheritdoc/>
        public virtual int Receive(byte[] buffer, int size, SocketFlags socketFlags) =>
#if HAS_BUFFERS
            socket.Receive(buffer.AsSpan(0, size), socketFlags);
#else
            socket.Receive(buffer, size, socketFlags);
#endif

        /// <inheritdoc/>
        public virtual int Receive(byte[] buffer, int offset, int size, SocketFlags socketFlags) =>
#if HAS_BUFFERS
            socket.Send(buffer.AsSpan(offset, size), socketFlags);
#else
            socket.Receive(buffer, offset, size, socketFlags);
#endif

#if HAS_BUFFERS
        /// <inheritdoc/>
        public virtual int Send(ReadOnlySpan<byte> buffer, SocketFlags socketFlags) =>
            socket.Send(buffer, socketFlags);

        /// <inheritdoc/>
        public virtual int Receive(Span<byte> buffer, SocketFlags socketFlags) =>
            socket.Receive(buffer, socketFlags);
#else
        /// <inheritdoc/>
        public virtual int Send(byte[] buffer, SocketFlags socketFlags) =>
            socket.Send(buffer, socketFlags);

        /// <inheritdoc/>
        public virtual int Receive(byte[] buffer, SocketFlags socketFlags) =>
            socket.Receive(buffer, socketFlags);
#endif

        /// <inheritdoc/>
        public virtual Stream GetStream() => new NetworkStream(socket);
    }
}
