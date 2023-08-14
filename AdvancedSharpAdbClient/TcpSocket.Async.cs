#if HAS_TASK
// <copyright file="TcpSocket.Async.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.Net.Sockets;
using System.Threading;

namespace AdvancedSharpAdbClient
{
    public partial class TcpSocket
    {
#if NETCOREAPP2_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        /// <inheritdoc/>
        public virtual async Task<int> SendAsync(byte[] buffer, int offset, int size, SocketFlags socketFlags, CancellationToken cancellationToken = default) =>
            await socket.SendAsync(buffer.AsMemory().Slice(offset, size), socketFlags, cancellationToken);
#else
        /// <inheritdoc/>
        public virtual async Task<int> SendAsync(byte[] buffer, int offset, int size, SocketFlags socketFlags, CancellationToken cancellationToken = default) =>
            await Utilities.Run(() => Send(buffer, offset, size, socketFlags), cancellationToken);
#endif

#if NETCOREAPP2_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        /// <inheritdoc/>
        public virtual async Task<int> ReceiveAsync(byte[] buffer, int offset, int size, SocketFlags socketFlags, CancellationToken cancellationToken = default) =>
            await socket.ReceiveAsync(buffer.AsMemory().Slice(offset, size), socketFlags, cancellationToken);
#else
        /// <inheritdoc/>
        public virtual Task<int> ReceiveAsync(byte[] buffer, int offset, int size, SocketFlags socketFlags, CancellationToken cancellationToken = default) =>
            socket.ReceiveAsync(buffer, offset, size, socketFlags, cancellationToken);
#endif
    }
}
#endif