#if HAS_TASK
// <copyright file="TcpSocket.Async.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace AdvancedSharpAdbClient
{
    public partial class TcpSocket
    {
#if NET6_0_OR_GREATER
        /// <inheritdoc/>
        [MemberNotNull(nameof(EndPoint))]
        public virtual async ValueTask ConnectAsync(EndPoint endPoint, CancellationToken cancellationToken = default)
        {
            if (endPoint is not (IPEndPoint or DnsEndPoint))
            {
                throw new NotSupportedException("Only TCP endpoints are supported");
            }

            EndPoint = endPoint;
            await Socket.ConnectAsync(endPoint, cancellationToken).ConfigureAwait(false);
            Socket.Blocking = true;
        }

        /// <inheritdoc/>
        public virtual ValueTask ReconnectAsync(CancellationToken cancellationToken = default)
        {
            if (Socket.Connected)
            {
                // Already connected - nothing to do.
                return ValueTask.CompletedTask;
            }
            else
            {
                Socket.Dispose();
                Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                return ConnectAsync(EndPoint!, cancellationToken);
            }
        }
#else
        /// <inheritdoc/>
        [MemberNotNull(nameof(EndPoint))]
        public virtual async Task ConnectAsync(EndPoint endPoint, CancellationToken cancellationToken = default)
        {
            if (endPoint is not (IPEndPoint or DnsEndPoint))
            {
                throw new NotSupportedException("Only TCP endpoints are supported");
            }

            using (CancellationTokenRegistration cancellationTokenRegistration = cancellationToken.Register(Socket.Close))
            {
                EndPoint = endPoint;
                await Extensions.Yield();
                Socket.Connect(endPoint);
            }
            Socket.Blocking = true;
        }

        /// <inheritdoc/>
        public virtual Task ReconnectAsync(CancellationToken cancellationToken = default)
        {
            if (Socket.Connected)
            {
                // Already connected - nothing to do.
                return Extensions.CompletedTask;
            }
            else
            {
                Socket.Dispose();
                Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                return ConnectAsync(EndPoint!, cancellationToken);
            }
        }
#endif

        /// <inheritdoc/>
        public virtual Task<int> SendAsync(byte[] buffer, int size, SocketFlags socketFlags, CancellationToken cancellationToken = default) =>
            Socket.SendAsync(buffer, size, socketFlags, cancellationToken);

        /// <inheritdoc/>
        public virtual Task<int> SendAsync(byte[] buffer, int offset, int size, SocketFlags socketFlags, CancellationToken cancellationToken = default) =>
            Socket.SendAsync(buffer, offset, size, socketFlags, cancellationToken);

        /// <inheritdoc/>
        public virtual Task<int> ReceiveAsync(byte[] buffer, int size, SocketFlags socketFlags, CancellationToken cancellationToken = default) =>
            Socket.ReceiveAsync(buffer, size, socketFlags, cancellationToken);

        /// <inheritdoc/>
        public virtual Task<int> ReceiveAsync(byte[] buffer, int offset, int size, SocketFlags socketFlags, CancellationToken cancellationToken = default) =>
            Socket.ReceiveAsync(buffer, offset, size, socketFlags, cancellationToken);

#if HAS_BUFFERS
        /// <inheritdoc/>
        public ValueTask<int> SendAsync(ReadOnlyMemory<byte> buffer, SocketFlags socketFlags, CancellationToken cancellationToken = default) =>
            Socket.SendAsync(buffer, socketFlags, cancellationToken);

        /// <inheritdoc/>
        public ValueTask<int> ReceiveAsync(Memory<byte> buffer, SocketFlags socketFlags, CancellationToken cancellationToken = default) =>
            Socket.ReceiveAsync(buffer, socketFlags, cancellationToken);
#else
        /// <inheritdoc/>
        public virtual Task<int> SendAsync(byte[] buffer, SocketFlags socketFlags, CancellationToken cancellationToken = default) =>
            Socket.SendAsync(buffer, socketFlags, cancellationToken);

        /// <inheritdoc/>
        public virtual Task<int> ReceiveAsync(byte[] buffer, SocketFlags socketFlags, CancellationToken cancellationToken = default) =>
            Socket.ReceiveAsync(buffer, socketFlags, cancellationToken);
#endif
    }
}
#endif