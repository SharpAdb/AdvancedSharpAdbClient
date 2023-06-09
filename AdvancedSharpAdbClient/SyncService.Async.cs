#if HAS_TASK
// <copyright file="SyncService.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using AdvancedSharpAdbClient.Exceptions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;

namespace AdvancedSharpAdbClient
{
    public partial class SyncService
    {
        /// <inheritdoc/>
        public async Task OpenAsync(CancellationToken cancellationToken = default)
        {
            // target a specific device
            await Socket.SetDeviceAsync(Device, cancellationToken);

            await Socket.SendAdbRequestAsync("sync:", cancellationToken);
            _ = await Socket.ReadAdbResponseAsync(cancellationToken);
        }

        /// <summary>
        /// Reopen this connection.
        /// </summary>
        /// <param name="socket">A <see cref="IAdbSocket"/> that enables to connection with the adb server.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public Task ReopenAsync(IAdbSocket socket, CancellationToken cancellationToken = default)
        {
            if (Socket != null)
            {
                Socket.Dispose();
                Socket = null;
            }
            Socket = socket;
            return OpenAsync(cancellationToken);
        }

        /// <summary>
        /// Reopen this connection.
        /// </summary>
        /// <param name="client">A connection to an adb server.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        public Task ReopenAsync(IAdbClient client, CancellationToken cancellationToken = default) => ReopenAsync(Factories.AdbSocketFactory(client.EndPoint), cancellationToken);

        /// <inheritdoc/>
        public async Task PushAsync(Stream stream, string remotePath, int permissions, DateTimeOffset timestamp, IProgress<int> progress, CancellationToken cancellationToken = default)
        {
            ExceptionExtensions.ThrowIfNull(stream);

            ExceptionExtensions.ThrowIfNull(remotePath);

            if (remotePath.Length > MaxPathLength)
            {
                throw new ArgumentOutOfRangeException(nameof(remotePath), $"The remote path {remotePath} exceeds the maximum path size {MaxPathLength}");
            }

            await Socket.SendSyncRequestAsync(SyncCommand.SEND, remotePath, permissions, cancellationToken);

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
                int read = stream.Read(buffer, headerSize, maxDataSize);
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
                await Socket.SendAsync(buffer, startPosition, read + dataBytes.Length + lengthBytes.Length, cancellationToken);

                SyncProgressChanged?.Invoke(this, new SyncProgressChangedEventArgs(totalBytesRead, totalBytesToProcess));

                // Let the caller know about our progress, if requested
                if (progress != null && totalBytesToProcess != 0)
                {
                    progress.Report((int)(100.0 * totalBytesRead / totalBytesToProcess));
                }
            }

            // create the DONE message
            int time = (int)timestamp.ToUnixTimeSeconds();
            await Socket.SendSyncRequestAsync(SyncCommand.DONE, time, cancellationToken);

            // read the result, in a byte array containing 2 int
            // (id, size)
            SyncCommand result = await Socket.ReadSyncResponseAsync(cancellationToken);

            if (result == SyncCommand.FAIL)
            {
                string message = await Socket.ReadSyncStringAsync(cancellationToken);

                throw new AdbException(message);
            }
            else if (result != SyncCommand.OKAY)
            {
                throw new AdbException($"The server sent an invalid response {result}");
            }
        }

        /// <inheritdoc/>
        public async Task PullAsync(string remoteFilePath, Stream stream, IProgress<int> progress, CancellationToken cancellationToken = default)
        {
            ExceptionExtensions.ThrowIfNull(remoteFilePath);

            ExceptionExtensions.ThrowIfNull(stream);

            // Get file information, including the file size, used to calculate the total amount of bytes to receive.
            FileStatistics stat = await StatAsync(remoteFilePath, cancellationToken);
            long totalBytesToProcess = stat.Size;
            long totalBytesRead = 0;

            byte[] buffer = new byte[MaxBufferSize];

            await Socket.SendSyncRequestAsync(SyncCommand.RECV, remoteFilePath, cancellationToken);

            while (true)
            {
                SyncCommand response = await Socket.ReadSyncResponseAsync(cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();

                if (response == SyncCommand.DONE)
                {
                    break;
                }
                else if (response == SyncCommand.FAIL)
                {
                    string message = await Socket.ReadSyncStringAsync(cancellationToken);
                    throw new AdbException($"Failed to pull '{remoteFilePath}'. {message}");
                }
                else if (response != SyncCommand.DATA)
                {
                    throw new AdbException($"The server sent an invalid response {response}");
                }

                // The first 4 bytes contain the length of the data packet
                byte[] reply = new byte[4];
                _ = await Socket.ReadAsync(reply, cancellationToken);

                if (!BitConverter.IsLittleEndian)
                {
                    Array.Reverse(reply);
                }

                int size = BitConverter.ToInt32(reply, 0);

                if (size > MaxBufferSize)
                {
                    throw new AdbException($"The adb server is sending {size} bytes of data, which exceeds the maximum chunk size {MaxBufferSize}");
                }

                // now read the length we received
                await Socket.ReadAsync(buffer, size, cancellationToken);
#if NETCOREAPP2_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER
                await stream.WriteAsync(buffer.AsMemory(0, size), cancellationToken);
#elif !NET35
                await stream.WriteAsync(buffer, 0, size, cancellationToken);
#else
                await Utilities.Run(() => stream.Write(buffer, 0, size));
#endif
                totalBytesRead += size;

                SyncProgressChanged?.Invoke(this, new SyncProgressChangedEventArgs(totalBytesRead, totalBytesToProcess));

                // Let the caller know about our progress, if requested
                if (progress != null && totalBytesToProcess != 0)
                {
                    progress.Report((int)(100.0 * totalBytesRead / totalBytesToProcess));
                }
            }
        }

        /// <inheritdoc/>
        public async Task<FileStatistics> StatAsync(string remotePath, CancellationToken cancellationToken = default)
        {
            // create the stat request message.
            await Socket.SendSyncRequestAsync(SyncCommand.STAT, remotePath, cancellationToken);

            if (await Socket.ReadSyncResponseAsync(cancellationToken) != SyncCommand.STAT)
            {
                throw new AdbException($"The server returned an invalid sync response.");
            }

            // read the result, in a byte array containing 3 int
            // (mode, size, time)
            FileStatistics value = new()
            {
                Path = remotePath
            };

            await ReadStatisticsAsync(value, cancellationToken);

            return value;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FileStatistics>> GetDirectoryListingAsync(string remotePath, CancellationToken cancellationToken = default)
        {
            Collection<FileStatistics> value = new();

            // create the stat request message.
            await Socket.SendSyncRequestAsync(SyncCommand.LIST, remotePath, cancellationToken);

            while (true)
            {
                SyncCommand response = await Socket.ReadSyncResponseAsync(cancellationToken);

                if (response == SyncCommand.DONE)
                {
                    break;
                }
                else if (response != SyncCommand.DENT)
                {
                    throw new AdbException($"The server returned an invalid sync response.");
                }

                FileStatistics entry = new();
                await ReadStatisticsAsync(entry, cancellationToken);
                entry.Path = await Socket.ReadSyncStringAsync(cancellationToken);

                value.Add(entry);
            }

            return value;
        }

#if NETCOREAPP3_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        /// <inheritdoc/>
        public async IAsyncEnumerable<FileStatistics> GetDirectoryAsyncListing(string remotePath, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            // create the stat request message.
            await Socket.SendSyncRequestAsync(SyncCommand.LIST, remotePath, cancellationToken);

            while (true)
            {
                SyncCommand response = await Socket.ReadSyncResponseAsync(cancellationToken);

                if (response == SyncCommand.DONE)
                {
                    break;
                }
                else if (response != SyncCommand.DENT)
                {
                    throw new AdbException($"The server returned an invalid sync response.");
                }

                FileStatistics entry = new();
                await ReadStatisticsAsync(entry, cancellationToken);
                entry.Path = await Socket.ReadSyncStringAsync(cancellationToken);

                yield return entry;
            }
        }
#endif

        private async Task ReadStatisticsAsync(FileStatistics value, CancellationToken cancellationToken = default)
        {
            byte[] statResult = new byte[12];
            _ = await Socket.ReadAsync(statResult, cancellationToken);

            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(statResult, 0, 4);
                Array.Reverse(statResult, 4, 4);
                Array.Reverse(statResult, 8, 4);
            }

            value.FileMode = (UnixFileMode)BitConverter.ToInt32(statResult, 0);
            value.Size = BitConverter.ToInt32(statResult, 4);
            value.Time = Utilities.FromUnixTimeSeconds(BitConverter.ToInt32(statResult, 8));
        }
    }
}
#endif