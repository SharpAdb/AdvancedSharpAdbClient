#if HAS_TASK
// <copyright file="AdbServer.Async.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

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
        public virtual
#if NET6_0_OR_GREATER
            ValueTask
#else
            Task
#endif
            ReconnectAsync(CancellationToken cancellationToken = default) => Socket.ReconnectAsync(cancellationToken);

        /// <inheritdoc/>
        public virtual async Task SendAsync(byte[] data, int length, CancellationToken cancellationToken = default)
        {
            try
            {
                int count = await Socket.SendAsync(data, length != -1 ? length : data.Length, SocketFlags.None, cancellationToken).ConfigureAwait(false);

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
        public virtual async Task SendAsync(byte[] data, int offset, int length, CancellationToken cancellationToken = default)
        {
            try
            {
                int count = await Socket.SendAsync(data, offset, length != -1 ? length : data.Length - offset, SocketFlags.None, cancellationToken).ConfigureAwait(false);

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
            await SendSyncRequestAsync(command, pathBytes.Length, cancellationToken).ConfigureAwait(false);
            _ = await WriteAsync(pathBytes, cancellationToken).ConfigureAwait(false);
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

            _ = await WriteAsync(commandBytes, cancellationToken).ConfigureAwait(false);
            _ = await WriteAsync(lengthBytes, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public virtual async Task SendAdbRequestAsync(string request, CancellationToken cancellationToken = default)
        {
            byte[] data = AdbClient.FormAdbRequest(request);

            if (!await WriteAsync(data, cancellationToken).ConfigureAwait(false))
            {
                throw new IOException($"Failed sending the request '{request}' to ADB");
            }
        }

        /// <inheritdoc/>
        public virtual Task<int> ReadAsync(byte[] data, int length, CancellationToken cancellationToken = default) =>
            ReadAsync(data, 0, length, cancellationToken);

        /// <inheritdoc/>
        public virtual async Task<int> ReadAsync(byte[] data, int offset, int length, CancellationToken cancellationToken = default)
        {
            ExceptionExtensions.ThrowIfNull(data);
            ExceptionExtensions.ThrowIfNegative(offset);

            length = length != -1 ? length : data.Length;
            ExceptionExtensions.ThrowIfLessThan(data.Length, length, nameof(data));

            int count = -1;
            int totalRead = offset;

            while (count != 0 && totalRead < length)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    int left = length - totalRead;
                    int bufferLength = left < ReceiveBufferSize ? left : ReceiveBufferSize;

                    count = await Socket.ReceiveAsync(data, totalRead, bufferLength, SocketFlags.None, cancellationToken).ConfigureAwait(false);

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
            int read = await ReadAsync(reply, cancellationToken).ConfigureAwait(false);

            if (read == 0)
            {
                // There is no data to read
                return string.Empty;
            }

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
            _ = await ReadAsync(reply, cancellationToken).ConfigureAwait(false);

            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(reply);
            }

            int len =
#if HAS_BUFFERS
                BitConverter.ToInt32(reply);
#else
                BitConverter.ToInt32(reply, 0);
#endif

            // And get the string
            reply = new byte[len];
            _ = await ReadAsync(reply, cancellationToken).ConfigureAwait(false);

            string value = AdbClient.Encoding.GetString(reply);
            return value;
        }

        /// <inheritdoc/>
        public virtual async Task<SyncCommand> ReadSyncResponseAsync(CancellationToken cancellationToken = default)
        {
            byte[] data = new byte[4];
            _ = await ReadAsync(data, cancellationToken).ConfigureAwait(false);

            return SyncCommandConverter.GetCommand(data);
        }

        /// <inheritdoc/>
        public virtual async Task<AdbResponse> ReadAdbResponseAsync(CancellationToken cancellationToken = default)
        {
            AdbResponse response = await ReadAdbResponseInnerAsync(cancellationToken).ConfigureAwait(false);

            if (!response.IOSuccess || !response.Okay)
            {
                Socket.Dispose();
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
                await SendAdbRequestAsync($"host:transport:{device.Serial}", cancellationToken).ConfigureAwait(false);

                try
                {
                    AdbResponse response = await ReadAdbResponseAsync(cancellationToken).ConfigureAwait(false);
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

#if HAS_BUFFERS
        /// <inheritdoc/>
        public virtual async ValueTask SendAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
        {
            try
            {
                int count = await Socket.SendAsync(data, SocketFlags.None, cancellationToken).ConfigureAwait(false);
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
        public virtual async ValueTask<int> ReadAsync(Memory<byte> data, CancellationToken cancellationToken = default)
        {
            ExceptionExtensions.ThrowIfNull(data);

            int count = -1;
            int totalRead = 0;
            int length = data.Length;

            while (count != 0 && totalRead < length)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    int left = length - totalRead;
                    int bufferLength = left < ReceiveBufferSize ? left : ReceiveBufferSize;

                    count = await Socket.ReceiveAsync(data.Slice(totalRead, bufferLength), SocketFlags.None, cancellationToken).ConfigureAwait(false);

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
#else
        /// <inheritdoc/>
        public virtual async Task SendAsync(byte[] data, CancellationToken cancellationToken = default)
        {
            try
            {
                int count = await Socket.SendAsync(data, SocketFlags.None, cancellationToken).ConfigureAwait(false);

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
        public virtual Task<int> ReadAsync(byte[] data, CancellationToken cancellationToken = default) =>
            ReadAsync(data, 0, data.Length, cancellationToken);
#endif

        /// <summary>
        /// Write until all data in "data" is written or the connection fails or times out.
        /// </summary>
        /// <param name="data">The data to send.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous task.</param>
        /// <returns>Returns <see langword="true"/> if all data was written; otherwise, <see langword="false"/>.</returns>
        /// <remarks>This uses the default time out value.</remarks>
#if HAS_BUFFERS
        protected virtual async Task<bool> WriteAsync(Memory<byte> data, CancellationToken cancellationToken = default)
#else
        protected virtual async Task<bool> WriteAsync(byte[] data, CancellationToken cancellationToken = default)
#endif
        {
            try
            {
                await SendAsync(data, cancellationToken).ConfigureAwait(false);
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
            byte[] reply = new byte[4];
            _ = await ReadAsync(reply, cancellationToken).ConfigureAwait(false);

            if (IsOkay(reply))
            {
                return AdbResponse.OK;
            }
            else
            {
                string message = await ReadStringAsync(cancellationToken).ConfigureAwait(false);
                logger.LogError("Got reply '{0}', diag='{1}'", ReplyToString(reply), message);
                return AdbResponse.FromError(message);
            }
        }
    }
}
#endif