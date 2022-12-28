using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace AdvancedSharpAdbClient.FileManager
{
    internal class AdbFileSystem
    {
        internal const UnixFileMode ValidUnixFileModes =
            UnixFileMode.UserRead |
            UnixFileMode.UserWrite |
            UnixFileMode.UserExecute |
            UnixFileMode.GroupRead |
            UnixFileMode.GroupWrite |
            UnixFileMode.GroupExecute |
            UnixFileMode.OtherRead |
            UnixFileMode.OtherWrite |
            UnixFileMode.OtherExecute |
            UnixFileMode.StickyBit |
            UnixFileMode.SetGroup |
            UnixFileMode.SetUser;

        internal static void VerifyValidPath(string path, string argName)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }
            if (argName == null)
            {
                throw new ArgumentNullException(nameof(argName));
            }
            if (path.Contains('\0'))
            {
                throw new ArgumentException("Path contains invalid characters", argName);
            }
        }

        // This gets filtered by umask.
        internal const UnixFileMode DefaultUnixCreateDirectoryMode =
            UnixFileMode.UserRead |
            UnixFileMode.UserWrite |
            UnixFileMode.UserExecute |
            UnixFileMode.GroupRead |
            UnixFileMode.GroupWrite |
            UnixFileMode.GroupExecute |
            UnixFileMode.OtherRead |
            UnixFileMode.OtherWrite |
            UnixFileMode.OtherExecute;

        public static void CopyFile(IAdbClient client, DeviceData device, string sourceFullPath, string destFullPath, bool overwrite)
        {
            ConsoleOutputReceiver receiver = new ConsoleOutputReceiver();
            client.ExecuteRemoteCommand($"cp {(overwrite ? "-f " : "")}\"{sourceFullPath}\" \"{destFullPath}\"", device, receiver);
            string results = receiver.ToString().Trim();
            if (results.Length > 0)
            {
                throw new FileNotFoundException(results.Split(':').Last().Trim());
            }
        }

        private static void LinkOrCopyFile(IAdbClient client, DeviceData device, string sourceFullPath, string destFullPath)
        {
            ConsoleOutputReceiver receiver = new ConsoleOutputReceiver();
            client.ExecuteRemoteCommand($"ln \"{sourceFullPath}\" \"{destFullPath}\"", device, receiver);
            if (receiver.ToString().Trim().Length <= 0) { return; }
            CopyFile(client, device, sourceFullPath, destFullPath, overwrite: false);
        }

#nullable enable
        public static void ReplaceFile(IAdbClient client, DeviceData device, string sourceFullPath, string destFullPath, string? destBackupFullPath)
#nullable disable
        {
            if (destBackupFullPath != null)
            {
                LinkOrCopyFile(client, device, sourceFullPath, destBackupFullPath);
            }
            ConsoleOutputReceiver receiver = new ConsoleOutputReceiver();
            client.ExecuteRemoteCommand($"mv -f \"{sourceFullPath}\" \"{destFullPath}\"", device, receiver);
            string results = receiver.ToString().Trim();
            if (results.Length > 0)
            {
                throw new FileNotFoundException(results.Split(':').Last().Trim());
            }
        }

        public static void MoveFile(IAdbClient client, DeviceData device, string sourceFullPath, string destFullPath)
        {
            MoveFile(client, device, sourceFullPath, destFullPath, false);
        }

        public static void MoveFile(IAdbClient client, DeviceData device, string sourceFullPath, string destFullPath, bool overwrite)
        {
            ConsoleOutputReceiver receiver = new ConsoleOutputReceiver();
            client.ExecuteRemoteCommand($"mv {(overwrite ? "-f" : "-n")} \"{sourceFullPath}\" \"{destFullPath}\"", device, receiver);
            string results = receiver.ToString().Trim();
            if (results.Length > 0)
            {
                throw new FileNotFoundException(results.Split(':').Last().Trim());
            }
        }

        public static void DeleteFile(IAdbClient client, DeviceData device, string fullPath)
        {
            ConsoleOutputReceiver receiver = new ConsoleOutputReceiver();
            client.ExecuteRemoteCommand($"rm \"{fullPath}\"", device, receiver);
            string results = receiver.ToString().Trim();
            if (results.Length > 0 && !results.EndsWith("No such file or directory"))
            {
                throw new FileNotFoundException(results.Split(':').Last().Trim());
            }
        }

        public static void CreateDirectory(IAdbClient client, DeviceData device, string fullPath)
            => CreateDirectory(client, device, fullPath, DefaultUnixCreateDirectoryMode);

        public static void CreateDirectory(IAdbClient client, DeviceData device, string fullPath, UnixFileMode unixCreateMode)
        {
            ConsoleOutputReceiver receiver = new ConsoleOutputReceiver();
            client.ExecuteRemoteCommand($"mkdir -m {Convert.ToString((int)unixCreateMode, 8)} \"{fullPath}\"", device, receiver);
            string results = receiver.ToString().Trim();
            if (results.Length > 0)
            {
                throw new FileNotFoundException(results.Split(':').Last().Trim());
            }
        }

        private static void CreateParentsAndDirectory(IAdbClient client, DeviceData device, string fullPath, UnixFileMode unixCreateMode)
        {
            ConsoleOutputReceiver receiver = new ConsoleOutputReceiver();
            client.ExecuteRemoteCommand($"mkdir -p -m {Convert.ToString((int)unixCreateMode, 8)} \"{fullPath}\"", device, receiver);
            string results = receiver.ToString().Trim();
            if (results.Length > 0)
            {
                throw new FileNotFoundException(results.Split(':').Last().Trim());
            }
        }

        internal static void MoveDirectory(IAdbClient client, DeviceData device, string sourceFullPath, string destFullPath)
        {
            ConsoleOutputReceiver receiver = new ConsoleOutputReceiver();
            client.ExecuteRemoteCommand($"mv -a -f \"{sourceFullPath}\" \"{destFullPath}\"", device, receiver);
            string results = receiver.ToString().Trim();
            if (results.Length > 0)
            {
                throw new FileNotFoundException(results.Split(':').Last().Trim());
            }
        }

        public static void RemoveDirectory(IAdbClient client, DeviceData device, string fullPath)
        {
            ConsoleOutputReceiver receiver = new ConsoleOutputReceiver();
            client.ExecuteRemoteCommand($"rm -r \"{fullPath}\"", device, receiver);
            string results = receiver.ToString().Trim();
            if (results.Length > 0 && !results.EndsWith("No such file or directory"))
            {
                throw new FileNotFoundException(results.Split(':').Last().Trim());
            }
        }

        /// <summary>Determines whether the specified directory name should be ignored.</summary>
        /// <param name="name">The name to evaluate.</param>
        /// <returns>true if the name is "." or ".."; otherwise, false.</returns>
        private static bool ShouldIgnoreDirectory(string name)
        {
            return name == "." || name == "..";
        }

        public static UnixFileMode GetUnixFileMode(IAdbClient client, DeviceData device, string fullPath)
        {
            ConsoleOutputReceiver receiver = new ConsoleOutputReceiver();
            client.ExecuteRemoteCommand($"stat -c '%a' \"{fullPath}\"", device, receiver);
            string results = receiver.ToString().Trim();
            if (results == "?")
            {
                return (UnixFileMode)(-1);
            }
            else if (int.TryParse(results, out _))
            {
                return (UnixFileMode)Convert.ToInt32(results, 8);
            }
            else
            {
                throw new FileNotFoundException(results.Split(':').Last().Trim());
            }
        }

        public static void SetUnixFileMode(IAdbClient client, DeviceData device, string fullPath, UnixFileMode mode)
        {
            ConsoleOutputReceiver receiver = new ConsoleOutputReceiver();
            client.ExecuteRemoteCommand($"chmod {Convert.ToString((int)mode, 8)} \"{fullPath}\"", device, receiver);
            string results = receiver.ToString().Trim();
            if (results.Length > 0)
            {
                throw new FileNotFoundException(results.Split(':').Last().Trim());
            }
        }

        public static DateTimeOffset GetCreationTime(IAdbClient client, DeviceData device, string fullPath)
        {
            ConsoleOutputReceiver receiver = new ConsoleOutputReceiver();
            client.ExecuteRemoteCommand($"stat -c '%W' \"{fullPath}\"", device, receiver);
            string results = receiver.ToString().Trim();
            if (results == "?")
            {
                return DateTimeHelper.ToDateTime(0);
            }
            else if (long.TryParse(results, out long time))
            {
                return DateTimeHelper.ToDateTime(time);
            }
            else
            {
                throw new FileNotFoundException(results.Split(':').Last().Trim());
            }
        }

        public static DateTimeOffset GetLastAccessTime(IAdbClient client, DeviceData device, string fullPath)
        {
            ConsoleOutputReceiver receiver = new ConsoleOutputReceiver();
            client.ExecuteRemoteCommand($"stat -c '%X' \"{fullPath}\"", device, receiver);
            string results = receiver.ToString().Trim();
            if (results == "?")
            {
                return DateTimeHelper.ToDateTime(0);
            }
            else if (long.TryParse(results, out long time))
            {
                return DateTimeHelper.ToDateTime(time);
            }
            else
            {
                throw new FileNotFoundException(results.Split(':').Last().Trim());
            }
        }

        public static DateTimeOffset GetLastWriteTime(IAdbClient client, DeviceData device, string fullPath)
        {
            ConsoleOutputReceiver receiver = new ConsoleOutputReceiver();
            client.ExecuteRemoteCommand($"stat -c '%Y' \"{fullPath}\"", device, receiver);
            string results = receiver.ToString().Trim();
            if (results == "?")
            {
                return DateTimeHelper.ToDateTime(0);
            }
            else if (long.TryParse(results, out long time))
            {
                return DateTimeHelper.ToDateTime(time);
            }
            else
            {
                throw new FileNotFoundException(results.Split(':').Last().Trim());
            }
        }

        public static bool DirectoryExists(IAdbClient client, DeviceData device, string fullPath)
        {
            ConsoleOutputReceiver receiver = new ConsoleOutputReceiver();
            client.ExecuteRemoteCommand($"[ -d \"{fullPath}\" ] && echo 1 || echo 0", device, receiver);
            string results = receiver.ToString().Trim();
            return results == "1";
        }

        public static bool FileExists(IAdbClient client, DeviceData device, string fullPath)
        {
            ConsoleOutputReceiver receiver = new ConsoleOutputReceiver();
            client.ExecuteRemoteCommand($"[ -f \"{fullPath}\" ] && echo 1 || echo 0", device, receiver);
            string results = receiver.ToString().Trim();
            return results == "1";
        }
    }
}
