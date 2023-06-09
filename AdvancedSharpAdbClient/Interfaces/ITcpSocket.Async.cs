#if HAS_TASK
// <copyright file="ITcpSocket.Async.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System.Net.Sockets;
using System.Threading;

namespace AdvancedSharpAdbClient
{
    public partial interface ITcpSocket
    {
        /// <summary>
        /// Asynchronously sends the specified number of bytes of data to a connected
        /// <see cref="ITcpSocket"/>, starting at the specified <paramref name="offset"/>,
        /// and using the specified <paramref name="socketFlags"/>.
        /// </summary>
        /// <param name="buffer">An array of type Byte that contains the data to be sent.</param>
        /// <param name="offset">The position in the data buffer at which to begin sending data.</param>
        /// <param name="size">The number of bytes to send.</param>
        /// <param name="socketFlags">A bitwise combination of the SocketFlags values.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous task.</param>
        /// <returns>The number of bytes sent to the Socket.</returns>
        public Task<int> SendAsync(byte[] buffer, int offset, int size, SocketFlags socketFlags, CancellationToken cancellationToken);

        /// <summary>
        /// Receives the specified number of bytes from a bound <see cref="ITcpSocket"/> into the specified offset position of the
        /// receive buffer, using the specified SocketFlags.
        /// </summary>
        /// <param name="buffer">An array of type Byte that is the storage location for received data.</param>
        /// <param name="offset">The location in buffer to store the received data.</param>
        /// <param name="size">The number of bytes to receive.</param>
        /// <param name="socketFlags">A bitwise combination of the SocketFlags values.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous task.</param>
        /// <remarks>Cancelling the task will also close the socket.</remarks>
        /// <returns>The number of bytes received.</returns>
        Task<int> ReceiveAsync(byte[] buffer, int offset, int size, SocketFlags socketFlags, CancellationToken cancellationToken);
    }
}
#endif