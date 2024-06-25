#if HAS_TASK
// <copyright file="SyncService.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;

namespace AdvancedSharpAdbClient
{
    public partial class SyncService
    {
        /// <inheritdoc/>
        /// <remarks>This method has been invoked by the constructor.
        /// Do not use it unless you have closed the connection.
        /// Use <see cref="ReopenAsync(CancellationToken)"/> to reopen the connection.</remarks>
        public async Task OpenAsync(CancellationToken cancellationToken = default)
        {
            // target a specific device
            await Socket.SetDeviceAsync(Device, cancellationToken).ConfigureAwait(false);

            await Socket.SendAdbRequestAsync("sync:", cancellationToken).ConfigureAwait(false);
            _ = await Socket.ReadAdbResponseAsync(cancellationToken).ConfigureAwait(false);

            IsOutdate = false;
        }

        /// <inheritdoc/>
        public async Task ReopenAsync(CancellationToken cancellationToken = default)
        {
            await Socket.ReconnectAsync(true, cancellationToken).ConfigureAwait(false);
            await OpenAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public virtual async Task PushAsync(Stream stream, string remotePath, UnixFileStatus permission, DateTimeOffset timestamp, Action<SyncProgressChangedEventArgs>? callback = null, CancellationToken cancellationToken = default)
        {
            if (IsProcessing) { throw new InvalidOperationException($"The {nameof(SyncService)} is currently processing a request. Please {nameof(Clone)} a new {nameof(ISyncService)} or wait until the process is finished."); }

            ExceptionExtensions.ThrowIfNull(stream);
            ExceptionExtensions.ThrowIfNull(remotePath);

            if (remotePath.Length > MaxPathLength)
            {
                throw new ArgumentOutOfRangeException(nameof(remotePath), $"The remote path {remotePath} exceeds the maximum path size {MaxPathLength}");
            }

            if (IsOutdate) { await ReopenAsync(cancellationToken).ConfigureAwait(false); }

            try
            {
                await Socket.SendSyncRequestAsync(SyncCommand.SEND, remotePath, permission, cancellationToken).ConfigureAwait(false);
                IsProcessing = true;

                // create the buffer used to read.
                // we read max SYNC_DATA_MAX.
                byte[] buffer = new byte[MaxBufferSize];

                // We need 4 bytes of the buffer to send the 'DATA' command,
                // and an additional X bytes to inform how much data we are
                // sending.
                byte[] dataBytes = SyncCommand.DATA.GetBytes();
                byte[] lengthBytes = BitConverter.GetBytes(MaxBufferSize);
                int headerSize = dataBytes.Length + lengthBytes.Length;
                int reservedHeaderSize = headerSize;
                int maxDataSize = MaxBufferSize - reservedHeaderSize;
                lengthBytes = BitConverter.GetBytes(maxDataSize);

                // Try to get the total amount of bytes to transfer. This is not always possible, for example,
                // for forward-only streams.
                long totalBytesToProcess = stream.CanSeek ? stream.Length : 0;
                long totalBytesRead = 0;

                int read;
                // look while there is something to read
                while ((read =
#if HAS_BUFFERS
                    await stream.ReadAsync(buffer.AsMemory(headerSize, maxDataSize), cancellationToken).ConfigureAwait(false)
#else
                    await stream.ReadAsync(buffer, headerSize, maxDataSize, cancellationToken).ConfigureAwait(false)
#endif
                    ) > 0)
                {
                    // read up to SYNC_DATA_MAX
                    totalBytesRead += read;

                    if (read != maxDataSize)
                    {
                        // At the end of the line, so we need to recalculate the length of the header
                        lengthBytes = BitConverter.GetBytes(read);
                        headerSize = dataBytes.Length + lengthBytes.Length;
                    }

                    int startPosition = reservedHeaderSize - headerSize;

                    Buffer.BlockCopy(dataBytes, 0, buffer, startPosition, dataBytes.Length);
                    Buffer.BlockCopy(lengthBytes, 0, buffer, startPosition + dataBytes.Length, lengthBytes.Length);

                    // now send the data to the device
#if HAS_BUFFERS
                    await Socket.SendAsync(buffer.AsMemory(startPosition, read + dataBytes.Length + lengthBytes.Length), cancellationToken).ConfigureAwait(false);
#else
                    await Socket.SendAsync(buffer, startPosition, read + dataBytes.Length + lengthBytes.Length, cancellationToken).ConfigureAwait(false);
#endif
                    // Let the caller know about our progress, if requested
                    callback?.Invoke(new SyncProgressChangedEventArgs(totalBytesRead, totalBytesToProcess));

                    // check if we're canceled
                    cancellationToken.ThrowIfCancellationRequested();
                }

                // create the DONE message
                int time = (int)timestamp.ToUnixTimeSeconds();
                await Socket.SendSyncRequestAsync(SyncCommand.DONE, time, cancellationToken).ConfigureAwait(false);

                // read the result, in a byte array containing 2 int
                // (id, size)
                SyncCommand result = await Socket.ReadSyncResponseAsync(cancellationToken).ConfigureAwait(false);

                switch (result)
                {
                    case SyncCommand.FAIL:
                        string message = await Socket.ReadSyncStringAsync(cancellationToken).ConfigureAwait(false);
                        throw new AdbException(message);
                    case not SyncCommand.OKAY:
                        throw new AdbException($"The server sent an invalid response {result}");
                }
            }
            finally
            {
                IsOutdate = true;
                IsProcessing = false;
            }
        }

        /// <inheritdoc/>
        public virtual async Task PullAsync(string remotePath, Stream stream, Action<SyncProgressChangedEventArgs>? callback = null, CancellationToken cancellationToken = default)
        {
            if (IsProcessing) { throw new InvalidOperationException($"The {nameof(SyncService)} is currently processing a request. Please {nameof(Clone)} a new {nameof(ISyncService)} or wait until the process is finished."); }

            ExceptionExtensions.ThrowIfNull(remotePath);
            ExceptionExtensions.ThrowIfNull(stream);

            if (IsOutdate) { await ReopenAsync(cancellationToken).ConfigureAwait(false); }

            // Gets file information, including the file size, used to calculate the total amount of bytes to receive.
            FileStatistics stat = await StatAsync(remotePath, cancellationToken).ConfigureAwait(false);
            long totalBytesToProcess = stat.Size;
            long totalBytesRead = 0;

            byte[] buffer = new byte[MaxBufferSize];

            try
            {
                await Socket.SendSyncRequestAsync(SyncCommand.RECV, remotePath, cancellationToken).ConfigureAwait(false);
                IsProcessing = true;

                while (true)
                {
                    SyncCommand response = await Socket.ReadSyncResponseAsync(cancellationToken).ConfigureAwait(false);

                    switch (response)
                    {
                        case SyncCommand.DONE:
                            goto finish;
                        case SyncCommand.FAIL:
                            string message = await Socket.ReadSyncStringAsync(cancellationToken).ConfigureAwait(false);
                            throw new AdbException($"Failed to pull '{remotePath}'. {message}");
                        case not SyncCommand.DATA:
                            throw new AdbException($"The server sent an invalid response {response}");
                    }

                    // The first 4 bytes contain the length of the data packet
                    byte[] reply = new byte[4];
                    _ = await Socket.ReadAsync(reply, cancellationToken).ConfigureAwait(false);

                    int size = reply[0] | (reply[1] << 8) | (reply[2] << 16) | (reply[3] << 24);

                    if (size > MaxBufferSize)
                    {
                        throw new AdbException($"The adb server is sending {size} bytes of data, which exceeds the maximum chunk size {MaxBufferSize}");
                    }

                    // now read the length we received
#if HAS_BUFFERS
                    await Socket.ReadAsync(buffer.AsMemory(0, size), cancellationToken).ConfigureAwait(false);
                    await stream.WriteAsync(buffer.AsMemory(0, size), cancellationToken).ConfigureAwait(false);
#else
                    await Socket.ReadAsync(buffer, size, cancellationToken).ConfigureAwait(false);
                    await stream.WriteAsync(buffer, 0, size, cancellationToken).ConfigureAwait(false);
#endif
                    totalBytesRead += size;

                    // Let the caller know about our progress, if requested
                    callback?.Invoke(new SyncProgressChangedEventArgs(totalBytesRead, totalBytesToProcess));

                    // check if we're canceled
                    cancellationToken.ThrowIfCancellationRequested();
                }

            finish: return;
            }
            finally
            {
                IsOutdate = true;
                IsProcessing = false;
            }
        }

#if HAS_WINRT
        /// <inheritdoc/>
        [ContractVersion(typeof(UniversalApiContract), 65536u)]
        public virtual async Task PushAsync(IInputStream stream, string remotePath, UnixFileStatus permission, DateTimeOffset timestamp, Action<SyncProgressChangedEventArgs>? progress = null, CancellationToken cancellationToken = default)
        {
            if (IsProcessing) { throw new InvalidOperationException($"The {nameof(SyncService)} is currently processing a request. Please {nameof(Clone)} a new {nameof(ISyncService)} or wait until the process is finished."); }

            ExceptionExtensions.ThrowIfNull(stream);
            ExceptionExtensions.ThrowIfNull(remotePath);

            if (remotePath.Length > MaxPathLength)
            {
                throw new ArgumentOutOfRangeException(nameof(remotePath), $"The remote path {remotePath} exceeds the maximum path size {MaxPathLength}");
            }

            if (IsOutdate) { await ReopenAsync(cancellationToken).ConfigureAwait(false); }

            try
            {
                await Socket.SendSyncRequestAsync(SyncCommand.SEND, remotePath, permission, cancellationToken).ConfigureAwait(false);
                IsProcessing = true;

                // create the buffer used to read.
                // we read max SYNC_DATA_MAX.
                byte[] buffer = new byte[MaxBufferSize];

                // We need 4 bytes of the buffer to send the 'DATA' command,
                // and an additional X bytes to inform how much data we are
                // sending.
                byte[] dataBytes = SyncCommand.DATA.GetBytes();
                byte[] lengthBytes = BitConverter.GetBytes(MaxBufferSize);
                int headerSize = dataBytes.Length + lengthBytes.Length;
                int reservedHeaderSize = headerSize;
                int maxDataSize = MaxBufferSize - reservedHeaderSize;
                lengthBytes = BitConverter.GetBytes(maxDataSize);

                // Try to get the total amount of bytes to transfer. This is not always possible, for example,
                // for forward-only streams.
                ulong totalBytesToProcess = 0;
                ulong totalBytesRead = 0;

                try
                {
                    if (stream is IRandomAccessStream random)
                    {
                        totalBytesRead = random.Size;
                    }
                }
                catch { }

                // look while there is something to read
                while (true)
                {
                    // read up to SYNC_DATA_MAX
                    IBuffer results = await stream.ReadAsync(buffer.AsBuffer(headerSize, maxDataSize), (uint)maxDataSize, InputStreamOptions.None).AsTask(cancellationToken).ConfigureAwait(false);
                    uint read = results.Length;
                    totalBytesRead += read;

                    if (read == 0)
                    {
                        break;
                    }
                    else if (read != maxDataSize)
                    {
                        // At the end of the line, so we need to recalculate the length of the header
                        lengthBytes = BitConverter.GetBytes(read);
                        headerSize = dataBytes.Length + lengthBytes.Length;
                    }

                    int startPosition = reservedHeaderSize - headerSize;

                    Buffer.BlockCopy(dataBytes, 0, buffer, startPosition, dataBytes.Length);
                    Buffer.BlockCopy(lengthBytes, 0, buffer, startPosition + dataBytes.Length, lengthBytes.Length);

                    // now send the data to the device
#if HAS_BUFFERS
                    await Socket.SendAsync(buffer.AsMemory(startPosition, (int)(read + dataBytes.Length + lengthBytes.Length)), cancellationToken).ConfigureAwait(false);
#else
                    await Socket.SendAsync(buffer, startPosition, (int)(read + dataBytes.Length + lengthBytes.Length), cancellationToken).ConfigureAwait(false);
#endif
                    // Let the caller know about our progress, if requested
                    progress?.Invoke(new SyncProgressChangedEventArgs((long)totalBytesRead, (long)totalBytesToProcess));

                    // check if we're canceled
                    cancellationToken.ThrowIfCancellationRequested();
                }

                // create the DONE message
                int time = (int)timestamp.ToUnixTimeSeconds();
                await Socket.SendSyncRequestAsync(SyncCommand.DONE, time, cancellationToken).ConfigureAwait(false);

                // read the result, in a byte array containing 2 int
                // (id, size)
                SyncCommand result = await Socket.ReadSyncResponseAsync(cancellationToken).ConfigureAwait(false);

                switch (result)
                {
                    case SyncCommand.FAIL:
                        string message = await Socket.ReadSyncStringAsync(cancellationToken).ConfigureAwait(false);
                        throw new AdbException(message);
                    case not SyncCommand.OKAY:
                        throw new AdbException($"The server sent an invalid response {result}");
                }
            }
            finally
            {
                IsOutdate = true;
                IsProcessing = false;
            }
        }

        /// <inheritdoc/>
        public virtual async Task PullAsync(string remotePath, IOutputStream stream, Action<SyncProgressChangedEventArgs>? progress = null, CancellationToken cancellationToken = default)
        {
            if (IsProcessing) { throw new InvalidOperationException($"The {nameof(SyncService)} is currently processing a request. Please {nameof(Clone)} a new {nameof(ISyncService)} or wait until the process is finished."); }

            ExceptionExtensions.ThrowIfNull(remotePath);
            ExceptionExtensions.ThrowIfNull(stream);

            if (IsOutdate) { await ReopenAsync(cancellationToken).ConfigureAwait(false); }

            // Gets file information, including the file size, used to calculate the total amount of bytes to receive.
            FileStatistics stat = await StatAsync(remotePath, cancellationToken).ConfigureAwait(false);
            long totalBytesToProcess = stat.Size;
            long totalBytesRead = 0;

            byte[] buffer = new byte[MaxBufferSize];

            try
            {
                await Socket.SendSyncRequestAsync(SyncCommand.RECV, remotePath, cancellationToken).ConfigureAwait(false);
                IsProcessing = true;

                while (true)
                {
                    SyncCommand response = await Socket.ReadSyncResponseAsync(cancellationToken).ConfigureAwait(false);

                    switch (response)
                    {
                        case SyncCommand.DONE:
                            goto finish;
                        case SyncCommand.FAIL:
                            string message = await Socket.ReadSyncStringAsync(cancellationToken).ConfigureAwait(false);
                            throw new AdbException($"Failed to pull '{remotePath}'. {message}");
                        case not SyncCommand.DATA:
                            throw new AdbException($"The server sent an invalid response {response}");
                    }

                    // The first 4 bytes contain the length of the data packet
                    byte[] reply = new byte[4];
                    _ = await Socket.ReadAsync(reply, cancellationToken).ConfigureAwait(false);

                    int size = reply[0] | (reply[1] << 8) | (reply[2] << 16) | (reply[3] << 24);

                    if (size > MaxBufferSize)
                    {
                        throw new AdbException($"The adb server is sending {size} bytes of data, which exceeds the maximum chunk size {MaxBufferSize}");
                    }

                    // now read the length we received
#if HAS_BUFFERS
                    await Socket.ReadAsync(buffer.AsMemory(0, size), cancellationToken).ConfigureAwait(false);
#else
                    await Socket.ReadAsync(buffer, size, cancellationToken).ConfigureAwait(false);
#endif
                    uint write = await stream.WriteAsync(buffer.AsBuffer(0, size)).AsTask(cancellationToken).ConfigureAwait(false);
                    totalBytesRead += write;

                    // Let the caller know about our progress, if requested
                    progress?.Invoke(new SyncProgressChangedEventArgs(totalBytesRead, totalBytesToProcess));

                    // check if we're canceled
                    cancellationToken.ThrowIfCancellationRequested();
                }

                finish: return;
            }
            finally
            {
                IsOutdate = true;
                IsProcessing = false;
            }
        }
#endif

        /// <inheritdoc/>
        public async Task<FileStatistics> StatAsync(string remotePath, CancellationToken cancellationToken = default)
        {
            // create the stat request message.
            await Socket.SendSyncRequestAsync(SyncCommand.STAT, remotePath, cancellationToken).ConfigureAwait(false);

            SyncCommand response = await Socket.ReadSyncResponseAsync(cancellationToken).ConfigureAwait(false);
            if (response != SyncCommand.STAT)
            {
                throw new AdbException($"The server returned an invalid sync response {response}.");
            }

            // read the result, in a byte array containing 3 int
            // (mode, size, time)
            FileStatistics value = await ReadStatisticsAsync(cancellationToken).ConfigureAwait(false);
            value.Path = remotePath;

            return value;
        }

        /// <inheritdoc/>
        public async Task<List<FileStatistics>> GetDirectoryListingAsync(string remotePath, CancellationToken cancellationToken = default)
        {
            if (IsProcessing) { throw new InvalidOperationException($"The {nameof(SyncService)} is currently processing a request. Please {nameof(Clone)} a new {nameof(ISyncService)} or wait until the process is finished."); }
            if (IsOutdate) { await ReopenAsync(cancellationToken).ConfigureAwait(false); }
            bool isLocked = false;

        start:
            List<FileStatistics> value = [];

            try
            {
                // create the stat request message.
                await Socket.SendSyncRequestAsync(SyncCommand.LIST, remotePath, cancellationToken).ConfigureAwait(false);
                IsProcessing = true;

                while (true)
                {
                    SyncCommand response = await Socket.ReadSyncResponseAsync(cancellationToken).ConfigureAwait(false);

                    switch (response)
                    {
                        case 0 when isLocked:
                            throw new AdbException("The server returned an empty sync response.");
                        case 0:
                            Reopen();
                            isLocked = true;
                            goto start;
                        case SyncCommand.DONE:
                            goto finish;
                        case not SyncCommand.DENT:
                            throw new AdbException($"The server returned an invalid sync response {response}.");
                    }

                    FileStatistics entry = await ReadStatisticsAsync(cancellationToken).ConfigureAwait(false);
                    entry.Path = await Socket.ReadSyncStringAsync(cancellationToken).ConfigureAwait(false);

                    value.Add(entry);
                }

            finish:
                return value;
            }
            finally
            {
                IsOutdate = true;
                IsProcessing = false;
            }
        }

#if COMP_NETSTANDARD2_1
        /// <inheritdoc/>
        public async IAsyncEnumerable<FileStatistics> GetDirectoryAsyncListing(string remotePath, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (IsProcessing) { throw new InvalidOperationException($"The {nameof(SyncService)} is currently processing a request. Please {nameof(Clone)} a new {nameof(ISyncService)} or wait until the process is finished."); }
            if (IsOutdate) { await ReopenAsync(cancellationToken).ConfigureAwait(false); }
            bool isLocked = false;

            try
            {
                start:
                // create the stat request message.
                await Socket.SendSyncRequestAsync(SyncCommand.LIST, remotePath, cancellationToken).ConfigureAwait(false);
                IsProcessing = true;

                while (true)
                {
                    SyncCommand response = await Socket.ReadSyncResponseAsync(cancellationToken).ConfigureAwait(false);

                    switch (response)
                    {
                        case 0 when isLocked:
                            throw new AdbException("The server returned an empty sync response.");
                        case 0:
                            Reopen();
                            isLocked = true;
                            goto start;
                        case SyncCommand.DONE:
                            goto finish;
                        case not SyncCommand.DENT:
                            throw new AdbException($"The server returned an invalid sync response {response}.");
                    }

                    FileStatistics entry = await ReadStatisticsAsync(cancellationToken).ConfigureAwait(false);
                    entry.Path = await Socket.ReadSyncStringAsync(cancellationToken).ConfigureAwait(false);

                    yield return entry;
                    isLocked = true;
                }

                finish:
                yield break;
            }
            finally
            {
                IsOutdate = true;
                IsProcessing = false;
            }
        }
#endif

        /// <summary>
        /// Asynchronously reads the statistics of a file from the socket.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the task.</param>
        /// <returns>A <see cref="Task{FileStatistics}"/> which returns a <see cref="FileStatistics"/> object that contains information about the file.</returns>
        protected async Task<FileStatistics> ReadStatisticsAsync(CancellationToken cancellationToken = default)
        {
#if HAS_BUFFERS
            Memory<byte> statResult = new byte[12];
            _ = await Socket.ReadAsync(statResult, cancellationToken).ConfigureAwait(false);
            return EnumerableBuilder.FileStatisticsCreator(statResult.Span);
#else
            byte[] statResult = new byte[12];
            _ = await Socket.ReadAsync(statResult, cancellationToken).ConfigureAwait(false);
            int index = 0;
            return new FileStatistics
            {
                FileMode = (UnixFileStatus)ReadInt32(statResult),
                Size = ReadInt32(statResult),
                Time = DateTimeExtensions.FromUnixTimeSeconds(ReadInt32(statResult))
            };
            int ReadInt32(byte[] data) => data[index++] | (data[index++] << 8) | (data[index++] << 16) | (data[index++] << 24);
#endif
        }
    }
}
#endif