using AdvancedSharpAdbClient.DeviceCommands;
using AdvancedSharpAdbClient.Exceptions;
using System;
using System.Diagnostics;
using System.IO;

namespace AdvancedSharpAdbClient.FileManager
{
    /// <summary>
    /// Provides static methods for the creation, copying, deletion, moving, and opening of a single file.
    /// </summary>
    public class AdbFile
    {
        /// <summary>
        /// Copies an existing file to a new file.
        /// An exception is raised if the destination file already exists.
        /// </summary>
        /// <param name="client">The connection to the adb server.</param>
        /// <param name="device">The device on which to run the command.</param>
        /// <param name="sourceFileName">The file to copy.</param>
        /// <param name="destFileName">The name of the destination file. This cannot be a directory or an existing file.</param>
        public static void Copy(IAdbClient client, DeviceData device, string sourceFileName, string destFileName)
            => Copy(client, device, sourceFileName, destFileName, overwrite: false);

        /// <summary>
        /// Copies an existing file to a new file.
        /// If <paramref name="overwrite"/> is false, an exception will be
        /// raised if the destination exists. Otherwise it will be overwritten.
        /// </summary>
        /// <param name="client">The connection to the adb server.</param>
        /// <param name="device">The device on which to run the command.</param>
        /// <param name="sourceFileName">The file to copy.</param>
        /// <param name="destFileName">The name of the destination file. This cannot be a directory.</param>
        /// <param name="overwrite"><see langword="true"/> if the destination file can be overwritten; otherwise, <see langword="false"/>.</param>
        public static void Copy(IAdbClient client, DeviceData device, string sourceFileName, string destFileName, bool overwrite)
        {
            if (sourceFileName == null)
            {
                throw new ArgumentNullException(nameof(sourceFileName));
            }

            if (destFileName == null)
            {
                throw new ArgumentNullException(nameof(destFileName));
            }

            AdbFileSystem.CopyFile(client, device, LinuxPath.GetFullPath(sourceFileName), LinuxPath.GetFullPath(destFileName), overwrite);
        }

        /// <summary>
        /// Deletes the specified file.
        /// </summary>
        /// <param name="client">The connection to the adb server.</param>
        /// <param name="device">The device on which to run the command.</param>
        /// <param name="path">The name of the file to be deleted. Wildcard characters are not supported.</param>
        // Deletes a file. The file specified by the designated path is deleted.
        // If the file does not exist, Delete succeeds without throwing
        // an exception.
        public static void Delete(IAdbClient client, DeviceData device, string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }
            AdbFileSystem.DeleteFile(client, device, LinuxPath.GetFullPath(path));
        }

        /// <summary>
        /// Determines whether the specified file exists.
        /// </summary>
        /// <param name="client">The connection to the adb server.</param>
        /// <param name="device">The device on which to run the command.</param>
        /// <param name="path">The file to check.</param>
        /// <returns><see langword="true"/> if the caller has the required permissions
        /// and path contains the name of an existing file; otherwise, <see langword="false"/>.
        /// This method also returns <see langword="false"/> if <paramref name="path"/>
        /// is <see langword="null"/>, an invalid path, or a zero-length string. If the caller
        /// does not have sufficient permissions to read the specified file, no exception is thrown
        /// and the method returns <see langword="false"/> regardless of the existence of <paramref name="path"/>.</returns>
        // Tests whether a file exists. The result is true if the file
        // given by the specified path exists; otherwise, the result is
        // false.  Note that if path describes a directory,
        // Exists will return true.
        public static bool Exists(IAdbClient client, DeviceData device, string path)
        {
            try
            {
                if (path == null) { return false; }
                if (path.Length == 0) { return false; }

                path = LinuxPath.GetFullPath(path);

                // After normalizing, check whether path ends in directory separator.
                // Otherwise, FillAttributeInfo removes it and we may return a false positive.
                // GetFullPath should never return null
                Debug.Assert(path != null, "File.Exists: GetFullPath returned null");
                if (path.Length > 0 && LinuxPath.IsDirectorySeparator(path[path.Length - 1]))
                {
                    return false;
                }

                return AdbFileSystem.FileExists(client, device, path);
            }
            catch (FileNotFoundException) { }
            catch (PermissionDeniedException) { }
            catch (ArgumentException) { }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }

            return false;
        }

        // File and Directory UTC APIs treat a DateTimeKind.Unspecified as UTC whereas
        // ToUniversalTime treats this as local.
        internal static DateTimeOffset GetUtcDateTimeOffset(DateTime dateTime)
            => dateTime.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(dateTime, DateTimeKind.Utc)
                : dateTime.ToUniversalTime();

        /// <summary>
        /// Returns the creation date and time of the specified file or directory.
        /// </summary>
        /// <param name="client">The connection to the adb server.</param>
        /// <param name="device">The device on which to run the command.</param>
        /// <param name="path">The file or directory for which to obtain creation date and time information.</param>
        /// <returns>A <see cref="DateTime"/> structure set to the creation date and time for the specified file or directory. This value is expressed in local time.</returns>
        public static DateTime GetCreationTime(IAdbClient client, DeviceData device, string path)
            => AdbFileSystem.GetCreationTime(client, device, LinuxPath.GetFullPath(path)).LocalDateTime;

        /// <summary>
        /// Returns the creation date and time, in Coordinated Universal Time (UTC), of the specified file or directory.
        /// </summary>
        /// <param name="client">The connection to the adb server.</param>
        /// <param name="device">The device on which to run the command.</param>
        /// <param name="path">The file or directory for which to obtain creation date and time information.</param>
        /// <returns>A <see cref="DateTime"/> structure set to the creation date and time for the specified file or directory. This value is expressed in UTC time.</returns>
        public static DateTime GetCreationTimeUtc(IAdbClient client, DeviceData device, string path)
            => AdbFileSystem.GetCreationTime(client, device, LinuxPath.GetFullPath(path)).UtcDateTime;

        /// <summary>
        /// Returns the date and time the specified file or directory was last accessed.
        /// </summary>
        /// <param name="client">The connection to the adb server.</param>
        /// <param name="device">The device on which to run the command.</param>
        /// <param name="path">Returns the date and time the specified file or directory was last accessed.</param>
        /// <returns>A <see cref="DateTime"/> structure set to the date and time that the specified file or directory was last accessed. This value is expressed in local time.</returns>
        public static DateTime GetLastAccessTime(IAdbClient client, DeviceData device, string path)
            => AdbFileSystem.GetLastAccessTime(client, device, LinuxPath.GetFullPath(path)).LocalDateTime;

        /// <summary>
        /// Returns the date and time, in Coordinated Universal Time (UTC), that the specified file or directory was last accessed.
        /// </summary>
        /// <param name="client">The connection to the adb server.</param>
        /// <param name="device">The device on which to run the command.</param>
        /// <param name="path">The file or directory for which to obtain access date and time information.</param>
        /// <returns>A <see cref="DateTime"/> structure set to the date and time that the specified file or directory was last accessed. This value is expressed in UTC time.</returns>
        public static DateTime GetLastAccessTimeUtc(IAdbClient client, DeviceData device, string path)
            => AdbFileSystem.GetLastAccessTime(client, device, LinuxPath.GetFullPath(path)).UtcDateTime;

        /// <summary>
        /// Returns the date and time the specified file or directory was last written to.
        /// </summary>
        /// <param name="client">The connection to the adb server.</param>
        /// <param name="device">The device on which to run the command.</param>
        /// <param name="path">The file or directory for which to obtain write date and time information.</param>
        /// <returns>A <see cref="DateTime"/> structure set to the date and time that the specified file or directory was last written to. This value is expressed in local time.</returns>
        public static DateTime GetLastWriteTime(IAdbClient client, DeviceData device, string path)
            => AdbFileSystem.GetLastAccessTime(client, device, LinuxPath.GetFullPath(path)).LocalDateTime;

        /// <summary>
        /// Returns the date and time, in Coordinated Universal Time (UTC), that the specified file or directory was last written to.
        /// </summary>
        /// <param name="client">The connection to the adb server.</param>
        /// <param name="device">The device on which to run the command.</param>
        /// <param name="path">The file or directory for which to obtain write date and time information.</param>
        /// <returns>A <see cref="DateTime"/> structure set to the date and time that the specified file or directory was last written to. This value is expressed in UTC time.</returns>
        public static DateTime GetLastWriteTimeUtc(IAdbClient client, DeviceData device, string path)
            => AdbFileSystem.GetLastAccessTime(client, device, LinuxPath.GetFullPath(path)).UtcDateTime;

        /// <summary>Gets the <see cref="UnixFileMode" /> of the file on the path.</summary>
        /// <param name="client">The connection to the adb server.</param>
        /// <param name="device">The device on which to run the command.</param>
        /// <param name="path">The path to the file.</param>
        /// <returns>The <see cref="UnixFileMode" /> of the file on the path.</returns>
        public static UnixFileMode GetUnixFileMode(IAdbClient client, DeviceData device, string path)
            => AdbFileSystem.GetUnixFileMode(client, device, path);

        /// <summary>Sets the specified <see cref="UnixFileMode" /> of the file on the specified path.</summary>
        /// <param name="client">The connection to the adb server.</param>
        /// <param name="device">The device on which to run the command.</param>
        /// <param name="path">The path to the file.</param>
        /// <param name="mode">The unix file mode.</param>
        public static void SetUnixFileMode(IAdbClient client, DeviceData device, string path, UnixFileMode mode)
            => AdbFileSystem.SetUnixFileMode(client, device, path, mode);


        /// <summary>
        /// Moves a specified file to a new location, providing the option to specify a new file name.
        /// </summary>
        /// <param name="client">The connection to the adb server.</param>
        /// <param name="device">The device on which to run the command.</param>
        /// <param name="sourceFileName">The name of the file to move. Can include a relative or absolute path.</param>
        /// <param name="destFileName">The new path and name for the file.</param>
        // Moves a specified file to a new location and potentially a new file name.
        // This method does work across volumes.
        public static void Move(IAdbClient client, DeviceData device, string sourceFileName, string destFileName)
            => Move(client, device, sourceFileName, destFileName, false);

        /// <summary>
        /// Moves a specified file to a new location, providing the options to specify a new file name and to overwrite the destination file if it already exists.
        /// </summary>
        /// <param name="client">The connection to the adb server.</param>
        /// <param name="device">The device on which to run the command.</param>
        /// <param name="sourceFileName">The name of the file to move. Can include a relative or absolute path.</param>
        /// <param name="destFileName">The new path and name for the file.</param>
        /// <param name="overwrite"><see langword="true"/> to overwrite the destination file if it already exists; <see langword="false"/> otherwise.</param>
        public static void Move(IAdbClient client, DeviceData device, string sourceFileName, string destFileName, bool overwrite)
        {
            if (sourceFileName == null)
            {
                throw new ArgumentNullException(nameof(sourceFileName));
            }

            if (destFileName == null)
            {
                throw new ArgumentNullException(nameof(destFileName));
            }

            string fullSourceFileName = LinuxPath.GetFullPath(sourceFileName);
            string fullDestFileName = LinuxPath.GetFullPath(destFileName);

            if (!AdbFileSystem.FileExists(client, device, fullSourceFileName))
            {
                throw new FileNotFoundException(string.Format("Could not find file '{0}'.", fullSourceFileName), fullSourceFileName);
            }

            AdbFileSystem.MoveFile(client, device, fullSourceFileName, fullDestFileName, overwrite);
        }

    }
}
