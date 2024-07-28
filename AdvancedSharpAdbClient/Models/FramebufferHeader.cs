// <copyright file="FramebufferHeader.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

#if WINDOWS_UWP
using System.Threading;
#endif

namespace AdvancedSharpAdbClient.Models
{
    /// <summary>
    /// Whenever the <c>framebuffer:</c> service is invoked, the adb server responds with the contents
    /// of the framebuffer, prefixed with a <see cref="FramebufferHeader"/> object that contains more
    /// information about the framebuffer.
    /// </summary>
#if HAS_BUFFERS
    [CollectionBuilder(typeof(EnumerableBuilder), nameof(EnumerableBuilder.FramebufferHeaderCreator))]
#endif
    [DebuggerDisplay($"{{{nameof(ToString)}(),nq}}")]
    public readonly struct FramebufferHeader : IReadOnlyList<byte>
    {
        /// <summary>
        /// The length of the head when <see cref="Version"/> is <see langword="2"/>.
        /// </summary>
        public const int MaxLength = 56;

        /// <summary>
        /// The length of the head when <see cref="Version"/> is <see langword="1"/>.
        /// </summary>
        public const int MiniLength = 52;

        /// <summary>
        /// Initializes a new instance of the <see cref="FramebufferHeader"/> struct based on a byte array which contains the data.
        /// </summary>
        /// <param name="data">The data that feeds the <see cref="FramebufferHeader"/> struct.</param>
        /// <remarks>As defined in <see href="https://android.googlesource.com/platform/system/core/+/master/adb/framebuffer_service.cpp"/></remarks>
        public FramebufferHeader(byte[] data)
        {
            if (data.Length is < MiniLength or > MaxLength)
            {
                throw new ArgumentOutOfRangeException(nameof(data), $"The length of {nameof(data)} must between {MiniLength} and {MaxLength}.");
            }

            int index = 0;

            Version = ReadUInt32(data);

            if (Version > 2)
            {
                // Technically, 0 is not a supported version either; we assume version 0 indicates
                // an empty framebuffer.
                throw new InvalidOperationException($"Framebuffer version {Version} is not supported");
            }

            Bpp = ReadUInt32(data);

            if (Version >= 2)
            {
                ColorSpace = ReadUInt32(data);
            }

            Size = ReadUInt32(data);
            Width = ReadUInt32(data);
            Height = ReadUInt32(data);

            Red = new ColorData(
                ReadUInt32(data),
                ReadUInt32(data));

            Blue = new ColorData(
                ReadUInt32(data),
                ReadUInt32(data));

            Green = new ColorData(
                ReadUInt32(data),
                ReadUInt32(data));

            Alpha = new ColorData(
                ReadUInt32(data),
                ReadUInt32(data));

            uint ReadUInt32(byte[] data) => (uint)(data[index++] | (data[index++] << 8) | (data[index++] << 16) | (data[index++] << 24));
        }

#if HAS_BUFFERS
        /// <summary>
        /// Initializes a new instance of the <see cref="FramebufferHeader"/> struct based on a byte array which contains the data.
        /// </summary>
        /// <param name="data">The data that feeds the <see cref="FramebufferHeader"/> struct.</param>
        /// <remarks>As defined in <see href="https://android.googlesource.com/platform/system/core/+/master/adb/framebuffer_service.cpp"/></remarks>
        public FramebufferHeader(ReadOnlySpan<byte> data)
        {
            if (data.Length is < MiniLength or > MaxLength)
            {
                throw new ArgumentOutOfRangeException(nameof(data), $"The length of {nameof(data)} must between {MiniLength} and {MaxLength}.");
            }

            int index = 0;

            Version = ReadUInt32(data);

            if (Version > 2)
            {
                // Technically, 0 is not a supported version either; we assume version 0 indicates
                // an empty framebuffer.
                throw new InvalidOperationException($"Framebuffer version {Version} is not supported");
            }

            Bpp = ReadUInt32(data);

            if (Version >= 2)
            {
                ColorSpace = ReadUInt32(data);
            }

            Size = ReadUInt32(data);
            Width = ReadUInt32(data);
            Height = ReadUInt32(data);

            Red = new ColorData(
                ReadUInt32(data),
                ReadUInt32(data));

            Blue = new ColorData(
                ReadUInt32(data),
                ReadUInt32(data));

            Green = new ColorData(
                ReadUInt32(data),
                ReadUInt32(data));

            Alpha = new ColorData(
                ReadUInt32(data),
                ReadUInt32(data));

            uint ReadUInt32(in ReadOnlySpan<byte> data) => (uint)(data[index++] | (data[index++] << 8) | (data[index++] << 16) | (data[index++] << 24));
        }
#endif

        /// <summary>
        /// Gets or sets the version of the framebuffer struct.
        /// </summary>
        public uint Version { get; init; }

        /// <summary>
        /// Gets or sets the number of bytes per pixel. Usual values include 32 or 24.
        /// </summary>
        public uint Bpp { get; init; }

        /// <summary>
        /// Gets or sets the color space. Only available starting with <see cref="Version"/> 2.
        /// </summary>
        public uint ColorSpace { get; init; }

        /// <summary>
        /// Gets or sets the total size, in bits, of the framebuffer.
        /// </summary>
        public uint Size { get; init; }

        /// <summary>
        /// Gets or sets the width, in pixels, of the framebuffer.
        /// </summary>
        public uint Width { get; init; }

        /// <summary>
        /// Gets or sets the height, in pixels, of the framebuffer.
        /// </summary>
        public uint Height { get; init; }

        /// <summary>
        /// Gets or sets information about the red color channel.
        /// </summary>
        public ColorData Red { get; init; }

        /// <summary>
        /// Gets or sets information about the blue color channel.
        /// </summary>
        public ColorData Blue { get; init; }

        /// <summary>
        /// Gets or sets information about the green color channel.
        /// </summary>
        public ColorData Green { get; init; }

        /// <summary>
        /// Gets or sets information about the alpha channel.
        /// </summary>
        public ColorData Alpha { get; init; }

        /// <summary>
        /// Gets the length of the head in bytes.
        /// </summary>
        public int Count => Version < 2 ? MiniLength : MaxLength;

        /// <inheritdoc/>
        public byte this[int index]
        {
            get
            {
                if (index < 0 || index >= Count)
                {
                    throw new IndexOutOfRangeException("Index was out of range. Must be non-negative and less than the size of the collection.");
                }

                if (index > 7 && Version < 2)
                {
                    index += 4;
                }

                return index switch
                {
                    < 4 => GetByte(Version),
                    < 8 => GetByte(Bpp),
                    < 12 => GetByte(ColorSpace),
                    < 16 => GetByte(Size),
                    < 20 => GetByte(Width),
                    < 24 => GetByte(Height),
                    < 28 => GetByte(Red.Offset),
                    < 32 => GetByte(Red.Length),
                    < 36 => GetByte(Blue.Offset),
                    < 40 => GetByte(Blue.Length),
                    < 44 => GetByte(Green.Offset),
                    < 48 => GetByte(Green.Length),
                    < 52 => GetByte(Alpha.Offset),
                    < 56 => GetByte(Alpha.Length),
                    _ => throw new IndexOutOfRangeException("Index was out of range. Must be non-negative and less than the size of the collection.")
                };

                byte GetByte(uint value) => (index % 4) switch
                {
                    0 => (byte)value,
                    1 => (byte)(value >> 8),
                    2 => (byte)(value >> 16),
                    3 => (byte)(value >> 24),
                    _ => throw new InvalidOperationException()
                };
            }
        }

        /// <summary>
        /// Creates a new <see cref="FramebufferHeader"/> object based on a byte array which contains the data.
        /// </summary>
        /// <param name="data">The data that feeds the <see cref="FramebufferHeader"/> struct.</param>
        /// <returns>A new <see cref="FramebufferHeader"/> object.</returns>
        public static FramebufferHeader Read(byte[] data) => new(data);

#if HAS_BUFFERS
        /// <summary>
        /// Creates a new <see cref="FramebufferHeader"/> object based on a byte array which contains the data.
        /// </summary>
        /// <param name="data">The data that feeds the <see cref="FramebufferHeader"/> struct.</param>
        /// <returns>A new <see cref="FramebufferHeader"/> object.</returns>
        public static FramebufferHeader Read(ReadOnlySpan<byte> data) => new(data);
#endif

#if HAS_IMAGING
        /// <summary>
        /// Converts a <see cref="byte"/> array containing the raw frame buffer data to a <see cref="Bitmap"/>.
        /// </summary>
        /// <param name="buffer">The buffer containing the image data.</param>
        /// <returns>A <see cref="Bitmap"/> that represents the image contained in the frame buffer, or <see langword="null"/>
        /// if the framebuffer does not contain any data. This can happen when DRM is enabled on the device.</returns>
#if NET
        [SupportedOSPlatform("windows6.1")]
#endif
        public Bitmap? ToImage(byte[] buffer)
        {
            ExceptionExtensions.ThrowIfNull(buffer);

            // This happens, for example, when DRM is enabled. In that scenario, no screenshot is taken on the device and an empty
            // framebuffer is returned; we'll just return null.
            if (Width == 0 || Height == 0 || Bpp == 0)
            {
                return null;
            }

            // The pixel format of the framebuffer may not be one that .NET recognizes, so we need to fix that
            PixelFormat pixelFormat = StandardizePixelFormat(ref buffer);

            Bitmap bitmap = new((int)Width, (int)Height, pixelFormat);
            BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, pixelFormat);
            Marshal.Copy(buffer, 0, bitmapData.Scan0, (int)Size);
            bitmap.UnlockBits(bitmapData);

            return bitmap;
        }

        /// <summary>
        /// Returns the <see cref="PixelFormat"/> that describes pixel format of an image that is stored according to the information
        /// present in this <see cref="FramebufferHeader"/>. Because the <see cref="PixelFormat"/> enumeration does not allow for all
        /// formats supported by Android, this method also takes a <paramref name="buffer"/> and reorganizes the bytes in the buffer to
        /// match the return value of this function.
        /// </summary>
        /// <param name="buffer">A byte array in which the images are stored according to this <see cref="FramebufferHeader"/>.</param>
        /// <returns>A <see cref="PixelFormat"/> that describes how the image data is represented in this <paramref name="buffer"/>.</returns>
#if NET
        [SupportedOSPlatform("windows6.1")]
#endif
        private PixelFormat StandardizePixelFormat(ref byte[] buffer)
        {
            // Initial parameter validation.
            ExceptionExtensions.ThrowIfNull(buffer);

            if (buffer.Length < Width * Height * (Bpp / 8))
            {
                throw new ArgumentOutOfRangeException(nameof(buffer), $"The buffer length {buffer.Length} is less than expected buffer " +
                    $"length ({Width * Height * (Bpp / 8)}) for a picture of width {Width}, height {Height} and pixel depth {Bpp}");
            }

            if (Width == 0 || Height == 0 || Bpp == 0)
            {
                throw new InvalidOperationException("Cannot cannulate the pixel format of an empty framebuffer");
            }

            // By far, the most common format is a 32-bit pixel format, which is either
            // RGB or RGBA, where each color has 1 byte.
            if (Bpp == 8 * 4)
            {
                // Require at least RGB to be present; and require them to be exactly one byte (8 bits) long.
                if (Red.Length != 8 || Blue.Length != 8 || Green.Length != 8)
                {
                    throw new ArgumentOutOfRangeException($"The pixel format with with RGB lengths of {Red.Length}:{Blue.Length}:{Green.Length} is not supported");
                }

                // Alpha can be present or absent, but must be 8 bytes long
                if (Alpha.Length is not (0 or 8))
                {
                    throw new ArgumentOutOfRangeException($"The alpha length {Alpha.Length} is not supported");
                }

                // Gets the index at which the red, bue, green and alpha values are stored.
                int redIndex = (int)Red.Offset / 8;
                int blueIndex = (int)Blue.Offset / 8;
                int greenIndex = (int)Green.Offset / 8;
                int alphaIndex = (int)Alpha.Offset / 8;
                
                byte[] array = new byte[(int)Size * 4];
                // Loop over the array and re-order as required
                for (int i = 0; i < (int)Size; i += 4)
                {
                    byte red = buffer[i + redIndex];
                    byte blue = buffer[i + blueIndex];
                    byte green = buffer[i + greenIndex];
                    byte alpha = buffer[i + alphaIndex];

                    // Convert to ARGB. Note, we're on a little endian system,
                    // so it's really BGRA. Confusing!
                    if (Alpha.Length == 8)
                    {
                        array[i + 3] = alpha;
                        array[i + 2] = red;
                        array[i + 1] = green;
                        array[i + 0] = blue;
                    }
                    else
                    {
                        array[i + 3] = 0;
                        array[i + 2] = red;
                        array[i + 1] = green;
                        array[i + 0] = blue;
                    }
                }
                buffer = array;

                // Returns RGB or RGBA, function of the presence of an alpha channel.
                return Alpha.Length == 0 ? PixelFormat.Format32bppRgb : PixelFormat.Format32bppArgb;
            }
            else if (Bpp == 8 * 3)
            {
                // For 24-bit image depths, we only support RGB.
                if (Red.Offset == 0
                    && Red.Length == 8
                    && Green.Offset == 8
                    && Green.Length == 8
                    && Blue.Offset == 16
                    && Blue.Length == 8
                    && Alpha.Offset == 24
                    && Alpha.Length == 0)
                {
                    return PixelFormat.Format24bppRgb;
                }
            }
            else if (Bpp == 5 + 6 + 5
                     && Red.Offset == 11
                     && Red.Length == 5
                     && Green.Offset == 5
                     && Green.Length == 6
                     && Blue.Offset == 0
                     && Blue.Length == 5
                     && Alpha.Offset == 0
                     && Alpha.Length == 0)
            {
                // For 16-bit image depths, we only support Rgb565.
                return PixelFormat.Format16bppRgb565;
            }

            // If not caught by any of the statements before, the format is not supported.
            throw new NotSupportedException($"Pixel depths of {Bpp} are not supported");
        }
#endif

#if WINDOWS_UWP
        /// <summary>
        /// Converts a <see cref="byte"/> array containing the raw frame buffer data to a <see cref="WriteableBitmap"/>.
        /// </summary>
        /// <param name="buffer">The buffer containing the image data.</param>
        /// <param name="dispatcher">The target <see cref="CoreDispatcher"/> to invoke the code on.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous task.</param>
        /// <returns>A <see cref="WriteableBitmap"/> that represents the image contained in the frame buffer, or <see langword="null"/>
        /// if the framebuffer does not contain any data. This can happen when DRM is enabled on the device.</returns>
        [ContractVersion(typeof(UniversalApiContract), 65536u)]
        public Task<WriteableBitmap?> ToBitmapAsync(byte[] buffer, CoreDispatcher dispatcher, CancellationToken cancellationToken = default)
        {
            if (dispatcher.HasThreadAccess)
            {
                return ToBitmapAsync(buffer, cancellationToken);
            }
            else
            {
                FramebufferHeader self = this;
                TaskCompletionSource<WriteableBitmap?> taskCompletionSource = new();
                _ = dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    try
                    {
                        WriteableBitmap? result = await self.ToBitmapAsync(buffer, cancellationToken).ConfigureAwait(false);
                        _ = taskCompletionSource.TrySetResult(result);
                    }
                    catch (Exception e)
                    {
                        _ = taskCompletionSource.TrySetException(e);
                    }
                });
                return taskCompletionSource.Task;
            }
        }

        /// <summary>
        /// Converts a <see cref="byte"/> array containing the raw frame buffer data to a <see cref="WriteableBitmap"/>.
        /// </summary>
        /// <param name="buffer">The buffer containing the image data.</param>
        /// <param name="dispatcher">The target <see cref="DispatcherQueue"/> to invoke the code on.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous task.</param>
        /// <returns>A <see cref="WriteableBitmap"/> that represents the image contained in the frame buffer, or <see langword="null"/>
        /// if the framebuffer does not contain any data. This can happen when DRM is enabled on the device.</returns>
        [ContractVersion(typeof(UniversalApiContract), 327680u)]
        public Task<WriteableBitmap?> ToBitmapAsync(byte[] buffer, DispatcherQueue dispatcher, CancellationToken cancellationToken = default)
        {
            if (ApiInformation.IsMethodPresent("Windows.System.DispatcherQueue", "HasThreadAccess") && dispatcher.HasThreadAccess)
            {
                return ToBitmapAsync(buffer, cancellationToken);
            }
            else
            {
                FramebufferHeader self = this;
                TaskCompletionSource<WriteableBitmap?> taskCompletionSource = new();
                if (!dispatcher.TryEnqueue(async () =>
                {
                    try
                    {
                        WriteableBitmap? result = await self.ToBitmapAsync(buffer, cancellationToken).ConfigureAwait(false);
                        _ = taskCompletionSource.TrySetResult(result);
                    }
                    catch (Exception e)
                    {
                        _ = taskCompletionSource.TrySetException(e);
                    }
                }))
                {
                    _ = taskCompletionSource.TrySetException(new InvalidOperationException("Failed to enqueue the operation"));
                }
                return taskCompletionSource.Task;
            }
        }

        /// <summary>
        /// Converts a <see cref="byte"/> array containing the raw frame buffer data to a <see cref="WriteableBitmap"/>.
        /// </summary>
        /// <param name="buffer">The buffer containing the image data.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> which can be used to cancel the asynchronous task.</param>
        /// <returns>A <see cref="WriteableBitmap"/> that represents the image contained in the frame buffer, or <see langword="null"/>
        /// if the framebuffer does not contain any data. This can happen when DRM is enabled on the device.</returns>
        [ContractVersion(typeof(UniversalApiContract), 65536u)]
        public async Task<WriteableBitmap?> ToBitmapAsync(byte[] buffer, CancellationToken cancellationToken = default)
        {
            ExceptionExtensions.ThrowIfNull(buffer);

            // This happens, for example, when DRM is enabled. In that scenario, no screenshot is taken on the device and an empty
            // framebuffer is returned; we'll just return null.
            if (Width == 0 || Height == 0 || Bpp == 0)
            {
                return null;
            }

            // The pixel format of the framebuffer may not be one that WinRT recognizes, so we need to fix that
            BitmapPixelFormat bitmapPixelFormat = StandardizePixelFormat(buffer, out BitmapAlphaMode alphaMode);

            using InMemoryRandomAccessStream random = new();
            BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, random).AsTask(cancellationToken);
            encoder.SetPixelData(bitmapPixelFormat, alphaMode, Width, Height, 96, 96, buffer);
            await encoder.FlushAsync().AsTask(cancellationToken);
            WriteableBitmap WriteableImage = new((int)Width, (int)Height);
            await WriteableImage.SetSourceAsync(random).AsTask(cancellationToken).ConfigureAwait(false);

            return WriteableImage;
        }

        /// <summary>
        /// Returns the <see cref="BitmapPixelFormat"/> that describes pixel format of an image that is stored according to the information
        /// present in this <see cref="FramebufferHeader"/>. Because the <see cref="BitmapPixelFormat"/> enumeration does not allow for all
        /// formats supported by Android, this method also takes a <paramref name="buffer"/> and reorganizes the bytes in the buffer to
        /// match the return value of this function.
        /// </summary>
        /// <param name="buffer">A byte array in which the images are stored according to this <see cref="FramebufferHeader"/>.</param>
        /// <param name="alphaMode">A <see cref="BitmapAlphaMode"/> which describes how the alpha channel is stored.</param>
        /// <returns>A <see cref="BitmapPixelFormat"/> that describes how the image data is represented in this <paramref name="buffer"/>.</returns>
        [ContractVersion(typeof(UniversalApiContract), 65536u)]
        private BitmapPixelFormat StandardizePixelFormat(byte[] buffer, out BitmapAlphaMode alphaMode)
        {
            // Initial parameter validation.
            ExceptionExtensions.ThrowIfNull(buffer);

            if (buffer.Length < Width * Height * (Bpp / 8))
            {
                throw new ArgumentOutOfRangeException(nameof(buffer), $"The buffer length {buffer.Length} is less than expected buffer " +
                    $"length ({Width * Height * (Bpp / 8)}) for a picture of width {Width}, height {Height} and pixel depth {Bpp}");
            }

            if (Width == 0 || Height == 0 || Bpp == 0)
            {
                throw new InvalidOperationException("Cannot cannulate the pixel format of an empty framebuffer");
            }

            // By far, the most common format is a 32-bit pixel format, which is either
            // RGB or RGBA, where each color has 1 byte.
            if (Bpp == 8 * 4)
            {
                // Require at least RGB to be present; and require them to be exactly one byte (8 bits) long.
                if (Red.Length != 8 || Blue.Length != 8 || Green.Length != 8)
                {
                    throw new ArgumentOutOfRangeException($"The pixel format with with RGB lengths of {Red.Length}:{Blue.Length}:{Green.Length} is not supported");
                }

                // Alpha can be present or absent, but must be 8 bytes long
                alphaMode = Alpha.Length switch
                {
                    0 => BitmapAlphaMode.Ignore,
                    8 => BitmapAlphaMode.Straight,
                    _ => throw new ArgumentOutOfRangeException($"The alpha length {Alpha.Length} is not supported"),
                };

                return BitmapPixelFormat.Rgba8;
            }

            // If not caught by any of the statements before, the format is not supported.
            throw new NotSupportedException($"Pixel depths of {Bpp} are not supported");
        }
#endif

        /// <inheritdoc/>
        public override string ToString()
        {
            StringBuilder builder =
                new StringBuilder(nameof(FramebufferHeader))
                    .Append(" { ")
                    .Append(nameof(Version))
                    .Append(" = ")
                    .Append(Version)
                    .Append(", ")
                    .Append(nameof(Bpp))
                    .Append(" = ")
                    .Append(Bpp);

            if (Version >= 2)
            {
                _ = builder
                    .Append(", ")
                    .Append(nameof(ColorSpace))
                    .Append(" = ")
                    .Append(ColorSpace);
            }

            return builder
                .Append(", ")
                .Append(nameof(Size))
                .Append(" = ")
                .Append(Size)
                .Append(", ")
                .Append(nameof(Width))
                .Append(" = ")
                .Append(Width)
                .Append(", ")
                .Append(nameof(Height))
                .Append(" = ")
                .Append(Height)
                .Append(", ")
                .Append(nameof(Red))
                .Append(" = ")
                .Append(Red)
                .Append(", ")
                .Append(nameof(Blue))
                .Append(" = ")
                .Append(Blue)
                .Append(", ")
                .Append(nameof(Green))
                .Append(" = ")
                .Append(Green)
                .Append(", ")
                .Append(nameof(Alpha))
                .Append(" = ")
                .Append(Alpha)
                .Append(" }")
                .ToString();
        }

        /// <inheritdoc/>
        public IEnumerator<byte> GetEnumerator()
        {
            foreach (uint value in GetEnumerable())
            {
                yield return (byte)value;
                yield return (byte)(value >> 8);
                yield return (byte)(value >> 16);
                yield return (byte)(value >> 24);
            }
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Gets an <see cref="IEnumerable{T}"/> which enumerates the values of this <see cref="FramebufferHeader"/>.
        /// </summary>
        /// <returns>The values of the <see cref="FramebufferHeader"/>.</returns>
        private IEnumerable<uint> GetEnumerable()
        {
            yield return Version;

            yield return Bpp;

            if (Version >= 2)
            {
                yield return ColorSpace;
            }

            yield return Size;
            yield return Width;
            yield return Height;

            yield return Red.Offset;
            yield return Red.Length;

            yield return Blue.Offset;
            yield return Blue.Length;

            yield return Green.Offset;
            yield return Green.Length;

            yield return Alpha.Offset;
            yield return Alpha.Length;
        }
    }
}
