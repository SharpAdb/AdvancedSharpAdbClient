// <copyright file="FramebufferHeader.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace AdvancedSharpAdbClient.Models
{
    /// <summary>
    /// Whenever the <c>framebuffer:</c> service is invoked, the adb server responds with the contents
    /// of the framebuffer, prefixed with a <see cref="FramebufferHeader"/> object that contains more
    /// information about the framebuffer.
    /// </summary>
    /// <remarks>As defined in <see href="https://android.googlesource.com/platform/system/core/+/master/adb/framebuffer_service.cpp"/></remarks>
#if HAS_BUFFERS
    [CollectionBuilder(typeof(EnumerableBuilder), nameof(EnumerableBuilder.FramebufferHeaderCreator))]
#endif
    [DebuggerDisplay($"{{{nameof(ToString)}(),nq}}")]
    public readonly struct FramebufferHeader : IReadOnlyList<byte>
    {
        /// <summary>
        /// The length of the head when <see cref="Version"/> is <see langword="1"/>.
        /// </summary>
        public const int MinLength = 52;

        /// <summary>
        /// The length of the head when <see cref="Version"/> is <see langword="2"/>.
        /// </summary>
        public const int MaxLength = 56;

        /// <summary>
        /// Initializes a new instance of the <see cref="FramebufferHeader"/> struct based on a byte array which contains the data.
        /// </summary>
        /// <param name="data">The data that feeds the <see cref="FramebufferHeader"/> struct.</param>
        public FramebufferHeader(byte[] data)
        {
            byte[] source;
            switch (data)
            {
                // Technically, 0 is not a supported version either; we assume version 0 indicates
                // an empty framebuffer.
                case { Length: < MinLength or > MaxLength }:
                    throw new ArgumentOutOfRangeException(nameof(data), $"The length of {nameof(data)} must be {MinLength} or {MaxLength}.");
                case [> 2, ..]:
                    throw new InvalidOperationException($"Framebuffer version {Version} is not supported");
                case [2, ..]:
                    goto default;
                case [< 2, ..]:
                    source = new byte[MaxLength];
                    Array.Copy(data, 0, source, 0, 2 * sizeof(uint));
                    Array.Copy(data, 2 * sizeof(uint), source, (2 * sizeof(uint)) + 4, MinLength - (2 * sizeof(uint)));
                    break;
                default:
                    source = data;
                    break;
            }
            this = Unsafe.As<byte, FramebufferHeader>(ref source[0]);
        }

#if HAS_BUFFERS
        /// <summary>
        /// Initializes a new instance of the <see cref="FramebufferHeader"/> struct based on a byte array which contains the data.
        /// </summary>
        /// <param name="data">The data that feeds the <see cref="FramebufferHeader"/> struct.</param>
        [OverloadResolutionPriority(1)]
        public FramebufferHeader(scoped in ReadOnlySpan<byte> data)
        {
            scoped ReadOnlySpan<byte> source = data switch
            {
                { Length: < MinLength or > MaxLength } => throw new ArgumentOutOfRangeException(nameof(data), $"The length of {nameof(data)} must be {MinLength} or {MaxLength}."),
                // Technically, 0 is not a supported version either; we assume version 0 indicates
                // an empty framebuffer.
                [> 2, ..] => throw new InvalidOperationException($"Framebuffer version {Version} is not supported"),
                [2, ..] => data,
                [< 2, ..] => [.. data[..(2 * sizeof(uint))], 0, 0, 0, 0, .. data[(2 * sizeof(uint))..]]
            };
            this = Unsafe.As<byte, FramebufferHeader>(ref MemoryMarshal.GetReference(source));
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
        public int Count => Version < 2 ? MinLength : MaxLength;

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
        [OverloadResolutionPriority(1)]
        public static FramebufferHeader Read(scoped ReadOnlySpan<byte> data) => new(data);
#endif

        /// <inheritdoc/>
        public override string ToString()
        {
            scoped DefaultInterpolatedStringHandler handler = new(121, 10);
            handler.AppendFormatted(GetType());
            handler.AppendLiteral($" {{ {nameof(Version)} = ");
            handler.AppendFormatted(Version);
            handler.AppendLiteral($", {nameof(Bpp)} = ");
            handler.AppendFormatted(Bpp);

            if (Version >= 2)
            {
                handler.AppendLiteral($", {nameof(ColorSpace)} = ");
                handler.AppendFormatted(ColorSpace);
            }

            handler.AppendLiteral($", {nameof(Size)} = ");
            handler.AppendFormatted(Size);
            handler.AppendLiteral($", {nameof(Width)} = ");
            handler.AppendFormatted(Width);
            handler.AppendLiteral($", {nameof(Height)} = ");
            handler.AppendFormatted(Height);
            handler.AppendLiteral($", {nameof(Red)} = ");
            handler.AppendFormatted(Red);
            handler.AppendLiteral($", {nameof(Blue)} = ");
            handler.AppendFormatted(Blue);
            handler.AppendLiteral($", {nameof(Green)} = ");
            handler.AppendFormatted(Green);
            handler.AppendLiteral($", {nameof(Alpha)} = ");
            handler.AppendFormatted(Alpha);
            handler.AppendLiteral(" }");
            return handler.ToStringAndClear();
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
