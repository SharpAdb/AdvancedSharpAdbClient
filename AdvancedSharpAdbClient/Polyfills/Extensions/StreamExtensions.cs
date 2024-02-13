#if NET35
// <copyright file="StreamExtensions.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.ComponentModel;
using System.IO;
using System.Threading;

namespace AdvancedSharpAdbClient.Polyfills
{
    /// <summary>
    /// Provides extension methods for the <see cref="Stream"/> class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal static class StreamExtensions
    {
        /// <summary>
        /// Asynchronously reads a sequence of bytes from the current stream, advances the position
        /// within the stream by the number of bytes read, and monitors cancellation requests.
        /// </summary>
        /// <param name="stream">The stream from which to read data.</param>
        /// <param name="buffer">The buffer to write the data into.</param>
        /// <param name="offset">The byte offset in <paramref name="buffer"/> at which to begin writing data from the stream.</param>
        /// <param name="count">The maximum number of bytes to read.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A task that represents the asynchronous read operation. The value of the TResult parameter
        /// contains the total number of bytes read into the buffer. The result value can be less than the number
        /// of bytes requested if the number of bytes currently available is less than the requested number,
        /// or it can be 0 (zero) if <paramref name="count"/> is 0 or if the end of the stream has been reached.</returns>
        /// <remarks>Cancelling the task will also close the stream.</remarks>
        public static Task<int> ReadAsync(this Stream stream, byte[] buffer, int offset, int count, CancellationToken cancellationToken = default)
        {
            // Register a callback so that when a cancellation is requested, the socket is closed.
            // This will cause an ObjectDisposedException to bubble up via TrySetResult, which we can catch
            // and convert to a TaskCancelledException - which is the exception we expect.
            CancellationTokenRegistration cancellationTokenRegistration = cancellationToken.Register(stream.Close);
            TaskCompletionSource<int> taskCompletionSource = new(stream);

            _ = stream.BeginRead(buffer, offset, count, iar =>
            {
                // this is the callback
                TaskCompletionSource<int> taskCompletionSource = (TaskCompletionSource<int>)iar.AsyncState;
                Stream stream = (Stream)taskCompletionSource.Task.AsyncState;

                try
                {
                    taskCompletionSource.TrySetResult(stream.EndRead(iar));
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
        }

        /// <summary>
        /// Asynchronously writes a sequence of bytes to the current stream, advances the current position
        /// within this stream by the number of bytes written, and monitors cancellation requests.
        /// </summary>
        /// <param name="stream">The stream from which to write data.</param>
        /// <param name="buffer">The buffer to write data from.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer"/> from which to begin copying bytes to the stream.</param>
        /// <param name="count">The maximum number of bytes to write.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A task that represents the asynchronous write operation.</returns>
        /// <remarks>Cancelling the task will also close the stream.</remarks>
        public static Task WriteAsync(this Stream stream, byte[] buffer, int offset, int count, CancellationToken cancellationToken = default)
        {
            // Register a callback so that when a cancellation is requested, the socket is closed.
            // This will cause an ObjectDisposedException to bubble up via TrySetResult, which we can catch
            // and convert to a TaskCancelledException - which is the exception we expect.
            CancellationTokenRegistration cancellationTokenRegistration = cancellationToken.Register(stream.Close);

            TaskCompletionSource<object?> taskCompletionSource = new(stream);

            _ = stream.BeginWrite(buffer, offset, count, iar =>
            {
                // this is the callback
                TaskCompletionSource<object?> taskCompletionSource = (TaskCompletionSource<object?>)iar.AsyncState;
                Stream stream = (Stream)taskCompletionSource.Task.AsyncState;

                try
                {
                    stream.EndWrite(iar);
                    taskCompletionSource.TrySetResult(null);
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
        }
    }
}
#endif