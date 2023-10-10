// <copyright file="FileStatistics.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;

namespace AdvancedSharpAdbClient
{
    /// <summary>
    /// Contains information about a file on the remote device.
    /// </summary>
    public class FileStatistics : IEquatable<FileStatistics>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileStatistics"/> class.
        /// </summary>
        public FileStatistics() { }

        /// <summary>
        /// Gets or sets the path of the file.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="UnixFileType"/> attributes of the file.
        /// </summary>
        public UnixFileType FileType { get; set; }

        /// <summary>
        /// Gets or sets the total file size, in bytes.
        /// </summary>
        public int Size { get; set; }

        /// <summary>
        /// Gets or sets the time of last modification.
        /// </summary>
        public DateTimeOffset Time { get; set; }

        /// <summary>
        /// Gets a <see cref="string"/> that represents the current <see cref="FileStatistics"/> object.
        /// </summary>
        /// <returns>The <see cref="Path"/> of the current <see cref="FileStatistics"/> object.</returns>
        public override string ToString() => Path;

        /// <inheritdoc/>
        public override bool Equals(object obj) => Equals(obj as FileStatistics);

        /// <inheritdoc/>
        public bool Equals(FileStatistics other) =>
            other is not null
                && Path == other.Path
                && FileType == other.FileType
                && Size == other.Size
                && Time == other.Time;

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(Path, FileType, Size, Time);
    }
}
