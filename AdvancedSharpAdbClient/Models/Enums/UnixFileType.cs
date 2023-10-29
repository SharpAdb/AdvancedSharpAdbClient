// <copyright file="UnixFileType.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;

namespace AdvancedSharpAdbClient.Models
{
    /// <summary>
    /// Describes the properties of a file on an Android device.
    /// </summary>
    [Flags]
    public enum UnixFileType : ushort
    {
        /// <summary>
        /// The mask that can be used to retrieve the file type from a <see cref="UnixFileType"/>.
        /// </summary>
        TypeMask = 0x8000,

        /// <summary>
        /// The file is a Unix socket.
        /// </summary>
        Socket = 0xC000,

        /// <summary>
        /// The file is a symbolic link.
        /// </summary>
        SymbolicLink = 0xA000,

        /// <summary>
        /// The file is a regular file.
        /// </summary>
        Regular = 0x8000,

        /// <summary>
        /// The file is a block device.
        /// </summary>
        Block = 0x6000,

        /// <summary>
        /// The file is a directory.
        /// </summary>
        Directory = 0x4000,

        /// <summary>
        /// The file is a character device.
        /// </summary>
        Character = 0x2000,

        /// <summary>
        /// The file is a first-in first-out queue.
        /// </summary>
        FIFO = 0x1000
    }
}
