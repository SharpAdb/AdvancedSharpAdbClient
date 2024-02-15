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
        /// <inheritdoc/>
        [MemberNotNull(nameof(EndPoint))]
        public async Task ConnectAsync(EndPoint endPoint, CancellationToken cancellationToken = default)
        {
            ExceptionExtensions.ThrowIfNull(endPoint);

            if (endPoint is not (IPEndPoint or DnsEndPoint))
            {
                throw new NotSupportedException("Only TCP endpoints are supported");
            }

            EndPoint = endPoint;
#if NET6_0_OR_GREATER
            await Socket.ConnectAsync(endPoint, cancellationToken).ConfigureAwait(false);
#else
            await Task.Factory.StartNew(() => Socket.Connect(endPoint), cancellationToken, TaskCreationOptions.None, TaskScheduler.Default).ConfigureAwait(false);
#endif
            Socket.Blocking = true;
        }

        /// <inheritdoc/>
        public Task ReconnectAsync(bool isForce, CancellationToken cancellationToken = default)
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
                return TaskExExtensions.CompletedTask;
            }
        }

        /// <summary>
        /// Asynchronously re-establishes the connection to a remote host. Assumes you have resolved the reason that caused the
        /// socket to disconnect.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous task.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        public Task ReconnectAsync(CancellationToken cancellationToken = default) => ReconnectAsync(false, cancellationToken);

        /// <inheritdoc/>
        public Task<int> SendAsync(byte[] buffer, SocketFlags socketFlags, CancellationToken cancellationToken = default) =>
#if NET6_0_OR_GREATER
            Socket.SendAsync(buffer, socketFlags, cancellationToken).AsTask();
#else
            Socket.SendAsync(buffer, socketFlags, cancellationToken);
#endif

        /// <inheritdoc/>
        public Task<int> SendAsync(byte[] buffer, int size, SocketFlags socketFlags, CancellationToken cancellationToken = default) =>
            Socket.SendAsync(buffer, size, socketFlags, cancellationToken);

        /// <inheritdoc/>
        public Task<int> SendAsync(byte[] buffer, int offset, int size, SocketFlags socketFlags, CancellationToken cancellationToken = default) =>
            Socket.SendAsync(buffer, offset, size, socketFlags, cancellationToken);

        /// <inheritdoc/>
        public Task<int> ReceiveAsync(byte[] buffer, SocketFlags socketFlags, CancellationToken cancellationToken = default) =>
#if NET6_0_OR_GREATER
            Socket.ReceiveAsync(buffer, socketFlags, cancellationToken).AsTask();
#else
            Socket.ReceiveAsync(buffer, socketFlags, cancellationToken);
#endif

        /// <inheritdoc/>
        public Task<int> ReceiveAsync(byte[] buffer, int size, SocketFlags socketFlags, CancellationToken cancellationToken = default) =>
            Socket.ReceiveAsync(buffer, size, socketFlags, cancellationToken);

        /// <inheritdoc/>
        public Task<int> ReceiveAsync(byte[] buffer, int offset, int size, SocketFlags socketFlags, CancellationToken cancellationToken = default) =>
            Socket.ReceiveAsync(buffer, offset, size, socketFlags, cancellationToken);

#if HAS_BUFFERS
        /// <inheritdoc/>
        public ValueTask<int> SendAsync(ReadOnlyMemory<byte> buffer, SocketFlags socketFlags, CancellationToken cancellationToken = default) =>
            Socket.SendAsync(buffer, socketFlags, cancellationToken);

        /// <inheritdoc/>
        public ValueTask<int> ReceiveAsync(Memory<byte> buffer, SocketFlags socketFlags, CancellationToken cancellationToken = default) =>
            Socket.ReceiveAsync(buffer, socketFlags, cancellationToken);
#endif
    }
}
#endif