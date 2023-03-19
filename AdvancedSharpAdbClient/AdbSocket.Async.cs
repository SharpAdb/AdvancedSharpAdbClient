using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using AdvancedSharpAdbClient.Exceptions;
using Microsoft.Extensions.Logging;

namespace AdvancedSharpAdbClient
{
    public partial class AdbSocket
    {
        /// <inheritdoc/>
        public virtual async Task<AdbResponse> ReadAdbResponseAsync()
        {
            AdbResponse response = await ReadAdbResponseInnerAsync();

            if (!response.IOSuccess || !response.Okay)
            {
                socket.Dispose();
                throw new AdbException($"An error occurred while reading a response from ADB: {response.Message}", response);
            }

            return response;
        }
        
        /// <summary>
        /// Reads the response from ADB after a command.
        /// </summary>
        /// <returns>A <see cref="AdbResponse"/> that represents the response received from ADB.</returns>
        protected async Task<AdbResponse> ReadAdbResponseInnerAsync()
        {
            AdbResponse resp = new();

            byte[] reply = new byte[4];
            await ReadAsync(reply);

            resp.IOSuccess = true;

            resp.Okay = IsOkay(reply);

            if (!resp.Okay)
            {
                string message = await ReadStringAsync();
                resp.Message = message;
#if HAS_LOGGER
                logger.LogError("Got reply '{0}', diag='{1}'", ReplyToString(reply), resp.Message);
#endif
            }

            return resp;
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
            
#if NET6_0_OR_GREATER
        
        /// <summary>
        /// Write until all data in "data" is written or the connection fails or times out.
        /// </summary>
        /// <param name="data">The data to send.</param>
        /// <returns>Returns <see langword="true"/> if all data was written; otherwise, <see langword="false"/>.</returns>
        /// <remarks>This uses the default time out value.</remarks>
        protected async Task<bool> WriteAsync(byte[] data)
        {
            try
            {
                await SendAsync(data, -1);
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
        
        /// <inheritdoc/>
        public virtual async Task SendAdbRequestAsync(string request)
        {
            byte[] data = AdbClient.FormAdbRequest(request);

            if (!await WriteAsync(data))
            {
                throw new IOException($"Failed sending the request '{request}' to ADB");
            }
        }
        
        /// <inheritdoc/>
        public virtual Task SendAsync(byte[] data, int length) => SendAsync(data, 0, length);

        /// <inheritdoc/>
        public virtual async Task SendAsync(byte[] data, int offset, int length)
        {
            try
            {
                int count = await socket.SendAsync(data, offset, length != -1 ? length : data.Length, SocketFlags.None);
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
#endif
    }
}