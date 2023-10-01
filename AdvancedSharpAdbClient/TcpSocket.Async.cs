#if HAS_TASK
// <copyright file="TcpSocket.Async.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace AdvancedSharpAdbClient
{
    public partial class TcpSocket
    {
#if NET6_0_OR_GREATER
        /// <inheritdoc/>
        public virtual async ValueTask ConnectAsync(EndPoint endPoint, CancellationToken cancellationToken = default)
        {
            if (endPoint is not (IPEndPoint or DnsEndPoint))
            {
                throw new NotSupportedException("Only TCP endpoints are supported");
            }

            await socket.ConnectAsync(endPoint, cancellationToken).ConfigureAwait(false);
            socket.Blocking = true;
            this.endPoint = endPoint;
        }

        /// <inheritdoc/>
        public virtual ValueTask ReconnectAsync(CancellationToken cancellationToken = default)
        {
            if (socket.Connected)
            {
                // Already connected - nothing to do.
                return ValueTask.CompletedTask;
            }
            else
            {
                socket.Dispose();
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                return ConnectAsync(endPoint, cancellationToken);
            }
        }
#endif

#if HAS_BUFFERS
        /// <inheritdoc/>
        public virtual Task<int> SendAsync(byte[] buffer, int size, SocketFlags socketFlags, CancellationToken cancellationToken = default) =>
            socket.SendAsync(buffer.AsMemory(0, size), socketFlags, cancellationToken).AsTask();

        /// <inheritdoc/>
        public virtual Task<int> SendAsync(byte[] buffer, int offset, int size, SocketFlags socketFlags, CancellationToken cancellationToken = default) =>
            socket.SendAsync(buffer.AsMemory(offset, size), socketFlags, cancellationToken).AsTask();

        /// <inheritdoc/>
        public ValueTask<int> SendAsync(ReadOnlyMemory<byte> buffer, SocketFlags socketFlags, CancellationToken cancellationToken = default) =>
            socket.SendAsync(buffer, socketFlags, cancellationToken);

        /// <inheritdoc/>
        public virtual Task<int> ReceiveAsync(byte[] buffer, int size, SocketFlags socketFlags, CancellationToken cancellationToken = default) =>
            socket.ReceiveAsync(buffer.AsMemory(0, size), socketFlags, cancellationToken).AsTask();

        /// <inheritdoc/>
        public virtual Task<int> ReceiveAsync(byte[] buffer, int offset, int size, SocketFlags socketFlags, CancellationToken cancellationToken = default) =>
            socket.ReceiveAsync(buffer.AsMemory(offset, size), socketFlags, cancellationToken).AsTask();

        /// <inheritdoc/>
        public ValueTask<int> ReceiveAsync(Memory<byte> buffer, SocketFlags socketFlags, CancellationToken cancellationToken = default) =>
            socket.ReceiveAsync(buffer, socketFlags, cancellationToken);
#else
        /// <inheritdoc/>
        public virtual Task<int> SendAsync(byte[] buffer, SocketFlags socketFlags, CancellationToken cancellationToken = default) =>
            socket.SendAsync(buffer, socketFlags, cancellationToken);

        /// <inheritdoc/>
        public virtual Task<int> SendAsync(byte[] buffer, int size, SocketFlags socketFlags, CancellationToken cancellationToken = default) =>
            socket.SendAsync(buffer, size, socketFlags, cancellationToken);

        /// <inheritdoc/>
        public virtual Task<int> SendAsync(byte[] buffer, int offset, int size, SocketFlags socketFlags, CancellationToken cancellationToken = default) =>
            socket.SendAsync(buffer, offset, size, socketFlags, cancellationToken);

        /// <inheritdoc/>
        public virtual Task<int> ReceiveAsync(byte[] buffer, SocketFlags socketFlags, CancellationToken cancellationToken = default) =>
            socket.ReceiveAsync(buffer, socketFlags, cancellationToken);

        /// <inheritdoc/>
        public virtual Task<int> ReceiveAsync(byte[] buffer, int size, SocketFlags socketFlags, CancellationToken cancellationToken = default) =>
            socket.ReceiveAsync(buffer, size, socketFlags, cancellationToken);

        /// <inheritdoc/>
        public virtual Task<int> ReceiveAsync(byte[] buffer, int offset, int size, SocketFlags socketFlags, CancellationToken cancellationToken = default) =>
            socket.ReceiveAsync(buffer, offset, size, socketFlags, cancellationToken);
#endif
    }
}
#endif