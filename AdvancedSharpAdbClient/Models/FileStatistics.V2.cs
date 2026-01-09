// <copyright file="FileStatistics.V2.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace AdvancedSharpAdbClient.Models
{
    /// <summary>
    /// Contains information about a file on the remote device (v2).
    /// </summary>
    /// <remarks><see href="https://android.googlesource.com/platform/system/adb/+/refs/heads/main/file_sync_service.h"/></remarks>
#if HAS_BUFFERS
    [CollectionBuilder(typeof(EnumerableBuilder), nameof(EnumerableBuilder.FileStatisticsV2Creator))]
#endif
    [DebuggerDisplay($"{{{nameof(GetType)}().{nameof(Type.ToString)}(),nq}} \\{{ {nameof(Path)} = {{{nameof(Path)}}}, {nameof(FileMode)} = {{{nameof(FileMode)}}}, {nameof(Size)} = {{{nameof(Size)}}}, {nameof(ModifiedTime)} = {{{nameof(ModifiedTime)}}} }}")]
    public struct FileStatisticsV2(FileStatisticsDataV2 data) : IEquatable<FileStatisticsV2>
#if NET7_0_OR_GREATER
        , IEqualityOperators<FileStatisticsV2, FileStatisticsV2, bool>
#endif
    {
        /// <summary>
        /// The data of the file.
        /// </summary>
        private readonly FileStatisticsDataV2 data = data;

        /// <summary>
        /// Gets the path of the file.
        /// </summary>
        public string Path { get; set; } = string.Empty;

        /// <summary>
        /// Gets the error code associated with the current operation.
        /// </summary>
        public readonly uint Error => data.Error;

        /// <summary>
        /// Gets the device identifier associated with this instance.
        /// </summary>
        public readonly ulong Device => data.Device;

        /// <summary>
        /// Gets the index node identifier associated with this entry.
        /// </summary>
        public readonly ulong IndexNode => data.IndexNode;

        /// <summary>
        /// Gets the <see cref="UnixFileStatus"/> attributes of the file.
        /// </summary>
        public readonly UnixFileStatus FileMode => (UnixFileStatus)data.Mode;

        /// <summary>
        /// Gets the number of hard links to the file or directory.
        /// </summary>
        public readonly uint LinkCount => data.LinkCount;

        /// <summary>
        /// Gets the unique identifier of the user associated with this instance.
        /// </summary>
        public readonly uint UserId => data.UserId;

        /// <summary>
        /// Gets the unique identifier for the group.
        /// </summary>
        public readonly uint GroupId => data.GroupId;

        /// <summary>
        /// Gets the size, in bytes.
        /// </summary>
        public readonly ulong Size => data.Size;

        /// <summary>
        /// Gets the last access time.
        /// </summary>
        public readonly DateTimeOffset AccessTime => DateTimeOffset.FromUnixTimeSeconds(data.AccessTime);

        /// <summary>
        /// Gets the time indicating when the item was last modified.
        /// </summary>
        public readonly DateTimeOffset ModifiedTime => DateTimeOffset.FromUnixTimeSeconds(data.ModifiedTime);

        /// <summary>
        /// Gets the time indicating when the last change occurred.
        /// </summary>
        public readonly DateTimeOffset ChangedTime => DateTimeOffset.FromUnixTimeSeconds(data.ChangedTime);

#if HAS_BUFFERS
        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="FileStatisticsV2"/>.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the <see cref="FileStatisticsV2"/>.</returns>
        public readonly ReadOnlySpan<byte>.Enumerator GetEnumerator() => data.GetEnumerator();
#endif

        /// <inheritdoc/>
        public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is FileStatisticsV2 other && Equals(other);

        /// <inheritdoc/>
        public readonly bool Equals(FileStatisticsV2 other) => Path == other.Path && data == other.data;

        /// <summary>
        /// Tests whether two <see cref='FileStatisticsV2'/> objects are equally.
        /// </summary>
        /// <param name="left">The <see cref='FileStatisticsV2'/> structure that is to the left of the equality operator.</param>
        /// <param name="right">The <see cref='FileStatisticsV2'/> structure that is to the right of the equality operator.</param>
        /// <returns>This operator returns <see langword="true"/> if the two <see cref="FileStatisticsV2"/> structures are equally; otherwise <see langword="false"/>.</returns>
        public static bool operator ==(FileStatisticsV2 left, FileStatisticsV2 right) => left.Equals(right);

        /// <summary>
        /// Tests whether two <see cref='FileStatisticsV2'/> objects are different.
        /// </summary>
        /// <param name="left">The <see cref='FileStatisticsV2'/> structure that is to the left of the inequality operator.</param>
        /// <param name="right">The <see cref='FileStatisticsV2'/> structure that is to the right of the inequality operator.</param>
        /// <returns>This operator returns <see langword="true"/> if the two <see cref="FileStatisticsV2"/> structures are unequally; otherwise <see langword="false"/>.</returns>
        public static bool operator !=(FileStatisticsV2 left, FileStatisticsV2 right) => !left.Equals(right);

        /// <inheritdoc/>
        public override readonly int GetHashCode() => HashCode.Combine(Path, data);

        /// <inheritdoc/>
        public override readonly string ToString() => string.Join('\t', FileMode.ToPermissionCode()!, Size, ModifiedTime, Path!);
    }
}
