// <copyright file="FileStatisticsData.V2.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace AdvancedSharpAdbClient.Models
{
    /// <summary>
    /// Contains information about a file on the remote device (v2).
    /// </summary>
    /// <param name="Error">The error code associated with the current operation.</param>
    /// <param name="Device">The device identifier associated with this instance.</param>
    /// <param name="IndexNode">The index node identifier associated with this entry.</param>
    /// <param name="Mode">The file mode value represented as an unsigned integer.</param>
    /// <param name="LinkCount">The number of hard links to the file or directory.</param>
    /// <param name="UserId">The unique identifier of the user associated with this instance.</param>
    /// <param name="GroupId">The unique identifier for the group.</param>
    /// <param name="Size">The size, in bytes, of the associated resource.</param>
    /// <param name="AccessTime">The last access time, in Unix timestamp format, for the associated resource.</param>
    /// <param name="ModifiedTime">The timestamp indicating when the item was last modified.</param>
    /// <param name="ChangedTime">The timestamp indicating when the last change occurred.</param>
    /// <remarks><see href="https://android.googlesource.com/platform/packages/modules/adb/+/refs/heads/main/file_sync_protocol.h"/></remarks>
#if HAS_BUFFERS
    [CollectionBuilder(typeof(EnumerableBuilder), nameof(EnumerableBuilder.FileStatisticsDataV2Creator))]
#endif
    [DebuggerDisplay($"{{{nameof(ToString)}(),nq}}")]
    [StructLayout(LayoutKind.Explicit)]
    public readonly record struct FileStatisticsDataEx(uint Error, ulong Device, ulong IndexNode, uint Mode, uint LinkCount, uint UserId, uint GroupId, ulong Size, long AccessTime, long ModifiedTime, long ChangedTime)
#if NET7_0_OR_GREATER
        : IEqualityOperators<FileStatisticsDataEx, FileStatisticsDataEx, bool>
#endif
    {
        /// <summary>
        /// The length of <see cref="FileStatisticsDataEx"/> in bytes.
        /// </summary>
        public const int Length = (5 * sizeof(uint)) + (3 * sizeof(ulong)) + (3 * sizeof(long));

        /// <summary>
        /// Gets the error code associated with the current operation.
        /// </summary>
        [field: FieldOffset(0)]
        public uint Error { get; init; } = Error;

        /// <summary>
        /// Gets the device identifier associated with this instance.
        /// </summary>
        [field: FieldOffset(sizeof(uint))]
        public ulong Device { get; init; } = Device;

        /// <summary>
        /// Gets the index node identifier associated with this entry.
        /// </summary>
        [field: FieldOffset(sizeof(uint) + sizeof(ulong))]
        public ulong IndexNode { get; init; } = IndexNode;

        /// <summary>
        /// Gets the file mode value represented as an unsigned integer.
        /// </summary>
        [field: FieldOffset(sizeof(uint) + (2 * sizeof(ulong)))]
        public uint Mode { get; init; } = Mode;

        /// <summary>
        /// Gets the number of hard links to the file or directory.
        /// </summary>
        [field: FieldOffset((2 * sizeof(uint)) + (2 * sizeof(ulong)))]
        public uint LinkCount { get; init; } = LinkCount;

        /// <summary>
        /// Gets the unique identifier of the user associated with this instance.
        /// </summary>
        [field: FieldOffset((3 * sizeof(uint)) + (2 * sizeof(ulong)))]
        public uint UserId { get; init; } = UserId;

        /// <summary>
        /// Gets the unique identifier for the group.
        /// </summary>
        [field: FieldOffset((4 * sizeof(uint)) + (2 * sizeof(ulong)))]
        public uint GroupId { get; init; } = GroupId;

        /// <summary>
        /// Gets the size, in bytes.
        /// </summary>
        [field: FieldOffset((5 * sizeof(uint)) + (2 * sizeof(ulong)))]
        public ulong Size { get; init; } = Size;

        /// <summary>
        /// Gets the last access time, in Unix timestamp format.
        /// </summary>
        [field: FieldOffset((5 * sizeof(uint)) + (3 * sizeof(ulong)))]
        public long AccessTime { get; init; } = AccessTime;

        /// <summary>
        /// Gets the timestamp indicating when the item was last modified.
        /// </summary>
        [field: FieldOffset((5 * sizeof(uint)) + (2 * sizeof(ulong)) + sizeof(ulong))]
        public long ModifiedTime { get; init; } = ModifiedTime;

        /// <summary>
        /// Gets the timestamp indicating when the last change occurred.
        /// </summary>
        [field: FieldOffset((5 * sizeof(uint)) + (2 * sizeof(ulong)) + (2 * sizeof(ulong)))]
        public long ChangedTime { get; init; } = ChangedTime;

        /// <summary>
        /// Returns a byte array containing the binary representation of this instance.
        /// </summary>
        /// <returns>A byte array that represents the contents of this instance. The length of the array is
        /// equal to the size of the structure in bytes.</returns>
        public unsafe byte[] ToArray()
        {
            byte[] array = new byte[Length];
            fixed (FileStatisticsDataEx* pData = &this)
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
        public unsafe ReadOnlySpan<byte> AsSpan() => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<FileStatisticsDataEx, byte>(ref Unsafe.AsRef(in this)), Length);

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="FileStatistics"/>.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the <see cref="FileStatistics"/>.</returns>
        public ReadOnlySpan<byte>.Enumerator GetEnumerator() => AsSpan().GetEnumerator();
#endif
    }
}
