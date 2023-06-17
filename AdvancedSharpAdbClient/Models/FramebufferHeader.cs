// <copyright file="FramebufferHeader.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using AdvancedSharpAdbClient.Exceptions;
using System;
using System.IO;
using System.Text;

namespace AdvancedSharpAdbClient
{
    /// <summary>
    /// Whenever the <c>framebuffer:</c> service is invoked, the adb server responds with the contents
    /// of the framebuffer, prefixed with a <see cref="FramebufferHeader"/> object that contains more
    /// information about the framebuffer.
    /// </summary>
    public struct FramebufferHeader
    {
        /// <summary>
        /// Gets or sets the version of the framebuffer struct.
        /// </summary>
        public uint Version { get; set; }

        /// <summary>
        /// Gets or sets the number of bytes per pixel. Usual values include 32 or 24.
        /// </summary>
        public uint Bpp { get; set; }

        /// <summary>
        /// Gets or sets the color space. Only available starting with <see cref="Version"/> 2.
        /// </summary>
        public uint ColorSpace { get; set; }

        /// <summary>
        /// Gets or sets the total size, in bits, of the framebuffer.
        /// </summary>
        public uint Size { get; set; }

        /// <summary>
        /// Gets or sets the width, in pixels, of the framebuffer.
        /// </summary>
        public uint Width { get; set; }

        /// <summary>
        /// Gets or sets the height, in pixels, of the framebuffer.
        /// </summary>
        public uint Height { get; set; }

        /// <summary>
        /// Gets or sets information about the red color channel.
        /// </summary>
        public ColorData Red { get; set; }

        /// <summary>
        /// Gets or sets information about the blue color channel.
        /// </summary>
        public ColorData Blue { get; set; }

        /// <summary>
        /// Gets or sets information about the green color channel.
        /// </summary>
        public ColorData Green { get; set; }

        /// <summary>
        /// Gets or sets information about the alpha channel.
        /// </summary>
        public ColorData Alpha { get; set; }

        /// <summary>
        /// Creates a new <see cref="FramebufferHeader"/> object based on a byte array which contains the data.
        /// </summary>
        /// <param name="data">The data that feeds the <see cref="FramebufferHeader"/> struct.</param>
        /// <returns>A new <see cref="FramebufferHeader"/> object.</returns>
        public static FramebufferHeader Read(byte[] data)
        {
            // as defined in https://android.googlesource.com/platform/system/core/+/master/adb/framebuffer_service.cpp
            FramebufferHeader header = default;

            // Read the data from a MemoryStream so we can use the BinaryReader to process the data.
            using MemoryStream stream = new(data);
            using BinaryReader reader = new(stream, Encoding.ASCII
#if !NETFRAMEWORK || NET45_OR_GREATER
                , leaveOpen: true
#endif
                );
            header.Version = reader.ReadUInt32();

            if (header.Version > 2)
            {
                // Technically, 0 is not a supported version either; we assume version 0 indicates
                // an empty framebuffer.
                throw new InvalidOperationException($"Framebuffer version {header.Version} is not supported");
            }

            header.Bpp = reader.ReadUInt32();

            if (header.Version >= 2)
            {
                header.ColorSpace = reader.ReadUInt32();
            }

            header.Size = reader.ReadUInt32();
            header.Width = reader.ReadUInt32();
            header.Height = reader.ReadUInt32();

            header.Red = new ColorData
            {
                Offset = reader.ReadUInt32(),
                Length = reader.ReadUInt32()
            };

            header.Blue = new ColorData
            {
                Offset = reader.ReadUInt32(),
                Length = reader.ReadUInt32()
            };

            header.Green = new ColorData
            {
                Offset = reader.ReadUInt32(),
                Length = reader.ReadUInt32()
            };

            header.Alpha = new ColorData
            {
                Offset = reader.ReadUInt32(),
                Length = reader.ReadUInt32()
            };

            return header;
        }

#if HAS_DRAWING
        /// <summary>
        /// Converts a <see cref="byte"/> array containing the raw frame buffer data to a <see cref="Bitmap"/>.
        /// </summary>
        /// <param name="buffer">The buffer containing the image data.</param>
        /// <returns>
        /// A <see cref="Bitmap"/> that represents the image contained in the frame buffer, or <see langword="null"/>
        /// if the framebuffer does not contain any data. This can happen when DRM is enabled on the device.
        /// </returns>
#if NET
        [SupportedOSPlatform("windows")]
#endif
        public readonly Bitmap ToImage(byte[] buffer)
        {
            ExceptionExtensions.ThrowIfNull(buffer);

            // This happens, for example, when DRM is enabled. In that scenario, no screenshot is taken on the device and an empty
            // framebuffer is returned; we'll just return null.
            if (Width == 0 || Height == 0 || Bpp == 0)
            {
                return null;
            }

            // The pixel format of the framebuffer may not be one that .NET recognizes, so we need to fix that
            PixelFormat pixelFormat = StandardizePixelFormat(buffer);

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
        [SupportedOSPlatform("windows")]
#endif
        private readonly PixelFormat StandardizePixelFormat(byte[] buffer)
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
                throw new InvalidOperationException("Cannot canulate the pixel format of an empty framebuffer");
            }

            // By far, the most common format is a 32-bit pixel format, which is either
            // RGB or RGBA, where each color has 1 byte.
            if (Bpp == 32)
            {
                // Require at least RGB to be present; and require them to be exactly one byte (8 bits) long.
                if (Red.Length != 8 || Blue.Length != 8 || Green.Length != 8)
                {
                    throw new ArgumentOutOfRangeException($"The pixel format with with RGB lengths of {Red.Length}:{Blue.Length}:{Green.Length} is not supported");
                }

                // Alpha can be present or absent, but must be 8 bytes long
                if (Alpha.Length is not 0 and not 8)
                {
                    throw new ArgumentOutOfRangeException($"The alpha length {Alpha.Length} is not supported");
                }

                // Get the index at which the red, bue, green and alpha values are stored.
                uint redIndex = Red.Offset / 8;
                uint blueIndex = Blue.Offset / 8;
                uint greenIndex = Green.Offset / 8;
                uint alphaIndex = Alpha.Offset / 8;

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
                        buffer[i + 3] = alpha;
                        buffer[i + 2] = red;
                        buffer[i + 1] = green;
                        buffer[i + 0] = blue;
                    }
                    else
                    {
                        buffer[i + 3] = red;
                        buffer[i + 2] = green;
                        buffer[i + 1] = blue;
                        buffer[i + 0] = 0;
                    }
                }

                // Return RGB or RGBA, function of the presence of an alpha channel.
                return Alpha.Length == 0 ? PixelFormat.Format32bppRgb : PixelFormat.Format32bppArgb;
            }
            else if (Bpp == 24)
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
            else if (Bpp == 16
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
        /// <returns>
        /// A <see cref="WriteableBitmap"/> that represents the image contained in the frame buffer, or <see langword="null"/>
        /// if the framebuffer does not contain any data. This can happen when DRM is enabled on the device.
        /// </returns>
        public readonly Task<WriteableBitmap> ToBitmap(byte[] buffer, CoreDispatcher dispatcher)
        {
            FramebufferHeader self = this;

            if (dispatcher.HasThreadAccess)
            {
                return ToBitmap(buffer);
            }
            else
            {
                TaskCompletionSource<WriteableBitmap> taskCompletionSource = new();

                _ = dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    try
                    {
                        taskCompletionSource.SetResult(await self.ToBitmap(buffer));
                    }
                    catch (Exception e)
                    {
                        taskCompletionSource.SetException(e);
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
        /// <returns>
        /// A <see cref="WriteableBitmap"/> that represents the image contained in the frame buffer, or <see langword="null"/>
        /// if the framebuffer does not contain any data. This can happen when DRM is enabled on the device.
        /// </returns>
        [ContractVersion(typeof(UniversalApiContract), 327680u)]
        public readonly Task<WriteableBitmap> ToBitmap(byte[] buffer, DispatcherQueue dispatcher)
        {
            FramebufferHeader self = this;

            if (ApiInformation.IsMethodPresent("Windows.System.DispatcherQueue", "HasThreadAccess") && dispatcher.HasThreadAccess)
            {
                return ToBitmap(buffer);
            }
            else
            {
                TaskCompletionSource<WriteableBitmap> taskCompletionSource = new();

                if (!dispatcher.TryEnqueue(async () =>
                {
                    try
                    {
                        taskCompletionSource.SetResult(await self.ToBitmap(buffer));
                    }
                    catch (Exception e)
                    {
                        taskCompletionSource.SetException(e);
                    }
                }))
                {
                    taskCompletionSource.SetException(new InvalidOperationException("Failed to enqueue the operation"));
                }

                return taskCompletionSource.Task;
            }
        }

        /// <summary>
        /// Converts a <see cref="byte"/> array containing the raw frame buffer data to a <see cref="WriteableBitmap"/>.
        /// </summary>
        /// <param name="buffer">The buffer containing the image data.</param>
        /// <returns>
        /// A <see cref="WriteableBitmap"/> that represents the image contained in the frame buffer, or <see langword="null"/>
        /// if the framebuffer does not contain any data. This can happen when DRM is enabled on the device.
        /// </returns>
        public readonly async Task<WriteableBitmap> ToBitmap(byte[] buffer)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            // This happens, for example, when DRM is enabled. In that scenario, no screenshot is taken on the device and an empty
            // framebuffer is returned; we'll just return null.
            if (Width == 0 || Height == 0 || Bpp == 0)
            {
                return null;
            }

            using MemoryStream stream = new(buffer);
            using IRandomAccessStream randomAccessStream = stream.AsRandomAccessStream();
            BitmapDecoder decoder = await BitmapDecoder.CreateAsync(randomAccessStream);
            SoftwareBitmap softwareBitmap = await decoder.GetSoftwareBitmapAsync();
            try
            {
                WriteableBitmap WriteableImage = new((int)decoder.PixelWidth, (int)decoder.PixelHeight);
                await WriteableImage.SetSourceAsync(randomAccessStream);
                return WriteableImage;
            }
            catch (Exception)
            {
                using InMemoryRandomAccessStream random = new();
                BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, random);
                encoder.SetSoftwareBitmap(softwareBitmap);
                await encoder.FlushAsync();
                WriteableBitmap WriteableImage = new((int)decoder.PixelWidth, (int)decoder.PixelHeight);
                await WriteableImage.SetSourceAsync(random);
                return WriteableImage;
            }
        }
#endif
    }
}
