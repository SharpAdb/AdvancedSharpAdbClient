#if HAS_IMAGING
// <copyright file="FramebufferHeader.Drawing.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace AdvancedSharpAdbClient.Models
{
    /// <summary>
    /// Provides extension methods for <see cref="FramebufferHeader"/> that allow to convert the raw data contained in a framebuffer to a <see cref="Bitmap"/>.
    /// </summary>
    public static class FramebufferHeaderDrawing
    {
        /// <summary>
        /// Converts a <see cref="byte"/> array containing the raw frame buffer data to a <see cref="Bitmap"/>.
        /// </summary>
        /// <param name="header">The header of the frame buffer, which contains information about how the image data is stored in the <paramref name="buffer"/>.</param>
        /// <param name="buffer">The buffer containing the image data.</param>
        /// <returns>A <see cref="Bitmap"/> that represents the image contained in the frame buffer, or <see langword="null"/>
        /// if the framebuffer does not contain any data. This can happen when DRM is enabled on the device.</returns>
#if NET
        [SupportedOSPlatform("windows6.1")]
#endif
        public static Bitmap? ToImage(this scoped in FramebufferHeader header, byte[] buffer)
        {
            ArgumentNullException.ThrowIfNull(buffer);

            // This happens, for example, when DRM is enabled. In that scenario, no screenshot is taken on the device and an empty
            // framebuffer is returned; we'll just return null.
            if (header.Width == 0 || header.Height == 0 || header.Bpp == 0)
            {
                return null;
            }

            // The pixel format of the framebuffer may not be one that .NET recognizes, so we need to fix that
            PixelFormat pixelFormat = header.StandardizePixelFormat(ref buffer);

            Bitmap bitmap = new((int)header.Width, (int)header.Height, pixelFormat);
            BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, pixelFormat);
            Marshal.Copy(buffer, 0, bitmapData.Scan0, (int)header.Size);
            bitmap.UnlockBits(bitmapData);

            return bitmap;
        }

        /// <summary>
        /// Returns the <see cref="PixelFormat"/> that describes pixel format of an image that is stored according to the information
        /// present in this <see cref="FramebufferHeader"/>. Because the <see cref="PixelFormat"/> enumeration does not allow for all
        /// formats supported by Android, this method also takes a <paramref name="buffer"/> and reorganizes the bytes in the buffer to
        /// match the return value of this function.
        /// </summary>
        /// <param name="header">The header of the frame buffer, which contains information about how the image data is stored in the <paramref name="buffer"/>.</param>
        /// <param name="buffer">A byte array in which the images are stored according to this <see cref="FramebufferHeader"/>.</param>
        /// <returns>A <see cref="PixelFormat"/> that describes how the image data is represented in this <paramref name="buffer"/>.</returns>
#if NET
        [SupportedOSPlatform("windows6.1")]
#endif
        private static PixelFormat StandardizePixelFormat(this scoped in FramebufferHeader header, ref byte[] buffer)
        {
            // Initial parameter validation.
            ArgumentNullException.ThrowIfNull(buffer);

            if (buffer.Length < header.Width * header.Height * (header.Bpp / 8))
            {
                throw new ArgumentOutOfRangeException(nameof(buffer), $"The buffer length {buffer.Length} is less than expected buffer " +
                    $"length ({header.Width * header.Height * (header.Bpp / 8)}) for a picture of width {header.Width}, height {header.Height} and pixel depth {header.Bpp}");
            }

            if (header.Width == 0 || header.Height == 0 || header.Bpp == 0)
            {
                throw new InvalidOperationException("Cannot cannulate the pixel format of an empty framebuffer");
            }

            // By far, the most common format is a 32-bit pixel format, which is either
            // RGB or RGBA, where each color has 1 byte.
            if (header.Bpp == 8 * 4)
            {
                // Require at least RGB to be present; and require them to be exactly one byte (8 bits) long.
                if (header.Red.Length != 8 || header.Blue.Length != 8 || header.Green.Length != 8)
                {
                    throw new ArgumentOutOfRangeException($"The pixel format with with RGB lengths of {header.Red.Length}:{header.Blue.Length}:{header.Green.Length} is not supported");
                }

                // Alpha can be present or absent, but must be 8 bytes long
                if (header.Alpha.Length is not (0 or 8))
                {
                    throw new ArgumentOutOfRangeException($"The alpha length {header.Alpha.Length} is not supported");
                }

                // Gets the index at which the red, bue, green and alpha values are stored.
                int redIndex = (int)header.Red.Offset / 8;
                int blueIndex = (int)header.Blue.Offset / 8;
                int greenIndex = (int)header.Green.Offset / 8;
                int alphaIndex = (int)header.Alpha.Offset / 8;

                byte[] array = new byte[(int)header.Size * 4];
                // Loop over the array and re-order as required
                for (int i = 0; i < (int)header.Size; i += 4)
                {
                    byte red = buffer[i + redIndex];
                    byte blue = buffer[i + blueIndex];
                    byte green = buffer[i + greenIndex];
                    byte alpha = buffer[i + alphaIndex];

                    // Convert to ARGB. Note, we're on a little endian system,
                    // so it's really BGRA. Confusing!
                    if (header.Alpha.Length == 8)
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
                return header.Alpha.Length == 0 ? PixelFormat.Format32bppRgb : PixelFormat.Format32bppArgb;
            }
            else if (header.Bpp == 8 * 3)
            {
                // For 24-bit image depths, we only support RGB.
                if (header.Red.Offset == 0
                    && header.Red.Length == 8
                    && header.Green.Offset == 8
                    && header.Green.Length == 8
                    && header.Blue.Offset == 16
                    && header.Blue.Length == 8
                    && header.Alpha.Offset == 24
                    && header.Alpha.Length == 0)
                {
                    return PixelFormat.Format24bppRgb;
                }
            }
            else if (header.Bpp == 5 + 6 + 5
                     && header.Red.Offset == 11
                     && header.Red.Length == 5
                     && header.Green.Offset == 5
                     && header.Green.Length == 6
                     && header.Blue.Offset == 0
                     && header.Blue.Length == 5
                     && header.Alpha.Offset == 0
                     && header.Alpha.Length == 0)
            {
                // For 16-bit image depths, we only support Rgb565.
                return PixelFormat.Format16bppRgb565;
            }

            // If not caught by any of the statements before, the format is not supported.
            throw new NotSupportedException($"Pixel depths of {header.Bpp} are not supported");
        }
    }
}
#endif