#if HAS_TASK
// <copyright file="AdbServer.Async.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using AdvancedSharpAdbClient.Exceptions;
using System;
using System.Globalization;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace AdvancedSharpAdbClient
{
    public partial class AdbSocket
    {
        /// <inheritdoc/>
        public Task SendAsync(byte[] data, int length, CancellationToken cancellationToken = default) => SendAsync(data, 0, length, cancellationToken);

        /// <inheritdoc/>
        public virtual async Task SendAsync(byte[] data, int offset, int length, CancellationToken cancellationToken = default)
        {
            try
            {
                int count = await socket.SendAsync(data, offset, length != -1 ? length : data.Length, SocketFlags.None, cancellationToken);
                if (count < 0)
                {
                    throw new AdbException("channel EOF");
                }
            }
            catch (SocketException ex)
            {
                logger.LogError(ex, ex.Message);
                throw;
            }
        }

        /// <inheritdoc/>
        public Task SendSyncRequestAsync(SyncCommand command, string path, int permissions, CancellationToken cancellationToken = default) =>
            SendSyncRequestAsync(command, $"{path},{permissions}", cancellationToken);

        /// <inheritdoc/>
        public virtual async Task SendSyncRequestAsync(SyncCommand command, string path, CancellationToken cancellationToken = default)
        {
            ExceptionExtensions.ThrowIfNull(path);

            byte[] pathBytes = AdbClient.Encoding.GetBytes(path);
            await SendSyncRequestAsync(command, pathBytes.Length, cancellationToken);
            _ = await WriteAsync(pathBytes, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task SendSyncRequestAsync(SyncCommand command, int length, CancellationToken cancellationToken = default)
        {
            // The message structure is:
            // First four bytes: command
            // Next four bytes: length of the path
            // Final bytes: path
            byte[] commandBytes = SyncCommandConverter.GetBytes(command);

            byte[] lengthBytes = BitConverter.GetBytes(length);

            if (!BitConverter.IsLittleEndian)
            {
                // Convert from big endian to little endian
                Array.Reverse(lengthBytes);
            }

            _ = await WriteAsync(commandBytes, cancellationToken);
            _ = await WriteAsync(lengthBytes, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task SendAdbRequestAsync(string request, CancellationToken cancellationToken = default)
        {
            byte[] data = AdbClient.FormAdbRequest(request);

            if (!await WriteAsync(data, cancellationToken))
            {
                throw new IOException($"Failed sending the request '{request}' to ADB");
            }
        }

        /// <inheritdoc/>
        public Task<int> ReadAsync(byte[] data, CancellationToken cancellationToken = default) =>
            ReadAsync(data, data.Length, cancellationToken);

        /// <inheritdoc/>
        public virtual async Task<int> ReadAsync(byte[] data, int length, CancellationToken cancellationToken = default)
        {
            ExceptionExtensions.ThrowIfNegative(length);

            ExceptionExtensions.ThrowIfNull(data);

            ExceptionExtensions.ThrowIfLessThan(data.Length, length, nameof(data));

            int count = -1;
            int totalRead = 0;

            while (count != 0 && totalRead < length)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    int left = length - totalRead;
                    int bufferLength = left < ReceiveBufferSize ? left : ReceiveBufferSize;

                    count = await socket.ReceiveAsync(data, totalRead, bufferLength, SocketFlags.None, cancellationToken).ConfigureAwait(false);

                    if (count < 0)
                    {
                        logger.LogError("read: channel EOF");
                        throw new AdbException("EOF");
                    }
                    else if (count == 0)
                    {
                        logger.LogInformation("DONE with Read");
                    }
                    else
                    {
                        totalRead += count;
                    }
                }
                catch (SocketException ex)
                {
                    throw new AdbException($"An error occurred while receiving data from the adb server: {ex.Message}.", ex);
                }
            }

            return totalRead;
        }

        /// <inheritdoc/>
        public virtual async Task<string> ReadStringAsync(CancellationToken cancellationToken = default)
        {
            // The first 4 bytes contain the length of the string
            byte[] reply = new byte[4];
            _ = await ReadAsync(reply, cancellationToken).ConfigureAwait(false);

            // Convert the bytes to a hex string
            string lenHex = AdbClient.Encoding.GetString(reply);
            int len = int.Parse(lenHex, NumberStyles.HexNumber);

            // And get the string
            reply = new byte[len];
            _ = await ReadAsync(reply, cancellationToken).ConfigureAwait(false);

            string value = AdbClient.Encoding.GetString(reply);
            return value;
        }

        /// <inheritdoc/>
        public virtual async Task<string> ReadSyncStringAsync(CancellationToken cancellationToken = default)
        {
            // The first 4 bytes contain the length of the string
            byte[] reply = new byte[4];
            _ = await ReadAsync(reply, cancellationToken);

            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(reply);
            }

            int len = BitConverter.ToInt32(reply, 0);

            // And get the string
            reply = new byte[len];
            _ = await ReadAsync(reply, cancellationToken);

            string value = AdbClient.Encoding.GetString(reply);
            return value;
        }

        /// <inheritdoc/>
        public virtual async Task<SyncCommand> ReadSyncResponseAsync(CancellationToken cancellationToken = default)
        {
            byte[] data = new byte[4];
            _ = await ReadAsync(data, cancellationToken);

            return SyncCommandConverter.GetCommand(data);
        }

        /// <inheritdoc/>
        public virtual async Task<AdbResponse> ReadAdbResponseAsync(CancellationToken cancellationToken = default)
        {
            AdbResponse response = await ReadAdbResponseInnerAsync(cancellationToken);

            if (!response.IOSuccess || !response.Okay)
            {
                socket.Dispose();
                throw new AdbException($"An error occurred while reading a response from ADB: {response.Message}", response);
            }

            return response;
        }

        /// <inheritdoc/>
        public virtual async Task SetDeviceAsync(DeviceData device, CancellationToken cancellationToken = default)
        {
            // if the device is not null, then we first tell adb we're looking to talk
            // to a specific device
            if (device != null)
            {
                await SendAdbRequestAsync($"host:transport:{device.Serial}", cancellationToken);

                try
                {
                    AdbResponse response = await ReadAdbResponseAsync(cancellationToken);
                }
                catch (AdbException e)
                {
                    if (string.Equals("device not found", e.AdbError, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new DeviceNotFoundException(device.Serial);
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Write until all data in "data" is written or the connection fails or times out.
        /// </summary>
        /// <param name="data">The data to send.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous task.</param>
        /// <returns>Returns <see langword="true"/> if all data was written; otherwise, <see langword="false"/>.</returns>
        /// <remarks>This uses the default time out value.</remarks>
        protected virtual async Task<bool> WriteAsync(byte[] data, CancellationToken cancellationToken = default)
        {
            try
            {
                await SendAsync(data, -1, cancellationToken);
            }
            catch (IOException e)
            {
                logger.LogError(e, e.Message);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Reads the response from ADB after a command.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous task.</param>
        /// <returns>A <see cref="AdbResponse"/> that represents the response received from ADB.</returns>
        protected virtual async Task<AdbResponse> ReadAdbResponseInnerAsync(CancellationToken cancellationToken = default)
        {
            AdbResponse rasps = new();

            byte[] reply = new byte[4];
            _ = await ReadAsync(reply, cancellationToken);

            rasps.IOSuccess = true;

            rasps.Okay = IsOkay(reply);

            if (!rasps.Okay)
            {
                string message = await ReadStringAsync(cancellationToken);
                rasps.Message = message;
                logger.LogError($"Got reply '{ReplyToString(reply)}', diag='{rasps.Message}'");
            }

            return rasps;
        }
    }
}
#endif