// <copyright file="FileStatistics.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace AdvancedSharpAdbClient.Models
{
    /// <summary>
    /// Contains information about a file on the remote device.
    /// </summary>
    /// <param name="data">The data of the file.</param>
#if HAS_BUFFERS
    [CollectionBuilder(typeof(EnumerableBuilder), nameof(EnumerableBuilder.FileStatisticsCreator))]
#endif
    [DebuggerDisplay($"{{{nameof(GetType)}().{nameof(Type.ToString)}(),nq}} \\{{ {nameof(Path)} = {{{nameof(Path)}}}, {nameof(FileMode)} = {{{nameof(FileMode)}}}, {nameof(Size)} = {{{nameof(Size)}}}, {nameof(Time)} = {{{nameof(Time)}}} }}")]
    public struct FileStatistics(FileStatisticsData data) : IEquatable<FileStatistics>
#if NET7_0_OR_GREATER
        , IEqualityOperators<FileStatistics, FileStatistics, bool>
#endif
    {
        /// <summary>
        /// The data of the file.
        /// </summary>
        private readonly FileStatisticsData data = data;

        /// <summary>
        /// Gets the path of the file.
        /// </summary>
        public string Path { get; set; } = string.Empty;

        /// <summary>
        /// Gets the <see cref="UnixFileStatus"/> attributes of the file.
        /// </summary>
        public readonly UnixFileStatus FileMode => (UnixFileStatus)data.Mode;

        /// <summary>
        /// Gets the total file size, in bytes.
        /// </summary>
        public readonly uint Size => data.Size;

        /// <summary>
        /// Gets the time of last modification.
        /// </summary>
        public readonly DateTimeOffset Time => DateTimeOffset.FromUnixTimeSeconds(data.Time);

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="FileStatistics"/>.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the <see cref="FileStatistics"/>.</returns>
        public readonly IEnumerator<byte> GetEnumerator() => data.GetEnumerator();

        /// <inheritdoc/>
        public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is FileStatistics other && Equals(other);

        /// <inheritdoc/>
        public readonly bool Equals(FileStatistics other) => Path == other.Path && data == other.data;

        /// <summary>
        /// Tests whether two <see cref='FileStatistics'/> objects are equally.
        /// </summary>
        /// <param name="left">The <see cref='FileStatistics'/> structure that is to the left of the equality operator.</param>
        /// <param name="right">The <see cref='FileStatistics'/> structure that is to the right of the equality operator.</param>
        /// <returns>This operator returns <see langword="true"/> if the two <see cref="FileStatistics"/> structures are equally; otherwise <see langword="false"/>.</returns>
        public static bool operator ==(FileStatistics left, FileStatistics right) => left.Equals(right);

        /// <summary>
        /// Tests whether two <see cref='FileStatistics'/> objects are different.
        /// </summary>
        /// <param name="left">The <see cref='FileStatistics'/> structure that is to the left of the inequality operator.</param>
        /// <param name="right">The <see cref='FileStatistics'/> structure that is to the right of the inequality operator.</param>
        /// <returns>This operator returns <see langword="true"/> if the two <see cref="FileStatistics"/> structures are unequally; otherwise <see langword="false"/>.</returns>
        public static bool operator !=(FileStatistics left, FileStatistics right) => !left.Equals(right);

        /// <inheritdoc/>
        public override readonly int GetHashCode() => HashCode.Combine(Path, data);

        /// <inheritdoc/>
        public override readonly string ToString() => string.Join('\t', FileMode.ToPermissionCode()!, Size, Time, Path!);
    }
}
