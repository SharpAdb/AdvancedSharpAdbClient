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
        public virtual async Task OpenAsync(CancellationToken cancellationToken = default)
        {
            // target a specific device
            await Socket.SetDeviceAsync(Device, cancellationToken).ConfigureAwait(false);

            await Socket.SendAdbRequestAsync("sync:", cancellationToken).ConfigureAwait(false);
            _ = await Socket.ReadAdbResponseAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public virtual async Task ReopenAsync(CancellationToken cancellationToken = default)
        {
            await Socket.ReconnectAsync(true, cancellationToken).ConfigureAwait(false);
            await OpenAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public virtual async Task PushAsync(Stream stream, string remotePath, int permissions, DateTimeOffset timestamp, IProgress<SyncProgressChangedEventArgs>? progress = null, CancellationToken cancellationToken = default)
        {
            ExceptionExtensions.ThrowIfNull(stream);
            ExceptionExtensions.ThrowIfNull(remotePath);

            if (remotePath.Length > MaxPathLength)
            {
                throw new ArgumentOutOfRangeException(nameof(remotePath), $"The remote path {remotePath} exceeds the maximum path size {MaxPathLength}");
            }

            await Socket.SendSyncRequestAsync(SyncCommand.SEND, remotePath, permissions, cancellationToken).ConfigureAwait(false);

            // create the buffer used to read.
            // we read max SYNC_DATA_MAX.
            byte[] buffer = new byte[MaxBufferSize];

            // We need 4 bytes of the buffer to send the 'DATA' command,
            // and an additional X bytes to inform how much data we are
            // sending.
            byte[] dataBytes = SyncCommandConverter.GetBytes(SyncCommand.DATA);
            byte[] lengthBytes = BitConverter.GetBytes(MaxBufferSize);
            int headerSize = dataBytes.Length + lengthBytes.Length;
            int reservedHeaderSize = headerSize;
            int maxDataSize = MaxBufferSize - reservedHeaderSize;
            lengthBytes = BitConverter.GetBytes(maxDataSize);

            // Try to get the total amount of bytes to transfer. This is not always possible, for example,
            // for forward-only streams.
            long totalBytesToProcess = stream.CanSeek ? stream.Length : 0;
            long totalBytesRead = 0;

            // look while there is something to read
            while (true)
            {
                // check if we're canceled
                cancellationToken.ThrowIfCancellationRequested();

                // read up to SYNC_DATA_MAX
                int read =
#if HAS_BUFFERS
                    await stream.ReadAsync(buffer.AsMemory(headerSize, maxDataSize), cancellationToken).ConfigureAwait(false);
#else
                    await stream.ReadAsync(buffer, headerSize, maxDataSize, cancellationToken).ConfigureAwait(false);
#endif
                totalBytesRead += read;

                if (read == 0)
                {
                    // we reached the end of the file
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
                await Socket.SendAsync(buffer.AsMemory(startPosition, read + dataBytes.Length + lengthBytes.Length), cancellationToken).ConfigureAwait(false);
#else
                await Socket.SendAsync(buffer, startPosition, read + dataBytes.Length + lengthBytes.Length, cancellationToken).ConfigureAwait(false);
#endif
                // Let the caller know about our progress, if requested
                progress?.Report(new SyncProgressChangedEventArgs(totalBytesRead, totalBytesToProcess));
            }

            // create the DONE message
            int time = (int)timestamp.ToUnixTimeSeconds();
            await Socket.SendSyncRequestAsync(SyncCommand.DONE, time, cancellationToken).ConfigureAwait(false);

            // read the result, in a byte array containing 2 int
            // (id, size)
            SyncCommand result = await Socket.ReadSyncResponseAsync(cancellationToken).ConfigureAwait(false);

            if (result == SyncCommand.FAIL)
            {
                string message = await Socket.ReadSyncStringAsync(cancellationToken).ConfigureAwait(false);

                throw new AdbException(message);
            }
            else if (result != SyncCommand.OKAY)
            {
                throw new AdbException($"The server sent an invalid response {result}");
            }
        }

        /// <inheritdoc/>
        public virtual async Task PullAsync(string remoteFilePath, Stream stream, IProgress<SyncProgressChangedEventArgs>? progress = null, CancellationToken cancellationToken = default)
        {
            ExceptionExtensions.ThrowIfNull(remoteFilePath);
            ExceptionExtensions.ThrowIfNull(stream);

            // Get file information, including the file size, used to calculate the total amount of bytes to receive.
            FileStatistics stat = await StatAsync(remoteFilePath, cancellationToken).ConfigureAwait(false);
            long totalBytesToProcess = stat.Size;
            long totalBytesRead = 0;

            byte[] buffer = new byte[MaxBufferSize];

            await Socket.SendSyncRequestAsync(SyncCommand.RECV, remoteFilePath, cancellationToken).ConfigureAwait(false);

            while (true)
            {
                SyncCommand response = await Socket.ReadSyncResponseAsync(cancellationToken).ConfigureAwait(false);
                cancellationToken.ThrowIfCancellationRequested();

                if (response == SyncCommand.DONE)
                {
                    break;
                }
                else if (response == SyncCommand.FAIL)
                {
                    string message = await Socket.ReadSyncStringAsync(cancellationToken).ConfigureAwait(false);
                    throw new AdbException($"Failed to pull '{remoteFilePath}'. {message}");
                }
                else if (response != SyncCommand.DATA)
                {
                    throw new AdbException($"The server sent an invalid response {response}");
                }

                // The first 4 bytes contain the length of the data packet
                byte[] reply = new byte[4];
                _ = await Socket.ReadAsync(reply, cancellationToken).ConfigureAwait(false);

                if (!BitConverter.IsLittleEndian)
                {
                    Array.Reverse(reply);
                }

                int size =
#if HAS_BUFFERS
                    BitConverter.ToInt32(reply);
#else
                    BitConverter.ToInt32(reply, 0);
#endif

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
                progress?.Report(new SyncProgressChangedEventArgs(totalBytesRead, totalBytesToProcess));
            }
        }

        /// <inheritdoc/>
        public virtual async Task<FileStatistics> StatAsync(string remotePath, CancellationToken cancellationToken = default)
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
        public virtual async Task<List<FileStatistics>> GetDirectoryListingAsync(string remotePath, CancellationToken cancellationToken = default)
        {
            bool isLocked = false;

            start:
            List<FileStatistics> value = [];

            // create the stat request message.
            await Socket.SendSyncRequestAsync(SyncCommand.LIST, remotePath, cancellationToken).ConfigureAwait(false);

            while (true)
            {
                SyncCommand response = await Socket.ReadSyncResponseAsync(cancellationToken).ConfigureAwait(false);

                if (response == 0)
                {
                    if (isLocked)
                    {
                        throw new AdbException("The server returned an empty sync response.");
                    }
                    else
                    {
                        Reopen();
                        isLocked = true;
                        goto start;
                    }
                }
                else if (response == SyncCommand.DONE)
                {
                    break;
                }
                else if (response != SyncCommand.DENT)
                {
                    throw new AdbException($"The server returned an invalid sync response {response}.");
                }

                FileStatistics entry = await ReadStatisticsAsync(cancellationToken).ConfigureAwait(false);
                entry.Path = await Socket.ReadSyncStringAsync(cancellationToken).ConfigureAwait(false);

                value.Add(entry);
            }

            return value;
        }

#if NETCOREAPP3_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        /// <inheritdoc/>
        public virtual async IAsyncEnumerable<FileStatistics> GetDirectoryAsyncListing(string remotePath, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            bool isLocked = false;

            start:
            // create the stat request message.
            await Socket.SendSyncRequestAsync(SyncCommand.LIST, remotePath, cancellationToken).ConfigureAwait(false);

            while (true)
            {
                SyncCommand response = await Socket.ReadSyncResponseAsync(cancellationToken).ConfigureAwait(false);

                if (response == 0)
                {
                    if (isLocked)
                    {
                        throw new AdbException("The server returned an empty sync response.");
                    }
                    else
                    {
                        Reopen();
                        isLocked = true;
                        goto start;
                    }
                }
                else if (response == SyncCommand.DONE)
                {
                    break;
                }
                else if (response != SyncCommand.DENT)
                {
                    throw new AdbException($"The server returned an invalid sync response {response}.");
                }

                FileStatistics entry = await ReadStatisticsAsync(cancellationToken).ConfigureAwait(false);
                entry.Path = await Socket.ReadSyncStringAsync(cancellationToken).ConfigureAwait(false);

                yield return entry;
                isLocked = true;
            }
        }
#endif

        /// <summary>
        /// Asynchronously reads the statistics of a file from the socket.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the task.</param>
        /// <returns>A <see cref="Task"/> which return a <see cref="FileStatistics"/> object that contains information about the file.</returns>
        protected async Task<FileStatistics> ReadStatisticsAsync(CancellationToken cancellationToken = default)
        {
            byte[] statResult = new byte[12];
            _ = await Socket.ReadAsync(statResult, cancellationToken).ConfigureAwait(false);

            int index = 0;

            return new FileStatistics
            {
                FileType = (UnixFileType)ReadInt32(in statResult),
                Size = ReadInt32(in statResult),
                Time = DateTimeExtensions.FromUnixTimeSeconds(ReadInt32(in statResult))
            };

            int ReadInt32(in byte[] data) => data[index++] | (data[index++] << 8) | (data[index++] << 16) | (data[index++] << 24);
        }
    }
}
#endif