// <copyright file="TcpSocket.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion. All rights reserved.
// </copyright>

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace AdvancedSharpAdbClient
{
    /// <summary>
    /// Implements the <see cref="ITcpSocket" /> interface using the standard <see cref="Socket"/>
    /// class.
    /// </summary>
    public class TcpSocket : ITcpSocket
    {
        private Socket socket;
        private EndPoint endPoint;

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpSocket"/> class.
        /// </summary>
        public TcpSocket()
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        /// <inheritdoc/>
        public bool Connected => socket.Connected;

        /// <inheritdoc/>
        public int ReceiveBufferSize
        {
            get => socket.ReceiveBufferSize;

            set => socket.ReceiveBufferSize = value;
        }

        /// <inheritdoc/>
        public void Connect(EndPoint endPoint)
        {
            if (!(endPoint is IPEndPoint || endPoint is DnsEndPoint))
            {
                throw new NotSupportedException();
            }

            socket.Connect(endPoint);
            socket.Blocking = true;
            this.endPoint = endPoint;
        }

        /// <inheritdoc/>
        public void Reconnect()
        {
            if (socket.Connected)
            {
                // Already connected - nothing to do.
                return;
            }

            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Connect(endPoint);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
#if !NET35
            socket.Dispose();
#else
            socket.Close();
#endif
        }

        /// <inheritdoc/>
        public int Send(byte[] buffer, int offset, int size, SocketFlags socketFlags)
        {
            return socket.Send(buffer, offset, size, socketFlags);
        }

        /// <inheritdoc/>
        public Stream GetStream()
        {
            return new NetworkStream(socket);
        }

        /// <inheritdoc/>
        public int Receive(byte[] buffer, int offset, SocketFlags socketFlags)
        {
            return socket.Receive(buffer, offset, socketFlags);
        }

        /// <inheritdoc/>
        public Task<int> ReceiveAsync(byte[] buffer, int offset, int size, SocketFlags socketFlags, CancellationToken cancellationToken)
        {
            return socket.ReceiveAsync(buffer, offset, size, socketFlags, cancellationToken);
        }
    }
}
