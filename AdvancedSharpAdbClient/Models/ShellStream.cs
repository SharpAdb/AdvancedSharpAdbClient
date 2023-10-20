// <copyright file="ShellStream.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;

namespace AdvancedSharpAdbClient
{
    /// <summary>Represents a <see cref="Stream"/> that wraps around an inner <see cref="Stream"/> that contains
    /// output from an Android shell command. In the shell output, the LF character is replaced by a
    /// CR LF character. This stream undoes that change.</summary>
    /// <remarks><seealso href="http://stackoverflow.com/questions/13578416/read-binary-stdout-data-from-adb-shell"/></remarks>
    public class ShellStream : Stream
    {
        private readonly bool closeStream;
        private byte? pendingByte;

        /// <summary>
        /// Initializes a new instance of the <seealso cref="ShellStream"/> class.
        /// </summary>
        /// <param name="inner">The inner stream that contains the raw data retrieved from the shell. This stream must be readable.</param>
        /// <param name="closeStream"><see langword="true"/> if the <see cref="ShellStream"/> should close the <paramref name="inner"/> stream when closed; otherwise, <see langword="false"/>.</param>
        public ShellStream(Stream inner, bool closeStream)
        {
            ExceptionExtensions.ThrowIfNull(inner);

            if (!inner.CanRead)
            {
                throw new ArgumentOutOfRangeException(nameof(inner));
            }

            Inner = inner;
            this.closeStream = closeStream;
        }

        /// <summary>
        /// Gets the inner stream from which data is being read.
        /// </summary>
        public Stream Inner { get; private set; }

        /// <inheritdoc/>
        public override bool CanRead => true;

        /// <inheritdoc/>
        public override bool CanSeek => false;

        /// <inheritdoc/>
        public override bool CanWrite => false;

        /// <inheritdoc/>
        public override long Length => throw new NotImplementedException();

        /// <inheritdoc/>
        public override long Position
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

#if HAS_BUFFERS
        /// <inheritdoc/>
        public override int Read(Span<byte> buffer)
        {
            if (buffer.Length == 0)
            {
                return 0;
            }

            // Read the raw data from the base stream. There may be a
            // 'pending byte' from a previous operation; if that's the case,
            // consume it.
            int read;

            if (pendingByte != null)
            {
                buffer[0] = pendingByte.Value;
                read = Inner.Read(buffer[1..]);
                read++;
                pendingByte = null;
            }
            else
            {
                read = Inner.Read(buffer);
            }

            // Loop over the data, and find a LF (0x0d) character. If it is
            // followed by a CR (0x0a) character, remove the LF chracter and
            // keep only the LF character intact.
            for (int i = 0; i < read - 1; i++)
            {
                if (buffer[i] == 0x0d && buffer[i + 1] == 0x0a)
                {
                    buffer[i] = 0x0a;

                    for (int j = i + 1; j < read - 1; j++)
                    {
                        buffer[j] = buffer[j + 1];
                    }

                    // Reset unused data to \0
                    buffer[read - 1] = 0;

                    // We have removed one byte from the array of bytes which has
                    // been read; but the caller asked for a fixed number of bytes.
                    // So we need to get the next byte from the base stream.
                    // If less bytes were received than asked, we know no more data is
                    // available so we can skip this step
                    if (read < buffer.Length)
                    {
                        read--;
                        continue;
                    }

                    byte[] miniBuffer = new byte[1];
                    int miniRead = Inner.Read(miniBuffer.AsSpan(0, 1));

                    if (miniRead == 0)
                    {
                        // If no byte was read, no more data is (currently) available, and reduce the
                        // number of bytes by 1.
                        read--;
                    }
                    else
                    {
                        // Append the byte to the buffer.
                        buffer[read - 1] = miniBuffer[0];
                    }
                }
            }

            // The last byte is a special case, to find out if the next byte is 0x0a
            // we need to read one more byte from the inner stream.
            if (read > 0 && buffer[read - 1] == 0x0d)
            {
                int nextByte = Inner.ReadByte();

                if (nextByte == 0x0a)
                {
                    // If the next byte is 0x0a, set the last byte to 0x0a. The underlying
                    // stream has already advanced because of the ReadByte call, so all is good.
                    buffer[read - 1] = 0x0a;
                }
                else
                {
                    // If the next byte was not 0x0a, store it as the 'pending byte' --
                    // the next read operation will fetch this byte. We can't do a Seek here,
                    // because e.g. the network stream doesn't support seeking.
                    pendingByte = (byte)nextByte;
                }
            }

            return read;
        }
#endif

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (count == 0)
            {
                return 0;
            }

            // Read the raw data from the base stream. There may be a
            // 'pending byte' from a previous operation; if that's the case,
            // consume it.
            int read;

            if (pendingByte != null)
            {
                buffer[offset] = pendingByte.Value;
                read = Inner.Read(buffer, offset + 1, count - 1);
                read++;
                pendingByte = null;
            }
            else
            {
                read = Inner.Read(buffer, offset, count);
            }

            // Loop over the data, and find a LF (0x0d) character. If it is
            // followed by a CR (0x0a) character, remove the LF chracter and
            // keep only the LF character intact.
            for (int i = offset; i < offset + read - 1; i++)
            {
                if (buffer[i] == 0x0d && buffer[i + 1] == 0x0a)
                {
                    buffer[i] = 0x0a;

                    for (int j = i + 1; j < offset + read - 1; j++)
                    {
                        buffer[j] = buffer[j + 1];
                    }

                    // Reset unused data to \0
                    buffer[offset + read - 1] = 0;

                    // We have removed one byte from the array of bytes which has
                    // been read; but the caller asked for a fixed number of bytes.
                    // So we need to get the next byte from the base stream.
                    // If less bytes were received than asked, we know no more data is
                    // available so we can skip this step
                    if (read < count)
                    {
                        read--;
                        continue;
                    }

                    byte[] miniBuffer = new byte[1];
                    int miniRead = Inner.Read(miniBuffer, 1);

                    if (miniRead == 0)
                    {
                        // If no byte was read, no more data is (currently) available, and reduce the
                        // number of bytes by 1.
                        read--;
                    }
                    else
                    {
                        // Append the byte to the buffer.
                        buffer[offset + read - 1] = miniBuffer[0];
                    }
                }
            }

            // The last byte is a special case, to find out if the next byte is 0x0a
            // we need to read one more byte from the inner stream.
            if (read > 0 && buffer[offset + read - 1] == 0x0d)
            {
                int nextByte = Inner.ReadByte();

                if (nextByte == 0x0a)
                {
                    // If the next byte is 0x0a, set the last byte to 0x0a. The underlying
                    // stream has already advanced because of the ReadByte call, so all is good.
                    buffer[offset + read - 1] = 0x0a;
                }
                else
                {
                    // If the next byte was not 0x0a, store it as the 'pending byte' --
                    // the next read operation will fetch this byte. We can't do a Seek here,
                    // because e.g. the network stream doesn't support seeking.
                    pendingByte = (byte)nextByte;
                }
            }

            return read;
        }

#if HAS_BUFFERS
        /// <inheritdoc/>
        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            if (buffer.Length == 0)
            {
                return 0;
            }

            // Read the raw data from the base stream. There may be a
            // 'pending byte' from a previous operation; if that's the case,
            // consume it.
            int read;

            if (pendingByte != null)
            {
                buffer.Span[0] = pendingByte.Value;
                read = await Inner.ReadAsync(buffer[1..], cancellationToken).ConfigureAwait(false);
                read++;
                pendingByte = null;
            }
            else
            {
                read = await Inner.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
            }

            byte[] miniBuffer = new byte[1];

            // Loop over the data, and find a LF (0x0d) character. If it is
            // followed by a CR (0x0a) character, remove the LF character and
            // keep only the LF character intact.
            for (int i = 0; i < read - 1; i++)
            {
                if (buffer.Span[i] == 0x0d && buffer.Span[i + 1] == 0x0a)
                {
                    buffer.Span[i] = 0x0a;

                    for (int j = i + 1; j < read - 1; j++)
                    {
                        buffer.Span[j] = buffer.Span[j + 1];
                    }

                    // Reset unused data to \0
                    buffer.Span[read - 1] = 0;

                    // We have removed one byte from the array of bytes which has
                    // been read; but the caller asked for a fixed number of bytes.
                    // So we need to get the next byte from the base stream.
                    // If less bytes were received than asked, we know no more data is
                    // available so we can skip this step
                    if (read < buffer.Length)
                    {
                        read--;
                        continue;
                    }

                    int miniRead = await Inner.ReadAsync(miniBuffer.AsMemory(0, 1), cancellationToken).ConfigureAwait(false);

                    if (miniRead == 0)
                    {
                        // If no byte was read, no more data is (currently) available, and reduce the
                        // number of bytes by 1.
                        read--;
                    }
                    else
                    {
                        // Append the byte to the buffer.
                        buffer.Span[read - 1] = miniBuffer[0];
                    }
                }
            }

            // The last byte is a special case, to find out if the next byte is 0x0a
            // we need to read one more byte from the inner stream.
            if (read > 0 && buffer.Span[read - 1] == 0x0d)
            {
                _ = await Inner.ReadAsync(miniBuffer.AsMemory(0, 1), cancellationToken).ConfigureAwait(false);
                int nextByte = miniBuffer[0];

                if (nextByte == 0x0a)
                {
                    // If the next byte is 0x0a, set the last byte to 0x0a. The underlying
                    // stream has already advanced because of the ReadByte call, so all is good.
                    buffer.Span[read - 1] = 0x0a;
                }
                else
                {
                    // If the next byte was not 0x0a, store it as the 'pending byte' --
                    // the next read operation will fetch this byte. We can't do a Seek here,
                    // because e.g. the network stream doesn't support seeking.
                    pendingByte = (byte)nextByte;
                }
            }

            return read;
        }
#endif

#if HAS_TASK
#if NET8_0_OR_GREATER
#pragma warning disable CA1835
#endif
        /// <inheritdoc/>
        public
#if !NETFRAMEWORK || NET45_OR_GREATER
            override
#endif
            async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken = default)
        {
            if (count == 0)
            {
                return 0;
            }

            // Read the raw data from the base stream. There may be a
            // 'pending byte' from a previous operation; if that's the case,
            // consume it.
            int read;

            if (pendingByte != null)
            {
                buffer[offset] = pendingByte.Value;
                read = await Inner.ReadAsync(buffer, offset + 1, count - 1, cancellationToken).ConfigureAwait(false);
                read++;
                pendingByte = null;
            }
            else
            {
                read = await Inner.ReadAsync(buffer, offset, count, cancellationToken).ConfigureAwait(false);
            }

            byte[] miniBuffer = new byte[1];

            // Loop over the data, and find a LF (0x0d) character. If it is
            // followed by a CR (0x0a) character, remove the LF character and
            // keep only the LF character intact.
            for (int i = offset; i < offset + read - 1; i++)
            {
                if (buffer[i] == 0x0d && buffer[i + 1] == 0x0a)
                {
                    buffer[i] = 0x0a;

                    for (int j = i + 1; j < offset + read - 1; j++)
                    {
                        buffer[j] = buffer[j + 1];
                    }

                    // Reset unused data to \0
                    buffer[offset + read - 1] = 0;

                    // We have removed one byte from the array of bytes which has
                    // been read; but the caller asked for a fixed number of bytes.
                    // So we need to get the next byte from the base stream.
                    // If less bytes were received than asked, we know no more data is
                    // available so we can skip this step
                    if (read < count)
                    {
                        read--;
                        continue;
                    }

                    int miniRead = await Inner.ReadAsync(miniBuffer, 1, cancellationToken).ConfigureAwait(false);

                    if (miniRead == 0)
                    {
                        // If no byte was read, no more data is (currently) available, and reduce the
                        // number of bytes by 1.
                        read--;
                    }
                    else
                    {
                        // Append the byte to the buffer.
                        buffer[offset + read - 1] = miniBuffer[0];
                    }
                }
            }

            // The last byte is a special case, to find out if the next byte is 0x0a
            // we need to read one more byte from the inner stream.
            if (read > 0 && buffer[offset + read - 1] == 0x0d)
            {
                _ = await Inner.ReadAsync(miniBuffer, 1, cancellationToken).ConfigureAwait(false);
                int nextByte = miniBuffer[0];

                if (nextByte == 0x0a)
                {
                    // If the next byte is 0x0a, set the last byte to 0x0a. The underlying
                    // stream has already advanced because of the ReadByte call, so all is good.
                    buffer[offset + read - 1] = 0x0a;
                }
                else
                {
                    // If the next byte was not 0x0a, store it as the 'pending byte' --
                    // the next read operation will fetch this byte. We can't do a Seek here,
                    // because e.g. the network stream doesn't support seeking.
                    pendingByte = (byte)nextByte;
                }
            }

            return read;
        }
#if NET8_0_OR_GREATER
#pragma warning restore CA1835
#endif
#endif

        /// <inheritdoc/>
        [DoesNotReturn]
        public override void Flush() => throw new NotImplementedException();

        /// <inheritdoc/>
        [DoesNotReturn]
        public override long Seek(long offset, SeekOrigin origin) => throw new NotImplementedException();

        /// <inheritdoc/>
        [DoesNotReturn]
        public override void SetLength(long value) => throw new NotImplementedException();

        /// <inheritdoc/>
        [DoesNotReturn]
        public override void Write(byte[] buffer, int offset, int count) => throw new NotImplementedException();

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (closeStream && Inner != null)
                {
                    Inner.Dispose();
                    Inner = null;
                }
            }

            base.Dispose(disposing);
        }

#if NETCOREAPP3_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        /// <inheritdoc/>
        public override async ValueTask DisposeAsync()
        {
            if (closeStream && Inner != null)
            {
                await Inner.DisposeAsync();
                Inner = null;
            }
            await base.DisposeAsync();
            GC.SuppressFinalize(this);
        }
#endif
    }
}
