// <copyright file="ColorData.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
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
    /// <para>Contains information about a color element. Information about each pixel on the screen
    /// is contained in a byte array (for example, a 4-byte array for 32-bit color depths), and
    /// a certain number of bits are reserved for each color.</para>
    /// <para>For example, in a 24-bit RGB structure, the first byte contains the red color,
    /// the next byte the green color and the last byte the blue color.</para>
    /// </summary>
    /// <param name="Offset">The offset, in bits, within the byte array for a pixel, at which the
    /// bytes that contain information for this color are stored.</param>
    /// <param name="Length">The number of bits that contain information for this color.</param>
#if HAS_BUFFERS
    [CollectionBuilder(typeof(EnumerableBuilder), nameof(EnumerableBuilder.ColorDataCreator))]
#endif
    [DebuggerDisplay($"{{{nameof(ToString)}(),nq}}")]
    public readonly record struct ColorData(uint Offset, uint Length) : IReadOnlyList<byte>
#if NET7_0_OR_GREATER
        , IEqualityOperators<ColorData, ColorData, bool>
#endif
    {
        /// <summary>
        /// The length of <see cref="ColorData"/> in bytes.
        /// </summary>
        public const int Size = 2 * sizeof(uint);

        /// <summary>
        /// Gets or sets the offset, in bits, within the byte array for a pixel, at which the
        /// bytes that contain information for this color are stored.
        /// </summary>
        public uint Offset { get; init; } = Offset;

        /// <summary>
        /// Gets or sets the number of bits that contain information for this color.
        /// </summary>
        public uint Length { get; init; } = Length;

        /// <summary>
        /// Gets the length of <see cref="ColorData"/> in bytes.
        /// </summary>
        readonly int IReadOnlyCollection<byte>.Count => Size;

        /// <inheritdoc/>
        public readonly byte this[int index] =>
            index is < 0 or >= Size
                ? throw new IndexOutOfRangeException("Index was out of range. Must be non-negative and less than the size of the collection.")
                : index switch
                {
                    0 => (byte)Offset,
                    1 => (byte)(Offset >> 8),
                    2 => (byte)(Offset >> 16),
                    3 => (byte)(Offset >> 24),

                    4 => (byte)Length,
                    5 => (byte)(Length >> 8),
                    6 => (byte)(Length >> 16),
                    7 => (byte)(Length >> 24),

                    _ => throw new IndexOutOfRangeException("Index was out of range. Must be non-negative and less than the size of the collection.")
                };

        /// <summary>
        /// Deconstruct the <see cref="ColorData"/> struct.
        /// </summary>
        /// <param name="offset">The offset, in bits, within the byte array for a pixel, at which the
        /// bytes that contain information for this color are stored.</param>
        /// <param name="length">The number of bits that contain information for this color.</param>
        public readonly void Deconstruct(out uint offset, out uint length)
        {
            offset = Offset;
            length = Length;
        }

        /// <inheritdoc/>
        public readonly IEnumerator<byte> GetEnumerator()
        {
            yield return (byte)Offset;
            yield return (byte)(Offset >> 8);
            yield return (byte)(Offset >> 16);
            yield return (byte)(Offset >> 24);

            yield return (byte)Length;
            yield return (byte)(Length >> 8);
            yield return (byte)(Length >> 16);
            yield return (byte)(Length >> 24);
        }

        /// <inheritdoc/>
        readonly IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Returns a byte array containing the binary representation of this instance.
        /// </summary>
        /// <returns>A byte array that represents the contents of this instance. The length of the array is
        /// equal to the size of the structure in bytes.</returns>
        public byte[] ToArray()
        {
            ref readonly ColorData data = ref this;
            unsafe
            {
                byte[] array = new byte[Size];
                fixed (ColorData* pData = &data)
                {
                    Marshal.Copy((nint)pData, array, 0, Size);
                    return array;
                }
            }
        }

#if HAS_BUFFERS
        /// <summary>
        /// Returns a read-only span of bytes representing the contents of this instance.
        /// </summary>
        /// <returns>A <see cref="ReadOnlySpan{Byte}"/> that provides a read-only view of the bytes in this instance.</returns>
        public ReadOnlySpan<byte> AsSpan() => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<ColorData, byte>(ref Unsafe.AsRef(in this)), Size);
#endif
    }
}
