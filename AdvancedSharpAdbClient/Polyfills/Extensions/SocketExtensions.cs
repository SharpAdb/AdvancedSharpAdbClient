#if HAS_TASK
// <copyright file="SocketExtensions.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.ComponentModel;
using System.Net.Sockets;
using System.Threading;

namespace AdvancedSharpAdbClient.Polyfills
{
    /// <summary>
    /// Provides extension methods for the <see cref="Socket"/> class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal static class SocketExtensions
    {
        /// <summary>
        /// Asynchronously receives data from a connected socket.
        /// </summary>
        /// <param name="socket">The socket from which to read data.</param>
        /// <param name="buffer">An array of type <see cref="byte"/> that is the storage location for the received data.</param>
        /// <param name="socketFlags">A bitwise combination of the <see cref="SocketFlags"/> values.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous task.</param>
        /// <remarks>Cancelling the task will also close the socket.</remarks>
        /// <returns>A <see cref="Task{Int32}"/> which returns the number of bytes received.</returns>
        public static Task<int> ReceiveAsync(this Socket socket, byte[] buffer, SocketFlags socketFlags, CancellationToken cancellationToken = default) =>
#if HAS_BUFFERS
            socket.ReceiveAsync(buffer.AsMemory(), socketFlags, cancellationToken).AsTask();
#else
            socket.ReceiveAsync(buffer, 0, buffer.Length, socketFlags, cancellationToken);
#endif

        /// <summary>
        /// Asynchronously receives data from a connected socket.
        /// </summary>
        /// <param name="socket">The socket from which to read data.</param>
        /// <param name="buffer">An array of type <see cref="byte"/> that is the storage location for the received data.</param>
        /// <param name="size">The number of bytes to receive.</param>
        /// <param name="socketFlags">A bitwise combination of the <see cref="SocketFlags"/> values.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous task.</param>
        /// <remarks>Cancelling the task will also close the socket.</remarks>
        /// <returns>A <see cref="Task{Int32}"/> which returns the number of bytes received.</returns>
        public static Task<int> ReceiveAsync(this Socket socket, byte[] buffer, int size, SocketFlags socketFlags, CancellationToken cancellationToken = default) =>
#if HAS_BUFFERS
            socket.ReceiveAsync(buffer.AsMemory(0, size), socketFlags, cancellationToken).AsTask();
#else
            socket.ReceiveAsync(buffer, 0, size, socketFlags, cancellationToken);
#endif

        /// <summary>
        /// Asynchronously receives data from a connected socket.
        /// </summary>
        /// <param name="socket">The socket from which to read data.</param>
        /// <param name="buffer">An array of type <see cref="byte"/> that is the storage location for the received data.</param>
        /// <param name="offset">The zero-based position in the <paramref name="buffer"/> parameter at which to start storing data.</param>
        /// <param name="size">The number of bytes to receive.</param>
        /// <param name="socketFlags">A bitwise combination of the <see cref="SocketFlags"/> values.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous task.</param>
        /// <remarks>Cancelling the task will also close the socket.</remarks>
        /// <returns>A <see cref="Task{Int32}"/> which returns the number of bytes received.</returns>
        public static Task<int> ReceiveAsync(this Socket socket, byte[] buffer, int offset, int size, SocketFlags socketFlags, CancellationToken cancellationToken = default)
        {
#if HAS_BUFFERS
            return socket.ReceiveAsync(buffer.AsMemory(offset, size), socketFlags, cancellationToken).AsTask();
#elif HAS_PROCESS

            // Register a callback so that when a cancellation is requested, the socket is closed.
            // This will cause an ObjectDisposedException to bubble up via TrySetResult, which we can catch
            // and convert to a TaskCancelledException - which is the exception we expect.
            CancellationTokenRegistration cancellationTokenRegistration = cancellationToken.Register(socket.Close);
            TaskCompletionSource<int> taskCompletionSource = new(socket);

            _ = socket.BeginReceive(buffer, offset, size, socketFlags, iar =>
            {
                // this is the callback
                TaskCompletionSource<int> taskCompletionSource = (TaskCompletionSource<int>)iar.AsyncState;
                Socket socket = (Socket)taskCompletionSource.Task.AsyncState;

                try
                {
                    taskCompletionSource.TrySetResult(socket.EndReceive(iar));
                }
                catch (ObjectDisposedException) when (cancellationToken.IsCancellationRequested)
                {
                    taskCompletionSource.TrySetCanceled();
                }
                catch (Exception ex)
                {
                    taskCompletionSource.TrySetException(ex);
                }
                finally
                {
                    cancellationTokenRegistration.Dispose();
                }
            }, taskCompletionSource);

            return taskCompletionSource.Task;
#else
            return Task.Factory.StartNew(() => socket.Receive(buffer, offset, size, socketFlags), cancellationToken, TaskCreationOptions.None, TaskScheduler.Default);
#endif
        }

        /// <summary>
        /// Asynchronously sends data to a connected socket.
        /// </summary>
        /// <param name="socket">The socket from which to send data.</param>
        /// <param name="buffer">An array of type <see cref="byte"/> that contains the data to be sent.</param>
        /// <param name="socketFlags">A bitwise combination of the <see cref="SocketFlags"/> values.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous task.</param>
        /// <remarks>Cancelling the task will also close the socket.</remarks>
        /// <returns>A <see cref="Task{Int32}"/> which returns the number of bytes sent.</returns>
        public static Task<int> SendAsync(this Socket socket, byte[] buffer, SocketFlags socketFlags, CancellationToken cancellationToken = default) =>
#if HAS_BUFFERS
            socket.SendAsync(buffer.AsMemory(), socketFlags, cancellationToken).AsTask();
#else
            socket.SendAsync(buffer, 0, buffer.Length, socketFlags, cancellationToken);
#endif

        /// <summary>
        /// Asynchronously sends data to a connected socket.
        /// </summary>
        /// <param name="socket">The socket from which to send data.</param>
        /// <param name="buffer">An array of type <see cref="byte"/> that contains the data to be sent.</param>
        /// <param name="size">The number of bytes to send.</param>
        /// <param name="socketFlags">A bitwise combination of the <see cref="SocketFlags"/> values.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous task.</param>
        /// <remarks>Cancelling the task will also close the socket.</remarks>
        /// <returns>A <see cref="Task{Int32}"/> which returns the number of bytes sent.</returns>
        public static Task<int> SendAsync(this Socket socket, byte[] buffer, int size, SocketFlags socketFlags, CancellationToken cancellationToken = default) =>
#if HAS_BUFFERS
            socket.SendAsync(buffer.AsMemory(0, size), socketFlags, cancellationToken).AsTask();
#else
            socket.SendAsync(buffer, 0, size, socketFlags, cancellationToken);
#endif

        /// <summary>
        /// Asynchronously sends data to a connected socket.
        /// </summary>
        /// <param name="socket">The socket from which to send data.</param>
        /// <param name="buffer">An array of type <see cref="byte"/> that contains the data to be sent.</param>
        /// <param name="offset">The position in the data buffer at which to begin sending data.</param>
        /// <param name="size">The number of bytes to send.</param>
        /// <param name="socketFlags">A bitwise combination of the <see cref="SocketFlags"/> values.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous task.</param>
        /// <remarks>Cancelling the task will also close the socket.</remarks>
        /// <returns>A <see cref="Task{Int32}"/> which returns the number of bytes sent.</returns>
        public static Task<int> SendAsync(this Socket socket, byte[] buffer, int offset, int size, SocketFlags socketFlags, CancellationToken cancellationToken = default)
        {
#if HAS_BUFFERS
            return socket.SendAsync(buffer.AsMemory(offset, size), socketFlags, cancellationToken).AsTask();
#elif HAS_PROCESS
            // Register a callback so that when a cancellation is requested, the socket is closed.
            // This will cause an ObjectDisposedException to bubble up via TrySetResult, which we can catch
            // and convert to a TaskCancelledException - which is the exception we expect.
            CancellationTokenRegistration cancellationTokenRegistration = cancellationToken.Register(socket.Dispose);
            TaskCompletionSource<int> taskCompletionSource = new(socket);

            _ = socket.BeginSend(buffer, offset, size, socketFlags, iar =>
            {
                // this is the callback
                TaskCompletionSource<int> taskCompletionSource = (TaskCompletionSource<int>)iar.AsyncState;
                Socket socket = (Socket)taskCompletionSource.Task.AsyncState;

                try
                {
                    taskCompletionSource.TrySetResult(socket.EndSend(iar));
                }
                catch (ObjectDisposedException) when (cancellationToken.IsCancellationRequested)
                {
                    taskCompletionSource.TrySetCanceled();
                }
                catch (Exception ex)
                {
                    taskCompletionSource.TrySetException(ex);
                }
                finally
                {
                    cancellationTokenRegistration.Dispose();
                }
            }, taskCompletionSource);

            return taskCompletionSource.Task;
#else
            return Task.Factory.StartNew(() => socket.Send(buffer, offset, size, socketFlags), cancellationToken, TaskCreationOptions.None, TaskScheduler.Default);
#endif
        }
    }
}
#endif