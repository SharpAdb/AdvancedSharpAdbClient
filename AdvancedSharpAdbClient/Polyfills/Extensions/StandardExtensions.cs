// <copyright file="StandardExtensions.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace AdvancedSharpAdbClient.Polyfills
{
    /// <summary>
    /// Provides extension methods for single target libraries to avoid missing methods.
    /// For example, a library target <c>.NET Standard 2.0</c> reference by a project target
    /// <c>.NET Core App 3.1</c> will missing <c>SyncCommandConverter.GetCommand(byte[])</c>.
    /// You need to use <see cref="GetCommand(byte[])"/> in the library instead.
    /// </summary>
    public static class StandardExtensions
    {
        #region IAdbSocket

        /// <summary>
        /// Sends the specified number of bytes of data to a <see cref="IAdbSocket"/>,
        /// </summary>
        /// <param name="adbSocket">An instance of a class that implements the <see cref="IAdbSocket"/> interface.</param>
        /// <param name="data">A <see cref="byte"/> array that acts as a buffer, containing the data to send.</param>
        public static void Send(IAdbSocket adbSocket, byte[] data) => adbSocket.Send(data);

        /// <summary>
        /// Reads from the socket until the array is filled, or no more data is coming(because
        /// the socket closed or the timeout expired).
        /// </summary>
        /// <param name="adbSocket">An instance of a class that implements the <see cref="IAdbSocket"/> interface.</param>
        /// <param name="data" >The buffer to store the read data into.</param>
        /// <returns>The total number of bytes read.</returns>
        /// <remarks>This uses the default time out value.</remarks>
        public static int Read(IAdbSocket adbSocket, byte[] data) => adbSocket.Read(data);

#if HAS_TASK
        /// <summary>
        /// Reconnects the <see cref="IAdbSocket"/> to the same endpoint it was initially connected to.
        /// Use this when the socket was disconnected by adb and you have restarted adb.
        /// </summary>
        /// <param name="adbSocket">An instance of a class that implements the <see cref="IAdbSocket"/> interface.</param>
        /// <param name="isForce">Force reconnect whatever the socket is connected or not.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous task.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        public static Task ReconnectAsync(IAdbSocket adbSocket, bool isForce, CancellationToken cancellationToken) =>
#if NET6_0_OR_GREATER
            adbSocket.ReconnectAsync(isForce, cancellationToken).AsTask();
#else
            adbSocket.ReconnectAsync(isForce, cancellationToken);
#endif

        /// <summary>
        /// Sends the specified number of bytes of data to a <see cref="IAdbSocket"/>,
        /// </summary>
        /// <param name="adbSocket">An instance of a class that implements the <see cref="IAdbSocket"/> interface.</param>
        /// <param name="data">A <see cref="byte"/> array that acts as a buffer, containing the data to send.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the task.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        public static Task SendAsync(IAdbSocket adbSocket, byte[] data, CancellationToken cancellationToken) =>
#if HAS_BUFFERS
            adbSocket.SendAsync(data, cancellationToken).AsTask();
#else
            adbSocket.SendAsync(data, cancellationToken);
#endif

        /// <summary>
        /// Reads a <see cref="string"/> from an <see cref="IAdbSocket"/> instance when
        /// the connection is in sync mode.
        /// </summary>
        /// <param name="adbSocket">An instance of a class that implements the <see cref="IAdbSocket"/> interface.</param>
        /// <param name="data" >The buffer to store the read data into.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the task.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. The result value of the task contains the number of bytes received.</returns>
        public static Task<int> ReadAsync(IAdbSocket adbSocket, byte[] data, CancellationToken cancellationToken) =>
#if HAS_BUFFERS
            adbSocket.ReadAsync(data, cancellationToken).AsTask();
#else
            adbSocket.ReadAsync(data, cancellationToken);
#endif
#endif

        #endregion

        #region IDeviceMonitor

#if HAS_TASK
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting
        /// unmanaged resources asynchronously.
        /// </summary>
        /// <param name="deviceMonitor">An instance of a class that implements the <see cref="IDeviceMonitor"/> interface.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous dispose operation.</returns>
        public static Task DisposeAsync(IDeviceMonitor deviceMonitor) =>
#if HAS_BUFFERS
            deviceMonitor.DisposeAsync().AsTask();
#else
            deviceMonitor.DisposeAsync();
#endif
#endif

        #endregion

        #region ITcpSocket

        /// <summary>
        /// Sends the specified number of bytes of data to a connected
        /// <see cref="ITcpSocket"/> using the specified <paramref name="socketFlags"/>.
        /// </summary>
        /// <param name="tcpSocket">An instance of a class that implements the <see cref="ITcpSocket"/> interface.</param>
        /// <param name="buffer">An array of type Byte that contains the data to be sent.</param>
        /// <param name="socketFlags">A bitwise combination of the SocketFlags values.</param>
        /// <returns>The number of bytes sent to the Socket.</returns>
        public static void Send(ITcpSocket tcpSocket, byte[] buffer, SocketFlags socketFlags) => tcpSocket.Send(buffer, socketFlags);

        /// <summary>
        /// Receives the specified number of bytes from a bound <see cref="ITcpSocket"/>
        /// using the specified SocketFlags.
        /// </summary>
        /// <param name="tcpSocket">An instance of a class that implements the <see cref="ITcpSocket"/> interface.</param>
        /// <param name="buffer">An array of type Byte that is the storage location for received data.</param>
        /// <param name="socketFlags">A bitwise combination of the SocketFlags values.</param>
        /// <returns>The number of bytes received.</returns>
        public static int Receive(ITcpSocket tcpSocket, byte[] buffer, SocketFlags socketFlags) => tcpSocket.Receive(buffer, socketFlags);

#if HAS_TASK
        /// <summary>
        /// Begins an asynchronous request for a connection to a remote host.
        /// </summary>
        /// <param name="tcpSocket">An instance of a class that implements the <see cref="ITcpSocket"/> interface.</param>
        /// <param name="endPoint">An <see cref="EndPoint"/> that represents the remote device.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous task.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        public static Task ConnectAsync(ITcpSocket tcpSocket, EndPoint endPoint, CancellationToken cancellationToken) =>
#if NET6_0_OR_GREATER
            tcpSocket.ConnectAsync(endPoint, cancellationToken).AsTask();
#else
            tcpSocket.ConnectAsync(endPoint, cancellationToken);
#endif

        /// <summary>
        /// Re-establishes the connection to a remote host. Assumes you have resolved the reason that caused the
        /// socket to disconnect.
        /// </summary>
        /// <param name="tcpSocket">An instance of a class that implements the <see cref="ITcpSocket"/> interface.</param>
        /// <param name="isForce">Force reconnect whatever the socket is connected or not.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous task.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        public static Task ReconnectAsync(ITcpSocket tcpSocket, bool isForce, CancellationToken cancellationToken) =>
#if NET6_0_OR_GREATER
            tcpSocket.ReconnectAsync(isForce, cancellationToken).AsTask();
#else
            tcpSocket.ReconnectAsync(isForce, cancellationToken);
#endif

        /// <summary>
        /// Asynchronously sends the specified number of bytes of data to a connected
        /// <see cref="ITcpSocket"/> using the specified <paramref name="socketFlags"/>.
        /// </summary>
        /// <param name="tcpSocket">An instance of a class that implements the <see cref="ITcpSocket"/> interface.</param>
        /// <param name="buffer">An array of type Byte that contains the data to be sent.</param>
        /// <param name="socketFlags">A bitwise combination of the SocketFlags values.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous task.</param>
        /// <returns>The number of bytes sent to the Socket.</returns>
        public static Task SendAsync(ITcpSocket tcpSocket, byte[] buffer, SocketFlags socketFlags, CancellationToken cancellationToken) =>
#if HAS_BUFFERS
            tcpSocket.SendAsync(buffer, socketFlags, cancellationToken).AsTask();
#else
            tcpSocket.SendAsync(buffer, socketFlags, cancellationToken);
#endif

        /// <summary>
        /// Receives the specified number of bytes from a bound <see cref="ITcpSocket"/>
        /// using the specified SocketFlags.
        /// </summary>
        /// <param name="tcpSocket">An instance of a class that implements the <see cref="ITcpSocket"/> interface.</param>
        /// <param name="buffer">An array of type Byte that is the storage location for received data.</param>
        /// <param name="socketFlags">A bitwise combination of the SocketFlags values.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous task.</param>
        /// <remarks>Cancelling the task will also close the socket.</remarks>
        /// <returns>The number of bytes received.</returns>
        public static Task<int> ReceiveAsync(ITcpSocket tcpSocket, byte[] buffer, SocketFlags socketFlags, CancellationToken cancellationToken) =>
#if HAS_BUFFERS
            tcpSocket.ReceiveAsync(buffer, socketFlags, cancellationToken).AsTask();
#else
            tcpSocket.ReceiveAsync(buffer, socketFlags, cancellationToken);
#endif
#endif

        #endregion

        #region SyncCommandConverter

        /// <summary>
        /// Determines which <see cref="SyncCommand"/> is represented by this byte array.
        /// </summary>
        /// <param name="value">A byte array that represents a <see cref="SyncCommand"/>.</param>
        /// <returns>The corresponding <see cref="SyncCommand"/>.</returns>
        public static SyncCommand GetCommand(byte[] value) => SyncCommandConverter.GetCommand(value);

        #endregion

        #region FramebufferHeader

        /// <summary>
        /// Creates a new <see cref="FramebufferHeader"/> object based on a byte array which contains the data.
        /// </summary>
        /// <param name="data">The data that feeds the <see cref="FramebufferHeader"/> struct.</param>
        /// <returns>A new <see cref="FramebufferHeader"/> object.</returns>
        public static FramebufferHeader Read(byte[] data) => new(data);

        #endregion
    }
}
