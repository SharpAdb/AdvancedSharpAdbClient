// <copyright file="FileStatistics.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace AdvancedSharpAdbClient.Models
{
    /// <summary>
    /// Contains information about a file on the remote device.
    /// </summary>
    /// <param name="data">The data of the file.</param>
    [DebuggerDisplay($"{{{nameof(GetType)}().{nameof(Type.ToString)}(),nq}} \\{{ {nameof(Path)} = {{{nameof(Path)}}}, {nameof(FileMode)} = {{{nameof(FileMode)}}}, {nameof(Size)} = {{{nameof(Size)}}}, {nameof(Time)} = {{{nameof(Time)}}} }}")]
    public sealed class FileStatistics(in FileStatisticsData data) : FileStatisticsBase<FileStatisticsData, FileStatistics>(data), IFileStatistics
#if NET7_0_OR_GREATER
        , IEqualityOperators<FileStatistics, FileStatistics, bool>
#endif
    {
        /// <summary>
        /// Gets the <see cref="UnixFileStatus"/> attributes of the file.
        /// </summary>
        public UnixFileStatus FileMode => (UnixFileStatus)data.Mode;

        /// <summary>
        /// Gets the total file size, in bytes.
        /// </summary>
        public uint Size => data.Size;

        /// <summary>
        /// Gets the time of last modification.
        /// </summary>
        public DateTimeOffset Time => DateTimeOffset.FromUnixTimeSeconds(data.Time);

        /// <inheritdoc/>
        ulong IFileStatistics.Size => Size;

        /// <inheritdoc/>
        public override bool Equals([NotNullWhen(true)] object? obj) => base.Equals(obj);

        /// <summary>
        /// Tests whether two <see cref="FileStatistics"/> objects are equally.
        /// </summary>
        /// <param name="left">The <see cref="FileStatistics"/> structure that is to the left of the equality operator.</param>
        /// <param name="right">The <see cref="FileStatistics"/> structure that is to the right of the equality operator.</param>
        /// <returns>This operator returns <see langword="true"/> if the two <see cref="FileStatistics"/> structures are equally; otherwise <see langword="false"/>.</returns>
        public static bool operator ==(FileStatistics? left, FileStatistics? right) => left == (right as FileStatisticsBase<FileStatisticsData, FileStatistics>);

        /// <summary>
        /// Tests whether two <see cref="FileStatistics"/> objects are different.
        /// </summary>
        /// <param name="left">The <see cref="FileStatistics"/> structure that is to the left of the inequality operator.</param>
        /// <param name="right">The <see cref="FileStatistics"/> structure that is to the right of the inequality operator.</param>
        /// <returns>This operator returns <see langword="true"/> if the two <see cref="FileStatistics"/> structures are unequally; otherwise <see langword="false"/>.</returns>
        public static bool operator !=(FileStatistics? left, FileStatistics? right) => !(left == right);

        /// <inheritdoc/>
        public override int GetHashCode() => base.GetHashCode();

        /// <inheritdoc/>
        public override string ToString() => string.Join("\t", FileMode.ToPermissionCode()!, Size, Time, Path!);
    }
}
