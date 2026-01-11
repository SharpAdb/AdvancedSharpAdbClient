// <copyright file="FileStatisticsData.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
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
    /// Contains information about a file on the remote device.
    /// </summary>
    /// <param name="Mode">The mode of the file.</param>
    /// <param name="Size">The total file size, in bytes.</param>
    /// <param name="Time">The time of last modification.</param>
    /// <remarks><see href="https://android.googlesource.com/platform/packages/modules/adb/+/refs/heads/main/file_sync_protocol.h"/></remarks>
#if HAS_BUFFERS
    [CollectionBuilder(typeof(EnumerableBuilder), nameof(EnumerableBuilder.FileStatisticsDataCreator))]
#endif
    [DebuggerDisplay($"{{{nameof(ToString)}(),nq}}")]
    public readonly record struct FileStatisticsData(uint Mode, uint Size, uint Time) : IReadOnlyList<byte>
#if NET7_0_OR_GREATER
        , IEqualityOperators<FileStatisticsData, FileStatisticsData, bool>
#endif
    {
        /// <summary>
        /// The length of <see cref="FileStatisticsData"/> in bytes.
        /// </summary>
        public const int Length = 3 * sizeof(uint);

        /// <summary>
        /// Gets the mode of the file.
        /// </summary>
        public uint Mode { get; init; } = Mode;

        /// <summary>
        /// Gets the total file size, in bytes.
        /// </summary>
        public uint Size { get; init; } = Size;

        /// <summary>
        /// Gets the time of last modification.
        /// </summary>
        public uint Time { get; init; } = Time;

        /// <summary>
        /// Gets the length of <see cref="FileStatisticsData"/> in bytes.
        /// </summary>
        readonly int IReadOnlyCollection<byte>.Count => Length;

        /// <summary>
        /// Deconstruct the <see cref="FileStatisticsData"/> struct.
        /// </summary>
        /// <param name="mode">The mode of the file.</param>
        /// <param name="size">The total file size, in bytes.</param>
        /// <param name="time">The time of last modification.</param>
        public readonly void Deconstruct(out uint mode, out uint size, out uint time)
        {
            mode = Mode;
            size = Size;
            time = Time;
        }

        /// <inheritdoc/>
        public readonly byte this[int index] =>
            index is < 0 or >= Length
                ? throw new IndexOutOfRangeException("Index was out of range. Must be non-negative and less than the size of the collection.")
                : index switch
                {
                    0 => (byte)Mode,
                    1 => (byte)(Mode >> 8),
                    2 => (byte)(Mode >> 16),
                    3 => (byte)(Mode >> 24),

                    4 => (byte)Size,
                    5 => (byte)(Size >> 8),
                    6 => (byte)(Size >> 16),
                    7 => (byte)(Size >> 24),

                    8 => (byte)Time,
                    9 => (byte)(Time >> 8),
                    10 => (byte)(Time >> 16),
                    11 => (byte)(Time >> 24),

                    _ => throw new IndexOutOfRangeException("Index was out of range. Must be non-negative and less than the size of the collection.")
                };

        /// <inheritdoc/>
        public IEnumerator<byte> GetEnumerator()
        {
            yield return (byte)Mode;
            yield return (byte)(Mode >> 8);
            yield return (byte)(Mode >> 16);
            yield return (byte)(Mode >> 24);

            yield return (byte)Size;
            yield return (byte)(Size >> 8);
            yield return (byte)(Size >> 16);
            yield return (byte)(Size >> 24);

            yield return (byte)Time;
            yield return (byte)(Time >> 8);
            yield return (byte)(Time >> 16);
            yield return (byte)(Time >> 24);
        }

        /// <inheritdoc/>
        readonly IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Returns a byte array containing the binary representation of this instance.
        /// </summary>
        /// <returns>A byte array that represents the contents of this instance. The length of the array is
        /// equal to the size of the structure in bytes.</returns>
        public unsafe byte[] ToArray()
        {
            byte[] array = new byte[Length];
            fixed (FileStatisticsData* pData = &this)
            {
                Marshal.Copy((nint)pData, array, 0, Length);
                return array;
            }
        }

#if HAS_BUFFERS
        /// <summary>
        /// Returns a read-only span of bytes representing the contents of this instance.
        /// </summary>
        /// <returns>A <see cref="ReadOnlySpan{Byte}"/> that provides a read-only view of the bytes in this instance.</returns>
        public ReadOnlySpan<byte> AsSpan() => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<FileStatisticsData, byte>(ref Unsafe.AsRef(in this)), Length);
#endif
    }
}