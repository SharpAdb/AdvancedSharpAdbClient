// <copyright file="ITcpSocket.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace AdvancedSharpAdbClient
{
    /// <summary>
    /// Provides an interface that allows access to the standard .NET <see cref="Socket"/>
    /// class. The main purpose of this interface is to enable mocking of the <see cref="Socket"/>
    /// in unit test scenarios.
    /// </summary>
    public partial interface ITcpSocket : IDisposable
    {
        /// <summary>
        /// Gets a value indicating whether a <see cref="ITcpSocket"/> is connected to a remote host as of the last
        /// <see cref="Send(byte[], int, SocketFlags)"/> or <see cref="Receive(byte[], int, SocketFlags)"/> operation.
        /// </summary>
        bool Connected { get; }

        /// <summary>
        /// Gets or sets a value that specifies the size of the receive buffer of the <see cref="ITcpSocket"/>.
        /// </summary>
        int ReceiveBufferSize { get; set; }

        /// <summary>
        /// Establishes a connection to a remote host.
        /// </summary>
        /// <param name="endPoint">An <see cref="EndPoint"/> that represents the remote device.</param>
        void Connect(EndPoint endPoint);

        /// <summary>
        /// Re-establishes the connection to a remote host. Assumes you have resolved the reason that caused the
        /// socket to disconnect.
        /// </summary>
        /// <param name="isForce">Force reconnect whatever the socket is connected or not.</param>
        void Reconnect(bool isForce = false);

        /// <summary>
        /// Sends the specified number of bytes of data to a connected
        /// <see cref="ITcpSocket"/> using the specified <paramref name="socketFlags"/>.
        /// </summary>
        /// <param name="buffer">An array of type Byte that contains the data to be sent.</param>
        /// <param name="socketFlags">A bitwise combination of the SocketFlags values.</param>
        /// <returns>The number of bytes sent to the Socket.</returns>
        int Send(byte[] buffer, SocketFlags socketFlags);

        /// <summary>
        /// Sends the specified number of bytes of data to a connected
        /// <see cref="ITcpSocket"/> using the specified <paramref name="socketFlags"/>.
        /// </summary>
        /// <param name="buffer">An array of type Byte that contains the data to be sent.</param>
        /// <param name="size">The number of bytes to send.</param>
        /// <param name="socketFlags">A bitwise combination of the SocketFlags values.</param>
        /// <returns>The number of bytes sent to the Socket.</returns>
        int Send(byte[] buffer, int size, SocketFlags socketFlags);

        /// <summary>
        /// Sends the specified number of bytes of data to a connected
        /// <see cref="ITcpSocket"/>, starting at the specified <paramref name="offset"/>,
        /// and using the specified <paramref name="socketFlags"/>.
        /// </summary>
        /// <param name="buffer">An array of type Byte that contains the data to be sent.</param>
        /// <param name="offset">The position in the data buffer at which to begin sending data.</param>
        /// <param name="size">The number of bytes to send.</param>
        /// <param name="socketFlags">A bitwise combination of the SocketFlags values.</param>
        /// <returns>The number of bytes sent to the Socket.</returns>
        int Send(byte[] buffer, int offset, int size, SocketFlags socketFlags);

        /// <summary>
        /// Receives the specified number of bytes from a bound <see cref="ITcpSocket"/>
        /// using the specified SocketFlags.
        /// </summary>
        /// <param name="buffer">An array of type Byte that is the storage location for received data.</param>
        /// <param name="socketFlags">A bitwise combination of the SocketFlags values.</param>
        /// <returns>The number of bytes received.</returns>
        int Receive(byte[] buffer, SocketFlags socketFlags);

        /// <summary>
        /// Receives the specified number of bytes from a bound <see cref="ITcpSocket"/>
        /// using the specified SocketFlags.
        /// </summary>
        /// <param name="buffer">An array of type Byte that is the storage location for received data.</param>
        /// <param name="size">The number of bytes to receive.</param>
        /// <param name="socketFlags">A bitwise combination of the SocketFlags values.</param>
        /// <returns>The number of bytes received.</returns>
        int Receive(byte[] buffer, int size, SocketFlags socketFlags);

        /// <summary>
        /// Receives the specified number of bytes from a bound <see cref="ITcpSocket"/>
        /// into the specified offset position of the receive buffer, using the specified SocketFlags.
        /// </summary>
        /// <param name="buffer">An array of type Byte that is the storage location for received data.</param>
        /// <param name="offset">The position in the <paramref name="buffer"/> parameter to store the received data.</param>
        /// <param name="size">The number of bytes to receive.</param>
        /// <param name="socketFlags">A bitwise combination of the SocketFlags values.</param>
        /// <returns>The number of bytes received.</returns>
        int Receive(byte[] buffer, int offset, int size, SocketFlags socketFlags);

#if HAS_BUFFERS
        /// <summary>
        /// Sends the specified number of bytes of data to a connected
        /// <see cref="ITcpSocket"/> using the specified <paramref name="socketFlags"/>.
        /// </summary>
        /// <param name="buffer">A span of bytes that contains the data to be sent.</param>
        /// <param name="socketFlags">A bitwise combination of the SocketFlags values.</param>
        /// <returns>The number of bytes sent to the Socket.</returns>
        int Send(ReadOnlySpan<byte> buffer, SocketFlags socketFlags)
#if COMP_NETSTANDARD2_1
            => Send(buffer.ToArray(), buffer.Length, socketFlags)
#endif
            ;

        /// <summary>
        /// Receives the specified number of bytes from a bound <see cref="ITcpSocket"/>
        /// using the specified SocketFlags.
        /// </summary>
        /// <param name="buffer">A span of bytes that is the storage location for the received data.</param>
        /// <param name="socketFlags">A bitwise combination of the SocketFlags values.</param>
        /// <returns>The number of bytes received.</returns>
        int Receive(Span<byte> buffer, SocketFlags socketFlags)
#if COMP_NETSTANDARD2_1
        {
            byte[] bytes = new byte[buffer.Length];
            int length = Receive(bytes, bytes.Length, socketFlags);
            for (int i = 0; i < length; i++)
            {
                buffer[i] = bytes[i];
            }
            return length;
        }
#else
            ;
#endif
#endif

        /// <summary>
        /// Gets the underlying <see cref="Stream"/>.
        /// </summary>
        /// <returns>The underlying stream.</returns>
        Stream GetStream();

        /// <summary>
        /// Closes the <see cref="Socket"/> connection and releases all associated resources.
        /// </summary>
        void Close();
    }
}
