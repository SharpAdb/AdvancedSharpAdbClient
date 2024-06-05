#if HAS_TASK
// <copyright file="ITcpSocket.Async.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace AdvancedSharpAdbClient
{
    public partial interface ITcpSocket
    {
        /// <summary>
        /// Begins an asynchronous request for a connection to a remote host.
        /// </summary>
        /// <param name="endPoint">An <see cref="EndPoint"/> that represents the remote device.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous task.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        Task ConnectAsync(EndPoint endPoint, CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously re-establishes the connection to a remote host. Assumes you have resolved the reason that caused the
        /// socket to disconnect.
        /// </summary>
        /// <param name="isForce">Force reconnect whatever the socket is connected or not.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous task.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        Task ReconnectAsync(bool isForce, CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously sends the specified number of bytes of data to a connected
        /// <see cref="ITcpSocket"/> using the specified <paramref name="socketFlags"/>.
        /// </summary>
        /// <param name="buffer">An array of type Byte that contains the data to be sent.</param>
        /// <param name="socketFlags">A bitwise combination of the SocketFlags values.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous task.</param>
        /// <returns>A <see cref="Task{Int32}"/> which returns the number of bytes sent to the Socket.</returns>
        Task<int> SendAsync(byte[] buffer, SocketFlags socketFlags, CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously sends the specified number of bytes of data to a connected
        /// <see cref="ITcpSocket"/> using the specified <paramref name="socketFlags"/>.
        /// </summary>
        /// <param name="buffer">An array of type Byte that contains the data to be sent.</param>
        /// <param name="size">The number of bytes to send.</param>
        /// <param name="socketFlags">A bitwise combination of the SocketFlags values.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous task.</param>
        /// <returns>A <see cref="Task{Int32}"/> which returns the number of bytes sent to the Socket.</returns>
        Task<int> SendAsync(byte[] buffer, int size, SocketFlags socketFlags, CancellationToken cancellationToken);

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
        /// <returns>A <see cref="Task{Int32}"/> which returns the number of bytes sent to the Socket.</returns>
        Task<int> SendAsync(byte[] buffer, int offset, int size, SocketFlags socketFlags, CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously receives the specified number of bytes from a bound <see cref="ITcpSocket"/>
        /// using the specified SocketFlags.
        /// </summary>
        /// <param name="buffer">An array of type Byte that is the storage location for received data.</param>
        /// <param name="socketFlags">A bitwise combination of the SocketFlags values.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous task.</param>
        /// <remarks>Cancelling the task will also close the socket.</remarks>
        /// <returns>A <see cref="Task{Int32}"/> which returns the number of bytes received.</returns>
        Task<int> ReceiveAsync(byte[] buffer, SocketFlags socketFlags, CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously receives the specified number of bytes from a bound <see cref="ITcpSocket"/>
        /// using the specified SocketFlags.
        /// </summary>
        /// <param name="buffer">An array of type Byte that is the storage location for received data.</param>
        /// <param name="size">The number of bytes to receive.</param>
        /// <param name="socketFlags">A bitwise combination of the SocketFlags values.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous task.</param>
        /// <remarks>Cancelling the task will also close the socket.</remarks>
        /// <returns>A <see cref="Task{Int32}"/> which returns the number of bytes received.</returns>
        Task<int> ReceiveAsync(byte[] buffer, int size, SocketFlags socketFlags, CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously receives the specified number of bytes from a bound <see cref="ITcpSocket"/>
        /// into the specified offset position of the receive buffer, using the specified SocketFlags.
        /// </summary>
        /// <param name="buffer">An array of type Byte that is the storage location for received data.</param>
        /// <param name="offset">The location in buffer to store the received data.</param>
        /// <param name="size">The number of bytes to receive.</param>
        /// <param name="socketFlags">A bitwise combination of the SocketFlags values.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous task.</param>
        /// <remarks>Cancelling the task will also close the socket.</remarks>
        /// <returns>A <see cref="Task{Int32}"/> which returns the number of bytes received.</returns>
        Task<int> ReceiveAsync(byte[] buffer, int offset, int size, SocketFlags socketFlags, CancellationToken cancellationToken);

#if HAS_BUFFERS
        /// <summary>
        /// Asynchronously sends the specified number of bytes of data to a connected
        /// <see cref="ITcpSocket"/> using the specified <paramref name="socketFlags"/>.
        /// </summary>
        /// <param name="buffer">An array of type Byte that contains the data to be sent.</param>
        /// <param name="socketFlags">A bitwise combination of the SocketFlags values.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous task.</param>
        /// <returns>A <see cref="ValueTask{Int32}"/> which returns the number of bytes sent to the Socket.</returns>
        public ValueTask<int> SendAsync(ReadOnlyMemory<byte> buffer, SocketFlags socketFlags, CancellationToken cancellationToken)
#if COMP_NETSTANDARD2_1
            => new(ReceiveAsync(buffer.ToArray(), socketFlags, cancellationToken))
#endif
            ;

        /// <summary>
        /// Asynchronously receives the specified number of bytes from a bound <see cref="ITcpSocket"/>
        /// using the specified SocketFlags.
        /// </summary>
        /// <param name="buffer">An array of type Byte that is the storage location for received data.</param>
        /// <param name="socketFlags">A bitwise combination of the SocketFlags values.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous task.</param>
        /// <remarks>Cancelling the task will also close the socket.</remarks>
        /// <returns>A <see cref="ValueTask{Int32}"/> which returns the number of bytes received.</returns>
        public ValueTask<int> ReceiveAsync(Memory<byte> buffer, SocketFlags socketFlags, CancellationToken cancellationToken)
#if COMP_NETSTANDARD2_1
        {
            byte[] bytes = new byte[buffer.Length];
            return new(ReceiveAsync(bytes, socketFlags, cancellationToken).ContinueWith(x =>
            {
                int length = x.Result;
                for (int i = 0; i < length; i++)
                {
                    buffer.Span[i] = bytes[i];
                }
                return length;
            }));
        }
#else
            ;
#endif
#endif
    }
}
#endif