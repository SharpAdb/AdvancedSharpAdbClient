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
    /// <remarks><see href="https://android.googlesource.com/platform/system/adb/+/refs/heads/main/file_sync_service.h"/></remarks>
#if HAS_BUFFERS
    [CollectionBuilder(typeof(EnumerableBuilder), nameof(EnumerableBuilder.FileStatisticsCreator))]
#endif
    [DebuggerDisplay($"{nameof(FileStatistics)} \\{{ {nameof(Path)} = {{{nameof(Path)}}}, {nameof(FileMode)} = {{{nameof(FileMode)}}}, {nameof(Size)} = {{{nameof(Size)}}}, {nameof(Time)} = {{{nameof(Time)}}} }}")]
    public struct FileStatistics : IEquatable<FileStatistics>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileStatistics"/> struct.
        /// </summary>
        public FileStatistics() { }

        /// <summary>
        /// Gets or sets the path of the file.
        /// </summary>
        public string Path { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the <see cref="UnixFileStatus"/> attributes of the file.
        /// </summary>
        public UnixFileStatus FileMode { get; init; }

        /// <summary>
        /// Gets or sets the total file size, in bytes.
        /// </summary>
        public int Size { get; init; }

        /// <summary>
        /// Gets or sets the time of last modification.
        /// </summary>
        public DateTimeOffset Time { get; init; }

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="FileStatistics"/>.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the <see cref="FileStatistics"/>.</returns>
        public readonly IEnumerator<byte> GetEnumerator()
        {
            int mode = (int)FileMode;
            yield return (byte)mode;
            yield return (byte)(mode >> 8);
            yield return (byte)(mode >> 16);
            yield return (byte)(mode >> 24);

            yield return (byte)Size;
            yield return (byte)(Size >> 8);
            yield return (byte)(Size >> 16);
            yield return (byte)(Size >> 24);

            long time = Time.ToUnixTimeSeconds();
            yield return (byte)time;
            yield return (byte)(time >> 8);
            yield return (byte)(time >> 16);
            yield return (byte)(time >> 24);
        }

        /// <inheritdoc/>
        public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is FileStatistics other && Equals(other);

        /// <inheritdoc/>
        public readonly bool Equals(FileStatistics other) =>
            Path == other.Path
                && FileMode == other.FileMode
                && Size == other.Size
                && Time == other.Time;

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
        public override readonly int GetHashCode() => HashCode.Combine(Path, FileMode, Size, Time);

        /// <inheritdoc/>
        public override readonly string ToString() => StringExtensions.Join('\t', FileMode.ToPermissionCode(), Size, Time, Path);
    }
}
