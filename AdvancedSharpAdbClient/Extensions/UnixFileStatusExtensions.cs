// <copyright file="UnixFileStatusExtensions.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion, yungd1plomat, wherewhere. All rights reserved.
// </copyright>

using System;
using System.Collections;
using System.Collections.Generic;

namespace AdvancedSharpAdbClient.Models
{
    /// <summary>
    /// Provides extension methods for the <see cref="UnixFileStatus"/> enum. Provides overloads for commonly used functions.
    /// </summary>
    public static class UnixFileStatusExtensions
    {
        /// <summary>
        /// Gets the type of the given file status.
        /// </summary>
        /// <param name="mode">File status to process.</param>
        /// <returns>The type of the file status.</returns>
        public static UnixFileStatus GetFileType(this UnixFileStatus mode) => mode & UnixFileStatus.TypeMask;

        /// <summary>
        /// Gets the permissions of the given file status.
        /// </summary>
        /// <param name="mode">File status to process.</param>
        /// <returns>The permissions of the given file status.</returns>
        public static UnixFileStatus GetPermissions(this UnixFileStatus mode) => mode & UnixFileStatus.AllPermissions;

        /// <summary>
        /// Gets the access permissions of the given file status.
        /// </summary>
        /// <param name="mode">File status to process.</param>
        /// <returns>The access permissions of the given file status.</returns>
        public static UnixFileStatus GetAccessPermissions(this UnixFileStatus mode) => mode & UnixFileStatus.AccessPermissions;

        /// <summary>
        /// Checks if the given file status corresponds to a directory, as if determined by <see cref="UnixFileStatus.Directory"/>.
        /// </summary>
        /// <param name="mode">File status to check.</param>
        /// <returns><see langword="true"/> if the type indicated <paramref name="mode"/> refers to a directory, otherwise <see langword="false"/>.</returns>
        public static bool IsDirectory(this UnixFileStatus mode) => mode.GetFileType() == UnixFileStatus.Directory;

        /// <summary>
        /// Checks if the given file status or path corresponds to a character special file, as if determined by <see cref="UnixFileStatus.Character"/>.
        /// Examples of character special files are character devices such as <c>/dev/null</c>, <c>/dev/tty</c>, <c>/dev/audio</c>, or <c>/dev/nvram</c> on Linux.
        /// </summary>
        /// <param name="mode">File status to check.</param>
        /// <returns><see langword="true"/> if the type indicated <paramref name="mode"/> refers to a character device, otherwise <see langword="false"/>.</returns>
        public static bool IsCharacterFile(this UnixFileStatus mode) => mode.GetFileType() == UnixFileStatus.Character;

        /// <summary>
        /// Checks if the given file status corresponds to a block special file, as if determined by <see cref="UnixFileStatus.Block"/>.
        /// Examples of block special files are block devices such as <c>/dev/sda</c> or <c>/dev/loop0</c> on Linux.
        /// </summary>
        /// <param name="mode">File status to check.</param>
        /// <returns><see langword="true"/> if the type indicated <paramref name="mode"/> refers to a block device, otherwise <see langword="false"/>.</returns>
        public static bool IsBlockFile(this UnixFileStatus mode) => mode.GetFileType() == UnixFileStatus.Block;

        /// <summary>
        /// Checks if the given file status corresponds to a regular file, as if determined by <see cref="UnixFileStatus.Regular"/>.
        /// </summary>
        /// <param name="mode">File status to check.</param>
        /// <returns><see langword="true"/> if the type indicated by <paramref name="mode"/> refers to a regular file, otherwise <see langword="false"/>.</returns>
        public static bool IsRegularFile(this UnixFileStatus mode) => mode.GetFileType() == UnixFileStatus.Regular;

        /// <summary>
        /// Checks if the given file status corresponds to a FIFO or pipe file as if determined by <see cref="UnixFileStatus.FIFO"/>.
        /// </summary>
        /// <param name="mode">File status to check.</param>
        /// <returns><see langword="true"/> if the type indicated <paramref name="mode"/> refers to a FIFO pipe, otherwise <see langword="false"/>.</returns>
        public static bool IsFIFO(this UnixFileStatus mode) => mode.GetFileType() == UnixFileStatus.FIFO;

        /// <summary>
        /// Checks if the given file status corresponds to a symbolic link, as if determined by <see cref="UnixFileStatus.SymbolicLink"/>.
        /// </summary>
        /// <param name="mode">File status to check.</param>
        /// <returns><see langword="true"/> if the type indicated <paramref name="mode"/> refers to a symbolic link, otherwise <see langword="false"/>.</returns>
        public static bool IsSymbolicLink(this UnixFileStatus mode) => mode.GetFileType() == UnixFileStatus.SymbolicLink;

        /// <summary>
        /// Checks if the given file status or path corresponds to a named IPC socket, as if determined by <see cref="UnixFileStatus.Socket"/>.
        /// </summary>
        /// <param name="mode">File status to check.</param>
        /// <returns><see langword="true"/> if the type indicated <paramref name="mode"/> refers to a named socket, otherwise <see langword="false"/>.</returns>
        public static bool IsSocket(this UnixFileStatus mode) => mode.GetFileType() == UnixFileStatus.Socket;

        /// <summary>
        /// Checks if the given file status corresponds to a file of type other type. That is, the file exists, but is neither regular file, nor directory nor a symlink.
        /// </summary>
        /// <param name="mode">File status to check.</param>
        /// <returns><see langword="true"/> if the type indicated <paramref name="mode"/> refers to a file that is not regular file, directory, or a symlink, otherwise <see langword="false"/>.</returns>
        public static bool IsOther(this UnixFileStatus mode) => mode.GetFileType() is not (UnixFileStatus.Regular or UnixFileStatus.Directory or UnixFileStatus.SymbolicLink);

        /// <summary>
        /// Checks if the given file type is known, equivalent to <c>mode.GetFileType() != <see cref="UnixFileStatus.None"/></c>.
        /// </summary>
        /// <param name="mode">File type to check.</param>
        /// <returns><see langword="true"/> if the given file type is a known file type, otherwise <see langword="false"/>.</returns>
        public static bool IsTypeKnown(this UnixFileStatus mode) => mode.GetFileType() != UnixFileStatus.None;

        /// <summary>
        /// Parses a Unix permission code into a <see cref="UnixFileStatus"/>.
        /// </summary>
        /// <param name="code">The permission code to parse.</param>
        /// <returns>A <see cref="UnixFileStatus"/> representing the parsed permission code.</returns>
        public static UnixFileStatus FromPermissionCode(string code)
        {
            ExceptionExtensions.ThrowIfNull(code);

            if (code.Length is not (9 or 10))
            {
                try
                {
                    return (UnixFileStatus)Convert.ToInt32(code, 8);
                }
                catch
                {
                    throw new ArgumentOutOfRangeException(nameof(code), $"The length of {nameof(code)} should be 9 or 10, but it is {code.Length}.");
                }
            }

            UnixFileStatus mode = UnixFileStatus.None;
            int index = code.Length;

            mode |= code[--index] switch
            {
                'x' => UnixFileStatus.OtherExecute,
                't' => UnixFileStatus.StickyBit | UnixFileStatus.OtherExecute,
                'T' => UnixFileStatus.StickyBit,
                '-' => UnixFileStatus.None,
                _ => throw new ArgumentOutOfRangeException(nameof(code), $"The char of index {index} should be 'x', 't', 'T' or '-', but it is {code[index]}.")
            };
            mode |= code[--index] switch
            {
                'w' => UnixFileStatus.OtherWrite,
                '-' => UnixFileStatus.None,
                _ => throw new ArgumentOutOfRangeException(nameof(code), $"The char of index {index} should be 'w' or '-', but it is {code[index]}.")
            };
            mode |= code[--index] switch
            {
                'r' => UnixFileStatus.OtherRead,
                '-' => UnixFileStatus.None,
                _ => throw new ArgumentOutOfRangeException(nameof(code), $"The char of index {index} should be 'r' or '-', but it is {code[index]}.")
            };

            mode |= code[--index] switch
            {
                'x' => UnixFileStatus.GroupExecute,
                's' => UnixFileStatus.SetGroup | UnixFileStatus.GroupExecute,
                'S' => UnixFileStatus.SetGroup,
                '-' => UnixFileStatus.None,
                _ => throw new ArgumentOutOfRangeException(nameof(code), $"The char of index {index} should be 'x', 's', 'S' or '-', but it is {code[index]}.")
            };
            mode |= code[--index] switch
            {
                'w' => UnixFileStatus.GroupWrite,
                '-' => UnixFileStatus.None,
                _ => throw new ArgumentOutOfRangeException(nameof(code), $"The char of index {index} should be 'w' or '-', but it is {code[index]}.")
            };
            mode |= code[--index] switch
            {
                'r' => UnixFileStatus.GroupRead,
                '-' => UnixFileStatus.None,
                _ => throw new ArgumentOutOfRangeException(nameof(code), $"The char of index {index} should be 'r' or '-', but it is {code[index]}.")
            };

            mode |= code[--index] switch
            {
                'x' => UnixFileStatus.UserExecute,
                's' => UnixFileStatus.SetUser | UnixFileStatus.UserExecute,
                'S' => UnixFileStatus.SetUser,
                '-' => UnixFileStatus.None,
                _ => throw new ArgumentOutOfRangeException(nameof(code), $"The char of index {index} should be 'x', 's', 'S' or '-', but it is {code[index]}.")
            };
            mode |= code[--index] switch
            {
                'w' => UnixFileStatus.UserWrite,
                '-' => UnixFileStatus.None,
                _ => throw new ArgumentOutOfRangeException(nameof(code), $"The char of index {index} should be 'w' or '-', but it is {code[index]}.")
            };
            mode |= code[--index] switch
            {
                'r' => UnixFileStatus.UserRead,
                '-' => UnixFileStatus.None,
                _ => throw new ArgumentOutOfRangeException(nameof(code), $"The char of index {index} should be 'r' or '-', but it is {code[index]}.")
            };

            if (index == 1)
            {
                mode |= code[0] switch
                {
                    'p' => UnixFileStatus.FIFO,
                    'c' => UnixFileStatus.Character,
                    'd' => UnixFileStatus.Directory,
                    'b' => UnixFileStatus.Block,
                    '-' => UnixFileStatus.Regular,
                    'l' => UnixFileStatus.SymbolicLink,
                    's' => UnixFileStatus.Socket,
                    _ => UnixFileStatus.None
                };
            }

            return mode;
        }

        /// <summary>
        /// Provides a string representation of the given <see cref="UnixFileStatus"/>.
        /// </summary>
        /// <param name="mode">The <see cref="UnixFileStatus"/> to process.</param>
        /// <returns>A string representation of the given <see cref="UnixFileStatus"/>.</returns>
        public static string ToPermissionCode(this UnixFileStatus mode)
        {
#if HAS_BUFFERS
            Span<char> code = stackalloc char[10];
#else
            char[] code = new char[10];
#endif
            BitArray array = new(new[] { (int)mode });

            code[9] = array[0]
                ? array[9] ? 't' : 'x'
                : array[9] ? 'T' : '-';
            code[8] = array[1] ? 'w' : '-';
            code[7] = array[2] ? 'r' : '-';

            code[6] = array[3]
                ? array[10] ? 's' : 'x'
                : array[10] ? 'S' : '-';
            code[5] = array[4] ? 'w' : '-';
            code[4] = array[5] ? 'r' : '-';

            code[3] = array[6]
                ? array[11] ? 's' : 'x'
                : array[11] ? 'S' : '-';
            code[2] = array[7] ? 'w' : '-';
            code[1] = array[8] ? 'r' : '-';

            code[0] = mode.GetFileType() switch
            {
                UnixFileStatus.FIFO => 'p',
                UnixFileStatus.Character => 'c',
                UnixFileStatus.Directory => 'd',
                UnixFileStatus.Block => 'b',
                UnixFileStatus.Regular => '-',
                UnixFileStatus.SymbolicLink => 'l',
                UnixFileStatus.Socket => 's',
                UnixFileStatus.None => '\0',
                _ => '?'
            };

            return new string(code);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="UnixFileStatus"/>.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the <see cref="UnixFileStatus"/>.</returns>
        public static IEnumerator<byte> GetEnumerator(this UnixFileStatus mode)
        {
            int num = (int)mode;
            yield return (byte)num;
            yield return (byte)(num >> 8);
            yield return (byte)(num >> 16);
            yield return (byte)(num >> 24);
        }
    }
}
