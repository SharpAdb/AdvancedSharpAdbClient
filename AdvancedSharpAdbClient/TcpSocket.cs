// <copyright file="TcpSocket.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace AdvancedSharpAdbClient
{
    /// <summary>
    /// Implements the <see cref="ITcpSocket"/> interface using the standard <see cref="System.Net.Sockets.Socket"/> class.
    /// </summary>
    [DebuggerDisplay($"{nameof(TcpSocket)} \\{{ {nameof(Socket)} = {{{nameof(Socket)}}}, {nameof(Connected)} = {{{nameof(Connected)}}}, {nameof(EndPoint)} = {{{nameof(EndPoint)}}}, {nameof(ReceiveBufferSize)} = {{{nameof(ReceiveBufferSize)}}} }}")]
    public sealed partial class TcpSocket : ITcpSocket, ICloneable<TcpSocket>, ICloneable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TcpSocket"/> class.
        /// </summary>
        public TcpSocket() => Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        /// <summary>
        /// The underlying socket that manages the connection.
        /// </summary>
        public Socket Socket { get; private set; }

        /// <summary>
        /// The <see cref="System.Net.EndPoint"/> at which the socket is listening.
        /// </summary>
        public EndPoint? EndPoint { get; private set; }

        /// <inheritdoc/>
        public bool Connected => Socket.Connected;

        /// <inheritdoc/>
        public int ReceiveBufferSize
        {
            get => Socket.ReceiveBufferSize;
            set => Socket.ReceiveBufferSize = value;
        }

        /// <inheritdoc/>
        [MemberNotNull(nameof(EndPoint))]
        public void Connect(EndPoint endPoint)
        {
            ExceptionExtensions.ThrowIfNull(endPoint);

            if (endPoint is not (IPEndPoint or DnsEndPoint))
            {
                throw new NotSupportedException("Only TCP endpoints are supported");
            }

            EndPoint = endPoint;
            Socket.Connect(endPoint);
            Socket.Blocking = true;
        }

        /// <inheritdoc/>
        public void Reconnect(bool isForce = false)
        {
            if (isForce || !Socket.Connected)
            {
                Socket.Dispose();
                Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                Connect(EndPoint!);
            }
            else
            {
                // Already connected - nothing to do.
                return;
            }
        }

        /// <inheritdoc/>
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                Socket.Dispose();
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        public void Close() => Socket.Close();

        /// <inheritdoc/>
        public int Send(byte[] buffer, SocketFlags socketFlags) =>
            Socket.Send(buffer, socketFlags);

        /// <inheritdoc/>
        public int Send(byte[] buffer, int size, SocketFlags socketFlags) =>
            Socket.Send(buffer, size, socketFlags);

        /// <inheritdoc/>
        public int Send(byte[] buffer, int offset, int size, SocketFlags socketFlags) =>
            Socket.Send(buffer, offset, size, socketFlags);

        /// <inheritdoc/>
        public int Receive(byte[] buffer, SocketFlags socketFlags) =>
            Socket.Receive(buffer, socketFlags);

        /// <inheritdoc/>
        public int Receive(byte[] buffer, int size, SocketFlags socketFlags) =>
            Socket.Receive(buffer, size, socketFlags);

        /// <inheritdoc/>
        public int Receive(byte[] buffer, int offset, int size, SocketFlags socketFlags) =>
            Socket.Receive(buffer, offset, size, socketFlags);

#if HAS_BUFFERS
        /// <inheritdoc/>
        public int Send(ReadOnlySpan<byte> buffer, SocketFlags socketFlags) =>
            Socket.Send(buffer, socketFlags);

        /// <inheritdoc/>
        public int Receive(Span<byte> buffer, SocketFlags socketFlags) =>
            Socket.Receive(buffer, socketFlags);
#endif

        /// <inheritdoc/>
        public Stream GetStream() => new NetworkStream(Socket);

        /// <inheritdoc/>
        public override string ToString()
        {
            StringBuilder builder =
                new StringBuilder("The ")
                    .Append(nameof(TcpSocket));
            return (Connected
                ? builder.Append(" connect with ")
                         .Append(EndPoint)
                : EndPoint == null
                    ? builder.Append(" without initialized")
                    : builder.Append(" disconnect with ")
                             .Append(EndPoint)).ToString();
        }

        /// <inheritdoc/>
        public TcpSocket Clone()
        {
            TcpSocket socket = new();
            socket.Connect(EndPoint!);
            socket.ReceiveBufferSize = ReceiveBufferSize;
            return socket;
        }

        /// <inheritdoc/>
        object ICloneable.Clone() => Clone();
    }
}
