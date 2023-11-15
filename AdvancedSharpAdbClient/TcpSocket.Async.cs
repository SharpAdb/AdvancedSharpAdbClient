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
        public virtual ValueTask ReconnectAsync(bool isForce, CancellationToken cancellationToken = default)
        {
            if (isForce || !Socket.Connected)
            {
                Socket.Dispose();
                Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                return ConnectAsync(EndPoint!, cancellationToken);
            }
            else
            {
                // Already connected - nothing to do.
                return ValueTask.CompletedTask;
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
        public virtual Task ReconnectAsync(bool isForce, CancellationToken cancellationToken = default)
        {
            if (isForce || !Socket.Connected)
            {
                Socket.Dispose();
                Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                return ConnectAsync(EndPoint!, cancellationToken);
            }
            else
            {
                // Already connected - nothing to do.
                return Extensions.CompletedTask;
            }
        }
#endif

        /// <summary>
        /// Re-establishes the connection to a remote host. Assumes you have resolved the reason that caused the
        /// socket to disconnect.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous task.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        public
#if NET6_0_OR_GREATER
            ValueTask
#else
            Task
#endif
            ReconnectAsync(CancellationToken cancellationToken = default) => ReconnectAsync(false, cancellationToken);

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