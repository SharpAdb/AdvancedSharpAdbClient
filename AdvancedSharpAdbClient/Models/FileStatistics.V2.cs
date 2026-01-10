// <copyright file="FileStatistics.V2.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace AdvancedSharpAdbClient.Models
{
    /// <summary>
    /// Contains information about a file on the remote device (v2).
    /// </summary>
    /// <remarks><see href="https://android.googlesource.com/platform/packages/modules/adb/+/refs/heads/main/file_sync_protocol.h"/></remarks>
    [DebuggerDisplay($"{{{nameof(GetType)}().{nameof(Type.ToString)}(),nq}} \\{{ {nameof(Path)} = {{{nameof(Path)}}}, {nameof(FileMode)} = {{{nameof(FileMode)}}}, {nameof(Size)} = {{{nameof(Size)}}}, {nameof(ModifiedTime)} = {{{nameof(ModifiedTime)}}} }}")]
    public class FileStatisticsEx(FileStatisticsDataEx data) : FileStatisticsBase<FileStatisticsDataEx, FileStatisticsEx>(data), IFileStatistics
#if NET7_0_OR_GREATER
        , IEqualityOperators<FileStatisticsEx, FileStatisticsEx, bool>
#endif
    {
        /// <summary>
        /// Gets the error code associated with the current operation.
        /// </summary>
        public UnixErrorCode Error => (UnixErrorCode)data.Error;

        /// <summary>
        /// Gets the device identifier associated with this instance.
        /// </summary>
        public ulong Device => data.Device;

        /// <summary>
        /// Gets the index node identifier associated with this entry.
        /// </summary>
        public ulong IndexNode => data.IndexNode;

        /// <summary>
        /// Gets the <see cref="UnixFileStatus"/> attributes of the file.
        /// </summary>
        public UnixFileStatus FileMode => (UnixFileStatus)data.Mode;

        /// <summary>
        /// Gets the number of hard links to the file or directory.
        /// </summary>
        public uint LinkCount => data.LinkCount;

        /// <summary>
        /// Gets the unique identifier of the user associated with this instance.
        /// </summary>
        public uint UserId => data.UserId;

        /// <summary>
        /// Gets the unique identifier for the group.
        /// </summary>
        public uint GroupId => data.GroupId;

        /// <summary>
        /// Gets the size, in bytes.
        /// </summary>
        public ulong Size => data.Size;

        /// <summary>
        /// Gets the last access time.
        /// </summary>
        public DateTimeOffset AccessTime => DateTimeOffset.FromUnixTimeSeconds(data.AccessTime);

        /// <summary>
        /// Gets the time indicating when the item was last modified.
        /// </summary>
        public DateTimeOffset ModifiedTime => DateTimeOffset.FromUnixTimeSeconds(data.ModifiedTime);

        /// <summary>
        /// Gets the time indicating when the last change occurred.
        /// </summary>
        public DateTimeOffset ChangedTime => DateTimeOffset.FromUnixTimeSeconds(data.ChangedTime);

        /// <inheritdoc/>
        DateTimeOffset IFileStatistics.Time => ModifiedTime;

        /// <inheritdoc/>
        public override string ToString() => string.Join("\t", FileMode.ToPermissionCode()!, Size, ModifiedTime, Path!);

        /// <inheritdoc/>
        public override bool Equals([NotNullWhen(true)] object? obj) => base.Equals(obj);

        /// <summary>
        /// Tests whether two <see cref="FileStatisticsEx"/> objects are equally.
        /// </summary>
        /// <param name="left">The <see cref="FileStatisticsEx"/> structure that is to the left of the equality operator.</param>
        /// <param name="right">The <see cref="FileStatisticsEx"/> structure that is to the right of the equality operator.</param>
        /// <returns>This operator returns <see langword="true"/> if the two <see cref="FileStatisticsEx"/> structures are equally; otherwise <see langword="false"/>.</returns>
        public static bool operator ==(FileStatisticsEx? left, FileStatisticsEx? right) => left == (right as FileStatisticsBase<FileStatisticsDataEx, FileStatisticsEx>);

        /// <summary>
        /// Tests whether two <see cref="FileStatisticsEx"/> objects are different.
        /// </summary>
        /// <param name="left">The <see cref="FileStatisticsEx"/> structure that is to the left of the inequality operator.</param>
        /// <param name="right">The <see cref="FileStatisticsEx"/> structure that is to the right of the inequality operator.</param>
        /// <returns>This operator returns <see langword="true"/> if the two <see cref="FileStatisticsEx"/> structures are unequally; otherwise <see langword="false"/>.</returns>
        public static bool operator !=(FileStatisticsEx? left, FileStatisticsEx? right) => !(left == right);

        /// <inheritdoc/>
        public override int GetHashCode() => base.GetHashCode();
    }
}
