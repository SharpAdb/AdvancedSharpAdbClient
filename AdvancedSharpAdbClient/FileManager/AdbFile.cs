using AdvancedSharpAdbClient.DeviceCommands;
using AdvancedSharpAdbClient.Exceptions;
using Microsoft.Win32.SafeHandles;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
        /// Opens a text file, reads all the text in the file, and then closes the file.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="device"></param>
        /// <param name="path">The file to open for reading.</param>
        /// <returns>The file to open for reading.</returns>
        public static string ReadAllText(IAdbClient client, DeviceData device, string path)
            => ReadAllText(client, device, path, AdbClient.Encoding);

        /// <summary>
        /// Opens a file, reads all text in the file with the specified encoding, and then closes the file.
        /// </summary>
        /// <param name="client">The connection to the adb server.</param>
        /// <param name="device">The device on which to run the command.</param>
        /// <param name="path">The file to open for reading.</param>
        /// <param name="encoding">The encoding applied to the contents of the file.</param>
        /// <returns>A string containing all text in the file.</returns>
        public static string ReadAllText(IAdbClient client, DeviceData device, string path, Encoding encoding)
        {
            Validate(path, encoding);
            if (!Exists(client, device, path))
            {
                throw new FileNotFoundException(string.Format("Could not find file '{0}'.", path), path);
            }
            ConsoleOutputReceiver receiver = new ConsoleOutputReceiver();
            client.ExecuteRemoteCommand($"cat \"{path}\"", device, receiver, encoding);
            string results = receiver.ToString();
            return results.Substring(0, results.Length - 2);
        }

        /// <summary>
        /// Creates a new file, writes the specified string to the file, and then closes the file. If the target file already exists, it is overwritten.
        /// </summary>
        /// <param name="client">The connection to the adb server.</param>
        /// <param name="device">The device on which to run the command.</param>
        /// <param name="path">The file to write to.</param>
        /// <param name="contents">The string to write to the file.</param>
        public static void WriteAllText(IAdbClient client, DeviceData device, string path, string contents)
            => WriteAllText(client, device, path, contents, Encoding.UTF8);

        /// <summary>
        /// Creates a new file, writes the specified string to the file using the specified encoding, and then closes the file. If the target file already exists, it is overwritten.
        /// </summary>
        /// <param name="client">The connection to the adb server.</param>
        /// <param name="device">The device on which to run the command.</param>
        /// <param name="path">The file to write to.</param>
        /// <param name="contents">The string to write to the file.</param>
        /// <param name="encoding">The encoding to apply to the string.</param>
        public static void WriteAllText(IAdbClient client, DeviceData device, string path, string contents, Encoding encoding)
        {
            Validate(path, encoding);

            WriteToFile(client, device, path, FileMode.Create, contents, encoding);
        }

        /// <summary>
        /// Opens a binary file, reads the contents of the file into a byte array, and then closes the file.
        /// </summary>
        /// <param name="client">The connection to the adb server.</param>
        /// <param name="device">The device on which to run the command.</param>
        /// <param name="path">The file to open for reading.</param>
        /// <returns>A byte array containing the contents of the file.</returns>
        public static byte[] ReadAllBytes(IAdbClient client, DeviceData device, string path)
        {
            if (!Exists(client, device, path))
            {
                throw new FileNotFoundException(string.Format("Could not find file '{0}'.", path), path);
            }
            ByteReceiver receiver = new ByteReceiver();
            client.ExecuteRemoteCommand($"cat \"{path}\"", device, receiver);
            return receiver.Output;
        }

        /// <summary>
        /// Creates a new file, writes the specified byte array to the file, and then closes the file. If the target file already exists, it is overwritten.
        /// </summary>
        /// <param name="client">The connection to the adb server.</param>
        /// <param name="device">The device on which to run the command.</param>
        /// <param name="path">The file to write to.</param>
        /// <param name="bytes">The bytes to write to the file.</param>
        public static void WriteAllBytes(IAdbClient client, DeviceData device, string path, byte[] bytes)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException(nameof(path));
            }
            if (bytes == null)
            {
                throw new ArgumentNullException(nameof(bytes));
            }

            string result = string.Empty;
            for (int i = 0; i < bytes.Length; i++)
            {
                result += @$"\x{Convert.ToString(bytes[i], 16)}";
            }

            ConsoleOutputReceiver receiver = new ConsoleOutputReceiver();
            client.ExecuteRemoteCommand($"echo -e -n \"{result}\" > \"{path}\"", device, receiver);
            string results = receiver.ToString().Trim();
            if (results.Length > 0)
            {
                throw new FileNotFoundException(results.Split(':').Last().Trim());
            }
        }

        /// <summary>
        /// Opens a text file, reads all lines of the file, and then closes the file.
        /// </summary>
        /// <param name="client">The connection to the adb server.</param>
        /// <param name="device">The device on which to run the command.</param>
        /// <param name="path">The file to open for reading.</param>
        /// <returns>A string array containing all lines of the file.</returns>
        public static string[] ReadAllLines(IAdbClient client, DeviceData device, string path)
            => ReadAllLines(client, device, path, AdbClient.Encoding);

        /// <summary>
        /// Opens a file, reads all lines of the file with the specified encoding, and then closes the file.
        /// </summary>
        /// <param name="client">The connection to the adb server.</param>
        /// <param name="device">The device on which to run the command.</param>
        /// <param name="path">The file to open for reading.</param>
        /// <param name="encoding">The encoding applied to the contents of the file.</param>
        /// <returns>The encoding applied to the contents of the file.</returns>
        public static string[] ReadAllLines(IAdbClient client, DeviceData device, string path, Encoding encoding)
        {
            Validate(path, encoding);
            if (!Exists(client, device, path))
            {
                throw new FileNotFoundException(string.Format("Could not find file '{0}'.", path), path);
            }
            LinesCollectionReceiver receiver = new LinesCollectionReceiver();
            client.ExecuteRemoteCommand($"cat \"{path}\"", device, receiver, encoding);
            return receiver.Lines.ToArray();
        }

        /// <summary>
        /// Reads the lines of a file.
        /// </summary>
        /// <param name="client">The connection to the adb server.</param>
        /// <param name="device">The device on which to run the command.</param>
        /// <param name="path">The file to read.</param>
        /// <returns>All the lines of the file, or the lines that are the result of a query.</returns>
        public static IEnumerable<string> ReadLines(IAdbClient client, DeviceData device, string path)
            => ReadLines(client, device, path, AdbClient.Encoding);

        /// <summary>
        /// Read the lines of a file that has a specified encoding.
        /// </summary>
        /// <param name="client">The connection to the adb server.</param>
        /// <param name="device">The device on which to run the command.</param>
        /// <param name="path">The file to read.</param>
        /// <param name="encoding">The encoding that is applied to the contents of the file.</param>
        /// <returns>All the lines of the file, or the lines that are the result of a query.</returns>
        /// <exception cref="FileNotFoundException"></exception>
        public static IEnumerable<string> ReadLines(IAdbClient client, DeviceData device, string path, Encoding encoding)
        {
            Validate(path, encoding);
            if (!Exists(client, device, path))
            {
                throw new FileNotFoundException(string.Format("Could not find file '{0}'.", path), path);
            }
            LinesCollectionReceiver receiver = new LinesCollectionReceiver();
            client.ExecuteRemoteCommand($"cat \"{path}\"", device, receiver, encoding);
            return receiver.Lines;
        }

        /// <summary>
        /// Asynchronously reads the lines of a file.
        /// </summary>
        /// <param name="client">The connection to the adb server.</param>
        /// <param name="device">The device on which to run the command.</param>
        /// <param name="path">The file to read.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>The async enumerable that represents all the lines of the file, or the lines that are the result of a query.</returns>
        public static Task<IEnumerable<string>> ReadLinesAsync(IAdbClient client, DeviceData device, string path, CancellationToken cancellationToken = default)
            => ReadLinesAsync(client, device, path, AdbClient.Encoding, cancellationToken);

        /// <summary>
        /// Asynchronously reads the lines of a file that has a specified encoding.
        /// </summary>
        /// <param name="client">The connection to the adb server.</param>
        /// <param name="device">The device on which to run the command.</param>
        /// <param name="path">The file to read.</param>
        /// <param name="encoding">The encoding that is applied to the contents of the file.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>The async enumerable that represents all the lines of the file, or the lines that are the result of a query.</returns>
        public static async Task<IEnumerable<string>> ReadLinesAsync(IAdbClient client, DeviceData device, string path, Encoding encoding, CancellationToken cancellationToken = default)
        {
            Validate(path, encoding);
            if (!Exists(client, device, path))
            {
                throw new FileNotFoundException(string.Format("Could not find file '{0}'.", path), path);
            }
            LinesCollectionReceiver receiver = new LinesCollectionReceiver();
            await client.ExecuteRemoteCommandAsync($"cat \"{path}\"", device, receiver, encoding, cancellationToken);
            return receiver.Lines;
        }

        /// <summary>
        /// Creates a new file, write the specified string array to the file, and then closes the file.
        /// </summary>
        /// <param name="client">The connection to the adb server.</param>
        /// <param name="device">The device on which to run the command.</param>
        /// <param name="path">The file to write to.</param>
        /// <param name="contents">The string array to write to the file.</param>
        public static void WriteAllLines(IAdbClient client, DeviceData device, string path, string[] contents)
            => WriteAllLines(client, device, path, (IEnumerable<string>)contents);

        /// <summary>
        /// Creates a new file, writes a collection of strings to the file, and then closes the file.
        /// </summary>
        /// <param name="client">The connection to the adb server.</param>
        /// <param name="device">The device on which to run the command.</param>
        /// <param name="path">The file to write to.</param>
        /// <param name="contents">The lines to write to the file.</param>
        public static void WriteAllLines(IAdbClient client, DeviceData device, string path, IEnumerable<string> contents)
            => WriteAllLines(client, device, path, contents, Encoding.UTF8);

        /// <summary>
        /// Creates a new file, writes the specified string array to the file by using the specified encoding, and then closes the file.
        /// </summary>
        /// <param name="client">The connection to the adb server.</param>
        /// <param name="device">The device on which to run the command.</param>
        /// <param name="path">The file to write to.</param>
        /// <param name="contents">The string array to write to the file.</param>
        /// <param name="encoding">An <see cref="Encoding"/> object that represents the character encoding applied to the string array.</param>
        public static void WriteAllLines(IAdbClient client, DeviceData device, string path, string[] contents, Encoding encoding)
            => WriteAllLines(client, device, path, (IEnumerable<string>)contents, encoding);

        /// <summary>
        /// Creates a new file by using the specified encoding, writes a collection of strings to the file, and then closes the file.
        /// </summary>
        /// <param name="client">The connection to the adb server.</param>
        /// <param name="device">The device on which to run the command.</param>
        /// <param name="path">The file to write to.</param>
        /// <param name="contents">The lines to write to the file.</param>
        /// <param name="encoding">The character encoding to use.</param>
        public static void WriteAllLines(IAdbClient client, DeviceData device, string path, IEnumerable<string> contents, Encoding encoding)
        {
            Validate(path, encoding);
            if (contents == null)
            {
                throw new ArgumentNullException(nameof(contents));
            }
            InternalWriteAllLines(client, device, path, false, encoding, contents);
        }

        private static void InternalWriteAllLines(IAdbClient client, DeviceData device, string path, bool isAppend, Encoding encoding, IEnumerable<string> contents)
        {
            Debug.Assert(contents != null);

            ConsoleOutputReceiver receiver = new ConsoleOutputReceiver();
            client.ExecuteRemoteCommand($"echo -n {(isAppend ? "\"\n\" >>" : "\"\" >")} \"{path}\"", device, receiver);
            string results = receiver.ToString().Trim();
            if (results.Length > 0)
            {
                throw new FileNotFoundException(results.Split(':').Last().Trim());
            }

            foreach (string line in contents)
            {
                byte[] bytes = encoding.GetBytes(line);
                string result = string.Empty;
                for (int i = 0; i < bytes.Length; i++)
                {
                    result += @$"\x{Convert.ToString(bytes[i], 16)}";
                }
                receiver = new ConsoleOutputReceiver();
                client.ExecuteRemoteCommand($"echo -e \"{result}\" >> \"{path}\"", device, receiver);
                results = receiver.ToString().Trim();
                if (results.Length > 0)
                {
                    throw new FileNotFoundException(results.Split(':').Last().Trim());
                }
            }
        }

        /// <summary>
        /// Opens a file, appends the specified string to the file, and then closes the file. If the file does not exist, this method creates a file, writes the specified string to the file, then closes the file.
        /// </summary>
        /// <param name="client">The connection to the adb server.</param>
        /// <param name="device">The device on which to run the command.</param>
        /// <param name="path">The file to append the specified string to.</param>
        /// <param name="contents">The string to append to the file.</param>
        public static void AppendAllText(IAdbClient client, DeviceData device, string path, string contents)
            => AppendAllText(client, device, path, contents, Encoding.UTF8);

        /// <summary>
        /// Appends the specified string to the file using the specified encoding, creating the file if it does not already exist.
        /// </summary>
        /// <param name="client">The connection to the adb server.</param>
        /// <param name="device">The device on which to run the command.</param>
        /// <param name="path">The file to append the specified string to.</param>
        /// <param name="contents">The string to append to the file.</param>
        /// <param name="encoding">The character encoding to use.</param>
        public static void AppendAllText(IAdbClient client, DeviceData device, string path, string contents, Encoding encoding)
        {
            Validate(path, encoding);

            WriteToFile(client, device, path, FileMode.Append, $"\n{contents}", encoding);
        }

        /// <summary>
        /// Appends lines to a file, and then closes the file. If the specified file does not exist, this method creates a file, writes the specified lines to the file, and then closes the file.
        /// </summary>
        /// <param name="client">The connection to the adb server.</param>
        /// <param name="device">The device on which to run the command.</param>
        /// <param name="path">The file to append the lines to. The file is created if it doesn't already exist.</param>
        /// <param name="contents">The lines to append to the file.</param>
        public static void AppendAllLines(IAdbClient client, DeviceData device, string path, IEnumerable<string> contents)
            => AppendAllLines(client, device, path, contents, Encoding.UTF8);

        /// <summary>
        /// Appends lines to a file by using a specified encoding, and then closes the file. If the specified file does not exist, this method creates a file, writes the specified lines to the file, and then closes the file.
        /// </summary>
        /// <param name="client">The connection to the adb server.</param>
        /// <param name="device">The device on which to run the command.</param>
        /// <param name="path">The file to append the lines to. The file is created if it doesn't already exist.</param>
        /// <param name="contents">The lines to append to the file.</param>
        /// <param name="encoding">The character encoding to use.</param>
        public static void AppendAllLines(IAdbClient client, DeviceData device, string path, IEnumerable<string> contents, Encoding encoding)
        {
            Validate(path, encoding);
            if (contents == null)
            {
                throw new ArgumentNullException(nameof(contents));
            }
            InternalWriteAllLines(client, device, path, true, encoding, contents);
        }

        /// <summary>
        /// Replaces the contents of a specified file with the contents of another file, deleting the original file, and creating a backup of the replaced file.
        /// </summary>
        /// <param name="client">The connection to the adb server.</param>
        /// <param name="device">The device on which to run the command.</param>
        /// <param name="sourceFileName">The name of a file that replaces the file specified by <paramref name="destinationFileName"/>.</param>
        /// <param name="destinationFileName">The name of the file being replaced.</param>
        /// <param name="destinationBackupFileName">The name of the backup file.</param>
#nullable enable
        public static void Replace(IAdbClient client, DeviceData device, string sourceFileName, string destinationFileName, string? destinationBackupFileName)
#nullable disable
        {
            if (sourceFileName == null)
            {
                throw new ArgumentNullException(nameof(sourceFileName));
            }
            if (destinationFileName == null)
            {
                throw new ArgumentNullException(nameof(destinationFileName));
            }

            AdbFileSystem.ReplaceFile(
                client,
                device,
                LinuxPath.GetFullPath(sourceFileName),
                LinuxPath.GetFullPath(destinationFileName),
                destinationBackupFileName != null ? LinuxPath.GetFullPath(destinationBackupFileName) : null);
        }

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

        /// <summary>
        /// Asynchronously opens a text file, reads all the text in the file, and then closes the file.
        /// </summary>
        /// <param name="client">The connection to the adb server.</param>
        /// <param name="device">The device on which to run the command.</param>
        /// <param name="path">The file to open for reading.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A task that represents the asynchronous read operation, which wraps the string containing all text in the file.</returns>
        public static Task<string> ReadAllTextAsync(IAdbClient client, DeviceData device, string path, CancellationToken cancellationToken = default(CancellationToken))
            => ReadAllTextAsync(client, device, path, AdbClient.Encoding, cancellationToken);

        /// <summary>
        /// Asynchronously opens a text file, reads all text in the file with the specified encoding, and then closes the file.
        /// </summary>
        /// <param name="client">The connection to the adb server.</param>
        /// <param name="device">The device on which to run the command.</param>
        /// <param name="path">The file to open for reading.</param>
        /// <param name="encoding">The encoding applied to the contents of the file.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A task that represents the asynchronous read operation, which wraps the string containing all text in the file.</returns>
        public static Task<string> ReadAllTextAsync(IAdbClient client, DeviceData device, string path, Encoding encoding, CancellationToken cancellationToken = default(CancellationToken))
        {
            Validate(path, encoding);

#if !NETFRAMEWORK || NET46_OR_GREATER
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled<string>(cancellationToken);
            }
#endif

            return InternalReadAllTextAsync(client, device, path, encoding, cancellationToken);
        }

        private static async Task<string> InternalReadAllTextAsync(IAdbClient client, DeviceData device, string path, Encoding encoding, CancellationToken cancellationToken)
        {
            Debug.Assert(!string.IsNullOrEmpty(path));
            Debug.Assert(encoding != null);

            cancellationToken.ThrowIfCancellationRequested();
            if (!Exists(client, device, path))
            {
                throw new FileNotFoundException(string.Format("Could not find file '{0}'.", path), path);
            }
            ConsoleOutputReceiver receiver = new ConsoleOutputReceiver();
            await client.ExecuteRemoteCommandAsync($"cat \"{path}\"", device, receiver, encoding, cancellationToken);
            string results = receiver.ToString();
            return results.Substring(0, results.Length - 2);
        }

        /// <summary>
        /// Asynchronously creates a new file, writes the specified string to the file, and then closes the file. If the target file already exists, it is overwritten.
        /// </summary>
        /// <param name="client">The connection to the adb server.</param>
        /// <param name="device">The device on which to run the command.</param>
        /// <param name="path">The file to write to.</param>
        /// <param name="contents">The string to write to the file.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A task that represents the asynchronous write operation.</returns>
        public static Task WriteAllTextAsync(IAdbClient client, DeviceData device, string path, string contents, CancellationToken cancellationToken = default(CancellationToken))
            => WriteAllTextAsync(client, device, path, contents, Encoding.UTF8, cancellationToken);

        /// <summary>
        /// Asynchronously creates a new file, writes the specified string to the file using the specified encoding, and then closes the file. If the target file already exists, it is overwritten.
        /// </summary>
        /// <param name="client">The connection to the adb server.</param>
        /// <param name="device">The device on which to run the command.</param>
        /// <param name="path">The file to write to.</param>
        /// <param name="contents">The string to write to the file.</param>
        /// <param name="encoding">The encoding to apply to the string.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A task that represents the asynchronous write operation.</returns>
        public static Task WriteAllTextAsync(IAdbClient client, DeviceData device, string path, string contents, Encoding encoding, CancellationToken cancellationToken = default(CancellationToken))
        {
            Validate(path, encoding);

#if !NETFRAMEWORK || NET46_OR_GREATER
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled(cancellationToken);
            }
#endif

            return WriteToFileAsync(client, device, path, FileMode.Create, contents, encoding, cancellationToken);
        }

        /// <summary>
        /// Asynchronously opens a binary file, reads the contents of the file into a byte array, and then closes the file.
        /// </summary>
        /// <param name="client">The connection to the adb server.</param>
        /// <param name="device">The device on which to run the command.</param>
        /// <param name="path">The file to open for reading.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A task that represents the asynchronous read operation, which wraps the byte array containing the contents of the file.</returns>
        public static async Task<byte[]> ReadAllBytesAsync(IAdbClient client, DeviceData device, string path, CancellationToken cancellationToken = default(CancellationToken))
        {
#if !NETFRAMEWORK || NET46_OR_GREATER
            if (cancellationToken.IsCancellationRequested)
            {
                return await Task.FromCanceled<byte[]>(cancellationToken);
            }
#endif
            if (!Exists(client, device, path))
            {
                throw new FileNotFoundException(string.Format("Could not find file '{0}'.", path), path);
            }
            ByteReceiver receiver = new ByteReceiver();
            await client.ExecuteRemoteCommandAsync($"cat \"{path}\"", device, receiver, cancellationToken);
            return receiver.Output;
        }

        /// <summary>
        /// Asynchronously creates a new file, writes the specified byte array to the file, and then closes the file. If the target file already exists, it is overwritten.
        /// </summary>
        /// <param name="client">The connection to the adb server.</param>
        /// <param name="device">The device on which to run the command.</param>
        /// <param name="path">The file to write to.</param>
        /// <param name="bytes">The bytes to write to the file.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A task that represents the asynchronous write operation.</returns>
        public static Task WriteAllBytesAsync(IAdbClient client, DeviceData device, string path, byte[] bytes, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException(nameof(path));
            }
            if (bytes == null)
            {
                throw new ArgumentNullException(nameof(bytes));
            }


#if !NETFRAMEWORK || NET46_OR_GREATER
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled<byte[]>(cancellationToken);
            }
#endif
            return Core(client, device, path, bytes, cancellationToken);

            static async Task Core(IAdbClient client, DeviceData device, string path, byte[] bytes, CancellationToken cancellationToken)
            {
                string result = string.Empty;
                await Utilities.Run(() =>
                {
                    for (int i = 0; i < bytes.Length; i++)
                    {
                        result += @$"\x{Convert.ToString(bytes[i], 16)}";
                    }
                }, cancellationToken);

                ConsoleOutputReceiver receiver = new ConsoleOutputReceiver();
                await client.ExecuteRemoteCommandAsync($"echo -e -n \"{result}\" > \"{path}\"", device, receiver, cancellationToken);
                string results = receiver.ToString().Trim();
                if (results.Length > 0)
                {
                    throw new FileNotFoundException(results.Split(':').Last().Trim());
                }
            }
        }

        /// <summary>
        /// Asynchronously opens a text file, reads all lines of the file, and then closes the file.
        /// </summary>
        /// <param name="client">The connection to the adb server.</param>
        /// <param name="device">The device on which to run the command.</param>
        /// <param name="path">The file to open for reading.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A task that represents the asynchronous read operation, which wraps the string array containing all lines of the file.</returns>
        public static Task<string[]> ReadAllLinesAsync(IAdbClient client, DeviceData device, string path, CancellationToken cancellationToken = default(CancellationToken))
            => ReadAllLinesAsync(client, device, path, AdbClient.Encoding, cancellationToken);

        /// <summary>
        /// Asynchronously opens a text file, reads all lines of the file with the specified encoding, and then closes the file.
        /// </summary>
        /// <param name="client">The connection to the adb server.</param>
        /// <param name="device">The device on which to run the command.</param>
        /// <param name="path">The file to open for reading.</param>
        /// <param name="encoding">The encoding applied to the contents of the file.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A task that represents the asynchronous read operation, which wraps the string array containing all lines of the file.</returns>
        public static Task<string[]> ReadAllLinesAsync(IAdbClient client, DeviceData device, string path, Encoding encoding, CancellationToken cancellationToken = default(CancellationToken))
        {
            Validate(path, encoding);

#if !NETFRAMEWORK || NET46_OR_GREATER
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled<string[]>(cancellationToken);
            }
#endif

            return InternalReadAllLinesAsync(client, device, path, encoding, cancellationToken);
        }

        private static async Task<string[]> InternalReadAllLinesAsync(IAdbClient client, DeviceData device, string path, Encoding encoding, CancellationToken cancellationToken)
        {
            Debug.Assert(!string.IsNullOrEmpty(path));
            Debug.Assert(encoding != null);

            cancellationToken.ThrowIfCancellationRequested();
            if (!Exists(client, device, path))
            {
                throw new FileNotFoundException(string.Format("Could not find file '{0}'.", path), path);
            }
            LinesCollectionReceiver receiver = new LinesCollectionReceiver();
            await client.ExecuteRemoteCommandAsync($"cat \"{path}\"", device, receiver, encoding, cancellationToken);
            return receiver.Lines.ToArray();
        }

        /// <summary>
        /// Asynchronously creates a new file, writes the specified lines to the file, and then closes the file.
        /// </summary>
        /// <param name="client">The connection to the adb server.</param>
        /// <param name="device">The device on which to run the command.</param>
        /// <param name="path">The file to write to.</param>
        /// <param name="contents">The lines to write to the file.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A task that represents the asynchronous write operation.</returns>
        public static Task WriteAllLinesAsync(IAdbClient client, DeviceData device, string path, IEnumerable<string> contents, CancellationToken cancellationToken = default(CancellationToken))
            => WriteAllLinesAsync(client, device, path, contents, Encoding.UTF8, cancellationToken);

        /// <summary>
        /// Asynchronously creates a new file, write the specified lines to the file by using the specified encoding, and then closes the file.
        /// </summary>
        /// <param name="client">The connection to the adb server.</param>
        /// <param name="device">The device on which to run the command.</param>
        /// <param name="path">The file to write to.</param>
        /// <param name="contents">The lines to write to the file.</param>
        /// <param name="encoding">The character encoding to use.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A task that represents the asynchronous write operation.</returns>
        public static Task WriteAllLinesAsync(IAdbClient client, DeviceData device, string path, IEnumerable<string> contents, Encoding encoding, CancellationToken cancellationToken = default(CancellationToken))
        {
            Validate(path, encoding);
            if (contents == null)
            {
                throw new ArgumentNullException(nameof(contents));
            }
#if !NETFRAMEWORK || NET46_OR_GREATER
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled<byte[]>(cancellationToken);
            }
#endif
            return InternalWriteAllLinesAsync(client, device, path, false, encoding, contents, cancellationToken);
        }

        private static async Task InternalWriteAllLinesAsync(IAdbClient client, DeviceData device, string path, bool isAppend, Encoding encoding, IEnumerable<string> contents, CancellationToken cancellationToken)
        {
            Debug.Assert(contents != null);

            ConsoleOutputReceiver receiver = new ConsoleOutputReceiver();
            await client.ExecuteRemoteCommandAsync($"echo -n {(isAppend ? "\"\n\" >>" : "\"\" >")} \"{path}\"", device, receiver, cancellationToken);
            string results = receiver.ToString().Trim();
            if (results.Length > 0)
            {
                throw new FileNotFoundException(results.Split(':').Last().Trim());
            }

            cancellationToken.ThrowIfCancellationRequested();

            foreach (string line in contents)
            {
                cancellationToken.ThrowIfCancellationRequested();
                byte[] bytes = encoding.GetBytes(line);
                string result = string.Empty;
                await Utilities.Run(() =>
                {
                    for (int i = 0; i < bytes.Length; i++)
                    {
                        result += @$"\x{Convert.ToString(bytes[i], 16)}";
                    }
                }, cancellationToken);
                receiver = new ConsoleOutputReceiver();
                await client.ExecuteRemoteCommandAsync($"echo -e \"{result}\" >> \"{path}\"", device, receiver, cancellationToken);
                results = receiver.ToString().Trim();
                if (results.Length > 0)
                {
                    throw new FileNotFoundException(results.Split(':').Last().Trim());
                }
            }
        }

        /// <summary>
        /// Asynchronously opens a file or creates a file if it does not already exist, appends the specified string to the file, and then closes the file.
        /// </summary>
        /// <param name="client">The connection to the adb server.</param>
        /// <param name="device">The device on which to run the command.</param>
        /// <param name="path">The file to append the specified string to.</param>
        /// <param name="contents">The string to append to the file.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A task that represents the asynchronous write operation.</returns>
        public static Task AppendAllTextAsync(IAdbClient client, DeviceData device, string path, string contents, CancellationToken cancellationToken = default(CancellationToken))
            => AppendAllTextAsync(client, device, path, contents, Encoding.UTF8, cancellationToken);

        /// <summary>
        /// Asynchronously opens a file or creates the file if it does not already exist, appends the specified string to the file using the specified encoding, and then closes the file.
        /// </summary>
        /// <param name="client">The connection to the adb server.</param>
        /// <param name="device">The device on which to run the command.</param>
        /// <param name="path">The file to append the specified string to.</param>
        /// <param name="contents">The string to append to the file.</param>
        /// <param name="encoding">The character encoding to use.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A task that represents the asynchronous write operation.</returns>
        public static Task AppendAllTextAsync(IAdbClient client, DeviceData device, string path, string contents, Encoding encoding, CancellationToken cancellationToken = default(CancellationToken))
        {
            Validate(path, encoding);

#if !NETFRAMEWORK || NET46_OR_GREATER
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled<byte[]>(cancellationToken);
            }
#endif

            return WriteToFileAsync(client, device, path, FileMode.Append, contents, encoding, cancellationToken);
        }

        /// <summary>
        /// Asynchronously appends lines to a file, and then closes the file. If the specified file does not exist, this method creates a file, writes the specified lines to the file, and then closes the file.
        /// </summary>
        /// <param name="client">The connection to the adb server.</param>
        /// <param name="device">The device on which to run the command.</param>
        /// <param name="path">The file to append the lines to. The file is created if it doesn't already exist.</param>
        /// <param name="contents">The lines to append to the file.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A task that represents the asynchronous write operation.</returns>
        public static Task AppendAllLinesAsync(IAdbClient client, DeviceData device, string path, IEnumerable<string> contents, CancellationToken cancellationToken = default(CancellationToken))
            => AppendAllLinesAsync(client, device, path, contents, Encoding.UTF8, cancellationToken);

        /// <summary>
        /// Asynchronously appends lines to a file by using a specified encoding, and then closes the file. If the specified file does not exist, this method creates a file, writes the specified lines to the file, and then closes the file.
        /// </summary>
        /// <param name="client">The connection to the adb server.</param>
        /// <param name="device">The device on which to run the command.</param>
        /// <param name="path">The file to append the lines to. The file is created if it doesn't already exist.</param>
        /// <param name="contents">The lines to append to the file.</param>
        /// <param name="encoding">The character encoding to use.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A task that represents the asynchronous write operation.</returns>
        public static Task AppendAllLinesAsync(IAdbClient client, DeviceData device, string path, IEnumerable<string> contents, Encoding encoding, CancellationToken cancellationToken = default(CancellationToken))
        {
            Validate(path, encoding);
            if (contents == null)
            {
                throw new ArgumentNullException(nameof(contents));
            }
#if !NETFRAMEWORK || NET46_OR_GREATER
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled<byte[]>(cancellationToken);
            }
#endif
            return InternalWriteAllLinesAsync(client, device, path, true, encoding, contents, cancellationToken);
        }

        private static void Validate(string path, Encoding encoding)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException(nameof(path));
            }
            if (encoding == null)
            {
                throw new ArgumentNullException(nameof(encoding));
            }
        }

        private static void WriteToFile(IAdbClient client, DeviceData device, string path, FileMode mode, string contents, Encoding encoding)
        {
            if (string.IsNullOrEmpty(contents)) { return; }

            byte[] bytes = encoding.GetBytes(contents);
            string result = string.Empty;
            for (int i = 0; i < bytes.Length; i++)
            {
                result += @$"\x{Convert.ToString(bytes[i], 16)}";
            }

            ConsoleOutputReceiver receiver = new ConsoleOutputReceiver();
            client.ExecuteRemoteCommand($"echo -e -n \"{result}\" {(mode != FileMode.Append ? ">" : ">>")} \"{path}\"", device, receiver);
            string results = receiver.ToString().Trim();
            if (results.Length > 0)
            {
                throw new FileNotFoundException(results.Split(':').Last().Trim());
            }
        }

        private static async Task WriteToFileAsync(IAdbClient client, DeviceData device, string path, FileMode mode, string contents, Encoding encoding, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(contents)) { return; }

            byte[] bytes = encoding.GetBytes(contents);
            string result = string.Empty;
            await Utilities.Run(() =>
            {
                for (int i = 0; i < bytes.Length; i++)
                {
                    result += @$"\x{Convert.ToString(bytes[i], 16)}";
                }
            }, cancellationToken);

            ConsoleOutputReceiver receiver = new ConsoleOutputReceiver();
            await client.ExecuteRemoteCommandAsync($"echo -e -n \"{result}\" {(mode != FileMode.Append ? ">" : ">>")} \"{path}\"", device, receiver, cancellationToken);
            string results = receiver.ToString().Trim();
            if (results.Length > 0)
            {
                throw new FileNotFoundException(results.Split(':').Last().Trim());
            }
        }
    }
}
