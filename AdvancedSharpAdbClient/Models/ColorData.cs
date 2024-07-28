// <copyright file="ColorData.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

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
    {
        /// <summary>
        /// The length of <see cref="ColorData"/> in bytes.
        /// </summary>
        private const int count = 8;

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
        public readonly int Count => count;

        /// <inheritdoc/>
        public readonly byte this[int index] =>
            index is < 0 or >= count
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
        /// Provides a string representation of the <see cref="ColorData"/> struct.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder"/> to append the string representation to.</param>
        /// <returns><see langword="true"/> if the members were appended to the <paramref name="builder"/>; otherwise, <see langword="false"/>.</returns>
        private bool PrintMembers(StringBuilder builder)
        {
            _ = builder.Append("Offset = ")
                       .Append(Offset)
                       .Append(", Length = ")
                       .Append(Length);
            return true;
        }

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
    }
}
