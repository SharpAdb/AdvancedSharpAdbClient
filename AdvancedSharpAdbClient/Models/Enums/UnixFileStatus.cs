// <copyright file="UnixFileStatus.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;

namespace AdvancedSharpAdbClient.Models
{
    /// <summary>
    /// Describes the properties of a file on an Android device.
    /// </summary>
    /// <remarks><see href="https://github.com/openbsd/src/blob/master/sys/sys/stat.h"/></remarks>
    [Flags]
    public enum UnixFileStatus
    {
        /// <summary>
        /// Empty property.
        /// </summary>
        /// <remarks>Permission code: <c>---------</c> (<c>000</c>)</remarks>
        None = 0x0000,

        /// <summary>
        /// Set user permission.
        /// </summary>
        /// <remarks>Permission code: <c>--S------</c> (<c>4000</c>)</remarks>
        SetUser = 0x0800,

        /// <summary>
        /// Set group permission.
        /// </summary>
        /// <remarks>Permission code: <c>-----S---</c> (<c>2000</c>)</remarks>
        SetGroup = 0x0400,

        /// <summary>
        /// Sticky bit permission.
        /// </summary>
        /// <remarks>Permission code: <c>--------T</c> (<c>1000</c>)</remarks>
        StickyBit = 0x0200,

        /// <summary>
        /// Read permission for owner.
        /// </summary>
        /// <remarks>Permission code: <c>r--------</c> (<c>400</c>)</remarks>
        UserRead = 0x0100,

        /// <summary>
        /// Write permission for owner.
        /// </summary>
        /// <remarks>Permission code: <c>-w-------</c> (<c>200</c>)</remarks>
        UserWrite = 0x0080,

        /// <summary>
        /// Execute permission for owner.
        /// </summary>
        /// <remarks>Permission code: <c>--x------</c> (<c>100</c>)</remarks>
        UserExecute = 0x0040,

        /// <summary>
        /// All owner permissions.
        /// </summary>
        /// <remarks>Permission code: <c>rwx------</c> (<c>700</c>)</remarks>
        UserAll = UserRead | UserWrite | UserExecute,

        /// <summary>
        /// The mask that can be used to retrieve the RWX for owner from a <see cref="UnixFileStatus"/>.
        /// </summary>
        /// <remarks>Gets RWX for owner by <c>mode &amp; <see cref="UserMask"/></c>.</remarks>
        UserMask = UserAll,

        /// <summary>
        /// Read permission for group.
        /// </summary>
        /// <remarks>Permission code: <c>---r-----</c> (<c>040</c>)</remarks>
        GroupRead = 0x0020,

        /// <summary>
        /// Write permission for group.
        /// </summary>
        /// <remarks>Permission code: <c>----w----</c> (<c>020</c>)</remarks>
        GroupWrite = 0x0010,

        /// <summary>
        /// Execute permission for group.
        /// </summary>
        /// <remarks>Permission code: <c>-----x---</c> (<c>010</c>)</remarks>
        GroupExecute = 0x0008,

        /// <summary>
        /// All group permissions.
        /// </summary>
        /// <remarks>Permission code: <c>---rwx---</c> (<c>070</c>)</remarks>
        GroupAll = GroupRead | GroupWrite | GroupExecute,

        /// <summary>
        /// The mask that can be used to retrieve the RWX for group from a <see cref="UnixFileStatus"/>.
        /// </summary>
        /// <remarks>Gets RWX for group by <c>mode &amp; <see cref="GroupMask"/></c>.</remarks>
        GroupMask = GroupAll,

        /// <summary>
        /// Read permission for others.
        /// </summary>
        /// <remarks>Permission code: <c>------r--</c> (<c>004</c>)</remarks>
        OtherRead = 0x0004,

        /// <summary>
        /// Write permission for others.
        /// </summary>
        /// <remarks>Permission code: <c>-------w-</c> (<c>001</c>)</remarks>
        OtherWrite = 0x0002,

        /// <summary>
        /// Execute permission for others.
        /// </summary>
        /// <remarks>Permission code: <c>--------x</c> (<c>001</c>)</remarks>
        OtherExecute = 0x0001,

        /// <summary>
        /// All others permissions.
        /// </summary>
        /// <remarks>Permission code: <c>------rwx</c> (<c>007</c>)</remarks>
        OtherAll = OtherRead | OtherWrite | OtherExecute,

        /// <summary>
        /// The mask that can be used to retrieve the RWX for others from a <see cref="UnixFileStatus"/>.
        /// </summary>
        /// <remarks>Gets RWX for others by <c>mode &amp; <see cref="OtherMask"/></c>.</remarks>
        OtherMask = OtherAll,

        /// <summary>
        /// The mask that can be used to retrieve the file type from a <see cref="UnixFileStatus"/>.
        /// </summary>
        /// <remarks>Gets file type by <c>mode &amp; <see cref="TypeMask"/></c>.</remarks>
        TypeMask = FIFO | Character | Directory | Block | Regular | SymbolicLink | Socket,

        /// <summary>
        /// The file is a first-in first-out queue.
        /// </summary>
        /// <remarks>Permission code: <c>p---------</c></remarks>
        FIFO = 0x1000,

        /// <summary>
        /// The file is a character device.
        /// </summary>
        /// <remarks>Permission code: <c>c---------</c></remarks>
        Character = 0x2000,

        /// <summary>
        /// The file is a directory.
        /// </summary>
        /// <remarks>Permission code: <c>d---------</c></remarks>
        Directory = 0x4000,

        /// <summary>
        /// The file is a block device.
        /// </summary>
        /// <remarks>Permission code: <c>b---------</c></remarks>
        Block = 0x6000,

        /// <summary>
        /// The file is a regular file.
        /// </summary>
        /// <remarks>Permission code: <c>----------</c></remarks>
        Regular = 0x8000,

        /// <summary>
        /// The file is a symbolic link.
        /// </summary>
        /// <remarks>Permission code: <c>l---------</c></remarks>
        SymbolicLink = 0xA000,

        /// <summary>
        /// The file is a Unix socket.
        /// </summary>
        /// <remarks>Permission code: <c>s---------</c></remarks>
        Socket = 0xC000,

        /// <summary>
        /// Save swapped text even after use.
        /// </summary>
        /// <remarks>Permission code: <c>--------T</c> (<c>1000</c>)</remarks>
        VTX = StickyBit,

        /// <summary>
        /// All access permissions.
        /// </summary>
        /// <remarks>Permission code: <c>rwxrwxrwx</c> (<c>777</c>)</remarks>
        AccessPermissions = UserMask | GroupMask | OtherMask,

        /// <summary>
        /// All permissions.
        /// </summary>
        /// <remarks>Permission code: <c>rwsrwsrwt</c> (<c>7777</c>)</remarks>
        AllPermissions = SetUser | SetGroup | StickyBit | UserMask | GroupMask | OtherMask,

        /// <summary>
        /// The default file mode.
        /// </summary>
        /// <remarks>Permission code: <c>rw-rw-rw-</c> (<c>666</c>)</remarks>
        DefaultFileMode = UserRead | UserWrite | GroupRead | GroupWrite | OtherRead | OtherWrite
    }
}
