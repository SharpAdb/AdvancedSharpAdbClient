using AdvancedSharpAdbClient.Exceptions;
using System;
using System.Globalization;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

#if HAS_LOGGER
using Microsoft.Extensions.Logging;
#endif

namespace AdvancedSharpAdbClient
{
    public partial class AdbSocket
    {
        /// <inheritdoc/>
        public virtual Task ReadAsync(byte[] data, CancellationToken cancellationToken = default) =>
            ReadAsync(data, data.Length, cancellationToken);

        /// <inheritdoc/>
        public virtual async Task<string> ReadStringAsync(CancellationToken cancellationToken = default)
        {
            // The first 4 bytes contain the length of the string
            byte[] reply = new byte[4];
            await ReadAsync(reply, cancellationToken).ConfigureAwait(false);

            // Convert the bytes to a hex string
            string lenHex = AdbClient.Encoding.GetString(reply);
            int len = int.Parse(lenHex, NumberStyles.HexNumber);

            // And get the string
            reply = new byte[len];
            await ReadAsync(reply, cancellationToken).ConfigureAwait(false);

            string value = AdbClient.Encoding.GetString(reply);
            return value;
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
        public virtual async Task SendAdbRequestAsync(string request, CancellationToken cancellationToken = default)
        {
            byte[] data = AdbClient.FormAdbRequest(request);

            if (!await WriteAsync(data, cancellationToken))
            {
                throw new IOException($"Failed sending the request '{request}' to ADB");
            }
        }

        /// <inheritdoc/>
        public virtual Task SendAsync(byte[] data, int length, CancellationToken cancellationToken = default) => SendAsync(data, 0, length, cancellationToken);

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
#if HAS_LOGGER
                logger.LogError(ex, ex.Message);
#endif
                throw ex;
            }
        }

        /// <inheritdoc/>
        public virtual async Task<int> ReadAsync(byte[] data, int length, CancellationToken cancellationToken = default)
        {
            if (length < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (data.Length < length)
            {
                throw new ArgumentOutOfRangeException(nameof(data));
            }

            int count = -1;
            int totalRead = 0;

            while (count != 0 && totalRead < length)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    int left = length - totalRead;
                    int buflen = left < ReceiveBufferSize ? left : ReceiveBufferSize;

                    count = await socket.ReceiveAsync(data, totalRead, buflen, SocketFlags.None, cancellationToken).ConfigureAwait(false);

                    if (count < 0)
                    {
#if HAS_LOGGER
                        logger.LogError("read: channel EOF");
#endif
                        throw new AdbException("EOF");
                    }
                    else if (count == 0)
                    {
#if HAS_LOGGER
                        logger.LogInformation("DONE with Read");
#endif
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

        /// <summary>
        /// Write until all data in "data" is written or the connection fails or times out.
        /// </summary>
        /// <param name="data">The data to send.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous task.</param>
        /// <returns>Returns <see langword="true"/> if all data was written; otherwise, <see langword="false"/>.</returns>
        /// <remarks>This uses the default time out value.</remarks>
        protected async Task<bool> WriteAsync(byte[] data, CancellationToken cancellationToken = default)
        {
            try
            {
                await SendAsync(data, -1, cancellationToken);
            }
#if HAS_LOGGER
            catch (IOException e)
            {
                logger.LogError(e, e.Message);
#else
            catch (IOException)
            {
#endif
                return false;
            }

            return true;
        }

        /// <summary>
        /// Reads the response from ADB after a command.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous task.</param>
        /// <returns>A <see cref="AdbResponse"/> that represents the response received from ADB.</returns>
        protected async Task<AdbResponse> ReadAdbResponseInnerAsync(CancellationToken cancellationToken = default)
        {
            AdbResponse resp = new();

            byte[] reply = new byte[4];
            await ReadAsync(reply);

            resp.IOSuccess = true;

            resp.Okay = IsOkay(reply);

            if (!resp.Okay)
            {
                string message = await ReadStringAsync(cancellationToken);
                resp.Message = message;
#if HAS_LOGGER
                logger.LogError("Got reply '{0}', diag='{1}'", ReplyToString(reply), resp.Message);
#endif
            }

            return resp;
        }
    }
}